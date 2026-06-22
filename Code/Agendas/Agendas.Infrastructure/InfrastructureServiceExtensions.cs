using Agendas.Application.Engine;
using Agendas.Application.Interfaces;
using Agendas.Application.UseCases;
using Agendas.Infrastructure.HttpClients;
using Agendas.Infrastructure.Persistence;
using Agendas.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Agendas.Infrastructure;

/// <summary>
/// Extensiones de registro de servicios de la capa Infrastructure del Agendas Service.
/// </summary>
public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// Registra DbContext, repositorios, HTTP clients tipados con Polly y use case handlers.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración de la aplicación.</param>
    public static IServiceCollection AddAgendasInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext — sin migraciones
        services.AddDbContext<AgendasDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure(3)));

        // Repositorios
        services.AddScoped<IAgendaRepository, AgendaRepository>();

        // HTTP Clients tipados con resiliencia Polly
        var catalogoUrl = configuration["ServiceUrls:CatalogoService"]
            ?? throw new InvalidOperationException("ServiceUrls:CatalogoService no configurado.");
        var pdfUrl = configuration["ServiceUrls:PdfService"]
            ?? throw new InvalidOperationException("ServiceUrls:PdfService no configurado.");

        services.AddHttpClient<ICatalogoHttpClient, CatalogoHttpClient>(c =>
        {
            c.BaseAddress = new Uri(catalogoUrl);
            c.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddStandardResilienceHandler(); // 3 reintentos + circuit breaker + timeout (AC-09)

        services.AddHttpClient<IPdfServiceHttpClient, PdfServiceHttpClient>(c =>
        {
            c.BaseAddress = new Uri(pdfUrl);
            c.Timeout = TimeSpan.FromSeconds(60); // PDF puede tomar hasta 30s según SPEC
        })
        .AddStandardResilienceHandler();

        // Componentes del motor de scheduling
        services.AddSingleton<ILanguageCompatibilityFilter, LanguageCompatibilityFilter>();
        services.AddSingleton<IAvailabilitySlotBuilder, AvailabilitySlotBuilder>();
        services.AddSingleton<ITravelTimeResolver, TravelTimeResolver>();
        services.AddSingleton<ISchedulingEngine, SchedulingEngine>();

        // Use case handlers
        services.AddScoped<GenerarAgendaHandler>();
        services.AddScoped<ConsultarAgendasHandler>();
        services.AddScoped<ConsultarAgendaHandler>();
        services.AddScoped<AnularAgendaHandler>();

        return services;
    }
}
