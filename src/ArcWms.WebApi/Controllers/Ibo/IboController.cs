using Arc.AppSeqs;
using ArcWms.WebApi.MetaData;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
using NHibernateAspNetCoreFilters;
using NHibernateUtils;
using OperationTypeAspNetCoreAuthorization;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 操作入库单。
/// </summary>
[ApiController]
[Route("api/ibo")]
public class IboController : ControllerBase
{
    readonly ISession _session;
    readonly ILogger<IboController> _logger;
    readonly FlowHelper _flowHelper;
    readonly IAppSeqService _appSeqService;
    readonly PalletizationHelper _palletizationHelper;
    readonly IBatchService _batchService;
    readonly InventoryKeyType _inventoryKeyType;
    readonly MaterialOptions _materialOptions;

    /// <summary>
    /// 初始化新实例。
    /// </summary>
    /// <param name="session"></param>
    /// <param name="inventoryKeyType"></param>
    /// <param name="appSeqService"></param>
    /// <param name="batchService"></param>
    /// <param name="palletizationHelper"></param>
    /// <param name="flowHelper"></param>
    /// <param name="opHelper"></param>
    /// <param name="materialOptions"></param>
    /// <param name="logger"></param>
    public IboController(
        ISession session,
        InventoryKeyType inventoryKeyType,
        IAppSeqService appSeqService,
        IBatchService batchService,
        PalletizationHelper palletizationHelper,
        FlowHelper flowHelper,
        MaterialOptions materialOptions,
        ILogger<IboController> logger)
    {
        _session = session;
        _inventoryKeyType = inventoryKeyType;
        _appSeqService = appSeqService;
        _flowHelper = flowHelper;
        _logger = logger;
        _palletizationHelper = palletizationHelper;
        _batchService = batchService;
        _materialOptions = materialOptions;
    }

    /// <summary>
    /// 获取入库单列表。
    /// </summary>
    /// <param name="args">查询参数</param>
    /// <returns></returns>
    [HttpPost("get-inbound-order-list")]
    [Transaction]
    [OperationType(OperationTypes.查看入库单)]
    public async Task<ListData<InboundOrderInfo>> GetInboundOrderList(InboundOrderListArgs args)
    {
        var pagedList = await _session.Query<InboundOrder>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
        return this.ListData(pagedList, x => new InboundOrderInfo
        {
            InboundOrderId = x.InboundOrderId,
            InboundOrderCode = x.InboundOrderCode,
            CreationTime = x.CreationTime,
            CreationUser = x.CreationUser,
            ModificationTime = x.ModificationTime,
            ModificationUser = x.ModificationUser,
            BizType = x.BizType,
            BizOrder = x.BizOrder,
            Closed = x.Closed,
            ClosedAt = x.ClosedAt,
            ClosedBy = x.ClosedBy,
            Comment = x.Comment,
            Lines = x.Lines.Select(i => new InboundLineInfo
            {
                InboundLineId = i.InboundLineId,
                MaterialId = i.Material.MaterialId,
                MaterialCode = i.Material.MaterialCode,
                MaterialType = i.Material.MaterialType,
                Description = i.Material.Description,
                Specification = i.Material.Specification,
                Batch = i.Batch,
                InventoryStatus = i.InventoryStatus,
                Uom = i.Uom,
                QuantityExpected = i.QuantityExpected,
                QuantityReceived = i.QuantityReceived,
                Comment = i.Comment,
            }).ToList(),
        });
    }


