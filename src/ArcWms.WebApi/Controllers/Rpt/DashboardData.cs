namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 仪表盘数据
/// </summary>
public record DashboardData
{
    /// <summary>
    /// 表示与日期关联的值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public record DateValuePair<T>
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; init; }

        /// <summary>
        /// 值
        /// </summary>
        public T Value { get; init; }
    }


    public record LocationData
    {
        /// <summary>
        /// 巷道总数
        /// </summary>
        public int StreetletCount { get; set; }

        /// <summary>
        /// 货位总数
        /// </summary>
        public int LocationCount { get; set; }

        /// <summary>
        /// 当前的货位利用率（有货货位/货位总数）
        /// </summary>
        public float LocationUsageRate { get; set; }

        /// <summary>
        /// 近7日（不含今日）每天的货位利用率（每天 23:50 采样数据）
        /// </summary>
        public List<DateValuePair<double>> LocationUsageRate7 { get; set; } = new List<DateValuePair<double>>();

        /// <summary>
        /// 可供入库的货位数量
        /// </summary>
        public int AvailableLocationCount { get; set; }

        /// <summary>
        /// 已禁入或禁出的货位数量
        /// </summary>
        public int DisabledLocationCount { get; set; }

    }

    public record InboundOrderData
    {
        /// <summary>
        /// 当前开启状态的入库单数量
        /// </summary>
        public int OpenInboundOrderCount { get; set; }

        /// <summary>
        /// 近7日（不含今日）每天创建的入库单数量
        /// </summary>
        public List<DateValuePair<int>> InboundOrderCount7 { get; set; } = new List<DateValuePair<int>>();
    }


    public record OutboundOrderData
    {
        /// <summary>
        /// 当前开启状态的出库单数量
        /// </summary>
        public int OpenOutboundOrderCount { get; set; }

        /// <summary>
        /// 近7日（不含今日）每天创建的出库单数量
        /// </summary>
        public List<DateValuePair<int>> OutboundOrderCount7 { get; set; } = new List<DateValuePair<int>>();
    }


    public record StockData
    {
        /// <summary>
        /// 库内现有物料个数
        /// </summary>
        public int MaterialCount { get; set; }


        /// <summary>
        /// 近7日（不含今日）每天产生的流入数量
        /// </summary>
        public List<DateValuePair<int>> FlowInCount7 { get; set; } = new List<DateValuePair<int>>();

        /// <summary>
        /// 近7日（含今日）每天产生的流出数量
        /// </summary>不
        public List<DateValuePair<int>> FlowOutCount7 { get; set; } = new List<DateValuePair<int>>();

        /// <summary>
        /// 当前货载数量
        /// </summary>
        public int UnitloadCount { get; set; }

        /// <summary>
        /// 当前库内的空托盘数量
        /// </summary>
        public int EmptyPalletCount { get; set; }
    }


    public record TaskData
    {
        /// <summary>
        /// 当前任务数量
        /// </summary>
        public int TaskCount { get; set; }

        /// <summary>
        /// 近7日（不含今日）每天下发的任务数量
        /// </summary>
        public List<DateValuePair<int>> TaskCount7 { get; set; } = new List<DateValuePair<int>>();
    }

    /// <summary>
    /// 查询时间
    /// </summary>
    public DateTime Time { get; set; } = DateTime.Now;


    /// <summary>
    /// 货位数据
    /// </summary>
    public LocationData Location { get; init; } = new LocationData();

    /// <summary>
    /// 入库单数据
    /// </summary>
    public InboundOrderData InboundOrder { get; init; } = new InboundOrderData();

    /// <summary>
    /// 出库单数据
    /// </summary>
    public OutboundOrderData OutboundOrder { get; init; } = new OutboundOrderData();

    /// <summary>
    /// 库存数据
    /// </summary>
    public StockData Stock { get; set; } = new StockData();

    /// <summary>
    /// 任务数据
    /// </summary>
    public TaskData Task { get; set; } = new TaskData();



}

