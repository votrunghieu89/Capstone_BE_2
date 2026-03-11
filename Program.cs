using Capstone_2_BE;
using Capstone_2_BE.DALs;
using Capstone_2_BE.DALs.Technician;
using Capstone_2_BE.Repositories;
using Capstone_2_BE.Repositories.Technician;
using Capstone_2_BE.Securities;
using Capstone_2_BE.Services;
using Capstone_2_BE.Services.Technician;
using Capstone_2_BE.Settings;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// Configure Redis
var redisSettings = new RedisSetting();
builder.Configuration.GetSection("Redis").Bind(redisSettings);

var redisOptions = new ConfigurationOptions
{
    EndPoints = { $"{redisSettings.Host}:{redisSettings.Port}" },
    Password = string.IsNullOrEmpty(redisSettings.Password) ? null : redisSettings.Password,
    DefaultDatabase = redisSettings.DefaultDatabase,
    AbortOnConnectFail = false
};
var redisConnection = ConnectionMultiplexer.Connect(redisOptions);

// Register Redis connection as Singleton
builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);

// Register Redis wrapper as Scoped
builder.Services.AddScoped<Redis>();

// Register repositories and services
builder.Services.AddScoped<IAuthenticationRepo, AuthenticationDAL>();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<Token>();
builder.Services.AddScoped<Email>();

// Register Technician repositories and services
builder.Services.AddScoped<ITechnicianProfileRepo, TechnicianProfileDAL>();
builder.Services.AddScoped<TechnicianProfileService>();
builder.Services.AddScoped<ITechnicianRatingRepo, TechnicianRatingDAL>();
builder.Services.AddScoped<TechnicianRatingService>();

// Register AWS S3
builder.Services.AddScoped<AWS>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
