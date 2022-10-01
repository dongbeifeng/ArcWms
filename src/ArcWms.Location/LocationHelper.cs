using NHibernate;
using NHibernate.Linq;

namespace ArcWms;

public sealed class LocationHelper
{
    readonly ISession _session;
    public LocationHelper(ISession session)
    {
        _session = session;
    }

    /// <summary>
    /// 重建巷道的统计信息。原有统计信息将被清除。此操作占用资源较多。
    /// </summary>
    public async Task RebuildStreetletStatAsync(Streetlet streetlet)
    {
        ArgumentNullException.ThrowIfNull(streetlet);

        streetlet.Usage.Clear();

        var keys = await _session.Query<Location>()
            .Where(x => x.Streetlet == streetlet && x.Exists)
            .GroupBy(x => new
            {
                x.StorageGroup,
                x.Specification,
                x.WeightLimit,
                x.HeightLimit
            })
            .Select(x => new StreetletUsageKey
            {
                StorageGroup = x.Key.StorageGroup!,
                Specification = x.Key.Specification,
                WeightLimit = x.Key.WeightLimit,
                HeightLimit = x.Key.HeightLimit
            })
            .ToListAsync()
            .ConfigureAwait(false);

        foreach (var key in keys)
        {
            await UpdateUsageAsync(streetlet, key).ConfigureAwait(false);
        }

        async Task UpdateUsageAsync(Streetlet streetlet, StreetletUsageKey key)
        {
            var q = _session.Query<Streetlet>()
                .Where(x => x == streetlet)
                .SelectMany(x => x.Locations)
                .Where(x => x.Exists
                    && x.StorageGroup == key.StorageGroup
                    && x.Specification == key.Specification
                    && x.WeightLimit == key.WeightLimit
                    && x.HeightLimit == key.HeightLimit
                );

            var total = await q.CountAsync().ConfigureAwait(false);
            var loaded = await q.Where(x => x.UnitloadCount > 0).CountAsync().ConfigureAwait(false);
            var available = await q.Where(x => x.UnitloadCount == 0 && x.IsInboundDisabled == false).CountAsync().ConfigureAwait(false);
            var inboundDisabled = await q.Where(x => x.IsInboundDisabled == true).CountAsync().ConfigureAwait(false);

            streetlet.Usage[key] = new StreetletUsageData
            {
                Total =  total,
                Available =  available,
                Loaded =  loaded,
                InboundDisabled =  inboundDisabled,
            };
        }
    }
}
