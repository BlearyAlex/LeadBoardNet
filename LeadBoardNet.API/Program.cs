using LeadBoard.Shared.Dtos.Settings;
using LeadBoardNet.API.Data;
using LeadBoardNet.API.Extensions;
using LeadBoardNet.API.Mapper;
using LeadBoardNet.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add database context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Cloudinary
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));

// Add services to the container.

// AutoMapper
builder.Services.AddAutoMapper(typeof(ProjectMappingProfile).Assembly);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add scoped services
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

var app = builder.Build();

app.UseErrorHandlingMiddleware();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
