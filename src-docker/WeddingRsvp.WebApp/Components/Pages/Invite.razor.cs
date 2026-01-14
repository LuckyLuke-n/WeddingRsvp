using Microsoft.AspNetCore.Components;
using WeddingRsvp.Client;

namespace WeddingRsvp.WebApp.Components.Pages;

public partial class Invite : ComponentBase
{
    [Inject] private IRsvpClient RsvpClient { get; set; } = null!;
    [Inject] private IInformationClient InformationClient { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<Rsvp> Logger { get; set; } = null!;
    
    [Parameter] public string? Id { get; set; }
    [Parameter] public string? Culture { get; set; }
    
    private RsvpGuest _rsvpGuest = new();
    private DynamicInformation _information = new();
    private string _errorMessage = string.Empty;
    
    protected override async Task OnInitializedAsync()
    {
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
        
        await GetRsvpAsync(id).ConfigureAwait(false);
        await GetInformationAsync(Culture).ConfigureAwait(false);
    }

    private async Task GetRsvpAsync(Guid id)
    {
        try
        {
            var response = await RsvpClient.GetRsvpAsync(id).ConfigureAwait(false);
            if (!response.IsSuccess)
            {
                Navigation.NavigateTo(NavigationMaster.Home);
                Logger.LogWarning("Cannot get rsvp {Id} with status code {StatusCode}.", id, response.ValueFail.StatusCode);
                return;
            }

            _rsvpGuest = response.ValueSuccess!;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Cannot get rsvp {Id}.", id);
            _errorMessage = "Cannot load the invite.";
        }
    }

    private async Task GetInformationAsync(string language)
    {
        try
        {
            var response = await InformationClient.GetInformationAsync(language).ConfigureAwait(false);
            if (!response.IsSuccess)
            {
                Navigation.NavigateTo(NavigationMaster.Home);
                Logger.LogWarning("Cannot get invitation for {Language} with status code {StatusCode}.", language, response.ValueFail.StatusCode);
                return;
            }

            _information = response.ValueSuccess!;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Cannot get invitation for {Language}.", language);
            _errorMessage = "Cannot load the invite.";
        }
    }
}