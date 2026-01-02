using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using WeddingRsvp.Abstractions.Models.Rsvps;
using WeddingRsvp.Client;

namespace WeddingRsvp.WebApp.Components.Pages;

public partial class Admin : ComponentBase
{
    private bool _isAuthenticated;
    private string _passphrase = "";
    private string? _editingId;

    private List<RsvpGuest> _invites = new();
    private DynamicInformation _information = new();

    private void Login()
    {
        // Replace "secret" with your desired passphrase
        if (_passphrase == "secret")
        {
            _isAuthenticated = true;
            // In a real app, you would fetch data from a service here
            LoadMockData();
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
            new() { Id = "1", Name = "Hubert", Salutation = "Lieber Hubert", NumberOfGuestsOvernight = 2, Response = ResponseType.Yes },
            new() { Id = "2", Name = "Anna", Salutation = "Dear Anna", NumberOfGuestsOvernight = 1, Response = ResponseType.None }
        };
        
        _information = new DynamicInformation
        {
            Language = "DE",
            InvitationText = "Wir laden euch herzlich ein...",
            Itinerary = new Dictionary<string, string> { { "14:00", "Trauung" }, { "18:00", "Dinner" } },
            Faqs = new Dictionary<string, string> { { "Dresscode", "Chic" } }
        };
    }

    private void AddRow()
    {
        var newId = Guid.NewGuid().ToString();
        var newInvite = new RsvpGuest { Id = newId, Name = "New Guest" };
        _invites.Insert(0, newInvite);
        _editingId = newId;
    }

    private void Save(RsvpGuest invite)
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
}