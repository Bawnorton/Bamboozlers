namespace Bamboozlers.Account;
public interface IIdentityRedirectManager
{
    public void RedirectTo(string url);
    public void RedirectTo(string uri, Dictionary<string, object?> queryParameters);
    public void RedirectToWithStatus(string uri, string message, HttpContext context);
    public void RedirectToCurrentPage();
    public void RedirectToCurrentPageWithStatus(string message, HttpContext context);
}

public class IdentityRedirectManagerWrapper(IIdentityRedirectManager redirectManager) : IIdentityRedirectManager
{
    public virtual void RedirectTo(string url)
    {
        redirectManager.RedirectTo(url);
    }
    
    public virtual void RedirectTo(string uri, Dictionary<string, object?> queryParameters)
    {
        redirectManager.RedirectTo(uri, queryParameters);
    }
    
    public virtual void RedirectToWithStatus(string uri, string message, HttpContext context)
    {
        redirectManager.RedirectToWithStatus(uri, message, context);
    }
    
    public virtual void RedirectToCurrentPage()
    {
        redirectManager.RedirectToCurrentPage();
    }
    
    public virtual void RedirectToCurrentPageWithStatus(string message, HttpContext context)
    {
        redirectManager.RedirectToCurrentPageWithStatus(message, context);
    }
}