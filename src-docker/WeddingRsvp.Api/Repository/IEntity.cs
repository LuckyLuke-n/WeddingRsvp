namespace WeddingRsvp.Api.Repository;

public interface IEntity
{
    Guid Id { get; set; }
    void SetAsNew();
}