    /// <summary>
    /// 获取入库单详细信息。
    /// </summary>
    /// <param name="args">查询参数。</param>
    /// <returns></returns>
    [HttpPost("get-inbound-order-detail")]
    [Transaction]
    [OperationType(OperationTypes.查看入库单)]
    public async Task<ApiData<InboundOrderInfo>> GetInboundOrderDetail(GetInboundOrderDetailArgs args)
    {
        ArgumentNullException.ThrowIfNull(args?.InboundOrderId);

        var inboundOrder = await _session.GetAsync<InboundOrder>(args.InboundOrderId);
        return this.Success(new InboundOrderInfo
        {
            InboundOrderId = inboundOrder.InboundOrderId,
            InboundOrderCode = inboundOrder.InboundOrderCode,
            CreationTime = inboundOrder.CreationTime,
            CreationUser = inboundOrder.CreationUser,
            ModificationTime = inboundOrder.ModificationTime,
            ModificationUser = inboundOrder.ModificationUser,
            BizType = inboundOrder.BizType,
            BizOrder = inboundOrder.BizOrder,
            Closed = inboundOrder.Closed,
            ClosedAt = inboundOrder.ClosedAt,
            ClosedBy = inboundOrder.ClosedBy,
            Comment = inboundOrder.Comment,
            Lines = inboundOrder.Lines.Select(i => new InboundLineInfo
            {
                InboundLineId = i.InboundLineId,
                MaterialId = i.Material.MaterialId,
                MaterialCode = i.Material.MaterialCode,
                MaterialType = i.Material.MaterialType,
                Description = i.Material.Description,
                Specification = i.Material.Specification,
                Batch = i.Batch,
                InventoryStatus = i.InventoryStatus,
                Uom = i.Uom,
                QuantityExpected = i.QuantityExpected,
                QuantityReceived = i.QuantityReceived,
                Comment = i.Comment,
            }).ToList(),
        });
    }


    /// <summary>
    /// 创建入库单
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [HttpPost("create-inbound-order")]
    [OperationType(OperationTypes.创建入库单)]
    [Transaction]
    public async Task<ApiData> CreateInboundOrder(CreateInboundOrderArgs args)
    {
        InboundBizType? bizType = _materialOptions.BizTypes
            .OfType<InboundBizType>()
            .SingleOrDefault(x => x.Value == args.BizType);

        if (bizType == null)
        {
            throw new NotSupportedException($"不支持的业务类型：{args.BizType}。");
        }


        string prefix = $"IBO{DateTime.Now:yyMMdd}";
        int next = await _appSeqService.GetNextAsync(prefix);
        var inboundOrderCode = $"{prefix}{next:00000}";
        InboundOrder inboundOrder = new InboundOrder();
        inboundOrder.InboundOrderCode = inboundOrderCode;
        inboundOrder.BizType = args.BizType;
        inboundOrder.BizOrder = args.BizOrder;
        inboundOrder.Comment = args.Comment;

        if (args.Lines == null || args.Lines.Count == 0)
        {
            throw new InvalidOperationException("入库单应至少有一个入库行。");
        }

        foreach (var lineInfo in args.Lines)
        {
            InboundLine line = new InboundLine();
            var material = await _session.Query<Material>()
                .Where(x => x.MaterialCode == lineInfo.MaterialCode)
                .SingleOrDefaultAsync();
            if (material == null)
            {
                throw new InvalidOperationException($"未找到编码为 {lineInfo.MaterialCode} 的物料。");
            }
            line.Material = material;
            line.QuantityExpected = lineInfo.QuantityExpected;
            line.QuantityReceived = 0;
            line.Batch = _batchService.Normalize(lineInfo.Batch);
            line.InventoryStatus = lineInfo.InventoryStatus;
            line.Uom = lineInfo.Uom;

            inboundOrder.AddLine(line);
            _logger.LogInformation("已添加入库单明细，物料 {materialCode}，批号 {batch}，应入数量 {quantity}", line.Material.MaterialCode, line.Batch, line.QuantityReceived);
        }

        await _session.SaveAsync(inboundOrder);
        _logger.LogInformation("已创建入库单 {inboundOrder}", inboundOrder);
        _ = await this.SaveOpAsync(inboundOrder.InboundOrderCode);

        return this.Success();
    }


