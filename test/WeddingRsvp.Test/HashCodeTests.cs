using WeddingRsvp.Abstractions.Models.Rsvps;
using WeddingRsvp.Client;

namespace WeddingRsvp.Test;

public class HashCodeTests
{
    private static RsvpGuest CreateBaseRsvpGuest()
    {
        return new RsvpGuest
        {
            Id = "test-id-123",
            Name = "John Doe",
            Response = ResponseType.Yes,
            BringPartner = ResponseType.No,
            NumberOfGuestsOvernight = 2,
            NumberOfMeatMenus = 1,
            NumberOfBrunchGuests = 1,
            NumberOfVegetarianMenus = 0,
            AdditionalInformation = "No dietary restrictions",
            IsPlural = false,
            Salutation = "Dear John"
        };
    }

    [Fact]
    public void GetHashCode_SameProperties_ReturnsSameHashCode()
    {
        // Arrange
        var rsvp1 = CreateBaseRsvpGuest();
        var rsvp2 = CreateBaseRsvpGuest();

        // Act
        var hash1 = rsvp1.GetHashCode();
        var hash2 = rsvp2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Theory]
    [InlineData("Different Name")]
    [InlineData("")]
    [InlineData("Jane Smith")]
    public void GetHashCode_NameChange_ReturnsDifferentHashCode(string differentName)
    {
        // Arrange
        var originalRsvp = CreateBaseRsvpGuest();
        var modifiedRsvp = CreateBaseRsvpGuest();
        modifiedRsvp.Name = differentName;

        // Act
        var originalHash = originalRsvp.GetHashCode();
        var modifiedHash = modifiedRsvp.GetHashCode();

        // Assert
        Assert.NotEqual(originalHash, modifiedHash);
    }

    [Theory]
    [InlineData(ResponseType.No)]
    [InlineData(ResponseType.None)]
    public void GetHashCode_ResponseChange_ReturnsDifferentHashCode(ResponseType differentResponse)
    {
        // Arrange
        var originalRsvp = CreateBaseRsvpGuest();
        var modifiedRsvp = CreateBaseRsvpGuest();
        modifiedRsvp.Response = differentResponse;

        // Act
        var originalHash = originalRsvp.GetHashCode();
        var modifiedHash = modifiedRsvp.GetHashCode();

        // Assert
        Assert.NotEqual(originalHash, modifiedHash);
    }

