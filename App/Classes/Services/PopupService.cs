using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Utility.Mediator;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Classes.Services;

public class PopupService : IPopupService
{
    public List<IPopupColleague> Colleagues { get; set; } = [];

    public void RegisterColleague(IPopupColleague popupColleague)
    {
        if (!Colleagues.Contains(popupColleague))
            Colleagues.Add(popupColleague);
    }

    public bool RemoveColleague(IPopupColleague popupColleague)
    {
        return Colleagues.Remove(popupColleague);
    }

    public async Task RequestKnownPopup(PopupType type, User? focusUser = null, int? chatId = null)
    {
        foreach (var colleague in Colleagues)
        {
            await colleague.OpenKnownPopup(type, focusUser, chatId);
        }
    }

    public async Task RequestNewPopup(RenderFragment bodyContent, RenderFragment? headerContent = null,
        RenderFragment? footerContent = null)
    {
        foreach (var colleague in Colleagues)
        {
            await colleague.OpenNewPopup(bodyContent, headerContent, footerContent);
        }
    }

    public async Task RequestAlertPopup(RenderFragment bodyAlert, Task<bool> confirmationTask)
    {
        foreach (var colleague in Colleagues)
        {
            await colleague.OpenAlertPopup(bodyAlert, confirmationTask);
        }
    }

    public async Task RequestDismissPopup()
    {
        foreach (var colleague in Colleagues)
        {
            await colleague.DismissPopup();
        }
    }
}

public interface IPopupService : IPopupMediator;