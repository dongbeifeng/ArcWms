using NHibernate;
using NHibernate.Engine;
using NHibernate.Type;
using NHibernate.UserTypes;
using System.Data.Common;

namespace ArcWms.Mappings;

[Serializable]
public class StorageInfoType : ICompositeUserType
{
    public Type ReturnedClass
    {
        get { return typeof(StorageInfo); }
    }

    public new bool Equals(object x, object y)
    {
        if (x == y) return true;
        if (x is null || y is null) return false;
        StorageInfo lhs = (StorageInfo)x;
        StorageInfo rhs = (StorageInfo)y;

        return lhs.Equals(rhs);
    }

    public int GetHashCode(object x)
    {
        unchecked
        {
            StorageInfo a = (StorageInfo)x;
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
        string? palletSpecification = (string?)NHibernateUtil.String.NullSafeGet(rs, names[1], session, owner);
        string? outFlag = (string?)NHibernateUtil.String.NullSafeGet(rs, names[2], session, owner);
        float? weight = (float?)NHibernateUtil.Single.NullSafeGet(rs, names[3], session, owner);
        float? height = (float?)NHibernateUtil.Single.NullSafeGet(rs, names[4], session, owner);

        return new StorageInfo
        {
            StorageGroup = storageGroup,
            PalletSpecification = palletSpecification,
            OutFlag = outFlag,
            Weight = weight ?? 0,
            Height = height ?? 0,
        };
    }

    public void NullSafeSet(DbCommand st, object value, int index, bool[] settable, ISessionImplementor session)
    {
        StorageInfo info = value is null ? new StorageInfo() : (StorageInfo)value;

        if (settable[0]) NHibernateUtil.String.NullSafeSet(st, info.StorageGroup, index++, session);
        if (settable[1]) NHibernateUtil.String.NullSafeSet(st, info.PalletSpecification, index++, session);
        if (settable[2]) NHibernateUtil.String.NullSafeSet(st, info.OutFlag, index++, session);
        if (settable[3]) NHibernateUtil.Single.NullSafeSet(st, info.Weight, index++, session);
        if (settable[4]) NHibernateUtil.Single.NullSafeSet(st, info.Height, index++, session);
    }

    public string[] PropertyNames
    {
        get 
        { 
            return new string[] 
            {
                nameof(StorageInfo.StorageGroup),
                nameof(StorageInfo.PalletSpecification), 
                nameof(StorageInfo.OutFlag), 
                nameof(StorageInfo.Weight), 
                nameof(StorageInfo.Height),
            }; 
        }
    }

    public IType[] PropertyTypes
    {
        get { return new IType[] { NHibernateUtil.String, NHibernateUtil.String, NHibernateUtil.String, NHibernateUtil.Single, NHibernateUtil.Single }; }
    }

    public object GetPropertyValue(object component, int property)
    {
        var key = (StorageInfo)component;
        return property switch
        {
            0 => key.StorageGroup,
            1 => key.PalletSpecification,
            2 => key.OutFlag,
            3 => key.Weight,
            4 => key.Height,
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
