using PDF.Application.Interfaces;
using PDF.Application.UseCases;
using PDF.Infrastructure.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "PDF Service — PROCOMER Calendarización",
        Version = "v1",
        Description = "Microservicio de generación de PDF del itinerario de agenda en español (es-CR)."
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Servicios del PDF Service
builder.Services.AddSingleton<IPdfGeneratorService, QuestPdfGeneratorService>();
builder.Services.AddScoped<GenerarPdfHandler>();

// CORS: el PDF Service es invocado únicamente server-to-server desde el Agendas Service.
// No requiere CORS para peticiones de navegador.
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PDF Service v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
