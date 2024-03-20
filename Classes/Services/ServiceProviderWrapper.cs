namespace Bamboozlers.Classes.Services;

/// <summary>
/// Wrapper class for IServiceProvider.
/// </summary>
/// <param name="services">
/// The instance of IServiceProvider to be wrapped around.
/// </param>
public sealed class ServiceProviderWrapper(IServiceProvider services)
{
    /// <summary>
    /// Call upon the IServiceProvider instance to create a scope.
    /// </summary>
    public IServiceScope CreateScope()
    {
        return services.CreateScope();
    }

    /// <summary>
    /// Call upon the IServiceProvider instance to retrieve a service.
    /// </summary>
    public T? GetService<T>()
    {
        return services.GetService<T>();
    }
}