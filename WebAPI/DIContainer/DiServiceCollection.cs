namespace WebAPI.DI_container;

public class DiServiceCollection
{
    private List<ServiceDescriptor> _serviceDescriptors = new List<ServiceDescriptor>();
    
    public DiContainer GenerateContainer()
    {
        return new DiContainer(_serviceDescriptors);
    }

    public void RegisterScoped<TService>(TService implementation)
    {
        _serviceDescriptors.Add(new ServiceDescriptor(implementation, ServiceLifeTime.Scoped));
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
    
    
    public void RegisterTransient(Type serviceType)
    {
        _serviceDescriptors.Add(new ServiceDescriptor(serviceType, ServiceLifeTime.Scoped));
    }
    
    public void RegisterTransient<TService>()
    {
        _serviceDescriptors.Add(new ServiceDescriptor(typeof(TService), ServiceLifeTime.Transient));
    }
    
    public void RegisterTransient<TService, TImplementation>() where TImplementation : TService
    {
        _serviceDescriptors.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifeTime.Transient));
    }
}
