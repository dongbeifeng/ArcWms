using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 错误处理程序
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : ControllerBase
{
    /// <summary>
    /// 初始化新实例。
    /// </summary>
    public ErrorController()
    {
    }

    /// <summary>
    /// 生产环境错误处理
    /// </summary>
    /// <returns></returns>
    [Route("/error")]
    public ApiData Error()
    {
        var errorContext = HttpContext.Features.Get<IExceptionHandlerFeature>();
        if (errorContext != null)
        {
            return this.Error(errorContext.Error.Message);
        }
        return this.Error("未知错误");
    }

    /// <summary>
    /// 开发环境错误处理
    /// </summary>
    /// <param name="webHostEnvironment"></param>
    /// <returns></returns>
    [Route("/error-local-development")]
    public ApiData ErrorLocalDevelopment([FromServices] IWebHostEnvironment webHostEnvironment)
    {
        if (webHostEnvironment.EnvironmentName != "Development")
        {
            throw new InvalidOperationException("This shouldn't be invoked in non-development environments.");
        }
        var errorContext = HttpContext.Features.Get<IExceptionHandlerFeature>();
        if (errorContext != null)
        {
            return this.Error(errorContext.Error.Message);
        }
        return this.Error("未知错误");
    }
}

