using AutoMapper;
using HotelListing.Core.DTOs;
using HotelListing.Core.Models;
using HotelListing.Core.Services;
using HotelListing.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Controllers {
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : ControllerBase {
		private readonly UserManager<ApiUser> _userManager;
		private readonly ILogger<AccountController> _logger;
		private readonly IMapper _mapper;
		private readonly IAuthManager _authManager;

		public AccountController(UserManager<ApiUser> userManager, ILogger<AccountController> logger, IMapper mapper, IAuthManager authManager) {
			_userManager = userManager;
			_logger = logger;
			_mapper = mapper;
			_authManager = authManager;
		}

		[HttpPost]
		[Route("register")]
		[ProducesResponseType(StatusCodes.Status202Accepted)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> Register([FromBody] UserDTO userDTO) {
			_logger.LogInformation($"Registration Attempt for {userDTO.Email}");
			if (!ModelState.IsValid) {
				return BadRequest(ModelState);
			}

			var user = _mapper.Map<ApiUser>(userDTO);
			user.UserName = userDTO.Email;
			var result = await _userManager.CreateAsync(user, userDTO.Password);

			if (!result.Succeeded) {
				foreach (var error in result.Errors) {
					ModelState.AddModelError(error.Code, error.Description);

				}
				return BadRequest(ModelState);
			}
			await _userManager.AddToRolesAsync(user, userDTO.Roles);
			return Accepted();
		}

		[HttpPost]
		[Route("login")]
		[ProducesResponseType(StatusCodes.Status202Accepted)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> Login([FromBody] LoginUserDTO loginDTO) {
			_logger.LogInformation($"Login Attempt for {loginDTO.Email}");
			if (!ModelState.IsValid) {
				return BadRequest(ModelState);
			}

			if (!await _authManager.ValidateUser(loginDTO)) {
				return Unauthorized();
			}

			return Accepted(new TokenRequest { Token = await _authManager.CreateToken(), RefreshToken = await _authManager.CreateRefreshToken() });

		}

		[HttpPost]
		[Route("refreshtoken")]
		public async Task<IActionResult> RefreshToken([FromBody] TokenRequest request) {
			var tokenRequest = await _authManager.VerifyRefreshToken(request);
			if (tokenRequest is null) {
				return Unauthorized();
			}
			return Ok(tokenRequest);
		}
	}
}
