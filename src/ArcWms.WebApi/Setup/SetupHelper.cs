#if SETUP

using ArcWms;
using ArcWms.WebApi.Models;
using Microsoft.AspNetCore.Identity;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Linq;
using NHibernate.Tool.hbm2ddl;
using System.Text.RegularExpressions;


public class SetupHelper
{
    readonly Configuration _nhConfiguration;
    readonly Func<Streetlet> _createStreetlet;
    readonly Func<Location> _createLocation;
    readonly Func<Cell> _createCell;
    readonly LocationHelper _locationHelper;
    readonly UserManager<ApplicationUser> _userManager;
    readonly RoleManager<ApplicationRole> _roleManager;
    readonly ISession _session;
    readonly IConfiguration _configuration;

    /// <summary>
    /// 初始化新实例。
    /// </summary>
    /// <param name="nhConfiguration"></param>
    /// <param name="createLocation"></param>
    /// <param name="locationHelper"></param>
    /// <param name="userManager"></param>
    /// <param name="roleManager"></param>
    /// <param name="session"></param>
    /// <param name="configuration"></param>
    public SetupHelper(
        Configuration nhConfiguration,
        Func<Streetlet> createStreetlet,
        Func<Location> createLocation,
        Func<Cell> createCell,
        LocationHelper locationHelper,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ISession session,
        IConfiguration configuration
        )
    {
        _nhConfiguration = nhConfiguration;
        _createStreetlet = createStreetlet;
        _configuration = configuration;
        _createLocation = createLocation;
        _createCell = createCell;
        _locationHelper = locationHelper;
        _userManager = userManager;
        _roleManager = roleManager;
        _session = session;
    }

