using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using WeddingRsvp.Client;
using WeddingRsvp.WebApp.Configurations;
using WeddingRsvp.WebApp.Diagnostics.Meters;

namespace WeddingRsvp.WebApp.Components.Pages;

public partial class Admin : ComponentBase
{
    [Inject] private IRsvpClient RsvpClient { get; set; } = null!;
    [Inject] private IInformationClient InformationClient { get; set; } = null!;
    [Inject] private ILogger<Admin> Logger { get; set; } = null!;
    [Inject] private IOptions<WebAppConfiguration> WebAppConfiguration { get; set; } = null!;
    [Inject] private ISettingsClient SettingsClient { get; set; } = null!;

    private bool _isAuthenticated;
    private string _passphrase = string.Empty;
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private string _selectedLanguage = string.Empty;
    private string? _editingId;
    private string? _expandedId;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    
    private List<RsvpGuest> _invites = [];
    private List<DynamicInformation> _informationList = [];
    private DynamicInformation _information = new();

    private ApplicationSettings _settings = new();
    private string _emailRecipientsText = string.Empty;
    private DateTime _respondUntilLocalTime ;

    private SortColumn _sortColumn = SortColumn.Name;
    private bool _sortAscending = true;

    private enum SortColumn
    {
        Name,
        Salutation,
        IsPlural,
        Response,
        BringPartner,
        Overnight,
        Brunch,
        MeatMenu,
        VegetarianMenu,
        AdditionalInfo
    }

    private void ApplySort(SortColumn column)
    {
        if (_sortColumn == column)
            _sortAscending = !_sortAscending;
        else
        {
            _sortColumn = column;
            _sortAscending = true;
        }

        _invites = SortInvites(_invites).ToList();
    }

    private IEnumerable<RsvpGuest> SortInvites(IEnumerable<RsvpGuest> invites)
    {
        return _sortColumn switch
        {
            SortColumn.Name => _sortAscending
                ? invites.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                : invites.OrderByDescending(x => x.Name, StringComparer.OrdinalIgnoreCase),

            SortColumn.Salutation => _sortAscending
                ? invites.OrderBy(x => x.Salutation, StringComparer.OrdinalIgnoreCase)
                : invites.OrderByDescending(x => x.Salutation, StringComparer.OrdinalIgnoreCase),

            SortColumn.IsPlural => _sortAscending
                ? invites.OrderBy(x => x.IsPlural)
                : invites.OrderByDescending(x => x.IsPlural),

            SortColumn.Response => _sortAscending
                ? invites.OrderBy(x => x.Response)
                : invites.OrderByDescending(x => x.Response),

            SortColumn.BringPartner => _sortAscending
                ? invites.OrderBy(x => x.BringPartner)
                : invites.OrderByDescending(x => x.BringPartner),

            SortColumn.Overnight => _sortAscending
                ? invites.OrderBy(x => x.NumberOfGuestsOvernight)
                : invites.OrderByDescending(x => x.NumberOfGuestsOvernight),

            SortColumn.Brunch => _sortAscending
                ? invites.OrderBy(x => x.NumberOfBrunchGuests)
                : invites.OrderByDescending(x => x.NumberOfBrunchGuests),

            SortColumn.MeatMenu => _sortAscending
                ? invites.OrderBy(x => x.NumberOfMeatMenus)
                : invites.OrderByDescending(x => x.NumberOfMeatMenus),

            SortColumn.VegetarianMenu => _sortAscending
                ? invites.OrderBy(x => x.NumberOfVegetarianMenus)
                : invites.OrderByDescending(x => x.NumberOfVegetarianMenus),

            SortColumn.AdditionalInfo => _sortAscending
                ? invites
                    .OrderBy(x => string.IsNullOrWhiteSpace(x.AdditionalInformation))
                    .ThenBy(x => x.AdditionalInformation, StringComparer.OrdinalIgnoreCase)
                : invites
                    .OrderByDescending(x => string.IsNullOrWhiteSpace(x.AdditionalInformation))
                    .ThenByDescending(x => x.AdditionalInformation, StringComparer.OrdinalIgnoreCase),

            _ => invites
        };
    }

    private string SortIndicator(SortColumn column)
    {
        if (_sortColumn != column)
            return string.Empty;

        return _sortAscending ? "▲" : "▼";
    }

    private async Task LoginAsync()
    {
        if (string.Equals(_passphrase, WebAppConfiguration.Value.AdminPassword, StringComparison.Ordinal))
        {
            WebAppMeter.SuccessfulAdminLogin();
            _isAuthenticated = true;
            await LoadInformationAsync().ConfigureAwait(false);
            await LoadRsvpsAsync().ConfigureAwait(false);
            await LoadSettingsAsync().ConfigureAwait(false);
            ChangeLanguage("de");
        }
        else
        {
            _errorMessage = "Invalid passphrase.";
            WebAppMeter.FailedAdminLogin();
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
        {
            _invites = response.ValueSuccess!.ToList();
        }
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
    
    private void ToggleInfo(string id)
    {
        _expandedId = string.Equals(_expandedId, id) ? null : id;
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

    private async Task LoadSettingsAsync()
    {
        var response = await SettingsClient.GetSettingsAsync().ConfigureAwait(false);

        if (response.IsSuccess)
        {
            _settings = response.ValueSuccess!;
            _emailRecipientsText = string.Join(Environment.NewLine, _settings.EmailRecipients);
            _respondUntilLocalTime = _settings.RespondUntil;
        }
        else
        {
            _errorMessage = "Failed to load settings.";
            Logger.LogError("Failed to load settings with code {StatusCode}.", response.ValueFail.StatusCode);
        }
    }

    private async Task SaveSettingsAsync()
    {
        _settings.EmailRecipients = _emailRecipientsText
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
        
        _settings.RespondUntil = _respondUntilLocalTime;

        var response = await SettingsClient.UpdateSettingsAsync(_settings).ConfigureAwait(false);
        if (!response.IsSuccess)
        {
            if (response.ValueFail!.StatusCode == HttpStatusCode.BadRequest)
                _errorMessage = "Failed to update settings. Invalid input data.";
            else
                _errorMessage = "Failed to update settings.";

            Logger.LogError("Failed to update settings with code {StatusCode}.", response.ValueFail.StatusCode);
        }
        else
        {
            _successMessage = "Settings updated successfully.";
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