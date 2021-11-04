using System.Security.Cryptography.X509Certificates;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();

string? certFilePath = Environment.GetEnvironmentVariable("APP_CERT_FILE_PATH");
string? certKeyPath = Environment.GetEnvironmentVariable("APP_CERT_KEY_PATH");
if (certFilePath != null && certKeyPath != null)
{
    X509Certificate2 certificate = X509Certificate2.CreateFromPemFile(certFilePath, certKeyPath);
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ConfigureHttpsDefaults(adapterOptions =>
        {
            adapterOptions.ServerCertificate = certificate;
        });
    });
}

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();