    /// <summary>
    /// 生成巷道。
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    internal async Task GenerateStreetletAsync(
        string streetletCode, bool doubleDeep, string area,
        string? leftRack2, string? leftRack1, int leftBays, int leftLevels,
        string? rightRack1, string? rightRack2, int rightBays, int rightLevels,
        CancellationToken token)
    {
        Console.WriteLine("正在生成巷道");

        try
        {
            using (ITransaction tx = _session.BeginTransaction())
            {
                Console.WriteLine($"巷道编码：{streetletCode}，" +
                    $"{(doubleDeep ? "双深" : "单深")}，" +
                    $"库区：{area}，" +
                    $"左2货架编码：{leftRack2}，" +
                    $"左1货架编码：{leftRack1}，" +
                    $"左侧列数：{leftBays}，" +
                    $"左侧层数：{leftLevels}，" +
                    $"右1货架编码：{rightRack1}，" +
                    $"右2货架编码：{rightRack2}，" +
                    $"右侧列数：{rightBays}，" +
                    $"右侧层数：{rightLevels}");

                if (doubleDeep == false)
                {
                    if (leftRack2 is not null)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"未将巷道指定为双深，将忽略左2货架 {leftRack2}");
                        Console.ResetColor();
                    }

                    if (rightRack2 is not null)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"未将巷道指定为双深，将忽略右2货架 {rightRack2}");
                        Console.ResetColor();
                    }
                }


                Streetlet streetlet = _createStreetlet();
                streetlet.StreetletCode = streetletCode;
                streetlet.Area = area;
                streetlet.IsDoubleDeep = doubleDeep;

                await _session.SaveAsync(streetlet, token).ConfigureAwait(false);
                List<(string? rackCode, RackSide side, int deep)> racks = new List<(string? rack, RackSide side, int deep)>();
                if (doubleDeep)
                {
                    racks.Add((leftRack2, RackSide.Left, 2));
                    racks.Add((leftRack1, RackSide.Left, 1));
                    racks.Add((rightRack1, RackSide.Right, 1));
                    racks.Add((rightRack2, RackSide.Right, 2));
                }
                else
                {
                    racks.Add((leftRack1, RackSide.Left, 1));
                    racks.Add((rightRack1, RackSide.Right, 1));
                }

                Console.WriteLine("正在生成货位");
                foreach (var rack in racks.Where(x => x.side == RackSide.Left))
                {
                    if (rack.rackCode is not null)
                    {
                        await CreateRackAsync(streetlet, rack.rackCode, rack.side, rack.deep, leftBays, leftLevels);
                    }
                }
                foreach (var rack in racks.Where(x => x.side == RackSide.Right))
                {
                    if (rack.rackCode is not null)
                    {
                        await CreateRackAsync(streetlet, rack.rackCode, rack.side, rack.deep, rightBays, rightLevels);
                    }
                }

                await _session.FlushAsync(token).ConfigureAwait(false);
                int count = await _session.Query<Location>().Where(x => x.Streetlet == streetlet).CountAsync(token).ConfigureAwait(false);
                Console.WriteLine($"已生成货位，共 {count} 个");


                Console.WriteLine("正在更新 Cell.i1");
                ISQLQuery q1 = _session.CreateSQLQuery(@"
MERGE cells c
USING (SELECT cell_id, ROW_NUMBER() OVER(ORDER BY [level], bay, side) + :streetletId * 10000 AS i1
		FROM locations
        WHERE streetlet_id = :streetletId
    ) AS t
ON c.cell_id = t.cell_id
WHEN MATCHED THEN UPDATE SET c.i1 = t.i1;");
                await q1
                    .SetInt32("streetletId", streetlet.StreetletId)
                    .ExecuteUpdateAsync(token)
                    .ConfigureAwait(false);
                Console.WriteLine("已更新 Cell.i1");

                Console.WriteLine("正在更新 Cell.o1");
                ISQLQuery q2 = _session.CreateSQLQuery(@"
MERGE cells c
USING (SELECT cell_id, ROW_NUMBER() OVER(ORDER BY [level], bay, side) + :streetletId * 10000 AS o1
		FROM locations
        WHERE streetlet_id = :streetletId
    ) AS t
ON c.cell_id = t.cell_id
WHEN MATCHED THEN UPDATE SET c.o1 = t.o1;");
                await q2
                    .SetInt32("streetletId", streetlet.StreetletId)
                    .ExecuteUpdateAsync(token)
                    .ConfigureAwait(false);

                Console.WriteLine("已更新 Cell.o1");


                Console.WriteLine("正在生成统计数据");
                await _locationHelper.RebuildStreetletStatAsync(streetlet).ConfigureAwait(false);
                Console.WriteLine("已生成统计数据");

                await tx.CommitAsync(token).ConfigureAwait(false);
            }

            Console.WriteLine("已生成巷道");

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        async Task CreateRackAsync(Streetlet streetlet, string rackCode, RackSide side, int deep, int columns, int levels)
        {
            for (int i = 0; i < levels; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    int bay = j + 1;
                    int lv = i + 1;
                    Cell cell = _createCell();

                    {
                        string locCode = string.Format("{0}-{1:00}-{2:0}", rackCode, bay, lv);
                        Location loc = _createLocation.Invoke();
                        loc.LocationCode = locCode;
                        loc.LocationType = LocationTypes.S;
                        loc.Streetlet = streetlet;
                        loc.Bay = bay;
                        loc.Level = lv;
                        loc.InboundLimit = 1;
                        loc.OutboundLimit = 1;
                        loc.Side = side;
                        loc.Deep = deep;
                        loc.StorageGroup = "普通";
                        loc.Specification = "普通";
                        loc.Cell = cell;
                        cell.Locations.Add(loc);
                    }

                    await _session.SaveAsync(cell, token).ConfigureAwait(false);
                    foreach (var loc in cell.Locations)
                    {
                        await _session.SaveAsync(loc, token).ConfigureAwait(false);
                    }

                }
            }
        }
    }



    internal const string AspnetIdentityScript = @"
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserTokens]') AND type in (N'U'))
ALTER TABLE [dbo].[AspNetUserTokens] DROP CONSTRAINT IF EXISTS [FK_AspNetUserTokens_AspNetUsers_UserId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserRoles]') AND type in (N'U'))
ALTER TABLE [dbo].[AspNetUserRoles] DROP CONSTRAINT IF EXISTS [FK_AspNetUserRoles_AspNetUsers_UserId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserRoles]') AND type in (N'U'))
ALTER TABLE [dbo].[AspNetUserRoles] DROP CONSTRAINT IF EXISTS [FK_AspNetUserRoles_AspNetRoles_RoleId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserLogins]') AND type in (N'U'))
ALTER TABLE [dbo].[AspNetUserLogins] DROP CONSTRAINT IF EXISTS [FK_AspNetUserLogins_AspNetUsers_UserId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserClaims]') AND type in (N'U'))
ALTER TABLE [dbo].[AspNetUserClaims] DROP CONSTRAINT IF EXISTS [FK_AspNetUserClaims_AspNetUsers_UserId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetRoleClaims]') AND type in (N'U'))
ALTER TABLE [dbo].[AspNetRoleClaims] DROP CONSTRAINT IF EXISTS [FK_AspNetRoleClaims_AspNetRoles_RoleId]
GO
/****** Object:  Index [UserNameIndex]    Script Date: 2021/4/16 10:23:42 ******/
DROP INDEX IF EXISTS [UserNameIndex] ON [dbo].[AspNetUsers]
GO
/****** Object:  Index [EmailIndex]    Script Date: 2021/4/16 10:23:42 ******/
DROP INDEX IF EXISTS [EmailIndex] ON [dbo].[AspNetUsers]
GO
/****** Object:  Index [IX_AspNetUserRoles_RoleId]    Script Date: 2021/4/16 10:23:42 ******/
DROP INDEX IF EXISTS [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles]
GO
/****** Object:  Index [IX_AspNetUserLogins_UserId]    Script Date: 2021/4/16 10:23:42 ******/
DROP INDEX IF EXISTS [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins]
GO
/****** Object:  Index [IX_AspNetUserClaims_UserId]    Script Date: 2021/4/16 10:23:42 ******/
DROP INDEX IF EXISTS [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims]
GO
/****** Object:  Index [RoleNameIndex]    Script Date: 2021/4/16 10:23:42 ******/
DROP INDEX IF EXISTS [RoleNameIndex] ON [dbo].[AspNetRoles]
GO
/****** Object:  Index [IX_AspNetRoleClaims_RoleId]    Script Date: 2021/4/16 10:23:42 ******/
DROP INDEX IF EXISTS [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims]
GO
/****** Object:  Table [dbo].[AspNetUserTokens]    Script Date: 2021/4/16 10:23:42 ******/
DROP TABLE IF EXISTS [dbo].[AspNetUserTokens]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 2021/4/16 10:23:42 ******/
DROP TABLE IF EXISTS [dbo].[AspNetUsers]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 2021/4/16 10:23:42 ******/
DROP TABLE IF EXISTS [dbo].[AspNetUserRoles]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 2021/4/16 10:23:42 ******/
DROP TABLE IF EXISTS [dbo].[AspNetUserLogins]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 2021/4/16 10:23:42 ******/
DROP TABLE IF EXISTS [dbo].[AspNetUserClaims]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 2021/4/16 10:23:42 ******/
DROP TABLE IF EXISTS [dbo].[AspNetRoles]
GO
/****** Object:  Table [dbo].[AspNetRoleClaims]    Script Date: 2021/4/16 10:23:42 ******/
DROP TABLE IF EXISTS [dbo].[AspNetRoleClaims]
GO

