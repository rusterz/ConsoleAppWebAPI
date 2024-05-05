using System.Reflection;
using WebAPI.Controllers.Abstracts;

namespace WebAPI.DI_container;

public class DiServiceCollection
{
    private List<ServiceDescriptor> _serviceDescriptors = new List<ServiceDescriptor>();
    
    public void AddControllers()
    {
        var controllerBaseType = typeof(Controller);
        var assembly = Assembly.GetExecutingAssembly(); 

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsSubclassOf(controllerBaseType) && !type.IsAbstract)
            {
                RegisterScoped(type); 
            }
        }
    }
    
    public void RegisterScoped(Type serviceType)
    {
        _serviceDescriptors.Add(new ServiceDescriptor(serviceType, ServiceLifeTime.Scoped));
    }

    public void RegisterScoped<TService>()
    {
        _serviceDescriptors.Add(new ServiceDescriptor(typeof(TService), ServiceLifeTime.Scoped));
    }

    public void RegisterScoped<TService, TImplementation>() where TImplementation : TService
    {
        _serviceDescriptors.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifeTime.Scoped));
    }
    
    public void RegisterSingleton<TService>()
    {
        _serviceDescriptors.Add(new ServiceDescriptor(typeof(TService), ServiceLifeTime.Singleton));
    }

    public void RegisterSingleton<TService, TImplementation>() where TImplementation : TService
    {
        _serviceDescriptors.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifeTime.Singleton));
    }
    
    public void RegisterSingleton<TService>(TService implementaion)
    {
        _serviceDescriptors.Add(new ServiceDescriptor(implementaion, ServiceLifeTime.Singleton));
    }
    
    public void RegisterTransient<TService>()
    {
        _serviceDescriptors.Add(new ServiceDescriptor(typeof(TService), ServiceLifeTime.Transient));
    }
    
    public void RegisterTransient<TService, TImplementation>() where TImplementation : TService
    {
        _serviceDescriptors.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifeTime.Transient));
    }

    public DiContainer GenerateContainer()
    {
        return new DiContainer(_serviceDescriptors);
    }
}
