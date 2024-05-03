namespace WebAPI.DI_container;

public class ServiceDescriptor
{
    public Type ServiceType { get; }
    
    public Type ImplementationType { get; }
    public object Implementation { get; internal set; }
    
    public ServiceLifeTime LifeTime { get; set; }

    public ServiceDescriptor(object implementation, ServiceLifeTime lifeTime)
    {
        ServiceType = implementation.GetType();
        Implementation = implementation;
        LifeTime = lifeTime;
    }
    
    public ServiceDescriptor(Type serviceType, ServiceLifeTime lifeTime)
    {
        ServiceType = serviceType;
        LifeTime = lifeTime;
    }
    
    public ServiceDescriptor(Type serviceType, Type implementationType,  ServiceLifeTime lifeTime)
    {
        ServiceType = serviceType;
        ImplementationType = implementationType;
        LifeTime = lifeTime;
    }
}
