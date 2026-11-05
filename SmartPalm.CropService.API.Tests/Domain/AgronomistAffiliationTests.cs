using SmartPalm.CropService.Domain.Plantations;

namespace SmartPalm.CropService.UnitTests;

public class AgronomistAffiliationTests
{
    [Fact]
    public void Detach_is_not_repeatable()
    {
        var affiliation = new AgronomistAffiliation(5, 9);
        affiliation.Detach();

        Assert.False(affiliation.IsActive);
        Assert.Throws<InvalidOperationException>(affiliation.Detach);
    }
}
