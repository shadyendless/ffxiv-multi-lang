using System.Linq;
using Dalamud.Game;
using Lumina.Excel;

namespace FFXIVMultiLang.Utils.Globals;

public static class Excel
{
    [Obsolete]
    public static ExcelSheet<T> GetSheet<T>(ClientLanguage? language = null) where T : ExcelRow
        => Service.DataManager.GetExcelSheet<T>(language ?? Service.ClientState.ClientLanguage)!;

    [Obsolete]
    public static uint GetRowCount<T>() where T : ExcelRow
        => GetSheet<T>().RowCount;

    [Obsolete]
    public static T? GetRow<T>(uint rowId, uint subRowId = uint.MaxValue, ClientLanguage? language = null) where T : ExcelRow
        => GetSheet<T>(language).GetRow(rowId, subRowId);

    [Obsolete]
    public static T? FindRow<T>(Func<T?, bool> predicate) where T : ExcelRow
        => GetSheet<T>().FirstOrDefault(predicate, null);
}
