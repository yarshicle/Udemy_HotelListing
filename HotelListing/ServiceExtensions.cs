using HotelListing.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace HotelListing {
	public static class ServiceExtensions {
		public static void ConfigureIdentity(this IServiceCollection services) {
			var builder = services.AddIdentityCore<ApiUser>(q => { q.User.RequireUniqueEmail = true; });

			builder = new IdentityBuilder(builder.UserType, typeof(IdentityRole), services);
			builder.AddEntityFrameworkStores<DatabaseContext>().AddDefaultTokenProviders();
		}

		public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration) {
			var jwtSettings = configuration.GetSection("Jwt");
			var key = Environment.GetEnvironmentVariable("HotelListingKey"); //This key has been added to the Machine with the following command prompt command: setx HotelListingKey "38ffc0bb-af45-4cea-95c6-a4c55f09baea" /M

			services.AddAuthentication(o => {
				o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(o => {
				o.TokenValidationParameters = new TokenValidationParameters {
					ValidateIssuer = true,
					ValidateAudience = false,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = jwtSettings.GetSection("issuer").Value,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!))
				};
			});
		}

	}
}
