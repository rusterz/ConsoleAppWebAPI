using WebAPI.DI_container;
using WebAPI.Server;

class Program
{
    static async Task Main(string[] args)
    {
        var services = new DiServiceCollection();
        string[] prefixes = { "http://localhost:5000/" };

        var container = services.GenerateContainer();
        var server = new MyHttpServer(prefixes, services, container);

        await server.Start();
        Console.WriteLine("Server started on http://localhost:5000/");
    }
}





