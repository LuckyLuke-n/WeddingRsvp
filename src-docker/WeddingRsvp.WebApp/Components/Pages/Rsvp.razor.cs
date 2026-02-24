using Microsoft.AspNetCore.Components;
using WeddingRsvp.Abstractions.Models.Rsvps;
using WeddingRsvp.Client;
using WeddingRsvp.WebApp.Diagnostics.Meters;

namespace WeddingRsvp.WebApp.Components.Pages;

public partial class Rsvp : ComponentBase
{
    [Inject] private IRsvpClient RsvpClient { get; set; } = null!;
    
    [Inject] private INotificationClient NotificationClient { get; set; } = null!;

    [Inject] private NavigationManager Navigation { get; set; } = null!;

    [Inject] private ILogger<Rsvp> Logger { get; set; } = null!;

    [Parameter] public string? Id { get; set; }

    [Parameter] public string? Culture { get; set; }

    [SupplyParameterFromForm] private RsvpGuest RsvpGuest { get; set; } = null!;

    private string _errorMessage = string.Empty;
    private int _rsvpHash = 0;

    protected override async Task OnInitializedAsync()
    {
        RsvpGuest ??= new();
        _rsvpHash = RsvpGuest.GetHashCode();

        if (string.IsNullOrEmpty(Culture))
        {
            Navigation.NavigateTo(NavigationMaster.Home);
            return;
        }

        if (string.IsNullOrEmpty(Id))
        {
            Navigation.NavigateTo(NavigationMaster.Home);
            return;
        }

        if (!Guid.TryParse(Id, out var id))
        {
            Logger.LogWarning("Cannot parse rsvp id {Id}.", Id);
            Navigation.NavigateTo(NavigationMaster.NotFound);
            return;
        }

        try
        {
            var response = await RsvpClient.GetRsvpAsync(id).ConfigureAwait(false);
            if (!response.IsSuccess)
            {
                Logger.LogWarning("Cannot get rsvp {Id} with status code {StatusCode}.", id,
                    response.ValueFail.StatusCode);
                _errorMessage = "Cannot load the rsvp.";
                return;
            }

            RsvpGuest = response.ValueSuccess!;
            _rsvpHash = RsvpGuest.GetHashCode();
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Cannot get rsvp {Id}.", id);
        }
    }

    private async Task HandleValidSubmitAsync()
    {
        if (_rsvpHash == RsvpGuest.GetHashCode())
        {
            Logger.LogInformation("No changes in Rsvp {RsvpId} detected. Clients not called.", RsvpGuest.Id);
            Navigation.NavigateTo(NavigationMaster.Invite(Culture!, RsvpGuest.Id));
            return;
        }

        if (RsvpGuest.Response == ResponseType.No)
            RsvpGuest.UpdateForNotAttending();
        
        var response = await RsvpClient.UpdateRsvpAsync(rsvp: RsvpGuest, isAdmin: false ).ConfigureAwait(false);

        if (response.IsSuccess)
        {
            Navigation.NavigateTo(NavigationMaster.Invite(Culture!, RsvpGuest.Id));
            WebAppMeter.CountValidResponse(RsvpGuest.Id);
            
            var res = await NotificationClient.SendNotificationAsync(RsvpGuest).ConfigureAwait(false);
            if (!res.IsSuccess)
                Logger.LogError("Cannot send email for {RsvpId} with status code {StatusCode}.", RsvpGuest.Id, res.ValueFail.StatusCode);
            
            return;
        }

        Logger.LogError("Cannot update rsvp {Id}.", RsvpGuest.Id);
        _errorMessage = "Cannot update the invite.";
        WebAppMeter.CountFailedResponse(RsvpGuest.Id);
    }
}