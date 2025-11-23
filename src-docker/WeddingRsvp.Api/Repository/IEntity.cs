namespace WeddingRsvp.Api.Repository;

public interface IEntity
{
    string Id { get; set; }
    void SetAsNew();
}