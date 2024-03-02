namespace Bamboozlers.Classes;

public interface IViewDelegate
{
    public string GetViewName();


    IDictionary<string, object> GetParameters();
} 