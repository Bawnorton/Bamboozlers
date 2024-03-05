namespace Bamboozlers.Account;
public interface IIdentityRedirectManager
{
    public void RedirectTo(string url);
    public void RedirectTo(string uri, Dictionary<string, object?> queryParameters);
    public void RedirectToWithStatus(string uri, string message, HttpContext context);
    public void RedirectToCurrentPage();
    public void RedirectToCurrentPageWithStatus(string message, HttpContext context);
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
    
    public virtual void RedirectTo(string uri, Dictionary<string, object?> queryParameters)
    {
        _redirectManager.RedirectTo(uri, queryParameters);
    }
    
    public virtual void RedirectToWithStatus(string uri, string message, HttpContext context)
    {
        _redirectManager.RedirectToWithStatus(uri, message, context);
    }
    
    public virtual void RedirectToCurrentPage()
    {
        _redirectManager.RedirectToCurrentPage();
    }
    
    public virtual void RedirectToCurrentPageWithStatus(string message, HttpContext context)
    {
        _redirectManager.RedirectToCurrentPageWithStatus(message, context);
    }
    
}