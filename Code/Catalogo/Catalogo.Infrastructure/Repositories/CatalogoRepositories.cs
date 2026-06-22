using Catalogo.Application.Interfaces;
using Catalogo.Domain.Entities;
using Catalogo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Infrastructure.Repositories;

/// <summary>Implementación del repositorio de oficinas.</summary>
public class OficinaRepository(CatalogoDbContext context) : IOficinaRepository
{
    /// <inheritdoc/>
    public async Task<IEnumerable<Oficina>> GetAllAsync() =>
        await context.Oficinas.AsNoTracking().ToListAsync();

    /// <inheritdoc/>
    public async Task<Oficina?> GetByIdAsync(Guid id) =>
        await context.Oficinas.FirstOrDefaultAsync(o => o.Id == id);

    /// <inheritdoc/>
    public async Task AddAsync(Oficina oficina)
    {
        await context.Oficinas.AddAsync(oficina);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(Oficina oficina)
    {
        context.Oficinas.Update(oficina);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Oficina oficina)
    {
        // Eliminar manualmente los hijos que tienen FK RESTRICT hacia Oficinas,
        // ya que SQL Server no hace DELETE en cascada para estas relaciones.

        // 1. MatrizTraslados que usan esta oficina como origen o destino.
        var traslados = await context.MatrizTraslados
            .Where(t => t.OficinaOrigenId == oficina.Id || t.OficinaDestinoId == oficina.Id)
            .ToListAsync();
        context.MatrizTraslados.RemoveRange(traslados);

        // 2. Participantes inactivos asignados a esta oficina.
        //    (Los activos ya fueron bloqueados por RN-06 en el handler.)
        var participantes = await context.Participantes
            .Where(p => p.OficinaId == oficina.Id)
            .ToListAsync();
        context.Participantes.RemoveRange(participantes);

        context.Oficinas.Remove(oficina);
        await context.SaveChangesAsync();
    }
}

/// <summary>Implementación del repositorio de idiomas.</summary>
public class IdiomaRepository(CatalogoDbContext context) : IIdiomaRepository
{
    /// <inheritdoc/>
    public async Task<IEnumerable<Idioma>> GetAllAsync() =>
        await context.Idiomas.AsNoTracking().ToListAsync();

    /// <inheritdoc/>
    public async Task<IEnumerable<Idioma>> GetByIdsAsync(IEnumerable<Guid> ids) =>
        await context.Idiomas.Where(i => ids.Contains(i.Id)).ToListAsync();
}

/// <summary>Implementación del repositorio de la matriz de traslados.</summary>
public class MatrizTrasladoRepository(CatalogoDbContext context) : IMatrizTrasladoRepository
{
    /// <inheritdoc/>
    public async Task<IEnumerable<MatrizTraslado>> GetAllAsync() =>
        await context.MatrizTraslados
            .Include(t => t.OficinaOrigen)
            .Include(t => t.OficinaDestino)
            .AsNoTracking()
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<MatrizTraslado?> GetByIdAsync(Guid id) =>
        await context.MatrizTraslados.FirstOrDefaultAsync(t => t.Id == id);

    /// <inheritdoc/>
    public async Task<bool> ExisteParAsync(Guid origenId, Guid destinoId) =>
        await context.MatrizTraslados.AnyAsync(t =>
            t.OficinaOrigenId == origenId && t.OficinaDestinoId == destinoId);

    /// <inheritdoc/>
    public async Task AddParSimetricoAsync(MatrizTraslado directo, MatrizTraslado inverso)
    {
        // Si ya existe el par directo, actualizar en lugar de duplicar
        var existenteDirecto = await context.MatrizTraslados.FirstOrDefaultAsync(t =>
            t.OficinaOrigenId == directo.OficinaOrigenId && t.OficinaDestinoId == directo.OficinaDestinoId);
        var existenteInverso = await context.MatrizTraslados.FirstOrDefaultAsync(t =>
            t.OficinaOrigenId == inverso.OficinaOrigenId && t.OficinaDestinoId == inverso.OficinaDestinoId);

        if (existenteDirecto is not null)
            existenteDirecto.TiempoMinutos = directo.TiempoMinutos;
        else
            await context.MatrizTraslados.AddAsync(directo);

        if (existenteInverso is not null)
            existenteInverso.TiempoMinutos = inverso.TiempoMinutos;
        else
            await context.MatrizTraslados.AddAsync(inverso);

        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateParSimetricoAsync(Guid origenId, Guid destinoId, int nuevoTiempoMinutos)
    {
        var directo = await context.MatrizTraslados.FirstOrDefaultAsync(t =>
            t.OficinaOrigenId == origenId && t.OficinaDestinoId == destinoId);
        var inverso = await context.MatrizTraslados.FirstOrDefaultAsync(t =>
            t.OficinaOrigenId == destinoId && t.OficinaDestinoId == origenId);

        if (directo is not null) directo.TiempoMinutos = nuevoTiempoMinutos;
        if (inverso is not null) inverso.TiempoMinutos = nuevoTiempoMinutos;

        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteParAsync(Guid id)
    {
        var traslado = await context.MatrizTraslados.FirstOrDefaultAsync(t => t.Id == id);
        if (traslado is null) return;

        // Eliminar el simétrico (B→A cuando se pide borrar A→B)
        var simetrico = await context.MatrizTraslados.FirstOrDefaultAsync(t =>
            t.OficinaOrigenId == traslado.OficinaDestinoId &&
            t.OficinaDestinoId == traslado.OficinaOrigenId);

        if (simetrico is not null) context.MatrizTraslados.Remove(simetrico);
        context.MatrizTraslados.Remove(traslado);
        await context.SaveChangesAsync();
    }
}

/// <summary>
/// Implementación de IAgendaStatusChecker.
/// Consulta directamente la tabla Agendas (mismo Azure SQL DB) para verificar RN-03.
/// </summary>
public class AgendaStatusChecker(CatalogoDbContext context) : IAgendaStatusChecker
{
    /// <inheritdoc/>
    public async Task<bool> TieneAgendasActivasAsync(Guid inversorId) =>
        await context.AgendasStatus.AnyAsync(a =>
            a.InversorId == inversorId && a.Estado == "Activa");
}
