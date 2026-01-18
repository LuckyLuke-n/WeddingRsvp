using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using WeddingRsvp.Client;
using WeddingRsvp.WebApp.Configurations;

namespace WeddingRsvp.WebApp.Components.Pages;

public partial class Admin : ComponentBase
{
    [Inject] private IRsvpClient RsvpClient { get; set; } = null!;
    [Inject] private IInformationClient InformationClient { get; set; } = null!;
    [Inject] private ILogger<Admin> Logger { get; set; } = null!;
    [Inject] private IOptions<WebAppConfiguration> WebAppConfiguration { get; set; } = null!;

    private bool _isAuthenticated;
    private string _passphrase = string.Empty;
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private string _selectedLanguage = string.Empty;
    private string? _editingId;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;

    private List<RsvpGuest> _invites = [];
    private List<DynamicInformation> _informationList = [];
    private DynamicInformation _information = new();

    private async Task LoginAsync()
    {
        // Replace "secret" with your desired passphrase
        if (string.Equals(_passphrase, WebAppConfiguration.Value.AdminPassword, StringComparison.Ordinal))
        {
            _isAuthenticated = true;
            await LoadInformationAsync().ConfigureAwait(false);
            await LoadRsvpsAsync().ConfigureAwait(false);
            ChangeLanguage("de");
        }
        else
        {
            _errorMessage = "Invalid passphrase.";
        }
    }

    private void HandleKeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
            LoginAsync().Wait();
    }

    private async Task LoadInformationAsync()
    {
        var response = await InformationClient.GetInformationInAllLanguagesAsync().ConfigureAwait(false);

        if (response.IsSuccess)
            _informationList = response.ValueSuccess!.ToList();
        else
        {
            _errorMessage = "Failed to load information in all languages.";
            Logger.LogError("Failed to load information in all languages with code {StatusCode}.",
                response.ValueFail.StatusCode);
        }
    }

    private async Task LoadRsvpsAsync()
    {
        var response = await RsvpClient.GetAllRsvpsAsync().ConfigureAwait(false);

        if (response.IsSuccess)
            _invites = response.ValueSuccess!.ToList();
        else
        {
            _errorMessage = "Failed to load all rsvps.";
            Logger.LogError("Failed to load all rsvps with code {StatusCode}.", response.ValueFail.StatusCode);
        }
    }

    private async Task AddRow()
    {
        var newInvite = new RsvpGuest { Name = "New Guest", Salutation = "Dear guest"};
        var response = await RsvpClient.AddRsvpAsync(newInvite).ConfigureAwait(false);
        if (!response.IsSuccess || response.ValueSuccess is null)
        {
            Logger.LogError("Failed to add guest with code {StatusCode}.", response.ValueFail.StatusCode);
            _errorMessage = "Failed to add guest. Changes are displayed in the table but not saved.";
        }
        else
        {
            _invites.Insert(0, response.ValueSuccess);
            _editingId = response.ValueSuccess.Id;
        }
    }

    private async Task SaveInvite()
    {
        var invite = _invites.FirstOrDefault(x => x.Id == _editingId);
        if (invite is null)
        {
            Logger.LogError("Failed to update guest. Rsvp was null.");
            _errorMessage = "Failed to update guest.";
        }
        else
        {
            var response = await RsvpClient.UpdateRsvpAsync(invite, true).ConfigureAwait(false);
            if (!response.IsSuccess)
            {
                if (response.ValueFail!.StatusCode == HttpStatusCode.BadRequest)
                    _errorMessage = "Failed to update guest. Invalid input data.";
                else
                    _errorMessage = "Failed to update guest.";                 
                
                Logger.LogError("Failed to update guest with code {StatusCode}.", response.ValueFail.StatusCode);
            }
            _editingId = null;
        }
    }

    private async Task DeleteAsync(string id)
    {
        if (Guid.TryParse(id, out var guid))
        {
            var response = await RsvpClient.DeleteRsvpAsync(guid).ConfigureAwait(false);
            if (!response.IsSuccess)
            {
                Logger.LogError("Failed to delete guest with code {StatusCode}.", response.ValueFail.StatusCode);
                _errorMessage = "Failed to delete guest.";
            }
            else
            {
                _invites.RemoveAll(x => x.Id == id);
            }
        }
        else
        {
            Logger.LogError("Failed to delete guest.");
            _errorMessage = "Failed to delete guest.";
        }
    }

    private void UpdateDictionaryKey(Dictionary<string, string> dict, string oldKey, string? newKey)
    {
        if (string.IsNullOrWhiteSpace(newKey) || oldKey == newKey || dict.ContainsKey(newKey))
            return;

        var value = dict[oldKey];
        dict.Remove(oldKey);
        dict[newKey] = value;
    }

    private void AddDictionaryEntry(Dictionary<string, string> dict, string defaultKey, string defaultValue)
    {
        var key = defaultKey;
        int counter = 1;
        
        // Ensure we don't overwrite if "New Event" already exists
        while (dict.ContainsKey(key))
        {
            key = $"{defaultKey} {counter++}";
        }

        dict[key] = defaultValue;
        StateHasChanged();
    }

    private async Task SaveInformationAsync()
    {
        var exists = _informationList.Any(x => x.Language == _information.Language);

        if (exists)
        {
            var response = await InformationClient.UpdateInformationAsync(_information).ConfigureAwait(false);
            if (!response.IsSuccess)
            {
                if (response.ValueFail!.StatusCode == HttpStatusCode.BadRequest)
                    _errorMessage = "Failed to update information. Invalid input data.";
                else
                    _errorMessage = "Failed to update information.";
                
                Logger.LogError("Failed to update information in {Language} with code {StatusCode}.",
                    _information.Language, response.ValueFail.StatusCode);
            }
            else
                _successMessage = "Information updated successfully.";
        }
        else
        {
            var response = await InformationClient.AddInformationAsync(_information).ConfigureAwait(false);
            if (!response.IsSuccess || response.ValueSuccess is null)
            {
                Logger.LogError("Failed to add information in {Language} with code {StatusCode}.",
                    _information.Language, response.ValueFail.StatusCode);
                
                if (response.ValueFail!.StatusCode == HttpStatusCode.BadRequest)
                    _errorMessage = "Failed to add information. Invalid input data.";
                else
                    _errorMessage = "Failed to add information.";
            }
            else
            {
                _informationList.Add(response.ValueSuccess);
                _successMessage = "Information added successfully.";
            }
        }
    }

    /// <summary>
    /// Sets the currently selected language and loads the corresponding information.
    /// If the language does not exist, a new in memory entry is created.
    /// </summary>
    /// <param name="language"></param>
    private void ChangeLanguage(string language)
    {
        _information = _informationList.FirstOrDefault(x => x.Language == language,
            new() { Language = language });
    }
}