/****** Object:  Table [dbo].[AspNetRoleClaims]    Script Date: 2021/4/16 10:23:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoleClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 2021/4/16 10:23:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](256) NULL,
	[NormalizedName] [nvarchar](256) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 2021/4/16 10:23:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 2021/4/16 10:23:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[ProviderDisplayName] [nvarchar](max) NULL,
	[UserId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 2021/4/16 10:23:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](450) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 2021/4/16 10:23:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](450) NOT NULL,
	[UserName] [nvarchar](256) NULL,
	[NormalizedUserName] [nvarchar](256) NULL,
	[Email] [nvarchar](256) NULL,
	[NormalizedEmail] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEnd] [datetimeoffset](7) NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
 CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserTokens]    Script Date: 2021/4/16 10:23:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserTokens](
	[UserId] [nvarchar](450) NOT NULL,
	[LoginProvider] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[LoginProvider] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetRoleClaims_RoleId]    Script Date: 2021/4/16 10:23:42 ******/
CREATE NONCLUSTERED INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [RoleNameIndex]    Script Date: 2021/4/16 10:23:42 ******/
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex] ON [dbo].[AspNetRoles]
(
	[NormalizedName] ASC
)
WHERE ([NormalizedName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserClaims_UserId]    Script Date: 2021/4/16 10:23:42 ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserLogins_UserId]    Script Date: 2021/4/16 10:23:42 ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserRoles_RoleId]    Script Date: 2021/4/16 10:23:42 ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [EmailIndex]    Script Date: 2021/4/16 10:23:42 ******/
CREATE NONCLUSTERED INDEX [EmailIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedEmail] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UserNameIndex]    Script Date: 2021/4/16 10:23:42 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedUserName] ASC
)
WHERE ([NormalizedUserName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AspNetRoleClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetRoleClaims] CHECK CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserTokens]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserTokens] CHECK CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
GO

