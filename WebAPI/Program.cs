using WebAPI.DI_container;
using WebAPI.Server;
using WebAPI.Services;
using WebAPI.Services.GuidHelpers;

namespace WebAPI;

class Program
{
    static async Task Main(string[] args)
    {
        var services = new DiServiceCollection();
        MyHttpServer.AddControllers(services);
        
        services.RegisterSingleton<IRandomGuidProvider, RandomGuidProvider>();
        services.RegisterTransient<IGuidService, GuidService>();

        var container = services.GenerateContainer();
        
        var guidService = container.GetService<IGuidService>();
        guidService.PrintSomething();
        
        string prefix = "http://localhost:5000/";
        var server = new MyHttpServer(prefix, container);
        await server.Start();
    }
}