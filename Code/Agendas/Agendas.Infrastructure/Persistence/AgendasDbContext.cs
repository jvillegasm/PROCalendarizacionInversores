using Agendas.Domain.Entities;
using Agendas.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Agendas.Infrastructure.Persistence;

/// <summary>
/// DbContext del Agendas Service.
/// Solo mapea las entidades propias del dominio de agendas: Agenda y Reunion.
/// Comparte la misma Azure SQL Database que el Catálogo Service (DP-02).
/// </summary>
public class AgendasDbContext(DbContextOptions<AgendasDbContext> options) : DbContext(options)
{
    /// <summary>Tabla de agendas generadas.</summary>
    public DbSet<Agenda> Agendas { get; set; }

    /// <summary>Tabla de reuniones que conforman cada agenda.</summary>
    public DbSet<Reunion> Reuniones { get; set; }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Agenda
        modelBuilder.Entity<Agenda>(e =>
        {
            e.ToTable("Agendas");
            e.HasKey(x => x.Id);
            e.Property(x => x.InversorId).IsRequired();
            e.Property(x => x.Fecha).IsRequired();
            e.Property(x => x.Estado)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => (EstadoAgenda)Enum.Parse(typeof(EstadoAgenda), v));
            e.Property(x => x.FechaGeneracion).IsRequired();
            e.Property(x => x.FechaAnulacion);
        });

        // Reunion
        modelBuilder.Entity<Reunion>(e =>
        {
            e.ToTable("Reuniones");
            e.HasKey(x => x.Id);
            e.Property(x => x.ParticipanteId).IsRequired();
            e.Property(x => x.HoraInicio).IsRequired();
            e.Property(x => x.HoraFin).IsRequired();
            e.Property(x => x.OficinaId).IsRequired();
            e.Property(x => x.IdiomaReunion).IsRequired().HasMaxLength(100);
            e.Property(x => x.Orden).IsRequired();
            e.Property(x => x.TiempoTrasladoSiguiente).IsRequired();
            e.HasOne(x => x.Agenda).WithMany(a => a.Reuniones)
                .HasForeignKey(x => x.AgendaId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
