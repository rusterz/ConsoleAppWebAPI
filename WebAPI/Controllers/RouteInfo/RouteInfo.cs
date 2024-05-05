using System.Reflection;

namespace WebAPI.Controllers.RouteInfo;

public class RouteInfo
{
    public Type ControllerType { get; set; }
    public MethodInfo ControllerMethod { get; set; }

    public RouteInfo(Type controllerType, MethodInfo controllerMethod)
    {
        ControllerType = controllerType;
        ControllerMethod = controllerMethod;
    }
}