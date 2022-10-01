using NPOI.SS.Formula.Constant;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;

namespace ArcWms.WebApi;

/// <summary>
/// 使用 NPOI 读取 Excel 数据
/// </summary>
public static class ExcelUtil
{
    /// <summary>
    /// 读取 xlsx 文件，转换为 DataSet
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static DataSet ReadDataSet(string filePath)
    {
        DataSet dataSet = new DataSet(Path.GetFileNameWithoutExtension(filePath));

        XSSFWorkbook xssfWorkbook;

        using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            xssfWorkbook = new XSSFWorkbook(file);
        }

        for (int n = 0; n < xssfWorkbook.NumberOfSheets; n++)
        {
            NPOI.SS.UserModel.ISheet sheet = xssfWorkbook.GetSheetAt(n);
            var table = ToTable(sheet);
            dataSet.Tables.Add(table);
        }

        return dataSet;
    }

    /// <summary>
    /// 读取 xlsx 文件，转换为 DataSet
    /// </summary>
    /// <param name="filePath">工作簿文件路径</param>
    /// <param name="sheetName">工作表名</param>
    /// <returns></returns>
    public static DataTable? ReadDataTable(string filePath, string sheetName)
    {
        XSSFWorkbook xssfWorkbook;

        using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            xssfWorkbook = new XSSFWorkbook(file);
        }

        for (int n = 0; n < xssfWorkbook.NumberOfSheets; n++)
        {
            if (string.Equals(xssfWorkbook.GetSheetName(n), sheetName, StringComparison.OrdinalIgnoreCase) == false)
            {
                continue;
            }

            NPOI.SS.UserModel.ISheet sheet = xssfWorkbook.GetSheetAt(n);
            return ToTable(sheet);
        }

        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sheet"></param>
    /// <returns></returns>
    private static DataTable ToTable(ISheet sheet)
    {
        DataTable table = new DataTable(sheet.SheetName);

        IRow headerRow = sheet.GetRow(0);       //第一行为标题行
        int cellCount = headerRow.LastCellNum;  //LastCellNum = PhysicalNumberOfCells
        int rowCount = sheet.LastRowNum;        //LastRowNum = PhysicalNumberOfRows - 1

        //handling header.
        for (int i = headerRow.FirstCellNum; i < cellCount; i++)
        {
            DataColumn column = new DataColumn(headerRow.GetCell(i).StringCellValue);
            table.Columns.Add(column);
        }

        for (int i = (sheet.FirstRowNum + 1); i <= rowCount; i++)
        {
            IRow row = sheet.GetRow(i);
            DataRow dataRow = table.NewRow();

            if (row != null)
            {
                for (int j = row.FirstCellNum; j < cellCount; j++)
                {
                    if (row.GetCell(j) != null)
                    {
                        dataRow[j] = GetCellValue(row.GetCell(j));
                        var cell = row.GetCell(j);
                    }
                }
            }

            table.Rows.Add(dataRow);
        }

        return table;
    }

    //public static void WriteDataSet(string path, DataSet dataSet)
    //{
    //    var workBook = new XSSFWorkbook(path);
    //    WriteDataSet(workBook, dataSet);
    //    workBook.Write(new MemoryStream());
    //}


    public static void WriteDataSet(XSSFWorkbook workBook, DataSet dataSet)
    {
        foreach (DataTable table in dataSet.Tables)
        {
            WriteDataTable(workBook, table);
        }
    }


    public static void WriteDataTable(XSSFWorkbook workBook, DataTable dataTable)
    {
        var sheet = workBook.CreateSheet(dataTable.TableName);
        var dateStyle = workBook.CreateCellStyle();
        dateStyle.DataFormat = workBook.CreateDataFormat().GetFormat("yyyy-MM-dd");
        IRow headRow = sheet.CreateRow(0);
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            ICell cell = headRow.CreateCell(i);
            cell.SetCellValue(dataTable.Columns[i].Caption);
        }

        for (int i = 0; i < dataTable.Rows.Count; i++)
        {
            IRow newRow = sheet.CreateRow(i + 1);
            for (int j = 0; j < dataTable.Columns.Count; j++)
            {
                ICell cell = newRow.CreateCell(j);
                var val = dataTable.Rows[i][j];
                if (val == null || Convert.IsDBNull(val))
                {
                    continue;
                }

                if (val is bool b)
                {
                    cell.SetCellType(CellType.Boolean);
                    cell.SetCellValue(b);
                }
                else if (val is DateTime dt)
                {
                    cell.CellStyle = dateStyle;
                    cell.SetCellValue(dt);
                }
                else if (IsNumeric(val))
                {
                    cell.SetCellType(CellType.Numeric);
                    cell.SetCellValue(Convert.ToDouble(val));
                }
                else if (val is string str)
                {
                    cell.SetCellType(CellType.String);
                    cell.SetCellValue(str);
                }
                else
                {
                    cell.SetCellValue(val.ToString());
                }

            }
        }


        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            sheet.AutoSizeColumn(i);
        }

        bool IsNumeric(object? val)
        {
            if (val == null || Convert.IsDBNull(val))
            {
                return false;
            }

            return val is byte
                || val is sbyte
                || val is short
                || val is ushort
                || val is int
                || val is uint
                || val is long
                || val is ulong
                || val is float
                || val is double
                || val is decimal;
        }
    }


    public static object? GetCellValue(ICell cell)
    {
        return GetValue(cell.CellType);

        object? GetValue(CellType cellType)
        {
            switch (cellType)
            {
                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                    {
                        return cell.DateCellValue;
                    }
                    else
                    {
                        return cell.NumericCellValue;
                    }
                case CellType.String:
                    return cell.StringCellValue?.Trim();
                case CellType.Boolean:
                    return cell.BooleanCellValue;
                case CellType.Error:
                    return ErrorConstant.ValueOf(cell.ErrorCellValue).Text;
                case CellType.Formula:
                    return GetValue(cell.CachedFormulaResultType);
                default:
                    return cell.ToString()?.Trim();
            }
        }
    }

}
