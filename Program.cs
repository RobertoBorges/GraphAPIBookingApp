using GraphAPIBookingApp.Services;
using Microsoft.Identity.Web;
using Microsoft.Graph;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add Microsoft Identity Web with authentication
builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration, "AzureAd")
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();

// Add Microsoft Graph using Azure.Identity ClientSecretCredential
builder.Services.AddScoped<GraphServiceClient>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var azureAdSection = configuration.GetSection("AzureAd");

    var clientId = azureAdSection["ClientId"];
    var tenantId = azureAdSection["TenantId"];
    var clientSecret = azureAdSection["ClientSecret"];

    // Create the ClientSecretCredential
    var clientSecretCredential = new ClientSecretCredential(
        tenantId,
        clientId,
        clientSecret);

    // Create the Graph service client with Azure.Identity credential
    return new GraphServiceClient(clientSecretCredential, new[] { "https://graph.microsoft.com/.default" });
});

// Add custom services
builder.Services.AddScoped<IBookingService, GraphAPIBookingApp.Services.BookingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
