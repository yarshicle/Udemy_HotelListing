using HotelListing.Configurations;
using HotelListing.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DatabaseContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("sqlConnection"))
);

builder.Services.AddCors(o => {
	o.AddPolicy("Cors_AllowAll", builder =>
		builder.AllowAnyOrigin()
		.AllowAnyMethod()
		.AllowAnyHeader());
});

builder.Services.AddAutoMapper(typeof(MapperInitializer));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

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

app.UseAuthorization();

app.MapControllers();

app.Run();