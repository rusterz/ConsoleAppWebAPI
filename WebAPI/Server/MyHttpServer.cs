using System.Net;
using System.Reflection;
using System.Text.Json;
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

    public MyHttpServer(string prefix, DiContainer container)
    {
        _listener = new HttpListener();
        _container = container;
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
                var parameters = await PrepareParameters(context, routeInfo.ControllerMethod);
                
                if (parameters.Any(p => p == null))
                {
                    await SendErrorResponse(context.Response, "Invalid JSON data in request.", HttpStatusCode.BadRequest);
                    return;
                }

                var response = routeInfo.ControllerMethod.Invoke(controller, parameters) as Task<string>;
                await SendJsonResponse(context.Response, await response);
            }
            else
            {
                await SendErrorResponse(context.Response, "Route not found.", HttpStatusCode.NotFound);
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
    
    private async Task<object[]> PrepareParameters(HttpListenerContext context, MethodInfo method)
    {
        var parameters = new List<object>();
        var methodParams = method.GetParameters();

        foreach (var param in methodParams)
        {
            if (param.ParameterType == typeof(HttpListenerContext))
            {
                parameters.Add(context);
            }
            else if (context.Request.HttpMethod.ToUpper() == "POST" && 
                (param.ParameterType.IsClass || param.ParameterType.IsValueType && !param.ParameterType.IsPrimitive))
            {
                var dto = await ReadRequestBodyAsync(context.Request, param.ParameterType);
                parameters.Add(dto);
            }
            else
            {
                parameters.Add(Type.Missing);
            }
        }
    
        return parameters.ToArray();
    }

    private async Task<object> ReadRequestBodyAsync(HttpListenerRequest request, Type dtoType)
    {
        if (!request.HasEntityBody)
            return default(Type);
        
        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
        {
            try
            {
                var bodyString = await reader.ReadToEndAsync();
                return JsonSerializer.Deserialize(bodyString, dtoType);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
    
    private async Task SendJsonResponse(HttpListenerResponse response, string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseBody);
        response.ContentType = "application/json";
        response.ContentLength64 = buffer.Length;
        response.StatusCode = (int)statusCode;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
    }

    private async Task SendErrorResponse(HttpListenerResponse response, string message, HttpStatusCode errorStatusCode)
    {
        var errorObject = JsonSerializer.Serialize(new { error = message });
        await SendJsonResponse(response, errorObject, errorStatusCode);
    }
}
