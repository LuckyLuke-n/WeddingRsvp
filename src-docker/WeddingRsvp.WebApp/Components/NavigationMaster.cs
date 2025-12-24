namespace WeddingRsvp.WebApp.Components;

public static class NavigationMaster
{
    public static string Home => "/";
    public static string NotFound => "/not-found";
    public static string Invite( string id ) => $"/invite/{id}";
    public static string Rsvp( string id ) => $"/rsvp/{id}";
}