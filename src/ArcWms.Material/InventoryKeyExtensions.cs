using System.Linq.Dynamic.Core;
using System.Reflection;

namespace ArcWms;

/// <summary>
/// 提供库存键扩展方法。
/// </summary>
public static class InventoryKeyExtensions
{
    /// <summary>
    /// 获取对象的库存键。
    /// </summary>
    /// <param name="obj">实现 <see cref="IHasInventoryKey"/> 接口的对象。</param>
    /// <param name="keyType">库存键的类型。</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">在对象上未找到库存键中定义的属性。</exception>
    public static InventoryKey GetInventoryKey(this IHasInventoryKey obj, InventoryKeyType keyType)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(keyType);

        var entityProps = obj.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToArray();

        List<object?> values = new();
        Type type = keyType;
        foreach (var keyParam in type.GetConstructors()[0].GetParameters())
        {
            var entityProp = entityProps.SingleOrDefault(x => x.Name == keyParam.Name);
            if (entityProp == null)
            {
                throw new InvalidOperationException($"未找到属性，类型【{obj.GetType()}】，名称【{keyParam.Name}】");
            }
            object? value = entityProp.GetValue(obj);
            values.Add(value);
        }

        return keyType.CreateInstance(values.ToArray());

    }


    /// <summary>
    /// 设置对象的库存键。
    /// </summary>
    /// <param name="obj">实现 <see cref="IHasInventoryKey"/> 接口的对象。</param>
    /// <param name="key">库存键。</param>
    /// <exception cref="InvalidOperationException"></exception>
    public static void SetInventoryKey(this IHasInventoryKey obj, InventoryKey key)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(key);

        var entityProps = obj.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToArray();
        foreach (var keyProp in key.GetType().GetProperties())
        {
            var entityProp = entityProps.SingleOrDefault(x => x.Name == keyProp.Name);
            if (entityProp is null)
            {
                throw new InvalidOperationException($"未找到属性，类型【{obj.GetType()}】，名称【{keyProp.Name}】");
            }
            object? value = keyProp.GetValue(key);
            entityProp.SetValue(obj, value);
        }
    }


    /// <summary>
    /// 查询指定的库存键。
    /// </summary>
    /// <typeparam name="T">实现 <see cref="IHasInventoryKey"/> 接口的类型。</typeparam>
    /// <param name="q">查询对象。</param>
    /// <param name="inventoryKey">要查询的库存键。</param>
    /// <returns></returns>
    public static IQueryable<T> OfInventoryKey<T>(this IQueryable<T> q, InventoryKey inventoryKey) where T : class, IHasInventoryKey
    {
        ArgumentNullException.ThrowIfNull(q);
        ArgumentNullException.ThrowIfNull(inventoryKey);
        
        var components = GetComponents();
        string where = BuildWhereClause(components);
        q = q.Where(where, components.Select(x => x.value).ToArray());

        return q;

        (string name, object? value)[] GetComponents()
        {
            return inventoryKey.GetType()
                .GetProperties()
                .Select(x => (x.Name, x.GetValue(inventoryKey)))
                .ToArray();
        }
    }


    internal static string BuildWhereClause(params (string name, object? value)[] components)
    {
        return string.Join(" AND ", components.Select((x, i) => $"{x.name} = @{i}"));
    }


}