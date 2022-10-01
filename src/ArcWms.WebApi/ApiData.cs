using Arc.Ops;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using NHibernateUtils;
using OperationTypeAspNetCoreAuthorization;

namespace ArcWms.WebApi;

/// <summary>
/// 表示 WebApi 返回的数据
/// </summary>
public record ApiData
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// 唯一的请求Id
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// 主机
    /// </summary>
    public string? Host { get; init; }
}


/// <summary>
/// 表示 WebApi 返回的数据
/// </summary>
public record ApiData<TData> : ApiData
{
    /// <summary>
    /// 数据。
    /// </summary>
    public TData? Data { get; init; }
}

/// <summary>
/// 表示列表页数据
/// </summary>
/// <typeparam name="TElement"></typeparam>
public record ListData<TElement> : ApiData
{
    /// <summary>
    /// 数据
    /// </summary>
    public List<TElement>? Data { get; init; }

    /// <summary>
    /// 当前页
    /// </summary>
    public int CurrentPage { get; init; }

    /// <summary>
    /// 页大小
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// 记录总数
    /// </summary>
    public int Total { get; init; }
}


/// <summary>
/// 表示选项列表的数据
/// </summary>
/// <typeparam name="TElement"></typeparam>
public record OptionsData<TElement> : ApiData<List<TElement>>
{

}


/// <summary>
/// 用于生成 <see cref="ApiData"/> 的扩展属性。
/// </summary>
public static class ApiDataExtensions
{
    /// <summary>
    /// 生成表示操作成功的 <see cref="ApiData"/> 对象。
    /// </summary>
    /// <param name="controller"></param>
    /// <returns></returns>
    public static ApiData Success(this ControllerBase controller)
    {
        return new ApiData
        {
            Success = true,
            ErrorMessage = string.Empty,
            Host = controller.Request.Host.ToString(),
            TraceId = controller.HttpContext.TraceIdentifier,
        };
    }

    /// <summary>
    /// 生成表示操作成功的 <see cref="ApiData{TData}"/> 对象。
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="controller"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static ApiData<TData> Success<TData>(this ControllerBase controller, TData? data)
    {
        return new ApiData<TData>
        {
            Success = true,
            Data = data,
            ErrorMessage = string.Empty,
            Host = controller.Request.Host.ToString(),
            TraceId = controller.HttpContext.TraceIdentifier,
        };
    }

    /// <summary>
    /// 生成表示操作失败的 <see cref="ApiData"/> 对象。
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    public static ApiData Failure(this ControllerBase controller, string? errorMessage)
    {
        return new ApiData
        {
            Success = false,
            ErrorMessage = errorMessage,
            Host = controller.Request.Host.ToString(),
            TraceId = controller.HttpContext.TraceIdentifier,
        };
    }

    /// <summary>
    /// 生成表示操作失败的 <see cref="ApiData"/> 对象。
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    public static ApiData<TData> Failure<TData>(this ControllerBase controller, string? errorMessage)
    {
        return new ApiData<TData>
        {
            Success = false,
            ErrorMessage = errorMessage,
            Host = controller.Request.Host.ToString(),
            TraceId = controller.HttpContext.TraceIdentifier,
        };
    }

    /// <summary>
    /// 生成表示分页数据列表的对象。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="controller"></param>
    /// <param name="pagedList"></param>
    /// <returns></returns>
    public static ListData<T> ListData<T>(this ControllerBase controller, PagedList<T> pagedList)
    {
        return controller.ListData(pagedList, x => x);
    }

    /// <summary>
    /// 生成表示分页数据列表的对象。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <param name="controller"></param>
    /// <param name="pagedList"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static ListData<U> ListData<T, U>(this ControllerBase controller, PagedList<T> pagedList, Func<T, U> selector)
    {
        return new ListData<U>
        {
            Success = true,
            Data = pagedList.List.Select(selector).ToList(),
            CurrentPage = pagedList.CurrentPage,
            PageSize = pagedList.PageSize,
            Total = pagedList.Total,
            ErrorMessage = string.Empty,
            Host = controller.Request.Host.ToString(),
            TraceId = controller.HttpContext.TraceIdentifier,
        };
    }

    /// <summary>
    /// 生成选择列表数据对象。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="controller"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    public static OptionsData<T> OptionsData<T>(this ControllerBase controller, List<T> items)
    {
        return new OptionsData<T>
        {
            Success = true,
            Data = items,
            ErrorMessage = string.Empty,
            Host = controller.Request.Host.ToString(),
            TraceId = controller.HttpContext.TraceIdentifier,
        };
    }

    /// <summary>
    /// 生成表示错误的 <see cref="ApiData"/> 对象。
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    public static ApiData Error(this ControllerBase controller, string? errorMessage)
    {
        return new ApiData
        {
            Success = false,
            ErrorMessage = errorMessage,
            Host = controller.Request.Host.ToString(),
            TraceId = controller.HttpContext.TraceIdentifier,
        };
    }
}


/// <summary>
/// 提供操作记录相关的方法
/// </summary>
public static class SaveOpExtensions
{
    /// <summary>
    /// 使用 <see cref="OperationTypeAttribute.OperationType"/> 的值创建一个 Op 对象，
    /// 并使用 <see cref="TransactionAttribute"/> 所在的 <see cref="ISession"/> 保存到数据库。
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="format">用于填充 <see cref="Op.Comment"/> 属性的格式化字符串。</param>
    /// <param name="args">用于填充 <see cref="Op.Comment"/> 属性的格式化参数。</param>
    /// <returns></returns>
    public static Task<Op> SaveOpAsync(this ControllerBase controller, string format, params object[] args)
    {
        ISession? session = (ISession?)controller.HttpContext.Items[typeof(ISession)];
        if (session == null)
        {
            throw new Exception("未获取到 ISession。");
        }
        if (controller.HttpContext.GetOperationType() is not String operationType)
        {
            throw new Exception("未获取到操作类型。");
        }
        OpHelper opHelper = new OpHelper(session);
        string? url = controller.HttpContext?.Request?.GetDisplayUrl();
        string? ipAddress = controller.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        return opHelper.SaveOpAsync(operationType, url, ipAddress, format, args);
    }

}


