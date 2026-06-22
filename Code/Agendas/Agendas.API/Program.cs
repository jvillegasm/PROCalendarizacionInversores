using Agendas.API.Middleware;
using Agendas.Infrastructure;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ─── Servicios ─────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Agendas Service — PROCOMER Calendarización",
        Version = "v1",
        Description = "Microservicio de agendas: generación automática, consulta, anulación y descarga de PDF."
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Infrastructure (DbContext + repositorios + HTTP clients con Polly + handlers)
builder.Services.AddAgendasInfrastructure(builder.Configuration);

// CORS — orígenes explícitos según SPEC §4: solo URLs del frontend (Development y Production)
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// ─── Pipeline HTTP ──────────────────────────────────────────────────────────────
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Agendas Service v1");
    c.RoutePrefix = "swagger";
});

app.UseCors();

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

