using HotelListing.Core.DTOs;
using HotelListing.Core.Models;
using HotelListing.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelListing.Core.Services {
	public class AuthManager : IAuthManager {
		private readonly UserManager<ApiUser> _userManager;
		private readonly IConfiguration _configuration;
		private ApiUser _user = null!;

		public AuthManager(UserManager<ApiUser> userManager, IConfiguration configuration) {
			_userManager = userManager;
			_configuration = configuration;
		}

		public async Task<string> CreateToken() {
			var signingCredentials = GetSigningCredentials();
			var claims = await GetClaims();
			var token = GenerateTokenOptions(signingCredentials, claims);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims) {
			var jwtSettings = _configuration.GetSection("Jwt");
			var expiration = DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings.GetSection("lifetime").Value));
			var token = new JwtSecurityToken(
				issuer: jwtSettings.GetSection("issuer").Value,
				claims: claims,
				expires: expiration,
				signingCredentials: signingCredentials
			);

			return token;
		}

		private async Task<List<Claim>> GetClaims() {
			var claims = new List<Claim> {
				new Claim(ClaimTypes.Name, _user.UserName)
			};

			var roles = await _userManager.GetRolesAsync(_user);
			foreach (var role in roles) {
				claims.Add(new Claim(ClaimTypes.Role, role));
			}

			return claims;
		}

		private static SigningCredentials GetSigningCredentials() {
			var key = Environment.GetEnvironmentVariable("HotelListingKey");
			var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));
			return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
		}

		public async Task<bool> ValidateUser(LoginUserDTO userDTO) {
			_user = await _userManager.FindByNameAsync(userDTO.Email);
			var validPassword = await _userManager.CheckPasswordAsync(_user, userDTO.Password);
			return (_user != null && validPassword);
		}

		public async Task<string> CreateRefreshToken() {
			await _userManager.RemoveAuthenticationTokenAsync(_user, "HotelListingApi", "RefreshToken");
			var newRefreshToken = await _userManager.GenerateUserTokenAsync(_user, "HotelListingApi", "RefreshToken");
			await _userManager.SetAuthenticationTokenAsync(_user, "HotelListingApi", "RefreshToken", newRefreshToken);
			return newRefreshToken;
		}

		public async Task<TokenRequest> VerifyRefreshToken(TokenRequest request) {
			JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
			var tokenContent = jwtSecurityTokenHandler.ReadJwtToken(request.Token);
			var username = tokenContent.Claims.ToList().FirstOrDefault(q => q.Type == ClaimTypes.Name)?.Value;
			_user = await _userManager.FindByNameAsync(username);

			var isValid = await _userManager.VerifyUserTokenAsync(_user, "HotelListingApi", "RefreshToken", request.RefreshToken);
			if (isValid) {
				return new TokenRequest {
					Token = await CreateToken(),
					RefreshToken = await CreateRefreshToken()
				};
			}
			await _userManager.UpdateSecurityStampAsync(_user);

			return null!;
		}
	}
}
