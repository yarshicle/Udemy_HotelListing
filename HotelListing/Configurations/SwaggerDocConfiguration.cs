﻿using Microsoft.OpenApi.Models;

namespace HotelListing.Configurations {
	public class SwaggerDocConfiguration {
		public void AddSwaggerDoc(IServiceCollection services) {
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

				c.AddSecurityRequirement(new OpenApiSecurityRequirement() {
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
	}
}
