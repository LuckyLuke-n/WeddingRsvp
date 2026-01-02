using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using WeddingRsvp.Abstractions.Models.Rsvps;
using WeddingRsvp.Client;

namespace WeddingRsvp.WebApp.Components.Pages;

public partial class Admin : ComponentBase
{
    private bool _isAuthenticated;
    private string _passphrase = string.Empty;
    private string _selectedLanguage = string.Empty;
    private string? _editingId;

    private List<RsvpGuest> _invites = [];
    private List<DynamicInformation> _informationList = [];
    private DynamicInformation _information = new();

    private void Login()
    {
        // Replace "secret" with your desired passphrase
        if (_passphrase == "secret")
        {
            _isAuthenticated = true;
            // In a real app, you would fetch data from a service here
            LoadMockData();
            ChangeLanguage("de");
        }
    }

    private void HandleKeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
            Login();
    }

    private void LoadMockData()
    {
        _invites = new List<RsvpGuest>
        {
            new()
            {
                Id = "1", Name = "Hubert", Salutation = "Lieber Hubert", NumberOfGuestsOvernight = 2,
                Response = ResponseType.Yes
            },
            new()
            {
                Id = "2", Name = "Anna", Salutation = "Dear Anna", NumberOfGuestsOvernight = 1,
                Response = ResponseType.None
            }
        };

        _informationList = new List<DynamicInformation>
        {
            new()
            {
                Language = "de",
                InvitationText = "Wir laden euch herzlich ein...",
                Itinerary = new Dictionary<string, string> { { "14:00", "Trauung" }, { "18:00", "Dinner" } },
                Faqs = new Dictionary<string, string> { { "Dresscode", "Chic" } },
            },
            new()
            {
                Language = "en",
                InvitationText = "You are invited...",
                Itinerary = new Dictionary<string, string> { { "14:00", "Ceremeony" }, { "18:00", "Dinner" } },
                Faqs = new Dictionary<string, string> { { "Dresscode", "Chic" } },
            },
        };
    }

    private void AddRow()
    {
        var newId = Guid.NewGuid().ToString();
        var newInvite = new RsvpGuest { Id = newId, Name = "New Guest" };
        _invites.Insert(0, newInvite);
        _editingId = newId;
    }

    private void SaveInivite(RsvpGuest invite)
    {
        // Implement service call to persist changes
        _editingId = null;
    }

    private void Delete(string id)
    {
        _invites.RemoveAll(x => x.Id == id);
    }

    private void UpdateDictionaryKey(Dictionary<string, string> dict, string oldKey, string? newKey)
    {
        if (string.IsNullOrWhiteSpace(newKey) || oldKey == newKey || dict.ContainsKey(newKey)) return;

        var value = dict[oldKey];
        dict.Remove(oldKey);
        dict[newKey] = value;
    }

    private void SaveInformation()
    {
        // Implement service call to save _information
    }

    /// <summary>
    /// Sets the currently selected language and loads the corresponding information.
    /// If the language does not exist a new in memory entry is created.
    /// </summary>
    /// <param name="language"></param>
    private void ChangeLanguage(string language)
    {
        _information = _informationList.FirstOrDefault(x => x.Language == language,
            new() { Id = Guid.NewGuid().ToString(), Language = language });
    }
}