using Agendas.Application.DTOs;
using Agendas.Application.Interfaces;
using Agendas.Domain.Entities;
using Agendas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Agendas.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de agendas usando Entity Framework Core.
/// </summary>
public class AgendaRepository(AgendasDbContext context) : IAgendaRepository
{
    /// <inheritdoc/>
    public async Task<IEnumerable<Agenda>> GetAllAsync(FiltrosAgendaQuery filtros)
    {
        var query = context.Agendas
            .Include(a => a.Reuniones)
            .AsNoTracking()
            .AsQueryable();

        if (filtros.InversorId.HasValue)
            query = query.Where(a => a.InversorId == filtros.InversorId.Value);

        if (filtros.Fecha.HasValue)
            query = query.Where(a => a.Fecha.Date == filtros.Fecha.Value.Date);

        if (!string.IsNullOrEmpty(filtros.Estado))
            query = query.Where(a => a.Estado.ToString() == filtros.Estado);

        return await query.OrderByDescending(a => a.FechaGeneracion).ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Agenda?> GetByIdAsync(Guid id) =>
        await context.Agendas
            .Include(a => a.Reuniones)
            .FirstOrDefaultAsync(a => a.Id == id);

    /// <inheritdoc/>
    public async Task AddAsync(Agenda agenda)
    {
        await context.Agendas.AddAsync(agenda);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(Agenda agenda)
    {
        context.Agendas.Update(agenda);
        await context.SaveChangesAsync();
    }
}
