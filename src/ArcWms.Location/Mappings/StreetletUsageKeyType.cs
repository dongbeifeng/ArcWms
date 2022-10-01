using NHibernate;
using NHibernate.Engine;
using NHibernate.Type;
using NHibernate.UserTypes;
using System.Data.Common;

namespace ArcWms.Mappings;

[Serializable]
public class StreetletUsageKeyType : ICompositeUserType
{
    public Type ReturnedClass
    {
        get { return typeof(StreetletUsageKey); }
    }

    public new bool Equals(object x, object y)
    {
        if (x == y) return true;
        if (x is null || y is null) return false;
        StreetletUsageKey lhs = (StreetletUsageKey)x;
        StreetletUsageKey rhs = (StreetletUsageKey)y;

        return lhs.Equals(rhs);
    }

    public int GetHashCode(object x)
    {
        unchecked
        {
            StreetletUsageKey a = (StreetletUsageKey)x;
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
        string? storageGroup = (string?)NHibernateUtil.String.NullSafeGet(rs, names[0], session, owner);
        string? specification = (string?)NHibernateUtil.String.NullSafeGet(rs, names[1], session, owner);
        float? weightLimit = (float?)NHibernateUtil.Single.NullSafeGet(rs, names[2], session, owner);
        float? heightLimit = (float?)NHibernateUtil.Single.NullSafeGet(rs, names[3], session, owner);

        return new StreetletUsageKey
        {
            StorageGroup = storageGroup,
            Specification = specification,
            WeightLimit = weightLimit ?? 0f,
            HeightLimit = heightLimit ?? 0f,
        };
    }

    public void NullSafeSet(DbCommand st, object value, int index, bool[] settable, ISessionImplementor session)
    {
        StreetletUsageKey key = value is null ? new StreetletUsageKey() : (StreetletUsageKey)value;

        if (settable[0]) NHibernateUtil.String.NullSafeSet(st, key.StorageGroup, index++, session);
        if (settable[1]) NHibernateUtil.String.NullSafeSet(st, key.Specification, index++, session);
        if (settable[2]) NHibernateUtil.Single.NullSafeSet(st, key.WeightLimit, index++, session);
        if (settable[3]) NHibernateUtil.Single.NullSafeSet(st, key.HeightLimit, index++, session);
    }

    public string[] PropertyNames
    {
        get 
        { 
            return new string[] 
            { 
                nameof(StreetletUsageKey.StorageGroup),
                nameof(StreetletUsageKey.Specification),
                nameof(StreetletUsageKey.WeightLimit), 
                nameof(StreetletUsageKey.HeightLimit) 
            }; 
        }
    }

    public IType[] PropertyTypes
    {
        get 
        { 
            return new IType[]
            { 
                NHibernateUtil.String, 
                NHibernateUtil.String, 
                NHibernateUtil.Single, 
                NHibernateUtil.Single 
            };
        }
    }

    public object GetPropertyValue(object component, int property)
    {
        var key = (StreetletUsageKey)component;
        return property switch
        {
            0 => key.StorageGroup,
            1 => key.Specification,
            2 => key.WeightLimit,
            3 => key.HeightLimit,
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
