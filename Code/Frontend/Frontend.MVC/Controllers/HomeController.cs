using System.Diagnostics;
using Frontend.MVC.Models;
using Frontend.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.MVC.Controllers;

public class HomeController(
    CatalogoApiService catalogoService,
    AgendasApiService agendasService,
    IConfiguration configuration) : Controller
{
    public async Task<IActionResult> Index()
    {
        var inversores    = await catalogoService.GetInversoresAsync();
        var participantes = await catalogoService.GetParticipantesAsync();
        var oficinas      = await catalogoService.GetOficinasAsync();
        var agendas       = await agendasService.GetAgendasAsync();

        ViewBag.TotalInversores    = inversores.Count();
        ViewBag.TotalParticipantes = participantes.Count();
        ViewBag.TotalOficinas      = oficinas.Count();
        ViewBag.TotalAgendas       = agendas.Count();
        ViewBag.CatalogoUrl        = configuration["ServiceUrls:CatalogoService"];
        ViewBag.AgendasUrl         = configuration["ServiceUrls:AgendasService"];

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}

