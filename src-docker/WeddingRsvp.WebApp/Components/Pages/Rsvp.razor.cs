using Microsoft.AspNetCore.Components;
using WeddingRsvp.Client;

namespace WeddingRsvp.WebApp.Components.Pages;

public partial class Rsvp : ComponentBase
{
    [Parameter]
    public string? Id { get; set; }

    private RsvpGuest _rsvp = new();
    
    protected override void OnInitialized()
    {
        if (string.IsNullOrEmpty(Id))
            Navigation.NavigateTo("/");
        
    }

    private void HandleValidSubmit()
    {
        //
    }
}