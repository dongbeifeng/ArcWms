using NHibernate;
using NHibernate.Engine;
using NHibernate.Type;
using NHibernate.UserTypes;
using System.Data.Common;

namespace ArcWms.Mappings;

[Serializable]
public class StreetletUsageDataType : ICompositeUserType
{
    public Type ReturnedClass
    {
        get { return typeof(StreetletUsageData); }
    }

    public new bool Equals(object x, object y)
    {
        if (x == y) return true;
        if (x is null || y is null) return false;
        StreetletUsageData lhs = (StreetletUsageData)x;
        StreetletUsageData rhs = (StreetletUsageData)y;

        return lhs.Equals(rhs);
    }

    public int GetHashCode(object x)
    {
        unchecked
        {
            StreetletUsageData a = (StreetletUsageData)x;
            return a.GetHashCode();
        }
    }

    public object DeepCopy(object x)
    {
        return x;
    }

    public bool IsMutable
    {
        get { return false; }
    }

    public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
    {
        int? total = (int?)NHibernateUtil.Int32.NullSafeGet(rs, names[0], session, owner);
        int? available = (int?)NHibernateUtil.Int32.NullSafeGet(rs, names[1], session, owner);
        int? loaded = (int?)NHibernateUtil.Int32.NullSafeGet(rs, names[2], session, owner);
        int? inboundDisabled = (int?)NHibernateUtil.Int32.NullSafeGet(rs, names[3], session, owner);

        return new StreetletUsageData
        {
            Total = total ?? 0,
            Available = available ?? 0,
            Loaded = loaded ?? 0,
            InboundDisabled = inboundDisabled ?? 0,
        };
    }

    public void NullSafeSet(DbCommand st, object value, int index, bool[] settable, ISessionImplementor session)
    {
        StreetletUsageData data = value is null ? new StreetletUsageData() : (StreetletUsageData)value;

        if (settable[0]) NHibernateUtil.String.NullSafeSet(st, data.Total, index++, session);
        if (settable[1]) NHibernateUtil.String.NullSafeSet(st, data.Available, index++, session);
        if (settable[2]) NHibernateUtil.Single.NullSafeSet(st, data.Loaded, index++, session);
        if (settable[3]) NHibernateUtil.Single.NullSafeSet(st, data.InboundDisabled, index++, session);

    }

    public string[] PropertyNames
    {
        get
        {
            return new string[] 
            { 
                nameof(StreetletUsageData.Total), 
                nameof(StreetletUsageData.Available), 
                nameof(StreetletUsageData.Loaded), 
                nameof(StreetletUsageData.InboundDisabled) 
            };
        }
    }

    public IType[] PropertyTypes
    {
        get
        {
            return new IType[]
            {
                NHibernateUtil.Int32,
                NHibernateUtil.Int32,
                NHibernateUtil.Int32,
                NHibernateUtil.Int32
            };
        }
    }

    public object GetPropertyValue(object component, int property)
    {
        var data = (StreetletUsageData)component;
        return property switch
        {
            0 => data.Total,
            1 => data.Available,
            2 => data.Loaded,
            3 => data.InboundDisabled,
            _ => throw new(),
        };
    }

    public void SetPropertyValue(object component, int property, object value)
    {
        throw new Exception("Immutable");
    }

    public object Assemble(object cached, ISessionImplementor session, object owner)
    {
        return DeepCopy(cached);
    }

    public object Disassemble(object value, ISessionImplementor session)
    {
        return DeepCopy(value);
    }

    public object Replace(object original, object target, ISessionImplementor session, object owner)
    {
        return DeepCopy(original);
    }
}
