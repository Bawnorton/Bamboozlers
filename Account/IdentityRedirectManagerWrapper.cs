namespace Bamboozlers.Account;
public interface IIdentityRedirectManager
{
    public void RedirectTo(string url);
    
    public void RedirectTo(string url, Dictionary<string, object?> queryParameters);
}

public class IdentityRedirectManagerWrapper : IIdentityRedirectManager
{
    private readonly IIdentityRedirectManager _redirectManager;

    public IdentityRedirectManagerWrapper(IIdentityRedirectManager redirectManager)
    {
        _redirectManager = redirectManager;
    }

    public virtual void RedirectTo(string url)
    {
        _redirectManager.RedirectTo(url);
    }
    
    public virtual void RedirectTo(string url, Dictionary<string, object?> queryParameters)
    {
        _redirectManager.RedirectTo(url, queryParameters);
    }
}