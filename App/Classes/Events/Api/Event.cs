namespace Bamboozlers.Classes.Events.Api;

public abstract class Event<T> 
{
    public const string DefaultPhase = "Default";

    protected T invoker;

    public T Invoker()
    {
        return invoker;
    }

    public abstract void Register(T listener);

    public virtual void AddPhaseOrdering(string firstPhase, string secondPhase)
    {
        
    }
}