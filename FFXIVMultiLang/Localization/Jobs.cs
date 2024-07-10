using Dalamud.Game;
using Lumina.Excel.GeneratedSheets;
using System.Collections.Generic;

namespace FFXIVMultiLang.Localization;

public static class Jobs
{
    private static Dictionary<(string job, string currentLanguage, string targetLanguage), string> I18NJobs = new Dictionary<(string, string, string), string>()
    {
        { ("LANCER", "ENGLISH", "JAPANESE"), "ランサー" },
    };

    public static string GetJobNameForLanguage(string jobName, ClientLanguage language)
    {
        ClientLanguage originalLanguage = Services.ClientState.ClientLanguage;

        return I18NJobs[(jobName, originalLanguage.ToString().ToUpper(), language.ToString().ToUpper())];
    }

    public static string GetJobNameForMonsterNoteId(uint monsterNoteId, ClientLanguage language)
    {
        Dictionary<uint, string> lookupMap = new Dictionary<uint, string>()
        {
            { 4, Services.DataManager.GetExcelSheet<Addon>(language)?.GetRow(806)?.Text ?? "UNK" },
            { 5, Services.DataManager.GetExcelSheet<Addon>(language)?.GetRow(807)?.Text ?? "UNK" },
            { 6, Services.DataManager.GetExcelSheet<Addon>(language)?.GetRow(808)?.Text ?? "UNK" },
            { 7, Services.DataManager.GetExcelSheet<Addon>(language)?.GetRow(809)?.Text ?? "UNK" },
            { 8, Services.DataManager.GetExcelSheet<Addon>(language)?.GetRow(810)?.Text ?? "UNK" },
            { 9, Services.DataManager.GetExcelSheet<Addon>(language)?.GetRow(829)?.Text ?? "UNK" },
            { 10, Services.DataManager.GetExcelSheet<Addon>(language)?.GetRow(811)?.Text ?? "UNK" },
            { 11, Services.DataManager.GetExcelSheet<Addon>(language)?.GetRow(812)?.Text ?? "UNK" },
            { 12, Services.DataManager.GetExcelSheet<Addon>(language)?.GetRow(828)?.Text ?? "UNK" },
            { 13, Services.DataManager.GetExcelSheet<Addon>(language)?.GetRow(15467)?.Text ?? "UNK" },
        };

        return lookupMap.GetValueOrDefault(monsterNoteId, "UNK");
    }
}
