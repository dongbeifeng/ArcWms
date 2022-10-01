// 参考：https://blog.csdn.net/sd7o95o/article/details/114504446

using ArcWms.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ClaimTypes = System.Security.Claims.ClaimTypes;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 登录和已登录用户的相关功能
/// </summary>
[ApiController]
[Route("api/account")]
public class AccountController : ControllerBase
{
    readonly IOptions<JwtSetting> _jwtSetting;
    readonly UserManager<ApplicationUser> _userManager;
    readonly RoleManager<ApplicationRole> _roleManager;
    readonly SignInManager<ApplicationUser> _signInManager;
    readonly ILogger<AccountController> _logger;

    /// <summary>
    /// 初始化新实例
    /// </summary>
    /// <param name="userManager"></param>
    /// <param name="roleManager"></param>
    /// <param name="signInManager"></param>
    /// <param name="jwtSetting"></param>
    /// <param name="logger"></param>
    public AccountController(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        SignInManager<ApplicationUser> signInManager,
        IOptions<JwtSetting> jwtSetting,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _jwtSetting = jwtSetting;
        _logger = logger;
    }

    private string GenerateToken(IEnumerable<Claim> claims)
    {
        var key = Encoding.UTF8.GetBytes(_jwtSetting.Value.SecurityKey);

        //创建令牌
        var token = new JwtSecurityToken(
          issuer: _jwtSetting.Value.Issuer,
          audience: _jwtSetting.Value.Audience,
          signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
          claims: claims,
          notBefore: DateTime.Now,
          expires: DateTime.Now.AddMinutes(_jwtSetting.Value.TokenExpiry)
        );

        string jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }

    private async Task GenerateRefreshTokenAsync(ApplicationUser user)
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }
        user.RefreshToken = Convert.ToBase64String(randomNumber);
        user.RefreshTokenTime = DateTime.Now;
        user.RefreshTokenExpireTime = DateTime.Now.AddYears(99);
        await _userManager.UpdateAsync(user);
    }

    //private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    //{
    //    var key = Encoding.UTF8.GetBytes(_jwtSetting.Value.SecurityKey);

    //    var tokenValidationParameters = new TokenValidationParameters
    //    {
    //        ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
    //        ValidateIssuer = false,
    //        ValidateIssuerSigningKey = true,
    //        IssuerSigningKey = new SymmetricSecurityKey(key),
    //        ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
    //    };

    //    var tokenHandler = new JwtSecurityTokenHandler();
    //    SecurityToken securityToken;
    //    var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
    //    var jwtSecurityToken = securityToken as JwtSecurityToken;
    //    if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
    //    {
    //        throw new SecurityTokenException("Invalid token");
    //    }

    //    return principal;
    //}

    /// <summary>
    /// 刷新访问令牌。
    /// </summary>
    /// <remarks>
    /// 没有为不同设备分别保存刷新令牌，而是每个用户一个刷新令牌。
    /// 为实现多点登录，这里的实现只更新访问令牌，不更新刷新令牌，
    /// 否则，一个设备更新刷新令牌，将令其他设备上的刷新令牌失效。
    /// </remarks>
    /// <returns></returns>
    [HttpPost("refresh-token")]
    [Authorize]
    public async Task<ApiData<RefreshTokenInfo>> RefreshToken(RefreshTokenArgs args)
    {
        string refreshToken = args.RefreshToken;

        _logger.LogDebug("正在刷新访问令牌");
        var username = this.User.Identity?.Name;
        ApplicationUser? user = await _userManager.FindByNameAsync(username) ?? throw new("用户不存在");

        if (user.RefreshToken != refreshToken)
        {
            return this.Failure<RefreshTokenInfo>("无效的刷新令牌");
        }
        if (user.RefreshTokenExpireTime < DateTime.Now)
        {
            return this.Failure<RefreshTokenInfo>("刷新令牌已过期");
        }

        var newJwtToken = GenerateToken(this.User.Claims);

        _logger.LogInformation("已刷新访问令牌");

        return this.Success(new RefreshTokenInfo
        {
            Token = newJwtToken,
            TokenExpiry = _jwtSetting.Value.TokenExpiry,
            RefreshToken = user.RefreshToken
        });
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginArgs model)
    {
        // TODO 增加操作记录
        // TODO 打印日志

        if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
        {
            return Ok(new
            {
                status = "error",
                message = "用户名和密码不能为空"
            });
        }

        var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(model.UserName);
            var roles = await _userManager.GetRolesAsync(user);

            //创建用户身份标识，可按需要添加更多信息
            var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id, ClaimValueTypes.String), // 用户id
                    new Claim(ClaimTypes.Name, user.UserName), // 用户名
                };

            claims.AddRange(await _userManager.GetClaimsAsync(user));

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));

                ApplicationRole appRole = await _roleManager.FindByNameAsync(role);
                claims.AddRange(await _roleManager.GetClaimsAsync(appRole));
            }

            var jwt = GenerateToken(claims);
            if (user.RefreshToken == null || user.RefreshTokenExpireTime < DateTime.Now)
            {
                await GenerateRefreshTokenAsync(user);
                _logger.LogInformation("已创建刷新令牌");
            }
            else
            {
                _logger.LogInformation("未创建刷新令牌");
            }


            return Ok(new
            {
                status = "ok",
                token = jwt,
                tokenExpiry = _jwtSetting.Value.TokenExpiry,
                refreshToken = user.RefreshToken,
                type = "Bearer",
                userName = user.UserName,
                currentAuthority = roles
            });
        }

        return Ok(new
        {
            status = "error",
            message = "用户名或密码错误"
        });

    }

    /// <summary>
    /// 用户退出登录
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // TODO 增加操作记录
        // TODO 打印日志
        ApplicationUser user = await _userManager.FindByNameAsync(User.Identity?.Name);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpireTime = null;
            user.RefreshTokenTime = null;
            await _userManager.UpdateAsync(user);
        }

        return Ok(new
        {
            status = "ok",
            message = "退出登录成功"
        });

    }


    /// <summary>
    /// 已登录用户更改自己的密码
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ApiData> ChangePassword(ChangePasswordArgs args)
    {
        ApplicationUser? user = await _userManager.FindByNameAsync(User?.Identity?.Name);

        if (user == null)
        {
            throw new Exception("用户不存在");
        }

        var result = await _userManager.ChangePasswordAsync(user, args.OriginalPassword, args.Password);
        if (result.Succeeded)
        {
            return this.Success();
        }

        return this.Failure(string.Join(", ", result.Errors.Select(x => x.Description)));
    }

    /// <summary>
    /// 或者当前登录用户信息
    /// </summary>
    /// <returns></returns>
    [HttpPost("get-current-user")]
    [Authorize]
    public async Task<AntProCurrentUserInfo> GetCurrentUser()
    {
        ApplicationUser? user = await _userManager.FindByNameAsync(User?.Identity?.Name);

        AntProCurrentUserInfo userInfo = new AntProCurrentUserInfo();
        if (user != null)
        {
            userInfo.Name = user.UserName;
            userInfo.Userid = user.Id;
            userInfo.Email = user.Email;
            userInfo.Phone = user.PhoneNumber;
        }

        return userInfo;
    }

}

