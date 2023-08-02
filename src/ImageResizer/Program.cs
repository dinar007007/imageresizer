using System.Text.Json.Serialization;
using ImageResizer.Core;
using Swashbuckle.AspNetCore.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
var swaggerOptions = new SwaggerOptions();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ImageResizer",
    });
    var filePath = Path.Combine(AppContext.BaseDirectory, "ImageResizer.xml");
    options.IncludeXmlComments(filePath);
});
builder.Services.AddHttpClient<IImageGetter, ImageGetter>();
builder.Services.AddTransient<IImageGetter, ImageGetter>();
builder.Services.AddTransient<IImageResizer, ImageResizer.Core.ImageResizer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
