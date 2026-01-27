using FluentValidation;
using LeadBoard.Shared.Dtos.Settings;
using LeadBoardNet.API.Data;
using LeadBoardNet.API.Extensions;
using LeadBoardNet.API.Mapper;
using LeadBoardNet.API.Repositories;
using LeadBoardNet.API.Services;
using LeadBoardNet.API.Validations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add database context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Cloudinary
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));

// AutoMapper
builder.Services.AddAutoMapper(typeof(ProjectMappingProfile).Assembly);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Controllers 
builder.Services.AddControllers();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<ProjectRequestValidator>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add scoped services
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IImageManager, ImageManager>();

builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IProjectService, ProjectService>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

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
