using Lab8_PhamVanTung_2324801030079.Data;
using Lab8_PhamVanTung_2324801030079.Extensions;
using Lab8_PhamVanTung_2324801030079.Models.Cache;
using Lab8_PhamVanTung_2324801030079.Services;
using Lab8_PhamVanTung_2324801030079.Services.Navigation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddScoped<IServiceRequestOperations, ServiceRequestOperations>();

var app = builder.Build();

app.UseApplicationPipeline();

await using (var scope = app.Services.CreateAsyncScope())
{
 
    var seeder = scope.ServiceProvider.GetRequiredService<ApplicationDbSeeder>();
    await seeder.SeedAsync();

 
    var navigationCacheOperations = scope.ServiceProvider.GetRequiredService<INavigationCacheOperations>();
    await navigationCacheOperations.WarmUpAsync();

  
    var masterDataCacheOperations = scope.ServiceProvider.GetRequiredService<IMasterDataCacheOperations>();

  
    await masterDataCacheOperations.CreateMasterDataCacheAsync();
   
}

app.Run();