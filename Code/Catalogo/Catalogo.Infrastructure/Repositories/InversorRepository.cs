using Catalogo.Application.Interfaces;
using Catalogo.Domain.Entities;
using Catalogo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de inversores usando Entity Framework Core.
/// </summary>
public class InversorRepository(CatalogoDbContext context) : IInversorRepository
{
    /// <inheritdoc/>
    public async Task<IEnumerable<Inversor>> GetAllAsync() =>
        await context.Inversores
            .Include(i => i.InversoresIdiomas)
                .ThenInclude(ii => ii.Idioma)
            .AsNoTracking()
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<Inversor?> GetByIdAsync(Guid id) =>
        await context.Inversores
            .Include(i => i.InversoresIdiomas)
                .ThenInclude(ii => ii.Idioma)
            .FirstOrDefaultAsync(i => i.Id == id);

    /// <inheritdoc/>
    public async Task AddAsync(Inversor inversor)
    {
        await context.Inversores.AddAsync(inversor);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(Inversor inversor)
    {
        // Mismo patrón que ParticipanteRepository: extraer nuevos idiomas y vaciar la
        // colección de navegación ANTES del primer SaveChangesAsync para evitar que
        // DetectChanges() marque los nuevos InversorIdioma (clave compuesta ValueGeneratedNever)
        // como Modified en lugar de Added (causa DbUpdateConcurrencyException).
        var newIdiomas = inversor.InversoresIdiomas.ToList();
        inversor.InversoresIdiomas.Clear();

        var existentes = await context.InversoresIdiomas
            .Where(ii => ii.InversorId == inversor.Id)
            .ToListAsync();
        context.InversoresIdiomas.RemoveRange(existentes);

        // Primer save: DELETE idiomas antiguos + UPDATE propiedades escalares del inversor.
        await context.SaveChangesAsync();

        // Segundo save: INSERT de los nuevos idiomas.
        await context.InversoresIdiomas.AddRangeAsync(newIdiomas);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Inversor inversor)
    {
        context.Inversores.Remove(inversor);
        await context.SaveChangesAsync();
    }
}
