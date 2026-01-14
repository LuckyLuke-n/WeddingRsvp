using Microsoft.AspNetCore.Components;
using WeddingRsvp.Client;

namespace WeddingRsvp.WebApp.Components.Pages;

public partial class Rsvp : ComponentBase
{
    [Inject] private IRsvpClient RsvpClient { get; set; } = null!;

    [Inject] private NavigationManager Navigation { get; set; } = null!;

    [Inject] private ILogger<Rsvp> Logger { get; set; } = null!;

    [Parameter] public string? Id { get; set; }

    [Parameter] public string? Culture { get; set; }

    [SupplyParameterFromForm] private RsvpGuest RsvpGuest { get; set; } = null!;
    private string _errorMessage  = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        RsvpGuest ??= new();

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
                Navigation.NavigateTo(NavigationMaster.Home);
                Logger.LogWarning("Cannot get rsvp {Id} with status code {StatusCode}.", id, response.ValueFail.StatusCode);
                return;
            }

            RsvpGuest = response.ValueSuccess!;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Cannot get rsvp {Id}.", id);
        }
    }

    private async Task HandleValidSubmitAsync()
    {
        var response = await RsvpClient.UpdateRsvpAsync(RsvpGuest).ConfigureAwait(false);

        if (response.IsSuccess)
        {
            Navigation.NavigateTo(NavigationMaster.Invite(Culture!, RsvpGuest.Id));
            return;
        }

        Logger.LogError("Cannot update rsvp {Id}.", RsvpGuest.Id);
        _errorMessage = "Cannot update the invite.";
    }
}