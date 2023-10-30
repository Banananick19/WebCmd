using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebCmd.App;
using WebCmd.Lib;
using Options = Microsoft.Extensions.Options.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient();
builder.Configuration.AddConfiguration(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());
builder.Services.ConfigureFromEnvVariable<Config>("HOSTS");
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

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();


public static class BuilderExt
{
    public static IServiceCollection ConfigureFromEnvVariable<T>(this IServiceCollection collection, string envName) where T : class 
    {
            var envValue = Environment.GetEnvironmentVariable(envName);
            if (string.IsNullOrWhiteSpace(envName)) throw new ArgumentException("Empty env variable");
            T? conf = JsonConvert.DeserializeObject<T>(envValue);
            if (conf == null) throw new ArgumentException("Env value could not parse in excepted object");
            collection.AddOptions();
            return collection.AddSingleton<T>(conf!);
    } 
}
