using HotelListing;
using HotelListing.Configurations;
using HotelListing.Data;
using HotelListing.IRepository;
using HotelListing.Repository;
using HotelListing.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DatabaseContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("sqlConnection"))
);

builder.Services.AddAuthentication();
builder.Services.ConfigureIdentity();
builder.Services.ConfigureJWT(builder.Configuration);

builder.Services.AddCors(o => {
	o.AddPolicy("Cors_AllowAll", builder =>
		builder.AllowAnyOrigin()
		.AllowAnyMethod()
		.AllowAnyHeader());
});

builder.Services.AddAutoMapper(typeof(MapperInitializer));

builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthManager, AuthManager>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen();

AddSwaggerDoc(builder.Services);

builder.Services.AddControllers().AddNewtonsoftJson(o => o.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

builder.Host.UseSerilog((ctx, lc) => lc
	.WriteTo.Console()
	.WriteTo.File(
		path: "logs\\log-.txt",
		outputTemplate: "{Timestamp:" +
		"yyyy-MM-dd HH:mm:ss.fff zzz} [{Level: u3}] {Message:lj}{NewLine}{Exception}",
		rollingInterval: RollingInterval.Day,
		restrictedToMinimumLevel: LogEventLevel.Information
	)
);

var app = builder.Build();

// Configure the HTTP request pipeline.

//if (app.Environment.IsDevelopment()) {
app.UseSwagger();
app.UseSwaggerUI();
//}


app.UseCors("Cors_AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();



void AddSwaggerDoc(IServiceCollection services) {
	builder.Services.AddSwaggerGen(c => {
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
