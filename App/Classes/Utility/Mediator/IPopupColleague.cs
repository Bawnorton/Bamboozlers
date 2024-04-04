using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Data;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Classes.Utility.Mediator;

public interface IPopupColleague
{
    Task OpenKnownPopup(PopupType type, User? focusUser = null, int? chatId = null)
    {
        return Task.CompletedTask;
    }

    Task OpenNewPopup(RenderFragment bodyContent, RenderFragment? headerContent = null, RenderFragment? footerContent = null)
    {
        return Task.CompletedTask;
    }
    
    Task OpenAlertPopup(RenderFragment bodyAlert, Task<bool> confirmationTask)
    {
        return Task.CompletedTask;
    }

    Task DismissPopup()
    {
        return Task.CompletedTask;
    }
}