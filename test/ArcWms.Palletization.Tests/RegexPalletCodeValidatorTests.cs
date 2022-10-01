namespace ArcWms.Tests;

public class RegexPalletCodeValidatorTests
{
    [Fact()]
    public void IsValidTest()
    {
        RegexPalletCodeValidator sut = new RegexPalletCodeValidator(@"\d", "MSG");
        Assert.True(sut.IsValid("1", out _));
        Assert.False(sut.IsValid("a", out string msg));
        Assert.Equal("MSG", msg);
    }
}
