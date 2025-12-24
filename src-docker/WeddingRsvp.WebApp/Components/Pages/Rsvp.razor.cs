using Microsoft.AspNetCore.Components;
using WeddingRsvp.Client;

namespace WeddingRsvp.WebApp.Components.Pages;

public partial class Rsvp : ComponentBase
{
    [Inject] private IRsvpClient RsvpClient { get; set; } = null!;

    [Inject] private NavigationManager Navigation { get; set; } = null!;

    [Inject] private ILogger<Rsvp> Logger { get; set; } = null!;

    [Parameter] public string? Id { get; set; }

    private RsvpGuest _rsvp = new();

    protected override void OnInitialized()
    {
        if (string.IsNullOrEmpty(Id))
        {
            Navigation.NavigateTo(NavigationMaster.Home);
            return;
        }

        if ( !Guid.TryParse(Id, out var id ) )
        {
            Logger.LogWarning("Cannot parse rsvp id {Id}.", Id);
            Navigation.NavigateTo(NavigationMaster.NotFound);
            return;
        }

        try
        {
            var response = RsvpClient.GetRsvpAsync(id).Result;
            if (!response.IsSuccess)
            {
                Navigation.NavigateTo(NavigationMaster.Home);
                Logger.LogWarning("Cannot get rsvp {Id} with status code {StatusCode}.", id, response.ValueFail.StatusCode);
                return;
            }
        
            _rsvp = response.ValueSuccess!;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Cannot get rsvp {Id}.", id);
        }
    }

    private void HandleValidSubmit()
    {
        //
    }
}