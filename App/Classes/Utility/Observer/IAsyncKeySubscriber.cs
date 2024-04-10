using Bamboozlers.Classes.Services;

namespace Bamboozlers.Classes.Utility.Observer;

public interface IAsyncKeySubscriber : IAsyncSubscriber
{
    Task OnKeyPressed(KeyEventArgs keyEventArgs);
    Task OnKeyReleased(KeyEventArgs keyEventArgs);
}