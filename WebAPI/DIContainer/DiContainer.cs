namespace WebAPI.DI_container;

public class DiContainer
{
    private List<ServiceDescriptor> _serviceDescriptors;
    private Dictionary<Guid, Dictionary<Type, object>> _scopedInstances = new Dictionary<Guid, Dictionary<Type, object>>();

    public DiContainer(List<ServiceDescriptor> serviceDescriptors)
    {
        _serviceDescriptors = serviceDescriptors;
    }

    public object GetService(Type type, Guid requestId)
    {
        var descriptor = _serviceDescriptors.SingleOrDefault(x => x.ServiceType == type);

        if (descriptor == null) 
            throw new Exception($"Service type {type.Name } isn't registered");
        
        switch (descriptor.LifeTime)
        {
            case ServiceLifeTime.Singleton:
                if (descriptor.Implementation == null)
                {
                    descriptor.Implementation = CreateInstance(descriptor, requestId);
                }
                return descriptor.Implementation;
            case ServiceLifeTime.Scoped:
                return GetOrCreateScopedInstance(descriptor, requestId);
            case ServiceLifeTime.Transient:
                return CreateInstance(descriptor, requestId);
            default:
                throw new Exception("Unknown service lifetime");
        }
    }

    private object GetOrCreateScopedInstance(ServiceDescriptor descriptor, Guid requestId)
    {
        if (!_scopedInstances.TryGetValue(requestId, out var scopedInstances))
        {
            scopedInstances = new Dictionary<Type, object>();
            _scopedInstances[requestId] = scopedInstances;
        }

        if (!scopedInstances.TryGetValue(descriptor.ServiceType, out var instance))
        {
            instance = CreateInstance(descriptor, requestId);
            scopedInstances[descriptor.ServiceType] = instance;
        }

        return instance;
    }

    private object CreateInstance(ServiceDescriptor descriptor, Guid requestId)
    {
        var actualType = descriptor.ImplementationType ?? descriptor.ServiceType;

        if (actualType.IsAbstract || actualType.IsInterface)
            throw new Exception("Can't instantiate abstract classes or interfaces");

        var constructorInfo = actualType.GetConstructors().First();
        var parameters = constructorInfo.GetParameters()
            .Select(x => GetService(x.ParameterType, requestId)).ToArray();

        var implementation = Activator.CreateInstance(actualType, parameters);

        return implementation;
    }
    
    public T GetService<T>(Guid requestId = default)
    {
        if (requestId == default)
            requestId = Guid.Empty; 
        return (T)GetService(typeof(T), requestId);
    }
    
    public void ClearScopedInstances(Guid requestId)
    {
        if (_scopedInstances.TryGetValue(requestId, out var instances))
        {
            _scopedInstances.Remove(requestId);
            // TO DO: Очистка ресурсов
        }
    }
}
