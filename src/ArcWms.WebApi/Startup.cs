using Arc.AppSeqs.DependencyInjection;
using Arc.Ops.DependencyInjection;
using ArcWms.Cfg;
using ArcWms.WebApi.Handlers;
using ArcWms.WebApi.MetaData;
using ArcWms.WebApi.Models;
using Autofac;
using AutofacSerilogIntegration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OperationTypeAspNetCoreAuthorization;
using Quartz;
using Serilog;
using Serilog.Context;
using Serilog.Extensions.Logging;
using System.Net.Mime;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace ArcWms.WebApi;

public class Startup
{
    IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddResponseCompression();

        services.AddIdentity(_configuration.GetConnectionString("auth"));

        services.AddDbContext<LogDbContext>(options => options.UseSqlServer(_configuration.GetConnectionString("LogDb")));

        services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var result = new BadRequestObjectResult(context.ModelState);

                        result.ContentTypes.Add(MediaTypeNames.Application.Json);
                        result.ContentTypes.Add(MediaTypeNames.Application.Xml);

                        return result;
                    };
                });

        services.AddRouting(options =>
        {
            options.LowercaseUrls = true;
        });

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ArcWms.WebApi", Version = "v1" });

            // Set the comments path for the Swagger JSON and UI.
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "请输入带有 Bearer 的 Token",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                BearerFormat = "JWT",
                Scheme = "Bearer",
            });

            var scheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [scheme] = new string[0]
            });
        });

        services.AddHttpContextAccessor();

        services.AddTransient<IPrincipal>(provider => provider.GetService<IHttpContextAccessor>()?.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity()));


        #region 身份验证和授权

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                JwtSetting jwtSetting = _configuration.GetSection("JwtSetting").Get<JwtSetting>();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtSetting.Issuer,
                    ValidAudience = jwtSetting.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting.SecurityKey)),
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            })
            .AddIdentityCookies(o => { });

        services.AddOperationType();

        services.Configure<JwtSetting>(options =>
        {
            _configuration.GetSection("JwtSetting").Bind(options);
        });

        #endregion

        #region 集成 Quartz

        // TODO  services.AddTransient<HttpRequestJob>();
        services.Configure<QuartzOptions>(_configuration.GetSection("Quartz"));

        services.AddQuartz(q =>
        {
            // base quartz scheduler, job and trigger configuration
            q.UseMicrosoftDependencyInjectionJobFactory();
            // also add XML configuration and poll it for changes
            q.UseXmlSchedulingConfiguration(x =>
            {
                x.Files = new[] { "~/quartz_jobs.xml" };
                x.ScanInterval = TimeSpan.FromSeconds(60);
                x.FailOnFileNotFound = true;
                x.FailOnSchedulingError = true;
            });
        });


        // ASP.NET Core hosting
        services.AddQuartzServer(options =>
        {
            // TODO 设为 true
            // when shutting down we want jobs to complete gracefully
            options.WaitForJobsToComplete = false;
            options.StartDelay = TimeSpan.FromSeconds(15);
        });

        #endregion


    }


    public void ConfigureContainer(ContainerBuilder builder)
    {
        var loggerFactory = new SerilogLoggerFactory();

        builder.RegisterLogger();

        builder.AddAppSeqs();
        builder.AddOps();
        builder.AddNHibernate(loggerFactory);
        builder.AddMaterial(module =>
        {
            module.UseMaterial<Material>()
                .UseBatchService<DefaultBatchService>()
                .UseInventoryKey<InventoryKey2>()
                .UseFlow<Flow>()
                .Configure(opt =>
                {
                    opt.MaterialTypes = new[]
                    {
                        SupportedMaterialTypes.原材料,
                        SupportedMaterialTypes.成品
                    };

                    opt.InventoryStatus = new[]
                    {
                        SupportedInventoryStatuses.待检,
                        SupportedInventoryStatuses.合格,
                        SupportedInventoryStatuses.不合格,
                    };

                    opt.BizTypes = new BizType[]
                    {
                        SupportedBizTypes.独立入库,
                        SupportedBizTypes.独立出库,
                        SupportedBizTypes.待检转合格,
                        SupportedBizTypes.待检转不合格,
                        SupportedBizTypes.不合格转合格,
                        SupportedBizTypes.合格转不合格,
                    };
                });
        });

        builder.AddLocation(m =>
        {
            m.UseLocation<Location>();
            m.UseStreetlet<Streetlet>();
            m.UseCell<Cell>();
        });

        builder.AddPalletization(module =>
        {
            module.UseUnitload<Unitload>()
                .UseUnitloadItem<UnitloadItem>()
                .UseUnitloadSnapshot<UnitloadSnapshot>()
                .UseUnitloadItemSnapshot<UnitloadItemSnapshot>()
                .UseUnitloadStorageInfoProvider<DefaultUnitloadStorageInfoProvider>()
                .UsePalletCodeValidator(new RegexPalletCodeValidator(@"^P\d{2,3}$", "托盘号应为大写字母 P 后跟两到三位数字。"));
        });

        builder.AddStorageLocationAssignment(module =>
        {
            module.AddRule<SSRule>();
        }, loggerFactory);

        builder.AddTransportTask(module =>
        {
            module
                .UseTaskSender<DummyTaskSender>()
                .AddRequestHandler<上架请求处理程序>("上架")
                .AddCompletedTaskHandler<一般完成处理程序>("上架")
                .AddCompletedTaskHandler<一般完成处理程序>("下架");
        }, loggerFactory);

        builder.AddInboundOrder(module =>
        {
            module.UseInboundOrder<InboundOrder>()
                .UseInboundLine<InboundLine>();
        });

        builder.AddOutboundOrder(module =>
        {
            module.UseOutboundOrder<OutboundOrder>()
                .UseOutboundLine<OutboundLine>()
                .UseOutboundOrderAllocator<DefaultOutboundOrderAllocator>();
        });


    }

    public void Configure(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            //app.UseDeveloperExceptionPage();
            app.UseExceptionHandler("/error-local-development");
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ArcWms.WebApi v1"));
        }
        else
        {
            app.UseExceptionHandler("/error");
        }

        // TODO 改为安全的设置
        app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        app.UseHsts();

        app.UseResponseCompression();
        // app.UseHttpsRedirection();
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "download")),
            RequestPath = "/download"
        });

        app.Use(async (context, next) =>
        {
            using (LogContext.PushProperty("Polling", context.Request.Headers["User-Agent"].ToString() == "HttpRequestJob/1.0 (Quartz.net Job)"))
            {
                using (LogContext.PushProperty("RequestId", context.TraceIdentifier))
                {
                    await next();
                }
            }
        });

        app.UseSerilogRequestLogging();
        app.UseRouting();
        app.UseAuthentication();

        app.Use(async (context, next) =>
        {
            using (LogContext.PushProperty("UserName", context.User?.Identity?.Name ?? "-"))
            {
                await next();
            }
        });

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            // TODO 
            //endpoints.MapHub<MonitorHub>("/monitor");
            endpoints.MapControllers();
        });

    }


}
