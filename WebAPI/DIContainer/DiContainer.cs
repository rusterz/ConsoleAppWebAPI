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
        var descriptor = _serviceDescriptors
            .SingleOrDefault(x => x.ServiceType == type);

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
                if (!_scopedInstances.TryGetValue(requestId, out var requestScopedInstances ))
                {
                    requestScopedInstances = new Dictionary<Type, object>();
                    _scopedInstances[requestId] = requestScopedInstances;
                }
                if (!requestScopedInstances.TryGetValue(type, out var instance))
                {
                    instance = CreateInstance(descriptor, requestId);
                    requestScopedInstances[type] = instance;
                }
                return instance;
            case ServiceLifeTime.Transient:
                return CreateInstance(descriptor, requestId);
            default:
                throw new Exception("Unknown service lifetime");
        }
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
    
    public T GetService<T>(Guid requestId)
    {
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
