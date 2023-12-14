using Entities;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using ServiceContracts;
using Services;
using RepositoryContracts;
using Repositories;
using Serilog;
using FirstEmptyWebApplication19Core1.Filters.ActionFilters;

var builder = WebApplication.CreateBuilder(args);

/*
//disable logging provider:
builder.Host.ConfigureLogging(loggingProvider => { 
loggingProvider.ClearProviders();
    //loggingProvider.AddConsole();
    loggingProvider.AddDebug();
    loggingProvider.AddEventLog();
});
*/

//Serilog
builder.Host.UseSerilog((HostBuilderContext context, 
                        IServiceProvider services, 
                        LoggerConfiguration loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration) //reading from appsettings
    .ReadFrom.Services(services); //reading from services and make them avalilable
});





builder.Services.AddControllersWithViews(options=>
{
    //ha nincs argument, igy lehet ordert adni:
    //options.Filters.Add<ResponseHeaderActionFilter>(5);

    //ha nincs argument:
    //options.Filters.Add<ResponseHeaderActionFilter>();
    //ha nincs pl ilogger de van argument
    //options.Filters.Add(new ResponseHeaderActionFilter(null, "My-Key-From-Global", "My-Value-From-Global"));
    //ha van ilogger + argument
    var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<ResponseHeaderActionFilter>>();
    //IOrderedFilter nelkul (ResponseHeaderFilterben):
    //options.Filters.Add(new ResponseHeaderActionFilter(logger, "My-Key-From-Global", "My-Value-From-Global"));
    //IOrderedFilter-rel (ResponseHeaderFilterben)
    options.Filters.Add(new ResponseHeaderActionFilter(logger, "My-Key-From-Global", "My-Value-From-Global", 2));
});



//add services into Ioc container
builder.Services.AddScoped<ICountriesService, CountriesService>();
builder.Services.AddScoped<IPersonsService, PersonsService>();
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddScoped<IPersonsRepository, PersonsRepository>();

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    } 
    );

builder.Services.AddHttpLogging(options => {
    options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties
    | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
    ;
    });

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

//http request logginghoz kell!
//appsetting.development -et is modositani kell, loggingba warning helyettinfo: "Microsoft.AspNetCore": "Information"
app.UseHttpLogging();

/*
app.Logger.LogDebug("debug-message");

app.Logger.LogInformation("information-message");
app.Logger.LogWarning("warning-message");
app.Logger.LogError("error-message");
app.Logger.LogCritical("critical-message");
*/


//Ez kell, ha a wwwroot folderbe akarok wkhtmltopdf.exe-t rakni:
//Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");

//Ez kell, ha nem akarok a wwwroot folderbe akarok wkhtmltopdf.exe-t rakni:
IWebHostEnvironment env = app.Environment;
RotativaConfiguration.Setup((Microsoft.AspNetCore.Hosting.IHostingEnvironment)env);

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();

//add project -> xUnit Test Project

//***Entities project: ---Country entity---

/*ServiceContracts project: DTO folderben
CountryAddRequest classban ---ToCountry() method semmibol country object----
CountryResponse classban:--- CountryResponse entity, ToCountryResponse() method ami countrybol response
*/

/*Services project: --- Addcountry method ----, aminek countryAddRequest a parametere (ServiceContract/CountryAddRequest)
vagy hibat dob, vagy ToCountry()-t hivja be, vagy ToCountryResponst()-t hivja be.
*/

/*CRUDTest project: CountryServiceTest class: Services project 
tobb method van benne. CountryAddRequest-el dolgozik (ServiceContract/CountryAddRequest). 
Eredmenye: Assert.True...
*/

//-------------------------------------------------------------------------------

/* ICCountryService: List<CountryResponse> GetAllCountries(); */

/*CountryService:GetAllCountries() */

/*CountriesServiceTest: GetAllCountries() method eredmenyet hasznalja mint inputot*/


public partial class Program { } //integration test-hez kell