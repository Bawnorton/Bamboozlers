using System.ComponentModel.DataAnnotations;
using Bamboozlers.Account;
using Bamboozlers.Classes.AppDbContext;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Components.Settings;

public partial class CompProfile : CompSettings
{
    [Parameter]
    public bool EditingAllowed { get; set; }
}