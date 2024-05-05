using WebAPI.DI_container;
using WebAPI.Server;

namespace WebAPI;

class Program
{
    static async Task Main(string[] args)
    {
        var services = new DiServiceCollection();
        MyHttpServer.AddControllers(services);
        
        var container = services.GenerateContainer();
        string prefix = "http://localhost:5000/";
        
        var server = new MyHttpServer(prefix, container);
        await server.Start();
    }
}