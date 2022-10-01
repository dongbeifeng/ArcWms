using Arc.AppSeqs;
using ArcWms.WebApi.MetaData;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;
using NHibernateAspNetCoreFilters;
using NHibernateUtils;
using OperationTypeAspNetCoreAuthorization;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 提供出入库 api。
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OboController : ControllerBase
{
    readonly ISession _session;
    readonly ILogger<OboController> _logger;
    readonly IAppSeqService _appSeqService;
    readonly IOutboundOrderAllocator _outboundOrderAllocator;
    readonly OutboundOrderPickHelper _outboundOrderPickHelper;
    readonly FlowHelper _flowHelper;
    readonly MaterialOptions _materialOptions;

    /// <summary>
    /// 初始化新实例。
    /// </summary>
    /// <param name="session"></param>
    /// <param name="outboundOrderAllocator">出库单库存分配程序</param>
    /// <param name="outboundOrderPickHelper"></param>
    /// <param name="appSeqService"></param>
    /// <param name="flowHelper"></param>
    /// <param name="materialOptions"></param>
    /// <param name="logger"></param>
    public OboController(
        ISession session,
        IOutboundOrderAllocator outboundOrderAllocator,
        OutboundOrderPickHelper outboundOrderPickHelper,
        IAppSeqService appSeqService,
        FlowHelper flowHelper,
        MaterialOptions materialOptions,
        ILogger<OboController> logger
        )
    {
        _session = session;
        _outboundOrderAllocator = outboundOrderAllocator;
        _appSeqService = appSeqService;
        _flowHelper = flowHelper;
        _outboundOrderPickHelper = outboundOrderPickHelper;
        _materialOptions = materialOptions;
        _logger = logger;
    }

    /// <summary>
    /// 出库单列表
    /// </summary>
    /// <param name="args">查询参数</param>
    /// <returns></returns>
    [HttpPost("get-outbound-order-list")]
    [Transaction]
    [OperationType(OperationTypes.查看出库单)]
    public async Task<ListData<OutboundOrderInfo>> GetOutboundOrderList(OutboundOrderListArgs args)
    {
        var pagedList = await _session.Query<OutboundOrder>()
            .SearchAsync(args, args.Sort, args.Current, args.PageSize)
            .ConfigureAwait(false);
        return this.ListData(pagedList, x => new OutboundOrderInfo
        {
            OutboundOrderId = x.OutboundOrderId,
            OutboundOrderCode = x.OutboundOrderCode,
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
            Lines = x.Lines.Select(i => new OutboundLineInfo
            {
                OutboundLineId = i.OutboundLineId,
                MaterialId = i.Material.MaterialId,
                MaterialCode = i.Material.MaterialCode,
                MaterialType = i.Material.MaterialType,
                Description = i.Material.Description,
                Specification = i.Material.Specification,
                Batch = i.Batch,
                InventoryStatus = i.InventoryStatus,
                Uom = i.Uom,
                QuantityDemanded = i.QuantityDemanded,
                QuantityFulfilled = i.QuantityFulfilled,
                QuantityUnfulfilled = i.GetQuantityUnfulfilled(),
                Comment = i.Comment,
            }).ToList(),
            UnitloadCount = _session.Query<Unitload>().Where(u => u.CurrentUat == x).Count()
        });
    }

    /// <summary>
    /// 获取出库单详细信息。
    /// </summary>
    /// <param name="args">出库单Id</param>
    /// <returns></returns>
    [HttpPost("get-outbound-order-detail")]
    [Transaction]
    [OperationType(OperationTypes.查看出库单)]
    public async Task<ApiData<OutboundOrderInfo>> GetOutboundOrderDetail(GetOutboundOrderDetailArgs args)
    {
        ArgumentNullException.ThrowIfNull(args?.OutboundOrderId);
        var outboundOrder = await _session.GetAsync<OutboundOrder>(args.OutboundOrderId);
        return this.Success(new OutboundOrderInfo
        {
            OutboundOrderId = outboundOrder.OutboundOrderId,
            OutboundOrderCode = outboundOrder.OutboundOrderCode,
            CreationTime = outboundOrder.CreationTime,
            CreationUser = outboundOrder.CreationUser,
            ModificationTime = outboundOrder.ModificationTime,
            ModificationUser = outboundOrder.ModificationUser,
            BizType = outboundOrder.BizType,
            BizOrder = outboundOrder.BizOrder,
            Closed = outboundOrder.Closed,
            ClosedAt = outboundOrder.ClosedAt,
            ClosedBy = outboundOrder.ClosedBy,
            Comment = outboundOrder.Comment,
            Lines = outboundOrder.Lines.Select(i => new OutboundLineInfo
            {
                OutboundLineId = i.OutboundLineId,
                MaterialId = i.Material.MaterialId,
                MaterialCode = i.Material.MaterialCode,
                MaterialType = i.Material.MaterialType,
                Description = i.Material.Description,
                Specification = i.Material.Specification,
                Batch = i.Batch,
                InventoryStatus = i.InventoryStatus,
                Uom = i.Uom,
                QuantityDemanded = i.QuantityDemanded,
                QuantityFulfilled = i.QuantityFulfilled,
                QuantityUnfulfilled = i.GetQuantityUnfulfilled(),
                Comment = i.Comment,
            }).ToList(),
            UnitloadCount = _session.Query<Unitload>().Where(x => x.CurrentUat == outboundOrder).Count(),
        });
    }

    /// <summary>
    /// 获取分配给出库单的货载。
    /// </summary>
    /// <param name="args">查询参数。</param>
    /// <returns></returns>
    [HttpPost("get-allocated-unitloads")]
    [Transaction]
    [OperationType(OperationTypes.查看出库单)]
    public async Task<ApiData<UnitloadInfo[]>> GetAllocatedUnitloads(GetAllocatedUnitloadsArgs args)
    {
        ArgumentNullException.ThrowIfNull(args?.OutboundOrderId);
        var outboundOrder = await _session.GetAsync<OutboundOrder>(args.OutboundOrderId).ConfigureAwait(false);
        var unitloads = await _session.Query<Unitload>()
            .Where(x => x.CurrentUat == outboundOrder)
            .ToListAsync()
            .ConfigureAwait(false);
        return this.Success(unitloads
            .Select(x => DtoConvert.ToUnitloadInfo(x))
            .ToArray());
    }


    /// <summary>
    /// 创建出库单
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [HttpPost("create-outbound-order")]
    [OperationType(OperationTypes.创建出库单)]
    [Transaction]
    public async Task<ApiData> CreateOutboundOrder(CreateOutboundOrderArgs args)
    {
        OutboundBizType? bizType = _materialOptions.BizTypes
            .OfType<OutboundBizType>()
            .SingleOrDefault(x => x.Value == args.BizType);

        if (bizType == null)
        {
            throw new NotSupportedException($"不支持的业务类型：{args.BizType}。");
        }

        OutboundOrder outboundOrder = new OutboundOrder();

        string prefix = $"OBO{DateTime.Now:yyMMdd}";
        int next = await _appSeqService.GetNextAsync(prefix);
        outboundOrder.OutboundOrderCode = $"{prefix}{next:00000}";
        outboundOrder.BizType = args.BizType;
        outboundOrder.BizOrder = args.BizOrder;
        outboundOrder.Comment = args.Comment;

        if (args.Lines == null || args.Lines.Count == 0)
        {
            throw new InvalidOperationException("出库单应至少有一个出库行。");
        }

        foreach (var lineInfo in args.Lines)
        {
            OutboundLine line = new OutboundLine();
            var material = await _session.Query<Material>()
                .Where(x => x.MaterialCode == lineInfo.MaterialCode)
                .SingleOrDefaultAsync();
            if (material == null)
            {
                throw new InvalidOperationException($"未找到编码为 {lineInfo.MaterialCode} 的物料。");
            }
            line.Material = material;
            line.QuantityDemanded = lineInfo.QuantityDemanded;
            line.QuantityFulfilled = 0;
            line.Batch = lineInfo.Batch;
            line.InventoryStatus = lineInfo.InventoryStatus;
            line.Uom = lineInfo.Uom;

            outboundOrder.AddLine(line);
            _logger.LogInformation("已添加出库单明细，物料 {materialCode}，批号 {batch}，需求数量 {quantity}", line.Material.MaterialCode, line.Batch, line.QuantityDemanded);
        }

        await _session.SaveAsync(outboundOrder);
        _logger.LogInformation("已创建出库单 {outboundOrder}", outboundOrder);
        _ = await this.SaveOpAsync(outboundOrder.OutboundOrderCode);

        return this.Success();
    }


    /// <summary>
    /// 编辑出库单。
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [HttpPost("update-outbound-order")]
    [OperationType(OperationTypes.编辑出库单)]
    [Transaction]
    public async Task<ApiData> UpdateOutboundOrder(UpdateOutboundOrderArgs args)
    {
        ArgumentNullException.ThrowIfNull(args?.OutboundOrderId);
        OutboundOrder outboundOrder = _session.Get<OutboundOrder>(args.OutboundOrderId);
        if (outboundOrder == null)
        {
            String errMsg = String.Format("出库单不存在。", args.OutboundOrderId);
            throw new InvalidOperationException(errMsg);
        }

        if (outboundOrder.Closed)
        {
            String errMsg = String.Format("出库单已关闭，不能编辑。单号：{0}。", outboundOrder.OutboundOrderCode);
            throw new InvalidOperationException(errMsg);
        }

        var movingDown = await _session.Query<Outlet>().AnyAsync(x => x.CurrentUat == outboundOrder);
        if (movingDown)
        {
            String errMsg = String.Format("出库单正在下架，不能编辑。单号：{0}。", outboundOrder.OutboundOrderCode);
            throw new InvalidOperationException(errMsg);
        }

        outboundOrder.BizOrder = args.BizOrder;
        outboundOrder.Comment = args.Comment;

        if (args.Lines == null || args.Lines.Count == 0)
        {
            throw new InvalidOperationException("出库单应至少有一个出库行。");
        }

        foreach (var lineInfo in args.Lines)
        {
            switch (lineInfo.Op)
            {
                case "delete":
                    {
                        var line = outboundOrder.Lines.Single(x => x.OutboundLineId == lineInfo.OutboundLineId);
                        if (line.Dirty)
                        {
                            string errMsg = String.Format("已发生过出库操作的明细不能删除。出库行#{0}。", line.OutboundLineId);
                            throw new InvalidOperationException(errMsg);
                        }
                        outboundOrder.RemoveLine(line);
                        _logger.LogInformation("已删除出库单明细 {outboundLineId}", line.OutboundLineId);
                    }
                    break;
                case "edit":
                    {
                        var line = outboundOrder.Lines.Single(x => x.OutboundLineId == lineInfo.OutboundLineId);
                        line.QuantityDemanded = lineInfo.QuantityDemanded;
                        if (line.QuantityDemanded < line.QuantityFulfilled)
                        {
                            _logger.LogWarning("出库单明细 {outboundLineId} 的需求数量修改后小于已出数量", line.OutboundLineId);
                        }
                    }
                    break;
                case "add":
                    {
                        OutboundLine line = new OutboundLine();
                        var material = await _session.Query<Material>()
                            .Where(x => x.MaterialCode == lineInfo.MaterialCode)
                            .SingleOrDefaultAsync();
                        if (material == null)
                        {
                            throw new InvalidOperationException($"未找到物料。编码 {lineInfo.MaterialCode}。");
                        }
                        line.Material = material;
                        line.QuantityDemanded = lineInfo.QuantityDemanded;
                        line.QuantityFulfilled = 0;
                        line.Batch = lineInfo.Batch;
                        line.InventoryStatus = lineInfo.InventoryStatus;
                        line.Uom = lineInfo.Uom;

                        outboundOrder.AddLine(line);
                        _logger.LogInformation("已添加出库单明细，物料 {materialCode}，批号 {batch}，需求数量 {quantity}", line.Material.MaterialCode, line.Batch, line.QuantityDemanded);
                    }
                    break;
                default:
                    break;
            }
        }

        await _session.UpdateAsync(outboundOrder);
        _logger.LogInformation("已更新出库单 {outboundOrder}", outboundOrder);
        _ = await this.SaveOpAsync("{0}", outboundOrder);

        // TODO 
        //// 取消库内分配
        //_deliveryOrderAllocator.Value.CancelUnitLoadsOnRack(m);
        //foreach (var line in m.Lines)
        //{
        //    if (line.ComputeNumberAllocated() > line.NumberRequired)
        //    {
        //        String errMsg = String.Format("取消库内分配后，分配数量仍然大于应出数量，请处理完此出库单下所有路上和库外的货载再编辑。出库行#{0}。", line.Id);
        //        throw new InvalidOperationException(errMsg);
        //    }
        //}

        return this.Success();
    }


    /// <summary>
    /// 删除出库单
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [Transaction]
    [HttpPost("delete-outbound-order")]
    [OperationType(OperationTypes.删除出库单)]
    public async Task<ApiData> DeleteOutboundOrder(DeleteOutboundOrderArgs args)
    {
        ArgumentNullException.ThrowIfNull(args?.OutboundOrderId);
        OutboundOrder outboundOrder = await _session.GetAsync<OutboundOrder>(args.OutboundOrderId);
        if (outboundOrder.Lines.Any(x => x.Dirty))
        {
            throw new InvalidOperationException("出库单已发生过操作。");
        }

        await _session.DeleteAsync(outboundOrder);
        _logger.LogInformation("已删除出库单 {outboundOrder}", outboundOrder);
        await this.SaveOpAsync(outboundOrder.OutboundOrderCode);

        return this.Success();
    }

    /// <summary>
    /// 关闭出库单。
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [Transaction]
    [HttpPost("close-outbound-order")]
    [OperationType(OperationTypes.关闭出库单)]
    public async Task<ApiData> Close(CloseOutboundOrderArgs args)
    {
        ArgumentNullException.ThrowIfNull(args?.OutboundOrderId);
        OutboundOrder outboundOrder = await _session.GetAsync<OutboundOrder>(args.OutboundOrderId);

        if (outboundOrder == null)
        {
            throw new InvalidOperationException("出库单不存在。");
        }

        if (outboundOrder.Closed)
        {
            throw new InvalidOperationException($"出库单已关闭。");
        }

        // 关闭前检查
        await CheckOnCloseAsync(outboundOrder);

        outboundOrder.Closed = true;
        outboundOrder.ClosedAt = DateTime.Now;
        _session.Update(outboundOrder);

        await this.SaveOpAsync(outboundOrder.OutboundOrderCode);

        //  取消分配，以免关单后有未释放的货载
        foreach (var u in (await _session.Query<Unitload>()
                .Where(x => x.CurrentUat == outboundOrder)
                .ToListAsync().ConfigureAwait(false)
                ))
        {
            await _outboundOrderAllocator.DeallocateAsync(outboundOrder, u);
        }
        await _session.UpdateAsync(outboundOrder);

        _logger.LogInformation("已关闭出库单 {outboundOrder}", outboundOrder);

        return this.Success();
    }

    /// <summary>
    /// 为出库单分配库存。
    /// </summary>
    /// <param name="args">操作参数。</param>
    /// <returns></returns>
    [Transaction]
    [HttpPost("allocate-stock")]
    [OperationType(OperationTypes.分配库存)]
    public async Task<ApiData> Allocate(OutboundOrderAllocateStockArgs args)
    {
        OutboundOrder? outboundOrder = await _session.GetAsync<OutboundOrder>(args.OutboundOrderId);

        if (outboundOrder == null || outboundOrder.Closed)
        {
            throw new InvalidOperationException("出库单不存在或已关闭。");
        }

        await _outboundOrderAllocator.AllocateAsync(outboundOrder, args.Options);
        await this.SaveOpAsync("{0}: {1}", outboundOrder.OutboundOrderCode, args.Options);

        return this.Success();
    }


    /// <summary>
    /// 为出库单取消库内分配。
    /// </summary>
    /// <param name="args">操作参数。</param>
    /// <returns></returns>
    [Transaction]
    [HttpPost("deallocate-stock-in-rack")]
    [OperationType(OperationTypes.分配库存)]
    public async Task<ApiData> DeallocateInRack(OutboundOrderDeallocateStockArgs args)
    {
        ArgumentNullException.ThrowIfNull(args?.OutboundOrderId);
        OutboundOrder outboundOrder = await _session.GetAsync<OutboundOrder>(args.OutboundOrderId);

        if (outboundOrder == null || outboundOrder.Closed)
        {
            throw new InvalidOperationException("出库单不存在或已关闭。");
        }

        await _outboundOrderAllocator.DeallocateInRackAsync(outboundOrder);

        return this.Success();
    }

    /// <summary>
    /// 将托盘从出库单中取消分配
    /// </summary>
    /// <param name="args">取消参数</param>
    /// <returns></returns>
    [Transaction]
    [HttpPost("deallocate-pallets")]
    [OperationType(OperationTypes.分配库存)]
    public async Task<ApiData> Deallocate(OutboundOrderDeallocatePalletsArgs args)
    {
        if (args == null || args.OutboundOrderId == null || args.PalletCodes == null || args.PalletCodes.Length == 0)
        {
            throw new InvalidOperationException("未指定要取消分配的托盘。");
        }
        OutboundOrder outboundOrder = await _session.GetAsync<OutboundOrder>(args.OutboundOrderId);

        if (outboundOrder == null || outboundOrder.Closed)
        {
            throw new InvalidOperationException("出库单不存在或已关闭。");
        }
        List<Unitload> unitloads = await _session
            .Query<Unitload>()
            .Where(x => args.PalletCodes.Contains(x.PalletCode))
            .ToListAsync();
        foreach (var u in unitloads)
        {
            await _outboundOrderAllocator.DeallocateAsync(outboundOrder, u);
        }

        return this.Success();
    }

    /// <summary>
    /// 出库单下架
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [Transaction]
    [OperationType(OperationTypes.出库单下架)]
    [HttpPost("attach-to-outlets")]
    public async Task<ApiData> AttachToOutlets(OutboundOrderAttachToOutletsArgs args)
    {
        OutboundOrder obo = _session.Get<OutboundOrder>(args.OutboundOrderId);
        _logger.LogDebug("正在将出库单附加到出口");
        _logger.LogDebug("出库单 Id 是 {outboundOrderId}", args.OutboundOrderId);
        _logger.LogDebug("出货口是 {outlets}", args.Outlets);

        if (obo == null || obo.Closed)
        {
            string errmsg = string.Format("出库单不存在，或已关闭，#{0}。", args.OutboundOrderId);
            throw new InvalidOperationException(errmsg);
        }

        _logger.LogInformation("出库单单号 {outboundOrderCode}", obo.OutboundOrderCode);
        var count = await _session.Query<Unitload>()
            .Where(x => x.CurrentUat == obo && x.CurrentLocation.LocationType == LocationTypes.S)
            .CountAsync()
            .ConfigureAwait(false);
        if (count == 0)
        {
            _logger.LogWarning("出库单 {outboundOrderCode} 在货架上没有货载", obo.OutboundOrderCode);
            return this.Failure($"出库单 {obo.OutboundOrderCode} 在货架上没有货载");
        }

        if (args.Outlets == null)
        {
            args.Outlets = new string[0];
        }

        var arr = _session.Query<Outlet>().Where(x => args.Outlets.Contains(x.OutletCode)).ToArray();
        var prev = _session.Query<Outlet>().Where(x => x.CurrentUat == obo).ToArray();

        // 移除页面上没有指定的
        var deleted = prev.Where(x => arr.Contains(x) == false);
        foreach (var outlet in deleted)
        {
            _logger.LogDebug("将 {outboundOrderCode} 从 {outlet} 移除", obo.OutboundOrderCode, outlet.OutletCode);
            outlet.ResetCurrentUat();
        }

        // 添加页面上新增的
        var added = arr.Except(prev);
        foreach (var outlet in added)
        {
            outlet.SetCurrentUat(obo);
            _logger.LogDebug("将 {outboundOrderCode} 附加到 {outlet}", obo.OutboundOrderCode, outlet.OutletCode);
        }

        string str = string.Join(", ", _session.Query<Outlet>().Where(x => x.CurrentUat == obo).Select(x => x.OutletCode));
        await this.SaveOpAsync("{0}@{1}", obo.OutboundOrderCode, str);

        return this.Success();
    }


    /// <summary>
    /// 对出库单进行关闭前检查。
    /// </summary>
    /// <param name="outboundOrder"></param>
    private async Task CheckOnCloseAsync(OutboundOrder outboundOrder)
    {
        // 应始终检查出库单是否挂在出货口上
        var ports = await _session.Query<Outlet>().Where(x => x.CurrentUat == outboundOrder).Select(x => x.OutletCode).ToListAsync();
        if (ports.Count() > 0)
        {
            string str = string.Join(", ", ports.Select(x => x));
            string msg = string.Format("出库单正在下架。在出货口 {1}。", outboundOrder.OutboundOrderCode, str);
            throw new InvalidOperationException(msg);
        }

        // 出库单下有分配的货载时不允许关闭，否则，货载将无法释放。
        if (_session.Query<Unitload>().Where(x => x.CurrentUat == outboundOrder).Count() > 0)
        {
            string msg = string.Format("出库单下有分配的库存。", outboundOrder.OutboundOrderCode);
            throw new InvalidOperationException(msg);
        }

        // 应始终检查是否有移动的货载
        if (_session.Query<Unitload>().Where(x => x.CurrentUat == outboundOrder).Any(x => x.HasTask))
        {
            string msg = string.Format("出库单下有任务。", outboundOrder.OutboundOrderCode);
            throw new InvalidOperationException(msg);
        }


        //// TODO 考虑是否在库外有托盘时禁止取消
        //if (outboundOrder.UnitloadsAllocated.Any(x => x.InRack() == false))
        //{
        //    string msg = string.Format("出库单下有任务。", outboundOrder.OutboundOrderCode);
        //    throw new InvalidOperationException(msg);
        //}
    }

    /// <summary>
    /// 获取可用库存数量
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [Transaction]
    [HttpPost("get-available-quantity")]
    public async Task<ActionResult<ApiData<decimal>>> GetAvailableQuantity(GetAvailableQuantityArgs args)
    {
        string? material = args?.MaterialCode?.Trim();
        string? inventoryStatus = args?.InventoryStatus?.Trim();
        string? batch = args?.Batch?.Trim();
        string? outboundOrderCode = args?.OutboundOrderCode?.Trim();

        var q = _session.Query<UnitloadItem>()
            .Where(x => x.Unitload.OpHintType == null
                && x.Unitload.HasCountingError == false
                && x.Unitload.HasTask == false
            );

        if (material != null)
        {
            q = q.Where(x => x.Material.MaterialCode == material);
        }

        if (inventoryStatus != null)
        {
            q = q.Where(x => x.InventoryStatus == inventoryStatus);
        }

        if (batch != null)
        {
            q = q.Where(x => x.Batch == batch);
        }

        if (outboundOrderCode == null)
        {
            q = q.Where(x => x.Unitload.CurrentUat == null);
        }
        else
        {
            OutboundOrder o = await _session.Query<OutboundOrder>().Where(x => x.OutboundOrderCode == outboundOrderCode).SingleOrDefaultAsync().ConfigureAwait(false);
            if (o == null)
            {
                q = q.Where(x => x.Unitload.CurrentUat == null);
            }
            else
            {
                q = q.Where(x => x.Unitload.CurrentUat == null || x.Unitload.CurrentUat == o);
            }
        }

        var sum = q.Sum(x => (decimal?)x.Quantity) ?? 0m;

        string str = sum.ToString("0.###");
        return Content(str);
    }


    /// <summary>
    /// 从托盘中为出库单拣货
    /// </summary>
    /// <returns></returns>
    [Transaction]
    [HttpPost("pick")]
    [OperationType(OperationTypes.拣货)]
    public async Task<ApiData> Pick(OutboundOrderPickArgs args)
    {
        var palletCode = args.PalletCode;
        var pickInfos = args.PickInfos;

        if (palletCode == null)
        {
            throw new ArgumentNullException(nameof(palletCode));
        }
        Unitload? unitload = await _session.Query<Unitload>().SingleOrDefaultAsync(x => x.PalletCode == palletCode);
        if (unitload == null)
        {
            throw new InvalidOperationException($"货载不存在：{palletCode}。");
        }

        var outboundOrder = unitload.CurrentUat as OutboundOrder;
        if (outboundOrder == null)
        {
            string msg = string.Format("货载未分配给出库单。");
            throw new InvalidOperationException(msg);
        }

        OutboundBizType? bizType = _materialOptions.BizTypes
            .OfType<OutboundBizType>()
            .SingleOrDefault(x => x.Value == outboundOrder.BizType);

        if (bizType == null)
        {
            throw new NotSupportedException($"不支持的业务类型：{outboundOrder.BizType}。");
        }


        var op = await this.SaveOpAsync("{0}: {1}", palletCode, pickInfos);
        foreach (var pickInfo in pickInfos)
        {
            var inventoryKey = await _outboundOrderPickHelper.PickAsync(unitload, outboundOrder, pickInfo);

            if (inventoryKey != null)
            {
                // 生成流水记录
                Flow flow = await _flowHelper.GenerateFlowAsync(
                    inventoryKey,
                    FlowDirection.Outbound,
                    pickInfo.QuantityPicked,
                    bizType,
                    op.OperationType,
                    unitload.PalletCode,
                    outboundOrder.OutboundOrderCode,
                    outboundOrder.BizOrder
                    );
            }

        }
        return this.Success();
    }

}

