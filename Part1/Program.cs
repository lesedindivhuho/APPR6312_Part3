using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Part1.Data;
using Part1.Services;
using System.IO;
var builder = WebApplication.CreateBuilder(args);

var expectedWebRoot = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
if (!Directory.Exists(expectedWebRoot))
{
    Directory.CreateDirectory(expectedWebRoot);
    builder.Environment.WebRootPath = expectedWebRoot;
}
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); //enable Razor pages in addition to MVC controllers/views
builder.Services.AddDbContext<Part1Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Part1Context")));

// Azure Functions app (tax certificates and project update logging)
var functionsBaseUrl = builder.Configuration["AzureFunctions:BaseUrl"] ?? "http://localhost:7071/api";
var functionKey = builder.Configuration["AzureFunctions:FunctionKey"];
builder.Services.AddHttpClient(FunctionsClient.HttpClientName, client =>
{
    client.BaseAddress = new Uri(functionsBaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(10);

    // Only needed once the functions are deployed to Azure
    if (!string.IsNullOrWhiteSpace(functionKey))
    {
        client.DefaultRequestHeaders.Add("x-functions-key", functionKey);
    }
});
builder.Services.AddScoped<FunctionsClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
//Map both controllers and Razor Pages so the app runs regardless of page type.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();



// Lets the integration tests start the app with WebApplicationFactory<Program>
public partial class Program { }
