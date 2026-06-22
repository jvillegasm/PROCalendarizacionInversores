using Catalogo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Infrastructure.Persistence;

/// <summary>
/// DbContext del Catálogo Service. Mapea todas las entidades del catálogo a las tablas
/// existentes en Azure SQL Database (sin migraciones).
/// </summary>
public class CatalogoDbContext(DbContextOptions<CatalogoDbContext> options) : DbContext(options)
{
    /// <summary>Tabla de idiomas del catálogo.</summary>
    public DbSet<Idioma> Idiomas { get; set; }

    /// <summary>Tabla de inversores.</summary>
    public DbSet<Inversor> Inversores { get; set; }

    /// <summary>Tabla de unión inversores-idiomas.</summary>
    public DbSet<InversorIdioma> InversoresIdiomas { get; set; }

    /// <summary>Tabla de participantes.</summary>
    public DbSet<Participante> Participantes { get; set; }

    /// <summary>Tabla de unión participantes-idiomas.</summary>
    public DbSet<ParticipanteIdioma> ParticipantesIdiomas { get; set; }

    /// <summary>Tabla de bloques de disponibilidad de participantes.</summary>
    public DbSet<DisponibilidadParticipante> DisponibilidadParticipantes { get; set; }

    /// <summary>Tabla de oficinas físicas.</summary>
    public DbSet<Oficina> Oficinas { get; set; }

    /// <summary>Tabla de pares de traslado entre oficinas.</summary>
    public DbSet<MatrizTraslado> MatrizTraslados { get; set; }

    /// <summary>
    /// Vista de sólo lectura sobre la tabla Agendas del Agendas Service
    /// (misma BD compartida). Usada por IAgendaStatusChecker para RN-03.
    /// </summary>
    public DbSet<AgendaStatus> AgendasStatus { get; set; }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Idiomas
        modelBuilder.Entity<Idioma>(e =>
        {
            e.ToTable("Idiomas");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).IsRequired().HasMaxLength(100);
            e.Property(x => x.Codigo).IsRequired().HasMaxLength(10);
        });

        // Inversores
        modelBuilder.Entity<Inversor>(e =>
        {
            e.ToTable("Inversores");
            e.HasKey(x => x.Id);
            e.Property(x => x.NombreCompleto).IsRequired().HasMaxLength(200);
            e.Property(x => x.Empresa).IsRequired().HasMaxLength(200);
            e.Property(x => x.PaisOrigen).IsRequired().HasMaxLength(100);
            e.Property(x => x.LugarHospedaje).IsRequired().HasMaxLength(300);
            e.Property(x => x.FechaInicioVisita).IsRequired();
            e.Property(x => x.FechaFinVisita).IsRequired();
        });

        // InversoresIdiomas (PK compuesta)
        modelBuilder.Entity<InversorIdioma>(e =>
        {
            e.ToTable("InversoresIdiomas");
            e.HasKey(x => new { x.InversorId, x.IdiomaId });
            e.HasOne(x => x.Inversor).WithMany(i => i.InversoresIdiomas)
                .HasForeignKey(x => x.InversorId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Idioma).WithMany(i => i.InversoresIdiomas)
                .HasForeignKey(x => x.IdiomaId).OnDelete(DeleteBehavior.Restrict);
        });

        // Oficinas
        modelBuilder.Entity<Oficina>(e =>
        {
            e.ToTable("Oficinas");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).IsRequired().HasMaxLength(200);
            e.Property(x => x.DireccionFisica).IsRequired().HasMaxLength(400);
            e.Property(x => x.Coordenadas).HasMaxLength(100);
        });

        // Participantes
        modelBuilder.Entity<Participante>(e =>
        {
            e.ToTable("Participantes");
            e.HasKey(x => x.Id);
            e.Property(x => x.NombreCompleto).IsRequired().HasMaxLength(200);
            e.Property(x => x.Cargo).IsRequired().HasMaxLength(200);
            e.Property(x => x.Activo).IsRequired();
            e.HasOne(x => x.Oficina).WithMany(o => o.Participantes)
                .HasForeignKey(x => x.OficinaId).OnDelete(DeleteBehavior.Restrict);
        });

        // ParticipantesIdiomas (PK compuesta)
        modelBuilder.Entity<ParticipanteIdioma>(e =>
        {
            e.ToTable("ParticipantesIdiomas");
            e.HasKey(x => new { x.ParticipanteId, x.IdiomaId });
            e.HasOne(x => x.Participante).WithMany(p => p.ParticipantesIdiomas)
                .HasForeignKey(x => x.ParticipanteId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Idioma).WithMany(i => i.ParticipantesIdiomas)
                .HasForeignKey(x => x.IdiomaId).OnDelete(DeleteBehavior.Restrict);
        });

        // DisponibilidadParticipantes
        modelBuilder.Entity<DisponibilidadParticipante>(e =>
        {
            e.ToTable("DisponibilidadParticipantes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Fecha).IsRequired();
            e.Property(x => x.HoraInicio).IsRequired();
            e.Property(x => x.HoraFin).IsRequired();
            e.HasOne(x => x.Participante).WithMany(p => p.Disponibilidades)
                .HasForeignKey(x => x.ParticipanteId).OnDelete(DeleteBehavior.Cascade);
        });

        // MatrizTraslados
        modelBuilder.Entity<MatrizTraslado>(e =>
        {
            e.ToTable("MatrizTraslados");
            e.HasKey(x => x.Id);
            e.Property(x => x.TiempoMinutos).IsRequired();
            e.HasOne(x => x.OficinaOrigen).WithMany(o => o.TrasladosOrigen)
                .HasForeignKey(x => x.OficinaOrigenId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.OficinaDestino).WithMany(o => o.TrasladosDestino)
                .HasForeignKey(x => x.OficinaDestinoId).OnDelete(DeleteBehavior.Restrict);
        });

        // AgendaStatus — vista de sólo lectura sobre tabla del Agendas Service
        modelBuilder.Entity<AgendaStatus>(e =>
        {
            e.ToTable("Agendas");
            e.HasKey(x => x.Id);
            e.Property(x => x.InversorId).IsRequired();
            e.Property(x => x.Estado).IsRequired().HasMaxLength(20);
        });
    }
}

/// <summary>
/// Proyección de sólo lectura sobre la tabla Agendas para verificar RN-03.
/// No es entidad del dominio del Catálogo; solo se usa para consultas de estado.
/// </summary>
public class AgendaStatus
{
    /// <summary>Id de la agenda.</summary>
    public Guid Id { get; set; }

    /// <summary>Id del inversor dueño de la agenda.</summary>
    public Guid InversorId { get; set; }

    /// <summary>Estado de la agenda ("Activa" o "Anulada").</summary>
    public string Estado { get; set; } = string.Empty;
}