    /// <summary>
    /// 编辑入库单
    /// </summary>
    /// <param name="args">操作参数</param>
    /// <returns></returns>
    [HttpPost("update-inbound-order")]
    [OperationType(OperationTypes.编辑入库单)]
    [Transaction]
    public async Task<ApiData> UpdateInboundOrder(UpdateInboundOrderArgs args)
    {
        ArgumentNullException.ThrowIfNull(args?.InboundOrderId);

        InboundOrder inboundOrder = _session.Get<InboundOrder>(args.InboundOrderId);
        if (inboundOrder == null)
        {
            String errMsg = String.Format("入库单不存在。Id 是 {0}。", args.InboundOrderId);
            throw new InvalidOperationException(errMsg);
        }

        if (inboundOrder.Closed)
        {
            String errMsg = String.Format("入库单已关闭，不能编辑。单号：{0}。", inboundOrder.InboundOrderCode);
            throw new InvalidOperationException(errMsg);
        }

        inboundOrder.BizOrder = args.BizOrder;
        inboundOrder.Comment = args.Comment;

        if (args.Lines == null || args.Lines.Count == 0)
        {
            throw new InvalidOperationException("入库单应至少有一个入库行。");
        }

        foreach (var lineInfo in args.Lines)
        {
            switch (lineInfo.Op)
            {
                case "delete":
                    {
                        var line = inboundOrder.Lines.Single(x => x.InboundLineId == lineInfo.InboundLineId);
                        if (line.Dirty)
                        {
                            string errMsg = string.Format("已发生过入库操作的明细不能删除。入库行#{0}。", line.InboundLineId);
                            throw new InvalidOperationException(errMsg);
                        }
                        inboundOrder.RemoveLine(line);
                        _logger.LogInformation("已删除入库单明细 {inboundLineId}", line.InboundLineId);
                    }
                    break;
                case "edit":
                    {
                        var line = inboundOrder.Lines.Single(x => x.InboundLineId == lineInfo.InboundLineId);
                        line.QuantityExpected = lineInfo.QuantityExpected;
                        if (line.QuantityReceived < line.QuantityReceived)
                        {
                            _logger.LogWarning("入库单明细 {inboundLineId} 的应入数量修改后小于已入数量", line.InboundLineId);
                        }
                    }
                    break;
                case "add":
                    {
                        InboundLine line = new InboundLine();
                        var material = await _session.Query<Material>()
                            .Where(x => x.MaterialCode == lineInfo.MaterialCode)
                            .SingleOrDefaultAsync();
                        if (material == null)
                        {
                            throw new InvalidOperationException($"未找到物料。编码 {lineInfo.MaterialCode}。");
                        }
                        line.Material = material;
                        line.QuantityExpected = lineInfo.QuantityExpected;
                        line.QuantityReceived = 0;
                        line.Batch = _batchService.Normalize(lineInfo.Batch);
                        line.InventoryStatus = lineInfo.InventoryStatus;
                        line.Uom = lineInfo.Uom;

                        inboundOrder.AddLine(line);
                        _logger.LogInformation("已添加入库单明细，物料 {materialCode}，批号 {batch}，应入数量 {quantity}", line.Material.MaterialCode, line.Batch, line.QuantityReceived);
                    }
                    break;
                default:
                    break;
            }
        }

        await _session.UpdateAsync(inboundOrder);
        _logger.LogInformation("已更新入库单 {inboundOrder}", inboundOrder);
        _ = await this.SaveOpAsync("{0}", inboundOrder);

        return this.Success();
    }


    /// <summary>
    /// 删除入库单。
    /// </summary>
    /// <param name="args">操作参数。</param>
    /// <returns></returns>
    [Transaction]
    [HttpPost("delete-inbound-order")]
    [OperationType(OperationTypes.删除入库单)]
    public async Task<ApiData> DeleteInboundOrder(DeleteInboundOrderArgs args)
    {
        ArgumentNullException.ThrowIfNull(args?.InboundOrderId);
        InboundOrder inboundOrder = await _session.GetAsync<InboundOrder>(args.InboundOrderId);
        if (inboundOrder.Lines.Any(x => x.Dirty))
        {
            throw new InvalidOperationException("入库单已发生过操作。");
        }

        await _session.DeleteAsync(inboundOrder);
        _logger.LogInformation("已删除入库单 {inboundOrder}", inboundOrder);
        await this.SaveOpAsync(inboundOrder.InboundOrderCode);

        return this.Success();
    }

    /// <summary>
    /// 关闭入库单。
    /// </summary>
    /// <param name="args">操作参数。</param>
    /// <returns></returns>
    [Transaction]
    [HttpPost("close-inbound-order")]
    [OperationType(OperationTypes.关闭入库单)]
    public async Task<ApiData> Close(CloseInboundOrderArgs args)
    {
        ArgumentNullException.ThrowIfNull(args?.InboundOrderId);
        InboundOrder inboundOrder = await _session.GetAsync<InboundOrder>(args?.InboundOrderId);

        if (inboundOrder == null)
        {
            throw new Exception("入库单不存在。");
        }

        if (inboundOrder.Closed)
        {
            throw new InvalidOperationException($"入库单已关闭。{inboundOrder.InboundOrderCode}");
        }

        inboundOrder.Closed = true;
        inboundOrder.ClosedAt = DateTime.Now;
        _session.Update(inboundOrder);

        await this.SaveOpAsync(inboundOrder.InboundOrderCode);

        await _session.UpdateAsync(inboundOrder);

        _logger.LogInformation("已关闭入库单 {inboundOrder}", inboundOrder);

        return this.Success();
    }



