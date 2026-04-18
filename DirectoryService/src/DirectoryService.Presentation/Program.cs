using DirectoryService.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependency(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService"));
}

app.MapControllers();
app.Run();