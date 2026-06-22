using Catalogo.Application.Interfaces;
using Catalogo.Domain.Entities;
using Catalogo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Infrastructure.Repositories;

/// <summary>Implementación del repositorio de participantes.</summary>
public class ParticipanteRepository(CatalogoDbContext context) : IParticipanteRepository
{
    /// <inheritdoc/>
    public async Task<IEnumerable<Participante>> GetAllAsync() =>
        await context.Participantes
            .Include(p => p.Oficina)
            .Include(p => p.ParticipantesIdiomas).ThenInclude(pi => pi.Idioma)
            .Include(p => p.Disponibilidades)
            .AsNoTracking()
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<IEnumerable<Participante>> GetActivosAsync() =>
        await context.Participantes
            .Where(p => p.Activo)
            .Include(p => p.Oficina)
            .Include(p => p.ParticipantesIdiomas).ThenInclude(pi => pi.Idioma)
            .Include(p => p.Disponibilidades)
            .AsNoTracking()
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<Participante?> GetByIdAsync(Guid id) =>
        await context.Participantes
            .Include(p => p.Oficina)
            .Include(p => p.ParticipantesIdiomas).ThenInclude(pi => pi.Idioma)
            .Include(p => p.Disponibilidades)
            .FirstOrDefaultAsync(p => p.Id == id);

    /// <inheritdoc/>
    public async Task AddAsync(Participante participante)
    {
        await context.Participantes.AddAsync(participante);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(Participante participante)
    {
        // Extraer las nuevas relaciones ANTES de vaciar las colecciones.
        // Problema: DetectChanges() (llamado por SaveChangesAsync) itera las colecciones de
        // navegación; los nuevos ParticipanteIdioma tienen clave compuesta (ParticipanteId,
        // IdiomaId) que es ValueGeneratedNever, así que EF Core los marca como Modified en
        // lugar de Added → genera UPDATE sobre filas inexistentes → 0 rows →
        // DbUpdateConcurrencyException. Vaciar las colecciones antes del primer save evita
        // que DetectChanges las procese; luego se insertan explícitamente con AddRange.
        var newIdiomas         = participante.ParticipantesIdiomas.ToList();
        var newDisponibilidades = participante.Disponibilidades.ToList();

        participante.ParticipantesIdiomas.Clear();
        participante.Disponibilidades.Clear();

        // Marcar para borrado los registros actuales en BD.
        var idiomasExistentes = await context.ParticipantesIdiomas
            .Where(pi => pi.ParticipanteId == participante.Id).ToListAsync();
        context.ParticipantesIdiomas.RemoveRange(idiomasExistentes);

        var dispExistentes = await context.DisponibilidadParticipantes
            .Where(d => d.ParticipanteId == participante.Id).ToListAsync();
        context.DisponibilidadParticipantes.RemoveRange(dispExistentes);

        // Primer save: DELETE de relaciones antiguas + UPDATE de propiedades escalares
        // (el participante está tracked desde GetByIdAsync; DetectChanges detecta los cambios
        // en NombreCompleto, Cargo, OficinaId, Activo, etc.).
        await context.SaveChangesAsync();

        // Segundo save: INSERT de las nuevas relaciones.
        await context.ParticipantesIdiomas.AddRangeAsync(newIdiomas);
        await context.DisponibilidadParticipantes.AddRangeAsync(newDisponibilidades);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Participante participante)
    {
        context.Participantes.Remove(participante);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<int> CountActivosByOficinaAsync(Guid oficinaId) =>
        await context.Participantes.CountAsync(p => p.OficinaId == oficinaId && p.Activo);
}