    /// <summary>
    /// 入库单组盘
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [Transaction]
    [HttpPost("palletize")]
    [OperationType(OperationTypes.入库单组盘)]
    public async Task<ApiData> Palletize(IboPalletizeArgs args)
    {
        List<PalletizationItemInfo> items = new List<PalletizationItemInfo>();

        InboundOrder inboundOrder = await _session.Query<InboundOrder>().Where(x => x.InboundOrderCode == args.InboundOrderCode).SingleOrDefaultAsync();
        if (inboundOrder == null)
        {
            throw new InvalidOperationException($"入库单不存在：【{args.InboundOrderCode}】");
        }

        if (inboundOrder.Closed)
        {
            throw new InvalidOperationException($"入库单已关闭：【{inboundOrder.InboundOrderCode}】");
        }
        Material material = await _session.Query<Material>().SingleOrDefaultAsync(x => x.MaterialCode == args.MaterialCode);
        if (material == null)
        {
            throw new InvalidOperationException($"物料主数据不存在：【{args.MaterialCode}】");
        }

        var lines = inboundOrder.Lines.Where(x =>
                string.Equals(x.Material.MaterialCode, args.MaterialCode, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(x.Batch, args.Batch, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(x.InventoryStatus, args.InventoryStatus, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(x.Uom, args.Uom, StringComparison.InvariantCultureIgnoreCase))
            .ToArray();
        if (lines.Length == 0)
        {
            throw new InvalidOperationException("无法找到匹配的入库单明细");
        }

        _logger.LogDebug("找到 {count} 个匹配的入库单明细", lines.Length);
        foreach (var line in lines)
        {
            _logger.LogDebug("InboundLineId: {inboundLineId}", line.InboundLineId);
        }

        InventoryKey inventoryKey = lines[0].GetInventoryKey(_inventoryKeyType);
        items.Add(new PalletizationItemInfo 
        {
            InventoryKey = inventoryKey, 
            Quantity = args.Quantity,
            Fifo = args.Batch  // TODO 根据项目需求调整
        });

        var op = await this.SaveOpAsync($"托盘号：{args.PalletCode}");

        InboundBizType? bizType = _materialOptions.BizTypes
            .OfType<InboundBizType>()
            .SingleOrDefault(x => x.Value == inboundOrder.BizType);

        if (bizType == null)
        {
            throw new NotSupportedException($"不支持的业务类型：{inboundOrder.BizType}。");
        }        

        await _palletizationHelper.PalletizeAsync(args.PalletCode, items);
        await _flowHelper.GenerateFlowAsync(
            inventoryKey,
            FlowDirection.Inbound,
            args.Quantity,
            bizType,
            op.OperationType,
            args.PalletCode,
            inboundOrder.InboundOrderCode,
            inboundOrder.BizOrder);


        decimal qty = args.Quantity;
        _logger.LogDebug("注册数量：{qty}", qty);
        foreach (var line in lines)
        {
            decimal shortage = Math.Max(line.QuantityExpected - line.QuantityReceived, 0);
            _logger.LogDebug("入库单明细 {inboundLineId} 欠数 {shortage}", line.InboundLineId, shortage);
            decimal inc = Math.Min(shortage, qty);
            _logger.LogDebug("分配给 {inboundLineId} 的收货数量：{qty}", line.InboundLineId, inc);
            line.QuantityReceived += inc;
            qty -= inc;

            if (qty == 0)
            {
                break;
            }
        }

        if (qty > 0)
        {
            var line = lines.Last();
            line.QuantityReceived += qty;
            _logger.LogDebug("超收数量 {qty} 分配给最后一个入库单明细 {inboundLineId}", qty, line.InboundLineId);
        }

        await _session.LockAsync(inboundOrder, LockMode.Upgrade);

        return this.Success();
    }
}

