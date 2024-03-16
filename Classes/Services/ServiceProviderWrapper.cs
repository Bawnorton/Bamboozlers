namespace Bamboozlers.Classes.Services;

/// <summary>
/// Wrapper class for IServiceProvider.
/// </summary>
/// <param name="services">
/// The instance of IServiceProvider to be wrapped around.
/// </param>
public class ServiceProviderWrapper(IServiceProvider services)
{
    private IServiceProvider _services = services;
    
    /// <summary>
    /// Call upon the IServiceProvider instance to create a scope.
    /// </summary>
    public virtual IServiceScope CreateScope()
    {
        return _services.CreateScope();
    }

    /// <summary>
    /// Call upon the IServiceProvider instance to retrieve a service.
    /// </summary>
    public virtual T? GetService<T>()
    {
        return _services.GetService<T>();
    }
}