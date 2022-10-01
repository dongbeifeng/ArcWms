using Xunit.Abstractions;

namespace ArcWms.Tests;

public class LocationTest
{
    public LocationTest(ITestOutputHelper output)
    {
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    public void IsLoadedTest(int unitloadCount, bool expectedIsLoaded)
    {
        var loc = new Location();

        loc.UnitloadCount = unitloadCount;
        Assert.Equal(expectedIsLoaded, loc.IsLoaded());
    }


    [Theory]
    [InlineData(0, false, true)]
    [InlineData(0, true, false)]
    [InlineData(1, false, false)]
    [InlineData(1, true, false)]
    public void IsAvailableTest(int unitloadCount, bool isInboundDisabled, bool expectedIsAvailable)
    {
        var loc = new Location
        {
            UnitloadCount = unitloadCount,
            IsInboundDisabled = isInboundDisabled,
        };

        Assert.Equal(expectedIsAvailable, loc.IsAvailable());
    }


    [Theory]
    [InlineData(LocationTypes.S)]
    [InlineData(LocationTypes.K)]
    public void IncreaseUnitloadCountTest(string locationType)
    {
        var loc = new Location
        {
            LocationType = locationType,
        };

        loc.IncreaseUnitloadCount();
        Assert.Equal(1, loc.UnitloadCount);

        loc.IncreaseUnitloadCount();
        Assert.Equal(2, loc.UnitloadCount);
    }


    [Fact]
    public void IncreaseUnitloadCount_不能在N位置上调用()
    {
        var loc = new Location
        {
            LocationType = LocationTypes.N,
        };

        Assert.Throws<InvalidOperationException>(loc.IncreaseUnitloadCount);
    }



    [Theory]
    [InlineData(LocationTypes.S)]
    [InlineData(LocationTypes.K)]

    public void DecreaseUnitloadCountTest(string locationType)
    {
        var loc = new Location
        {
            LocationType = locationType,
            UnitloadCount = 2,
        };


        loc.DecreaseUnitloadCount();
        Assert.Equal(1, loc.UnitloadCount);

        loc.DecreaseUnitloadCount();
        Assert.Equal(0, loc.UnitloadCount);

        Assert.Throws<InvalidOperationException>(loc.DecreaseUnitloadCount);

    }

    [Fact]
    public void DecreaseUnitloadCount_不能在N位置上调用()
    {
        var loc = new Location
        {
            LocationCode = "",
            LocationType = LocationTypes.N,
        };

        Assert.Throws<InvalidOperationException>(loc.DecreaseUnitloadCount);
    }





}
