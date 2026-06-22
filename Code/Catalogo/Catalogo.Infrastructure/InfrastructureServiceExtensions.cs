using Catalogo.Application.Interfaces;
using Catalogo.Application.UseCases.Inversores;
using Catalogo.Application.UseCases.Oficinas;
using Catalogo.Application.UseCases.Participantes;
using Catalogo.Application.UseCases.Traslados;
using Catalogo.Infrastructure.Persistence;
using Catalogo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalogo.Infrastructure;

/// <summary>
/// Extensiones de registro de servicios de la capa Infrastructure del Catálogo Service.
/// Registra DbContext, repositorios y handlers de casos de uso en el contenedor DI.
/// </summary>
public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// Registra todos los servicios de infraestructura del Catálogo Service.
    /// </summary>
    /// <param name="services">Colección de servicios del host.</param>
    /// <param name="configuration">Configuración de la aplicación.</param>
    /// <returns>La misma colección para encadenamiento fluido.</returns>
    public static IServiceCollection AddCatalogoInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext — sin migraciones, mapeo a tablas existentes en Azure SQL
        services.AddDbContext<CatalogoDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure(3)));

        // Repositorios
        services.AddScoped<IInversorRepository, InversorRepository>();
        services.AddScoped<IParticipanteRepository, ParticipanteRepository>();
        services.AddScoped<IOficinaRepository, OficinaRepository>();
        services.AddScoped<IMatrizTrasladoRepository, MatrizTrasladoRepository>();
        services.AddScoped<IIdiomaRepository, IdiomaRepository>();
        services.AddScoped<IAgendaStatusChecker, AgendaStatusChecker>();

        // Use case handlers
        services.AddScoped<RegistrarInversorHandler>();
        services.AddScoped<ActualizarInversorHandler>();
        services.AddScoped<EliminarInversorHandler>();
        services.AddScoped<ConsultarInversoresHandler>();
        services.AddScoped<RegistrarParticipanteHandler>();
        services.AddScoped<ActualizarParticipanteHandler>();
        services.AddScoped<EliminarParticipanteHandler>();
        services.AddScoped<ConsultarParticipantesHandler>();
        services.AddScoped<RegistrarOficinaHandler>();
        services.AddScoped<ActualizarOficinaHandler>();
        services.AddScoped<ConsultarOficinasHandler>();
        services.AddScoped<EliminarOficinaHandler>();
        services.AddScoped<RegistrarTrasladoHandler>();
        services.AddScoped<ActualizarTrasladoHandler>();
        services.AddScoped<EliminarTrasladoHandler>();
        services.AddScoped<ConsultarTrasladosHandler>();

        return services;
    }
}
