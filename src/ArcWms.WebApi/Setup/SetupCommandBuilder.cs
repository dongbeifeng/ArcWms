#if SETUP

using System.CommandLine;

// setup show-connectionstring
// setup export-schema
// setup generate-streelet --streetlet-code S1 --area 第一区 --double-deep --left-rack-2 01 --left-rack-1 02 --left-bays 20 --left-levels 5 --right-rack-1 03 --right-rack-2 04 --right-bays 20 --right-levels 5
// setup create-keypoint --location-code 1001 --inbound-limit 3 --outbound-limit 3 --tag 入口

public class SetupCommandBuilder
{
    private readonly SetupHelper _setupHelper;
    private readonly RootCommand _rootCommand;
    public SetupCommandBuilder(SetupHelper setupHelper)
    {
        this._setupHelper = setupHelper;
        _rootCommand = new RootCommand("ArcWms 安装命令行");
    }

    void AddGenerateStreetletCommand()
    {
        Option<string> streetletCodeOption = new Option<string>("--streetlet-code", "指定巷道编码。例如：S1。") { IsRequired = true };
        Option<bool> doubleDeepOption = new Option<bool>("--double-deep", "指定是否双深巷道。");
        Option<string> areaOption = new Option<string>("--area", "指定库区。例如：第一区。") { IsRequired = true };

        Option<string> leftRack2Option = new Option<string>("--left-rack-2", "指定左侧 2 深货架的编码。例如：01。");
        Option<string> leftRack1Option = new Option<string>("--left-rack-1", "指定左侧 1 深货架的编码。例如：02。");
        Option<int> leftBaysOption = new Option<int>("--left-bays", "指定左侧货架的列数。例如：20。");
        Option<int> leftLevelsOption = new Option<int>("--left-levels", "指定左侧货架的层数。例如：5。");

        Option<string> rightRack1Option = new Option<string>("--right-rack-1", "指定右侧 1 深货架的编码。例如：03");
        Option<string> rightRack2Option = new Option<string>("--right-rack-2", "指定右侧 2 深货架的编码。例如：04");
        Option<int> rightBaysOption = new Option<int>("--right-bays", "指定右侧货架的列数。例如：20。");
        Option<int> rightLevelsOption = new Option<int>("--right-levels", "指定右侧货架的层数。例如：5。");

        var generateStreetletCommand = new Command("generate-streetlet", "生成巷道数据。")
        {
            streetletCodeOption, areaOption, doubleDeepOption,
            leftRack2Option, leftRack1Option, leftBaysOption, leftLevelsOption,
            rightRack1Option, rightRack2Option, rightBaysOption, rightLevelsOption,
        };
        _rootCommand.AddCommand(generateStreetletCommand);
        generateStreetletCommand.SetHandler<
            string, bool, string,
            string, string, int, int,
            string, string, int, int,
            CancellationToken
            >(_setupHelper.GenerateStreetletAsync, 
            streetletCodeOption, doubleDeepOption, areaOption, 
            leftRack2Option, leftRack1Option, leftBaysOption, leftLevelsOption, 
            rightRack1Option, rightRack2Option, rightBaysOption, rightLevelsOption);
    }


    void AddShowConnectionstringCommand()
    {
        var showConnectionstringCommand = new Command("show-connectionstring", "显示连接字符串。");
        _rootCommand.AddCommand(showConnectionstringCommand);
        showConnectionstringCommand.SetHandler(_setupHelper.ShowConnectionStringAsync);

    }

    void AddExportSchemaCommand()
    {
        var exportSchemaCommand = new Command("export-schema", "创建数据库架构，请小心操作，避免覆盖生产数据。");
        _rootCommand.AddCommand(exportSchemaCommand);
        exportSchemaCommand.SetHandler(_setupHelper.ExportSchemaAsync);
    }

    public RootCommand Build()
    {
        AddShowConnectionstringCommand();
        AddExportSchemaCommand();
        AddGenerateStreetletCommand();
        return _rootCommand;
    }
}

#endif

