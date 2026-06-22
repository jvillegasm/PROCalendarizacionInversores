using Frontend.MVC.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── Servicios ─────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// HTTP clients tipados para cada microservicio
var catalogoUrl = builder.Configuration["ServiceUrls:CatalogoService"]
    ?? throw new InvalidOperationException("ServiceUrls:CatalogoService no configurado.");
var agendasUrl = builder.Configuration["ServiceUrls:AgendasService"]
    ?? throw new InvalidOperationException("ServiceUrls:AgendasService no configurado.");

builder.Services.AddHttpClient<CatalogoApiService>(c =>
{
    c.BaseAddress = new Uri(catalogoUrl);
    c.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<AgendasApiService>(c =>
{
    c.BaseAddress = new Uri(agendasUrl);
    c.Timeout = TimeSpan.FromSeconds(60);
});

var app = builder.Build();

// ─── Pipeline HTTP ──────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

