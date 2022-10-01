using NHibernate;

namespace ArcWms.Tests;

public class LocationHelperTest
{

    [Fact]
    public async Task RebuildStreetletStatAsyncTest()
    {
        Streetlet streetlet = new Streetlet();
        for (int i = 0; i < 100; i++)
        {
            streetlet.Locations.Add(new Location
            {
                LocationCode = i.ToString("000"),
                Streetlet = streetlet,
                StorageGroup = "普通",
                Specification = "普通",
                WeightLimit = 1000,
                HeightLimit = 1.5f,
            });
        }

        ISession session = For<ISession>();
        session.Query<Streetlet>().Returns(new NHibernateUtils.TestingQueryable<Streetlet>(new Streetlet[] { streetlet }));
        session.Query<Location>().Returns(new NHibernateUtils.TestingQueryable<Location>(streetlet.Locations));

        LocationHelper sut = new LocationHelper(session);
        await sut.RebuildStreetletStatAsync(streetlet);

        Assert.Single(streetlet.Usage);
        var key1 = new StreetletUsageKey
        {
            StorageGroup = "普通",
            Specification = "普通",
            WeightLimit = 1000,
            HeightLimit = 1.5f,
        };

        Assert.Equal(key1, streetlet.Usage.Single().Key);
        Assert.Equal(100, streetlet.Usage[key1].Total);
        Assert.Equal(0, streetlet.Usage[key1].Loaded);
        Assert.Equal(100, streetlet.Usage[key1].Available);
        Assert.Equal(0, streetlet.Usage[key1].InboundDisabled);

        // -----

        streetlet.Locations.ElementAt(1).Exists = false;
        streetlet.Locations.ElementAt(2).IncreaseUnitloadCount();
        streetlet.Locations.ElementAt(3).IncreaseUnitloadCount();
        streetlet.Locations.ElementAt(3).IsInboundDisabled = true;
        streetlet.Locations.ElementAt(4).IsInboundDisabled = true;

        streetlet.Locations.ElementAt(5).StorageGroup = "成品";
        streetlet.Locations.ElementAt(5).Specification = "小托盘";
        streetlet.Locations.ElementAt(5).WeightLimit = 900;
        streetlet.Locations.ElementAt(5).HeightLimit = 2.0f;

        await sut.RebuildStreetletStatAsync(streetlet);

        Assert.Equal(2, streetlet.Usage.Count);
        var key2 = new StreetletUsageKey
        {
            StorageGroup = "成品",
            Specification = "小托盘",
            WeightLimit = 900,
            HeightLimit = 2.0f,
        };

        Assert.Equal(98, streetlet.Usage[key1].Total);
        Assert.Equal(2, streetlet.Usage[key1].Loaded);
        Assert.Equal(95, streetlet.Usage[key1].Available);
        Assert.Equal(2, streetlet.Usage[key1].InboundDisabled);

        Assert.Equal(1, streetlet.Usage[key2].Total);
        Assert.Equal(0, streetlet.Usage[key2].Loaded);
        Assert.Equal(1, streetlet.Usage[key2].Available);
        Assert.Equal(0, streetlet.Usage[key2].InboundDisabled);

    }
}
