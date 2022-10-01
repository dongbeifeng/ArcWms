using ArcWms.WebApi.MetaData;
using ArcWms.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.International.Converters.PinYinConverter;
using NHibernate.Linq;
using NHibernateAspNetCoreFilters;
using NHibernateUtils;
using OperationTypeAspNetCoreAuthorization;
using System.Data;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 提供物料 api。
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MatlController : ControllerBase
{
    readonly ILogger<MatlController> _logger;
    readonly ISession _session;
    readonly Func<Material> _materialFactory;
    readonly FlowHelper _flowHelper;
    readonly PalletizationHelper _palletizationHelper;
    readonly IPalletCodeValidator _palletCodeValidator;
    readonly InventoryKeyType _inventoryKeyType;
    readonly MaterialOptions _materialOptions;

    public MatlController(
        ISession session,
        InventoryKeyType inventoryKeyType,
        Func<Material> materialFactory,
        FlowHelper flowHelper,
        PalletizationHelper palletizationHelper,
        IPalletCodeValidator palletCodeValidator,
        MaterialOptions materialOptions,
        ILogger<MatlController> logger)
    {
        _logger = logger;
        _inventoryKeyType = inventoryKeyType;
        _materialFactory = materialFactory;
        _flowHelper = flowHelper;
        _palletizationHelper = palletizationHelper;
        _session = session;
        _palletCodeValidator = palletCodeValidator;
        _materialOptions = materialOptions;
    }

    /// <summary>
    /// 获取物料列表。
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [Transaction]
    [HttpPost("get-material-list")]
    [OperationType(OperationTypes.查看物料)]
    public async Task<ListData<MaterialInfo>> GetMaterialList(MaterialListArgs args)
    {
        var pagedList = await _session.Query<Material>().SearchAsync(args, args.Sort, args.Current, args.PageSize);

        return this.ListData(pagedList, x => new MaterialInfo
        {
            MaterialId = x.MaterialId,
            MaterialCode = x.MaterialCode,
            MaterialType = x.MaterialType,
            Description = x.Description,
            Specification = x.Specification,
            BatchEnabled = x.BatchEnabled,
            MaterialGroup = x.MaterialGroup,
            ValidDays = x.ValidDays,
            StandingTime = x.StandingTime,
            AbcClass = x.AbcClass,
            Uom = x.Uom,
            LowerBound = x.LowerBound,
            UpperBound = x.UpperBound,
            DefaultQuantity = x.DefaultQuantity,
            DefaultStorageGroup = x.DefaultStorageGroup,
            Comment = x.Comment
        });
    }

    /// <summary>
    /// 获取物料的选项列表。
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [Transaction]
    [HttpPost("get-material-options")]
    public async Task<OptionsData<MaterialInfo>> GetMaterialOptions(MaterialOptionsArgs args)
    {
        IQueryable<Material> q = _session.Query<Material>();
        if (args.InStockOnly)
        {
            q = _session.Query<UnitloadItem>().Select(x => x.Material);
        }

        var keyword = args.Keyword?.Trim();
        var type = args.MaterialType?.Trim();

        if (string.IsNullOrEmpty(keyword) == false)
        {
            q = q.Where(x =>
                x.MaterialCode.Contains(keyword)
                || x.Description!.Contains(keyword)
                || (x.MnemonicCode != null && x.MnemonicCode.Contains(keyword))
            );
        }

        if (type != null)
        {
            q = q.Where(x => x.MaterialType == type);
        }

        var items = await q
            .Select(x => new MaterialInfo
            {
                MaterialId = x.MaterialId,
                MaterialCode = x.MaterialCode,
                Description = x.Description,
                Specification = x.Specification,
                MaterialType = x.MaterialType,
                BatchEnabled = x.BatchEnabled,
                Uom = x.Uom,
            })
            .OrderBy(x => x.MaterialCode)
            .Distinct()
            .Take(args.Limit ?? 10)
            .ToListAsync();

        return this.OptionsData(items);
    }

    /// <summary>
    /// 查找有库存的批号。
    /// </summary>
    /// <returns></returns>
    [Transaction]
    [HttpPost("get-batch-options")]
    public async Task<OptionsData<string>> GetBatchOptions(BatchOptionsArgs args)
    {
        var keyword = args?.Keyword?.Trim();
        var materialCode = args?.MaterialCode?.Trim();
        var inventoryStatus = args?.InventoryStatus?.Trim();

        var q = _session.Query<UnitloadItem>();
        if (keyword != null)
        {
            q = q.Where(x => x.Batch.Contains(keyword));
        }

        if (materialCode != null)
        {
            q = q.Where(x => x.Material.MaterialCode == materialCode);
        }

        if (inventoryStatus != null)
        {
            q = q.Where(x => x.InventoryStatus == inventoryStatus);
        }

        List<string> arr = await q
            .Select(x => x.Batch)
            .OrderBy(x => x)
            .Distinct()
            .Take(args?.Limit ?? 10)
            .ToListAsync();

        return this.OptionsData(arr);
    }

    /// <summary>
    /// 获取物料类型的列表。
    /// </summary>
    /// <returns></returns>
    [HttpPost("get-material-types")]
    public OptionsData<MaterialTypeInfo> GetMaterialTypes()
    {
        var result = this.OptionsData(_materialOptions.MaterialTypes.Select(x => new MaterialTypeInfo
        {
            MaterialType = x.Value,
            DisplayName = x.Value
        }).ToList());
        return result;
    }

    /// <summary>
    /// 导入物料主数据。
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [OperationType(OperationTypes.导入物料主数据)]
    [Transaction]
    [HttpPost("import-materials")]
    public async Task<ApiData> ImportMaterials(IFormFile file)
    {
        string[] arr = new[] { ".xlsx", ".xls" };
        if (arr.Contains(Path.GetExtension(file.FileName)?.ToLower()) == false)
        {
            throw new InvalidOperationException("无效的文件扩展名。");
        }


        string filename = await WriteFileAsync(file);

        DataTable dt = ExcelUtil.ReadDataSet(filename).Tables[0];

        int imported = 0;
        int covered = 0;
        int empty = 0;

        foreach (DataRow row in dt.Rows)
        {
            string? mcode = Convert.ToString(row["编码"]);
            if (string.IsNullOrWhiteSpace(mcode))
            {
                // 忽略空行
                empty++;
                continue;
            }
            Material material = await _session.Query<Material>().Where(x => x.MaterialCode == mcode).SingleOrDefaultAsync();
            if (material != null)
            {
                covered++;
                _logger.LogWarning("将覆盖已存在的物料 {material}", material.MaterialCode);
            }
            else
            {
                material = _materialFactory.Invoke();
                material.MaterialCode = Convert.ToString(row["编码"]);
            }

            material.Description = Convert.ToString(row["描述"]);
            material.BatchEnabled = Convert.ToBoolean(row["批次管理"]);
            material.StandingTime = 24;
            material.ValidDays = Convert.ToInt32(row["有效天数"]);
            material.MaterialType = Convert.ToString(row["物料类型"]);
            if (_materialOptions.MaterialTypes.Contains(new MaterialType(material.MaterialType)) == false)
            {
                throw new InvalidOperationException($"不支持的物料类型：{material.MaterialType}。");
            }
            material.Uom = Convert.ToString(row["计量单位"])?.ToUpper();
            material.DefaultQuantity = Convert.ToDecimal(row["每托数量"]);
            material.Specification = Convert.ToString(row["规格型号"]);
            string pinyin = GetPinyin(material.Description);
            if (pinyin.Length > 20)
            {
                pinyin = pinyin.Substring(0, 20);
            }
            material.MnemonicCode = pinyin;
            if (row.Table.Columns.Contains("存储分组"))
            {
                material.DefaultStorageGroup = Convert.ToString(row["存储分组"]) ?? "普通";
            }
            else
            {
                material.DefaultStorageGroup = "普通";
            }

            await _session.SaveOrUpdateAsync(material);
            _logger.LogInformation("已导入物料 {material}", material.MaterialCode);
            imported++;
        }

        _ = await this.SaveOpAsync($"导入 {imported}，覆盖 {covered}");

        return this.Success($"导入 {imported}，覆盖 {covered}");


        static async Task<string> WriteFileAsync(IFormFile file)
        {
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "Upload\\files");
            Directory.CreateDirectory(dir);

            string fileName = $"up-m-{DateTime.Now:yyyyMMddHHmmss}" + Path.GetExtension(file.FileName);
            var path = Path.Combine(dir, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return path;
        }

        static string GetPinyin(string? text)
        {
            if (text == null)
            {
                return string.Empty;
            }
            var charArray = text
                .Select(ch =>
                {
                    if (ChineseChar.IsValidChar(ch))
                    {
                        ChineseChar cc = new ChineseChar(ch);
                        if (cc.Pinyins.Count > 0 && cc.Pinyins[0].Length > 0)
                        {
                            return cc.Pinyins[0][0];
                        }
                    }

                    return ch;
                })
                .ToArray();
            return new string(charArray);
        }
    }



    /// <summary>
    /// 获取流水列表。
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [Transaction]
    [HttpPost("get-flow-list")]
    public async Task<ListData<FlowInfo>> GetFlowList(FlowListArgs args)
    {
        var pagedList = await _session.Query<Flow>().SearchAsync(args, args.Sort, args.Current, args.PageSize);

        return this.ListData(pagedList, x => new FlowInfo
        {
            FlowId = x.FlowId,
            CreationTime = x.CreationTime,
            MaterialCode = x.Material?.MaterialCode,
            MaterialType = x.Material?.MaterialType,
            Description = x.Material?.Description,
            Batch = x.Batch,
            InventoryStatus = x.InventoryStatus,
            BizType = x.BizType,
            Direction = x.Direction,
            PalletCode = x.PalletCode,
            OrderCode = x.OrderCode,
            BizOrder = x.BizOrder,
            OperationType = x.OperationType,
            Quantity = x.Quantity,
            Uom = x.Uom,
            CreationUser = x.CreationUser,
            Comment = x.Comment,
        });
    }


    /// <summary>
    /// 获取业务类型选择列表。
    /// </summary>
    /// <returns></returns>
    [HttpPost("get-biz-types")]
    public  OptionsData<BizTypeInfo> GetBizTypes(GetBizTypeArgs args)
    {
        List<BizTypeInfo> list = new List<BizTypeInfo>();

        switch (args.Scope)
        {
            case "Inbound":
                list = _materialOptions.BizTypes.OfType<InboundBizType>().Select(x => new BizTypeInfo
                {
                    BizType = x.Value,
                    DisplayName = x.Value,
                }).ToList();
                break;
            case "Outbound":
                list = _materialOptions.BizTypes.OfType<OutboundBizType>().Select(x => new BizTypeInfo
                {
                    BizType = x.Value,
                    DisplayName = x.Value,
                }).ToList();
                break;
            case "StatusChanging":
                list = _materialOptions.BizTypes.OfType<StatusChangingBizType>().Select(x => new BizTypeInfo
                {
                    BizType = x.Value,
                    DisplayName = x.Value,
                    IssuingStatus = x.IssuingStatus.Value,
                    ReceivingStatus = x.ReceivingStatus.Value,
                }).ToList();
                break;
        }

        var result = this.OptionsData(list);
        return result;
    }

    /// <summary>
    /// 获取库存状态列表。
    /// </summary>
    /// <returns></returns>
    [HttpPost("get-inventory-status")]
    public OptionsData<InventoryStatusInfo> GetInventoryStatus()
    {
        var result = this.OptionsData(_materialOptions.InventoryStatus.Select(x => new InventoryStatusInfo
        {
            InventoryStatus = x.Value,
            DisplayName = x.Value
        }).ToList());
        return result;
    }

    /// <summary>
    /// 验证托盘号。
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [Transaction]
    [HttpPost("validate-pallet-code")]
    public async Task<ApiData> ValidatePalletCode(ValidatePalletCodeArgs args)
    {
        ArgumentNullException.ThrowIfNull(args?.PalletCode);

        string palletCode = args.PalletCode;
        var b = _palletCodeValidator.IsValid(palletCode, out string msg);
        if (b == false)
        {
            return this.Failure(msg);
        }

        bool e = await _session.Query<Unitload>().AnyAsync(x => x.PalletCode == palletCode);
        if (e)
        {
            return this.Failure("托盘号已占用");
        }

        return this.Success();
    }

    /// <summary>
    /// 货载列表
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [Transaction]
    [HttpPost("get-unitload-list")]
    [OperationType(OperationTypes.查看货载)]
    public async Task<ListData<UnitloadInfo>> GetUnitloadList(UnitloadListArgs args)
    {
        var pagedList = await _session.Query<Unitload>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
        return this.ListData(pagedList, x => DtoConvert.ToUnitloadInfo(x));
    }

    /// <summary>
    /// 货载项列表，用于在变更状态页面展示货载项
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [Transaction]
    [HttpPost("get-unitload-items-to-change-inventory-status")]
    [OperationType(OperationTypes.查看货载)]
    public async Task<ListData<ChangeInventoryStatusUnitloadItemInfo>> GetUnitloadItemsToChangeInventoryStatusList(GetUnitloadItemsToChangeInventoryStatusArgs args)
    {
        var pagedList = await _session.Query<UnitloadItem>().SearchAsync(args, args.Sort, 1, int.MaxValue);

        return this.ListData(pagedList, x => new ChangeInventoryStatusUnitloadItemInfo
        {
            UnitloadItemId = x.UnitloadItemId,
            PalletCode = x.Unitload?.PalletCode,
            LocationCode = x.Unitload?.CurrentLocation?.LocationCode,
            StreetletCode = x.Unitload?.CurrentLocation?.Streetlet?.StreetletCode,
            HasTask = x.Unitload?.HasTask ?? default,
            MaterialId = x.Material?.MaterialId ?? default,
            MaterialCode = x.Material?.MaterialCode,
            MaterialType = x.Material?.MaterialType,
            Description = x.Material?.Description,
            Specification = x.Material?.Specification,
            Batch = x.Batch,
            InventoryStatus = x.InventoryStatus,
            Quantity = x.Quantity,
            Uom = x.Uom,
            Allocated = (x.Unitload?.CurrentUat != null),
            CanChangeInventoryStatus = CanChangeInventoryStatus(x).ok,
            ReasonWhyInventoryStatusCannotBeChanged = CanChangeInventoryStatus(x).reason,
        });
    }

    /// <summary>
    /// 获取货载详情。
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [Transaction]
    [HttpPost("get-unitload-detail")]
    [OperationType(OperationTypes.查看货载)]
    public async Task<ApiData<UnitloadDetail>> GetUnitloadDetail(GetUnitloadDetailArgs args)
    {
        var palletCode = args.PalletCode;
        var unitload = await _session.Query<Unitload>()
            .Where(x => x.PalletCode == palletCode)
            .SingleOrDefaultAsync();
        if (unitload == null)
        {
            throw new InvalidOperationException("货载不存在。");
        }
        var task = await _session
            .Query<TransportTask>()
            .SingleOrDefaultAsync(x => x.Unitload == unitload);
        return this.Success(DtoConvert.ToUnitloadDetail(unitload, task));
    }

    /// <summary>
    /// 独立组盘
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [Transaction]
    [HttpPost("palletize-standalonely")]
    [OperationType(OperationTypes.独立组盘)]
    public async Task<ApiData> PalletizeStandalonely(PalletizeStandalonelyArgs args)
    {
        List<PalletizationItemInfo> items = new List<PalletizationItemInfo>();

        Material material = await _session.Query<Material>().SingleOrDefaultAsync(x => x.MaterialCode == args.MaterialCode);
        if (material == null)
        {
            throw new InvalidOperationException($"物料主数据不存在：【{args.MaterialCode}】");
        }
        InventoryKey2 inventoryKey = new InventoryKey2(material, args.Batch, args.InventoryStatus, args.Uom);

        items.Add(new PalletizationItemInfo
        {
            InventoryKey = inventoryKey,
            Quantity = args.Quantity,
            Fifo = args.Batch  // TODO 根据项目需求调整
        });


        var op = await this.SaveOpAsync($"托盘号：{args.PalletCode}");

        await _palletizationHelper.PalletizeAsync(args.PalletCode, items);
        // TODO 这里有硬编码文本
        await _flowHelper.GenerateFlowAsync(inventoryKey, FlowDirection.Inbound, args.Quantity, SupportedBizTypes.独立入库, op.OperationType, args.PalletCode);

        return this.Success();
    }

    /// <summary>
    /// 更改库存状态。
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [Transaction]
    [HttpPost("change-inventory-status")]
    [OperationType(OperationTypes.更改库存状态)]
    public async Task<ApiData> ChangeInventoryStatus(ChangeInventoryStatusArgs args)
    {
        if (string.IsNullOrWhiteSpace(args.BizType))
        {
            throw new InvalidOperationException("未提供业务类型。");
        }

        List<UnitloadItem> unitloadItems = await _session.Query<UnitloadItem>()
            .Where(x => args.UnitloadItemIds.Contains(x.UnitloadItemId))
            .ToListAsync();

        StatusChangingBizType? bizType = _materialOptions.BizTypes
            .OfType<StatusChangingBizType>()
            .SingleOrDefault(x => x.Value == args.BizType);

        if (bizType == null)
        {
            throw new NotSupportedException($"不支持的业务类型：{args.BizType}。");
        }

        if (unitloadItems.Count == 0)
        {
            throw new InvalidOperationException("未选中任何货载项。");
        }

        foreach (UnitloadItem item in unitloadItems)
        {
            if (item.InventoryStatus != bizType.IssuingStatus.Value)
            {
                throw new InvalidOperationException("货载项的状态与发出状态不一致");
            }

            var (ok, reason) = CanChangeInventoryStatus(item);
            if (ok == false)
            {
                throw new InvalidOperationException(reason);
            }
        }

        var op = await this.SaveOpAsync("{0}-->{1}", bizType.IssuingStatus.Value, bizType.ReceivingStatus.Value);

        foreach (var item in unitloadItems)
        {
            // TODO 扩展点：替换泛型参数 DefaultInventoryKey
            // 1 生成发货流水
            Flow flowOut = await _flowHelper.GenerateFlowAsync(item.GetInventoryKey(_inventoryKeyType),
                                                               FlowDirection.Outbound,
                                                               item.Quantity,
                                                               bizType,
                                                               op.OperationType,
                                                               item.Unitload.PalletCode).ConfigureAwait(false);
            // 2 更改库存数据
            item.InventoryStatus = bizType.ReceivingStatus.Value;

            // 3 生成收货流水
            Flow flowIn = await _flowHelper.GenerateFlowAsync(item.GetInventoryKey(_inventoryKeyType),
                                                              FlowDirection.Inbound,
                                                              item.Quantity,
                                                              bizType,
                                                              op.OperationType,
                                                              item.Unitload.PalletCode).ConfigureAwait(false);

            await _session.UpdateAsync(item.Unitload).ConfigureAwait(false);
        }

        return this.Success();
    }

    internal static (bool ok, string reason) CanChangeInventoryStatus(UnitloadItem item)
    {
        if (item.Unitload == null)
        {
            throw new Exception("货载明细不属于任何货载");
        }

        List<string> list = new List<string>();

        if (item.Unitload.CurrentUat != null)
        {
            list.Add("已分配");
        }

        if (item.Unitload.HasTask)
        {
            list.Add("有任务");
        }

        if (item.Unitload.HasCountingError)
        {
            list.Add("有盘点错误");
        }

        if (!string.IsNullOrWhiteSpace(item.Unitload.OpHintType))
        {
            list.Add("有操作提示");
        }

        if (list.Count > 0)
        {
            return (false, string.Join(", ", list));
        }

        return (true, string.Empty);
    }
}

