using System.Net;
using System.Reflection;
using WebAPI.Controllers.Abstracts;
using WebAPI.Controllers.Attributes;
using WebAPI.Controllers.RouteInfo;
using WebAPI.DI_container;

namespace WebAPI.Server;

public class MyHttpServer
{
    private static HttpListener _listener;
    private readonly DiContainer _container;
    private static Dictionary<string, RouteInfo> _routeTable = new Dictionary<string, RouteInfo>();

    public MyHttpServer(string[] prefixes, DiContainer container)
    {
        _listener = new HttpListener();
        _container = container;

        foreach (var prefix in prefixes)
            _listener.Prefixes.Add(prefix);
    }
    
    public async Task Start()
    {
        _listener.Start();
        while (true)
        {
            HttpListenerContext context = await _listener.GetContextAsync();
            await HandleRequest(context, _container);
        }
    }
    
    private async Task HandleRequest(HttpListenerContext context, DiContainer container)
    {
        Guid requestId = Guid.NewGuid();
        try
        {
            string requestUrl = $"{context.Request.HttpMethod}:{context.Request.Url.AbsolutePath.ToLower()}";
            if (_routeTable.TryGetValue(requestUrl, out var routeInfo))
            {
                var controller = (Controller)container.GetService(routeInfo.ControllerType, requestId);
                controller.SetContext(context);
                if (controller != null)
                {
                    var response = routeInfo.ControllerMethod.Invoke(controller, null) as Task<string>;

                    string responseBody = await response;
                    
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseBody);
                    context.Response.ContentType = "application/json";
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }
        finally
        {
            context.Response.OutputStream.Close();
            container.ClearScopedInstances(requestId); 
        }
    }
    
    public static void AddControllers(DiServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsSubclassOf(typeof(Controller)) && !type.IsAbstract)
            {
                services.RegisterScoped(type);
                foreach (var controllerMethod in type.GetMethods())
                {
                    var routeAttrs = controllerMethod.GetCustomAttributes<RouteAttribute>(true);
                    foreach (var routeAttr in routeAttrs)
                    {
                        var routeKey = $"{routeAttr.Method}:{routeAttr.Path}";
                        _routeTable[routeKey] = new RouteInfo(type, controllerMethod);
                    }
                }
            }
        }
    }
}
