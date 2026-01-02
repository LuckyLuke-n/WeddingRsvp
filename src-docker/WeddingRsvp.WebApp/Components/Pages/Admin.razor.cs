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
}