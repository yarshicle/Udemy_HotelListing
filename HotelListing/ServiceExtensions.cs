using AspNetCoreRateLimit;
using HotelListing.Data;
using HotelListing.Models;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
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

		public static void ConfigureSwaggerDoc(this IServiceCollection services) {
			services.AddSwaggerGen(c => {
				c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
					Description = @"JWT Authorization header using the Bearer scheme.
				Enter 'Bearer' [space] and then your token in the text input below.
				Example: 'Bearer 12345abcdef'",
					Name = "Authorization",
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.ApiKey,
					Scheme = "Bearer"
				});

				c.AddSecurityRequirement(new OpenApiSecurityRequirement {
				{
					new OpenApiSecurityScheme {
						Reference = new OpenApiReference {
							Type = ReferenceType.SecurityScheme,
							Id = "Bearer"
						},
						Scheme = "0auth2",
						Name = "Bearer",
						In = ParameterLocation.Header
					},
					new List<string>()
					}
				});

				c.SwaggerDoc("v1", new OpenApiInfo { Title = "HotelListing", Version = "v1" });
			});
		}

		public static void ConfigureExceptionHandler(this IApplicationBuilder app) {
			app.UseExceptionHandler(error => {
				error.Run(async context => {
					context.Response.StatusCode = StatusCodes.Status500InternalServerError;
					context.Response.ContentType = "application/json";
					var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
					if (contextFeature != null) {
						Log.Error($"Something Went Wrong in {contextFeature.Endpoint?.DisplayName}");
						await context.Response.WriteAsync(new Error {
							StatusCode = context.Response.StatusCode,
							Message = "Internal Server Error. Please Try Again Later"
						}.ToString());
					}
				});
			});
		}

		public static void ConfigureVersioning(this IServiceCollection services) {
			services.AddApiVersioning(o => {
				o.ReportApiVersions = true;
				o.AssumeDefaultVersionWhenUnspecified = true;
				o.DefaultApiVersion = new ApiVersion(1, 0);
				o.ApiVersionReader = new HeaderApiVersionReader("api-version");
			});
		}

		public static void ConfigureHttpCacheHeaders(this IServiceCollection services) {
			services.AddResponseCaching();
			services.AddHttpCacheHeaders(
				(expirationOpt) => {
					expirationOpt.MaxAge = 120;
					expirationOpt.CacheLocation = CacheLocation.Private;
				},
				(validationOpt) => {
					validationOpt.MustRevalidate = true;
				}
			);
		}

		public static void ConfigureRateLimiting(this IServiceCollection services) {
			var rateLimitRules = new List<RateLimitRule> {
				new RateLimitRule {
					Endpoint = "*",
					Limit = 1,
					Period = "5s"
				}
			};
			services.Configure<IpRateLimitOptions>(opt => {
				opt.GeneralRules = rateLimitRules;
			});
			services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
			services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
			services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
			services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
		}
	}
}
