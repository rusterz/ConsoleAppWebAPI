namespace WebAPI.DI_container;

public class DiContainer
{
    private List<ServiceDescriptor> _serviceDescriptors;
    public DiContainer(List<ServiceDescriptor> serviceDescriptors)
    {
        _serviceDescriptors = serviceDescriptors;
    }

    private object GetService(Type serviceType)
    {
        var descriptor = _serviceDescriptors
            .SingleOrDefault(x => x.ServiceType == serviceType);

        if (descriptor == null) 
            throw new Exception($"Service of type {serviceType.Name } isn't registered");
        
        if (descriptor.Implementation != null)
            return descriptor.Implementation;

        var actualType = descriptor.ImplementationType ?? descriptor.ServiceType;

        if (actualType.IsAbstract || actualType.IsInterface)
        {
            throw new Exception("Can not instantiate abstract classes or interfaces");
        }

        var constructorInfo = actualType.GetConstructors().First();

        var parameters = constructorInfo.GetParameters()
            .Select(x => GetService(x.ParameterType)).ToArray();
        
        var implementation = Activator.CreateInstance(actualType, parameters);

        if (descriptor.LifeTime == ServiceLifeTime.Singleton) 
            descriptor.Implementation = implementation;

        return implementation;
    }
    public T GetService<T>()
    {
        return (T)GetService(typeof(T));
    }
}