    [Theory]
    [InlineData(ResponseType.Yes)]
    [InlineData(ResponseType.None)]
    public void GetHashCode_BringPartnerChange_ReturnsDifferentHashCode(ResponseType differentBringPartner)
    {
        // Arrange
        var originalRsvp = CreateBaseRsvpGuest();
        var modifiedRsvp = CreateBaseRsvpGuest();
        modifiedRsvp.BringPartner = differentBringPartner;

        // Act
        var originalHash = originalRsvp.GetHashCode();
        var modifiedHash = modifiedRsvp.GetHashCode();

        // Assert
        Assert.NotEqual(originalHash, modifiedHash);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(10)]
    public void GetHashCode_NumberOfGuestsOvernightChange_ReturnsDifferentHashCode(int differentNumber)
    {
        // Arrange
        var originalRsvp = CreateBaseRsvpGuest();
        var modifiedRsvp = CreateBaseRsvpGuest();
        modifiedRsvp.NumberOfGuestsOvernight = differentNumber;

        // Act
        var originalHash = originalRsvp.GetHashCode();
        var modifiedHash = modifiedRsvp.GetHashCode();

        // Assert
        Assert.NotEqual(originalHash, modifiedHash);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(5)]
    public void GetHashCode_NumberOfMeatMenusChange_ReturnsDifferentHashCode(int differentNumber)
    {
        // Arrange
        var originalRsvp = CreateBaseRsvpGuest();
        var modifiedRsvp = CreateBaseRsvpGuest();
        modifiedRsvp.NumberOfMeatMenus = differentNumber;

        // Act
        var originalHash = originalRsvp.GetHashCode();
        var modifiedHash = modifiedRsvp.GetHashCode();

        // Assert
        Assert.NotEqual(originalHash, modifiedHash);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(3)]
    public void GetHashCode_NumberOfFishMenusChange_ReturnsDifferentHashCode(int differentNumber)
    {
        // Arrange
        var originalRsvp = CreateBaseRsvpGuest();
        var modifiedRsvp = CreateBaseRsvpGuest();
        modifiedRsvp.NumberOfBrunchGuests = differentNumber;

        // Act
        var originalHash = originalRsvp.GetHashCode();
        var modifiedHash = modifiedRsvp.GetHashCode();

        // Assert
        Assert.NotEqual(originalHash, modifiedHash);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public void GetHashCode_NumberOfVegetarianMenusChange_ReturnsDifferentHashCode(int differentNumber)
    {
        // Arrange
        var originalRsvp = CreateBaseRsvpGuest();
        var modifiedRsvp = CreateBaseRsvpGuest();
        modifiedRsvp.NumberOfVegetarianMenus = differentNumber;

        // Act
        var originalHash = originalRsvp.GetHashCode();
        var modifiedHash = modifiedRsvp.GetHashCode();

        // Assert
        Assert.NotEqual(originalHash, modifiedHash);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Has allergies to nuts")]
    [InlineData("Vegetarian option preferred")]
    [InlineData("Will arrive late")]
    public void GetHashCode_AdditionalInformationChange_ReturnsDifferentHashCode(string differentInfo)
    {
        // Arrange
        var originalRsvp = CreateBaseRsvpGuest();
        var modifiedRsvp = CreateBaseRsvpGuest();
        modifiedRsvp.AdditionalInformation = differentInfo;

        // Act
        var originalHash = originalRsvp.GetHashCode();
        var modifiedHash = modifiedRsvp.GetHashCode();

        // Assert
        Assert.NotEqual(originalHash, modifiedHash);
    }

    [Fact]
    public void GetHashCode_MultiplePropertiesChange_ReturnsDifferentHashCode()
    {
        // Arrange
        var originalRsvp = CreateBaseRsvpGuest();
        var modifiedRsvp = CreateBaseRsvpGuest();
        
        // Change multiple properties
        modifiedRsvp.Name = "Jane Smith";
        modifiedRsvp.Response = ResponseType.No;
        modifiedRsvp.NumberOfGuestsOvernight = 0;
        modifiedRsvp.AdditionalInformation = "Cannot attend";

        // Act
        var originalHash = originalRsvp.GetHashCode();
        var modifiedHash = modifiedRsvp.GetHashCode();

        // Assert
        Assert.NotEqual(originalHash, modifiedHash);
    }

    [Fact]
    public void GetHashCode_AllResponseFieldsChange_ReturnsDifferentHashCode()
    {
        // Arrange
        var originalRsvp = CreateBaseRsvpGuest();
        var modifiedRsvp = CreateBaseRsvpGuest();
        
        // Change both response fields
        modifiedRsvp.Response = ResponseType.No;
        modifiedRsvp.BringPartner = ResponseType.Yes;

        // Act
        var originalHash = originalRsvp.GetHashCode();
        var modifiedHash = modifiedRsvp.GetHashCode();

        // Assert
        Assert.NotEqual(originalHash, modifiedHash);
    }

    [Fact]
    public void GetHashCode_PropertiesNotInHashCode_DoNotAffectHash()
    {
        // Arrange
        var rsvp1 = CreateBaseRsvpGuest();
        var rsvp2 = CreateBaseRsvpGuest();
        
        // Change properties that are NOT included in GetHashCode implementation
        rsvp2.Id = "different-id-456";
        rsvp2.IsPlural = true;
        rsvp2.Salutation = "Dear Ms. Jane";

        // Act
        var hash1 = rsvp1.GetHashCode();
        var hash2 = rsvp2.GetHashCode();

        // Assert - Should be equal since these properties don't affect the hash
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void GetHashCode_EdgeCaseValues_HandledCorrectly()
    {
        // Arrange
        var rsvp1 = new RsvpGuest
        {
            Name = "",
            Response = ResponseType.None,
            BringPartner = ResponseType.None,
            NumberOfGuestsOvernight = 0,
            NumberOfMeatMenus = 0,
            NumberOfBrunchGuests = 0,
            NumberOfVegetarianMenus = 0,
            AdditionalInformation = ""
        };

        var rsvp2 = new RsvpGuest
        {
            Name = "Max Value Test",
            Response = ResponseType.Yes,
            BringPartner = ResponseType.Yes,
            NumberOfGuestsOvernight = int.MaxValue,
            NumberOfMeatMenus = int.MaxValue,
            NumberOfBrunchGuests = int.MaxValue,
            NumberOfVegetarianMenus = int.MaxValue,
            AdditionalInformation = new string('A', 1000) // Long string
        };

        // Act
        var hash1 = rsvp1.GetHashCode();
        var hash2 = rsvp2.GetHashCode();

        // Assert
        Assert.NotEqual(hash1, hash2);
    }
}