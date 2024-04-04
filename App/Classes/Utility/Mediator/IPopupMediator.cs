using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Data;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Classes.Utility.Mediator;

public interface IPopupMediator
{
    public List<IPopupColleague> Colleagues { get; set; }
    void RegisterColleague(IPopupColleague popupColleague);
    bool RemoveColleague(IPopupColleague popupColleague);
    Task RequestKnownPopup(PopupType type, User? focusUser = null, int? chatId = null);
    Task RequestNewPopup(RenderFragment bodyContent, RenderFragment? headerContent = null, RenderFragment? footerContent = null);
    Task RequestAlertPopup(RenderFragment bodyAlert, Task<bool> confirmationTask);
    Task RequestDismissPopup();
}