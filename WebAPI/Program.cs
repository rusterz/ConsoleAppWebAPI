using WebAPI.DI_container;
using WebAPI.Server;

class Program
{
    static async Task Main(string[] args)
    {
        var services = new DiServiceCollection();
        MyHttpServer.AddControllers(services);

        var container = services.GenerateContainer();
        string[] prefixes = { "http://localhost:5000/" };
        var server = new MyHttpServer(prefixes, container);

        await server.Start();
        Console.WriteLine("Server started on http://localhost:5000/");
    }
}





