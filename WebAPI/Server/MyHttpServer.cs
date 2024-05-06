using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
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
            var httpContext = new HttpContext(context);

            await HandleRequest(httpContext);
        }
    }
    
    private async Task HandleRequest(IHttpContext context)
    {
        Guid requestId = Guid.NewGuid();
        try
        {
            string requestUrl = $"{context.Request.HttpMethod}:{context.Request.Url.AbsolutePath.ToLower()}";
            if (_routeTable.TryGetValue(requestUrl, out var routeInfo))
            {
                var controller = (Controller)_container.GetService(routeInfo.ControllerType, requestId);
                var parameters = await PrepareParameters(context, routeInfo.ControllerMethod);
                
                if (parameters.Any(p => p == null) || parameters.Any(p => p == Type.Missing))
                {
                    await SendErrorResponseAsync(context.Response, "Invalid JSON data in request.", HttpStatusCode.BadRequest);
                    return;
                }

                var response = routeInfo.ControllerMethod.Invoke(controller, parameters) as Task<string>;
                await SendJsonResponseAsync(context.Response, await response);
            }
            else
            {
                await SendErrorResponseAsync(context.Response, "Route not found.", HttpStatusCode.NotFound);
            }
        }
        finally
        {
            context.Response.CloseOutputStream();
            _container.ClearScopedInstances(requestId); 
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
    
    private async Task<object[]> PrepareParameters(IHttpContext context, MethodInfo method)
    {
        var parameters = new List<object>();
        var methodParams = method.GetParameters();

        foreach (var param in methodParams)
        {
            if (param.ParameterType == typeof(IHttpContext))
            {
                parameters.Add(context);
            }
            else if (context.Request.HttpMethod.ToUpper() == "POST" && 
                (param.ParameterType.IsClass || param.ParameterType.IsValueType || param.ParameterType.IsPrimitive))
            {
                var dto = await ReadRequestBodyAsync(context.Request.InputStream, context.Request.ContentEncoding, param.ParameterType);
                parameters.Add(dto ?? Type.Missing);
            }
            else if (context.Request.HttpMethod.ToUpper() == "GET" && 
                (param.ParameterType.IsValueType || param.ParameterType.IsPrimitive))
            {
                string value = context.Request.QueryString[param.Name] ?? context.Request.Headers[param.Name];
                try
                {
                    parameters.Add(Convert.ChangeType(value, param.ParameterType));
                }
                catch
                {
                    parameters.Add(Type.Missing); 
                }
            }
            else
            {
                parameters.Add(Type.Missing);
            }
        }
    
        return parameters.ToArray();
    }

    private async Task<object> ReadRequestBodyAsync(Stream inputStream, Encoding contentEncoding, Type dtoType)
    {       
        using (var reader = new StreamReader(inputStream, contentEncoding))
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
    
    private async Task SendJsonResponseAsync(IResponseContext response, string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        response.SetContentAndWriteAsync(responseBody, "application/json", statusCode);
    }

    private async Task SendErrorResponseAsync(IResponseContext response, string message, HttpStatusCode errorStatusCode)
    {
        var errorObject = JsonSerializer.Serialize(new { error = message });
        await SendJsonResponseAsync(response, errorObject, errorStatusCode);
    }
}