ALTER TABLE [dbo].[AspNetUsers] ADD IsBuiltIn bit NOT NULL
GO
ALTER TABLE [dbo].[AspNetUsers] ADD Comment nvarchar(255)
GO
ALTER TABLE [dbo].[AspNetUsers] ADD RefreshToken nvarchar(255)
GO
ALTER TABLE [dbo].[AspNetUsers] ADD RefreshTokenTime datetime2
GO
ALTER TABLE [dbo].[AspNetUsers] ADD RefreshTokenExpireTime datetime2
GO
ALTER TABLE [dbo].[AspNetRoles] ADD IsBuiltIn bit NOT NULL
GO
ALTER TABLE [dbo].[AspNetRoles] ADD Comment nvarchar(255)
GO
";

    internal Task ShowConnectionStringAsync()
    {
        Console.WriteLine();

        Console.Write("NHibernate：");
        Console.WriteLine(_session.Connection.ConnectionString);

        Console.Write("auth：");
        Console.WriteLine(_configuration.GetValue<string>("ConnectionStrings:auth"));

        Console.Write("quartz.net：");
        Console.WriteLine(_configuration.GetValue<string>("ConnectionStrings:quartz.net"));

        Console.Write("LogDb：");
        Console.WriteLine(_configuration.GetValue<string>("ConnectionStrings:LogDb"));

        return Task.CompletedTask;
    }

    internal async Task ExportSchemaAsync()
    {
        Console.WriteLine("正在导出数据库架构");

        await ShowConnectionStringAsync().ConfigureAwait(false);

        try
        {
            using (ITransaction tx = _session.BeginTransaction())
            {
                Console.WriteLine("正在导出 NHibernate 映射");
                SchemaExport export = new SchemaExport(_nhConfiguration);
                await export.CreateAsync(false, true).ConfigureAwait(false);
                Console.WriteLine("已导出 NHibernate 映射");

                Console.WriteLine("正在创建 Aspnet Identity 表");
                foreach (var stmt in Regex.Split(AspnetIdentityScript, @"\bGO\b"))
                {
                    if (string.IsNullOrWhiteSpace(stmt) == false)
                    {
                        await _session.CreateSQLQuery(stmt).ExecuteUpdateAsync().ConfigureAwait(false);
                    }
                }
                Console.WriteLine("已创建 Aspnet Identity 表");

                Console.WriteLine("正在创建 N 位置");
                Location loc = _createLocation.Invoke();
                loc.LocationCode = LocationCodes.N;
                loc.LocationType = LocationTypes.N;
                loc.InboundLimit = 999;
                loc.OutboundLimit = 999;
                await _session.SaveAsync(loc).ConfigureAwait(false);
                Console.WriteLine("已创建 N 位置");

                await tx.CommitAsync().ConfigureAwait(false);

            }

            Console.WriteLine("正在创建内置用户");
            var role = new ApplicationRole { Name = "admin", IsBuiltIn = true };
            await _roleManager.CreateAsync(role).ConfigureAwait(false);

            ApplicationUser user = new ApplicationUser { UserName = "admin", IsBuiltIn = true };
            await _userManager.CreateAsync(user, "123456").ConfigureAwait(false);
            await _userManager.AddToRolesAsync(user, new[] { "admin" }).ConfigureAwait(false);

            Console.WriteLine("已创建内置用户，用户名 admin，密码 123456");

            Console.WriteLine("已导出数据库架构");


        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }



    }



    ///// <summary>
    ///// 创建关键点。
    ///// </summary>
    ///// <param name="options"></param>
    ///// <returns></returns>
    //internal async Task CreateKeyPointAsync(CreateKeyPointOptions options)
    //{
    //    Console.WriteLine($"正在创建关键点：{options.LocationCode}");

    //    try
    //    {
    //        using (ITransaction tx = _session.BeginTransaction())
    //        {
    //            Location loc = _locationFactory.Invoke();
    //            loc.LocationCode = options.LocationCode;
    //            loc.LocationType = LocationTypes.K;
    //            loc.InboundLimit = options.InbountLimit;
    //            loc.OutboundLimit = options.OutbountLimit;
    //            loc.Tag = options.Tag;
    //            await _session.SaveAsync(loc).ConfigureAwait(false);
    //            await tx.CommitAsync().ConfigureAwait(false);
    //        }
    //        Console.WriteLine($"已创建关键点：{options.LocationCode}");
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine(ex);
    //    }

    //}

}

#endif

