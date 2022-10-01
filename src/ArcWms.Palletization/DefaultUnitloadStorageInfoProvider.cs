namespace ArcWms;

public class DefaultUnitloadStorageInfoProvider : IUnitloadStorageInfoProvider
{
    public virtual string GetOutFlag(Unitload unitload)
    {
        ArgumentNullException.ThrowIfNull(unitload);

        if (unitload.Items.Any() == false)
        {
            return "空托盘";
        }
        var list = unitload.Items
            .Select(x => new
            {
                x.Batch,
                x.Material?.MaterialCode
            })
            .Distinct()
            .ToList();
        if (list.Count > 1)
        {
            return "混合";
        }
        return list.Single().Batch?.ToUpper() + "@" + list.Single().MaterialCode;
    }


    public virtual string GetStorageGroup(Unitload unitload)
    {
        ArgumentNullException.ThrowIfNull(unitload);

        var list = unitload.Items.Select(x => x.Material?.DefaultStorageGroup).Distinct();
        if (list.Count() == 1)
        {
            return list.Single()!;
        }
        else
        {
            throw new ApplicationException("无法确定存储分组。");
        }
    }

    public virtual string GetPalletSpecification(Unitload unitload)
    {
        return "普通";
    }
}
