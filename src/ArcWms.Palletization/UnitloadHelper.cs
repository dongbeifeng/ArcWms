using Autofac;
using Microsoft.Extensions.Logging;
using NHibernate;
using System.Reflection;

namespace ArcWms;


public sealed class UnitloadHelper
{
    readonly ISession _session;
    readonly Func<UnitloadSnapshot> _createUnitloadSnapshot;
    readonly Func<UnitloadItemSnapshot> _createUnitloadItemSnapshot;
    readonly ILogger<UnitloadHelper> _logger;


    public UnitloadHelper(ISession session, Func<UnitloadSnapshot> createUnitloadSnapshot, Func<UnitloadItemSnapshot> createUnitloadItemSnapshot, ILogger<UnitloadHelper> logger)
    {
        _session = session;
        _createUnitloadSnapshot = createUnitloadSnapshot;
        _createUnitloadItemSnapshot = createUnitloadItemSnapshot;
        _logger = logger;

    }


    public async Task EnterAsync(Unitload unitload, Location target)
    {
        ArgumentNullException.ThrowIfNull(unitload);
        ArgumentNullException.ThrowIfNull(target);

        if (unitload.CurrentLocation == target)
        {
            throw new InvalidOperationException("货载已在目标位置上。");
        }

        if (unitload.CurrentLocation != null)
        {
            throw new InvalidOperationException("应先将货载离开当前位置。");
        }

        this.SetCurrentLocation(unitload, target);

        if (target.LocationType != LocationTypes.N)
        {
            Keeping keeping = new Keeping
            {
                Unitload = unitload,
                Location = target
            };
            await _session.SaveAsync(keeping).ConfigureAwait(false);
            await _session.FlushAsync().ConfigureAwait(false);
            target.IncreaseUnitloadCount();
        }
    }



    public async Task LeaveCurrentLocationAsync(Unitload unitload)
    {
        ArgumentNullException.ThrowIfNull(unitload);

        if (unitload.CurrentLocation == null)
        {
            throw new InvalidOperationException("货载不在任何位置上。");
        }
        var loc = unitload.CurrentLocation;
        this.SetCurrentLocation(unitload, null);

        if (loc.LocationType != LocationTypes.N)
        {
            Keeping keeping = await _session.GetAsync<Keeping>(unitload.UnitloadId).ConfigureAwait(false);
            await _session.DeleteAsync(keeping).ConfigureAwait(false);
            await _session.FlushAsync().ConfigureAwait(false);
            loc.DecreaseUnitloadCount();
        }
    }


    private void SetCurrentLocation(Unitload unitload, Location? location)
    {
        if (unitload.CurrentLocation != location)
        {
            unitload.CurrentLocation = location;
            unitload.CurrentLocationTime = DateTime.Now;
        }
    }

    public UnitloadSnapshot GetSnapshot(Unitload unitload)
    {
        ArgumentNullException.ThrowIfNull(unitload);

        UnitloadSnapshot snapshot = _createUnitloadSnapshot.Invoke();
        CopyProperties(unitload, snapshot, new[]
        {
            nameof(UnitloadSnapshot.UnitloadSnapshotId),
            nameof(UnitloadSnapshot.SnapshotTime),
            nameof(UnitloadSnapshot.Items)
        });

        snapshot.SnapshotTime = DateTime.Now;

        foreach (var item in unitload.Items)
        {
            UnitloadItemSnapshot itemSnapshot = _createUnitloadItemSnapshot.Invoke();
            CopyProperties(item, itemSnapshot, new[]
            {
                    nameof(UnitloadItemSnapshot.UnitloadItemSnapshotId),
                    nameof(UnitloadItemSnapshot.Unitload),
            });
            snapshot.AddItem(itemSnapshot);
        }
        _logger.LogInformation("已获取 {unitload} 的快照", unitload);

        return snapshot;
    }

    /// <summary>
    /// 遍历目标对象的属性，从源对象复制同名属性的值，属性名不区分大小写。
    /// </summary>
    /// <param name="src"></param>
    /// <param name="dest"></param>
    /// <param name="excluded">目标类型中要排除的属性名称，属性名不区分大小写。</param>
    internal static void CopyProperties(object src, object dest, string[]? excluded = null)
    {
        ArgumentNullException.ThrowIfNull(src);
        ArgumentNullException.ThrowIfNull(dest);

        excluded ??= new string[0];

        var destProps = dest.GetType()
            .GetProperties()
            .Where(x => excluded.Contains(x.Name, StringComparer.OrdinalIgnoreCase) == false)
            .ToArray();
        foreach (var destProp in destProps)
        {
            var srcProp = src.GetType().GetProperty(destProp.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (srcProp is not null && destProp.PropertyType.IsAssignableFrom(srcProp.PropertyType))
            {
                object? val = srcProp.GetValue(src);
                destProp.SetValue(dest, val);
            }
        }
    }
}

