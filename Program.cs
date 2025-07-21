using GraphAPIBookingApp.Services;
using Microsoft.Identity.Web;
using Microsoft.Graph;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add Microsoft Identity Web authentication
builder.Services.AddAuthentication()
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

// Add Microsoft Graph - simplified approach
builder.Services.AddScoped<GraphServiceClient>(provider =>
{
    // For now, return a null GraphServiceClient since we're using mock data
    // In a real implementation, you would configure this properly with authentication
    return null!;
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
