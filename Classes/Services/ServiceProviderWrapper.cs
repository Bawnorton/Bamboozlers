namespace Bamboozlers.Classes.Services;

public class ServiceProviderWrapper(IServiceProvider services)
{
    private IServiceProvider _services = services;
    
    public virtual IServiceScope CreateScope()
    {
        return _services.CreateScope();
    }

    public virtual T? GetService<T>()
    {
        return _services.GetService<T>();
    }
}