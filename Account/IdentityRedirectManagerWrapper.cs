namespace Bamboozlers.Account;
public interface IIdentityRedirectManager
{
    public void RedirectTo(string url);
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
    
}