using Frontend.MVC.Models;
using System.Net.Http.Json;

namespace Frontend.MVC.Services;

/// <summary>
/// Servicio cliente HTTP para el Catálogo Service.
/// Consume directamente la URL pública (sin API Gateway según SPEC §5.1).
/// </summary>
public class CatalogoApiService(HttpClient httpClient)
{
    public async Task<IEnumerable<InversorViewModel>> GetInversoresAsync() =>
        await httpClient.GetFromJsonAsync<IEnumerable<InversorViewModel>>("api/inversores") ?? [];

    public async Task<InversorViewModel?> GetInversorAsync(Guid id) =>
        await httpClient.GetFromJsonAsync<InversorViewModel>($"api/inversores/{id}");

    public async Task<HttpResponseMessage> CrearInversorAsync(object request) =>
        await httpClient.PostAsJsonAsync("api/inversores", request);

    public async Task<HttpResponseMessage> ActualizarInversorAsync(Guid id, object request) =>
        await httpClient.PutAsJsonAsync($"api/inversores/{id}", request);

    public async Task<HttpResponseMessage> EliminarInversorAsync(Guid id) =>
        await httpClient.DeleteAsync($"api/inversores/{id}");

    public async Task<IEnumerable<ParticipanteViewModel>> GetParticipantesAsync() =>
        await httpClient.GetFromJsonAsync<IEnumerable<ParticipanteViewModel>>("api/participantes") ?? [];

    public async Task<IEnumerable<ParticipanteViewModel>> GetParticipantesActivosAsync() =>
        await httpClient.GetFromJsonAsync<IEnumerable<ParticipanteViewModel>>("api/participantes/activos") ?? [];

    public async Task<ParticipanteViewModel?> GetParticipanteAsync(Guid id) =>
        await httpClient.GetFromJsonAsync<ParticipanteViewModel>($"api/participantes/{id}");

    public async Task<HttpResponseMessage> CrearParticipanteAsync(object request) =>
        await httpClient.PostAsJsonAsync("api/participantes", request);

    public async Task<HttpResponseMessage> ActualizarParticipanteAsync(Guid id, object request) =>
        await httpClient.PutAsJsonAsync($"api/participantes/{id}", request);

    public async Task<HttpResponseMessage> EliminarParticipanteAsync(Guid id) =>
        await httpClient.DeleteAsync($"api/participantes/{id}");

    public async Task<IEnumerable<OficinaViewModel>> GetOficinasAsync() =>
        await httpClient.GetFromJsonAsync<IEnumerable<OficinaViewModel>>("api/oficinas") ?? [];

    public async Task<HttpResponseMessage> CrearOficinaAsync(object request) =>
        await httpClient.PostAsJsonAsync("api/oficinas", request);

    public async Task<HttpResponseMessage> EliminarOficinaAsync(Guid id) =>
        await httpClient.DeleteAsync($"api/oficinas/{id}");

    public async Task<IEnumerable<MatrizTrasladoViewModel>> GetTrasladosAsync() =>
        await httpClient.GetFromJsonAsync<IEnumerable<MatrizTrasladoViewModel>>("api/traslados") ?? [];

    public async Task<HttpResponseMessage> CrearTrasladoAsync(object request) =>
        await httpClient.PostAsJsonAsync("api/traslados", request);

    public async Task<IEnumerable<IdiomaViewModel>> GetIdiomasAsync() =>
        await httpClient.GetFromJsonAsync<IEnumerable<IdiomaViewModel>>("api/idiomas") ?? [];
}

/// <summary>
/// Servicio cliente HTTP para el Agendas Service.
/// </summary>
public class AgendasApiService(HttpClient httpClient)
{
    public async Task<IEnumerable<AgendaResumenViewModel>> GetAgendasAsync(
        Guid? inversorId = null, DateTime? fecha = null, string? estado = null)
    {
        var query = new List<string>();
        if (inversorId.HasValue) query.Add($"inversorId={inversorId}");
        if (fecha.HasValue) query.Add($"fecha={fecha.Value:yyyy-MM-dd}");
        if (!string.IsNullOrEmpty(estado)) query.Add($"estado={estado}");
        var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";
        return await httpClient.GetFromJsonAsync<IEnumerable<AgendaResumenViewModel>>($"agendas{qs}") ?? [];
    }

    public async Task<AgendaDetalleViewModel?> GetAgendaAsync(Guid id) =>
        await httpClient.GetFromJsonAsync<AgendaDetalleViewModel>($"agendas/{id}");

    public async Task<HttpResponseMessage> GenerarAgendaAsync(object request) =>
        await httpClient.PostAsJsonAsync("agendas/generar", request);

    public async Task<HttpResponseMessage> AnularAgendaAsync(Guid id) =>
        await httpClient.DeleteAsync($"agendas/{id}");

    public async Task<byte[]?> GetPdfAsync(Guid id)
    {
        var response = await httpClient.GetAsync($"agendas/{id}/pdf");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsByteArrayAsync();
    }
}
