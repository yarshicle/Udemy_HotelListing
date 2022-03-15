using AspNetCoreRateLimit;
using HotelListing.Core;
using HotelListing.Core.IRepository;
using HotelListing.Core.Repository;
using HotelListing.Core.Services;
using HotelListing.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DatabaseContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("sqlConnection"))
);

builder.Services.AddMemoryCache();

builder.Services.ConfigureRateLimiting();
builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureHttpCacheHeaders();

builder.Services.AddAuthentication();
builder.Services.ConfigureIdentity();
builder.Services.ConfigureJWT(builder.Configuration);

builder.Services.AddCors(o => {
	o.AddPolicy("Cors_AllowAll", builder =>
		builder.AllowAnyOrigin()
		.AllowAnyMethod()
		.AllowAnyHeader());
});

//builder.Services.AddAutoMapper(typeof(MapperInitializer));
builder.Services.ConfigureAutoMapper();

builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthManager, AuthManager>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen();

builder.Services.ConfigureSwaggerDoc();
builder.Services.ConfigureVersioning();

builder.Services.AddControllers(config => {
	config.CacheProfiles.Add("120SecondsDuration", new CacheProfile {
		Duration = 120
	});
}).AddNewtonsoftJson(o => o.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

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

app.ConfigureExceptionHandler();
app.UseHttpsRedirection();

app.UseCors("Cors_AllowAll");

app.UseResponseCaching();
app.UseHttpCacheHeaders();

app.UseIpRateLimiting();



app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();



