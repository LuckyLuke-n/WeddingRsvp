using Moq;

namespace WeddingRsvp.Integration.Fixtures;

public class TimeProviderFixture
{
    public Mock<TimeProvider> ProviderMock { get; } = new Mock<TimeProvider>();
}