using Dalamud.Game;
using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;
using Lumina.Text.Payloads;
using Lumina.Text.ReadOnly;

namespace FFXIVMultiLang.Utils;

public static class MacroString
{
    public static ReadOnlySeString ProcessMacroString(SeString input, ClientLanguage language, int lnum4 = 0)
    {
        //Services.PluginLog.Debug($"[Macro String -] {input.ToMacroString()}");
        var result = InternalProcessMacroString(input.AsReadOnly(), language, lnum4);
        //Services.PluginLog.Debug($"[Macro String =] {result}");
        return result;
    }

    private static ReadOnlySeString InternalProcessMacroString(ReadOnlySeString input, ClientLanguage language, int lnum4 = 0)
    {
        var sb = new SeStringBuilder();

        foreach (var payload in input)
        {
            switch (payload.Type)
            {
                case ReadOnlySePayloadType.Text:
                    sb.Append(payload);
                    break;

                case ReadOnlySePayloadType.Macro:
                    switch (payload.MacroCode)
                    {
                        case MacroCode.NonBreakingSpace:
                            sb.Append(" ");
                            break;

                        case MacroCode.Hyphen:
                            sb.Append("-");
                            break;

                        case MacroCode.SoftHyphen:
                            sb.Append("");
                            break;

                        case MacroCode.ColorType:
                            {
                                if (!payload.TryGetExpression(out var expr1))
                                    break;

                                if (!expr1.TryGetUInt(out var color))
                                    break;

                                if (color == 0) sb.PopColorType();
                                else sb.PushColorType(color);

                                break;
                            }

                        case MacroCode.EdgeColorType:
                            {
                                if (!payload.TryGetExpression(out var expr1))
                                    break;

                                if (!expr1.TryGetUInt(out var color))
                                    break;

                                if (color == 0) sb.PopEdgeColorType();
                                else sb.PushEdgeColorType(color);

                                break;
                            }

                        case MacroCode.Head:
                            {
                                if (!payload.TryGetExpression(out var expr1))
                                    break;

                                if (!expr1.TryGetString(out var text))
                                    break;

                                var str = InternalProcessMacroString(text, language, lnum4).ExtractText();
                                if (str.Length == 0)
                                    break;

                                sb.Append(str[..1].ToUpperInvariant());
                                sb.Append(str[1..]);
                                break;
                            }

                        case MacroCode.If:
                            {
                                if (!payload.TryGetExpression(out var comparison, out var expr2, out var expr3))
                                    break;

                                if (!expr2.TryGetString(out var trueBody))
                                    break;

                                if (!expr3.TryGetString(out var falseBody))
                                    break;

                                if (IfConditionResolver(comparison.ToString(), lnum4))
                                {
                                    sb.Append(InternalProcessMacroString(trueBody, language, lnum4));
                                }
                                else
                                {
                                    sb.Append(InternalProcessMacroString(falseBody, language, lnum4));
                                }

                                break;
                            }

                        case MacroCode.Sheet:
                            {
                                if (!payload.TryGetExpression(out var expr1, out var expr2, out var expr3))
                                    break;

                                if (!expr1.TryGetString(out var rawSheetName))
                                    break;

                                if (!expr2.TryGetInt(out var rowId))
                                    break;

                                if (!expr3.TryGetInt(out var columnIndex))
                                    break;

                                var sheetName = rawSheetName.ExtractText();
                                if (sheetName == "EObj")
                                    sheetName = "EObjName";

                                var resolvedRow = Services.DataManager.GameData.Excel.GetSheetRaw(sheetName, language.ToLumina())?.GetRow((uint)rowId);
                                var resolvedValue = Services.DataManager.GameData.Excel.GetSheetRaw(sheetName, language.ToLumina())?.GetRow((uint)rowId)?.ReadColumn<SeString>(columnIndex);

                                if (sheetName == "Item")
                                {
                                    var rarity = resolvedRow?.ReadColumn<byte>(12);

                                    if (rarity != null)
                                    {
                                        sb.PushColorType((uint)(547 + rarity * 2));
                                        sb.PushEdgeColorType((uint)(547 + rarity * 2 + 1));
                                        sb.Append(resolvedValue);
                                        sb.PopEdgeColorType();
                                        sb.PopColorType();
                                    }
                                }
                                else
                                {
                                    sb.Append(resolvedValue);
                                }

                                break;
                            }

                        case MacroCode.DeNoun:
                            sb.Append(SharedNounHandler(payload, ClientLanguage.German));
                            break;

                        case MacroCode.EnNoun:
                            sb.Append(SharedNounHandler(payload, ClientLanguage.English));
                            break;

                        case MacroCode.FrNoun:
                            sb.Append(SharedNounHandler(payload, ClientLanguage.French));
                            break;

                        case MacroCode.JaNoun:
                            sb.Append(SharedNounHandler(payload, ClientLanguage.Japanese));
                            break;

                        case MacroCode.NewLine:
                            sb.Append("\n");
                            break;

                        default:
                            Services.PluginLog.Warning($"UNHANDLED MACRO: {payload.MacroCode}");
                            sb.Append(payload);
                            break;
                    }
                    break;
            }
        }
        return sb.ToReadOnlySeString();
    }

    private static ReadOnlySeString SharedNounHandler(ReadOnlySePayload payload, ClientLanguage language)
    {
        SeStringBuilder sb = new SeStringBuilder();

        if (!payload.TryGetExpression(out var expr1, out var expr2, out var expr3, out var expr4, out var expr5))
            return sb.ToReadOnlySeString();

        if (!expr1.TryGetString(out var rawSheetName))
            return sb.ToReadOnlySeString();

        if (!expr2.TryGetInt(out var person))
            return sb.ToReadOnlySeString();

        if (!expr3.TryGetInt(out var rowId))
            return sb.ToReadOnlySeString();

        if (!expr4.TryGetInt(out var amount))
            return sb.ToReadOnlySeString();

        if (!expr5.TryGetInt(out var @case))
            return sb.ToReadOnlySeString();

        var sheetName = rawSheetName.ExtractText();
        if (sheetName == "EObj")
            sheetName = "EObjName";

        if (sheetName == "Item")
        {
            var item = Services.DataManager.GetExcelSheet<Item>(language)?.GetRow((uint)rowId);

            if (item != null)
            {
                sb.PushColorType((uint)(547 + item.Rarity * 2));
                sb.PushEdgeColorType((uint)(547 + item.Rarity * 2 + 1));
                sb.Append(Services.TextDecoder.ProcessNoun(language, sheetName, person, rowId, amount, @case));
                sb.PopEdgeColorType();
                sb.PopColorType();
            }
        }
        else
        {
            sb.Append(Services.TextDecoder.ProcessNoun(language, sheetName, person, rowId, amount, @case));
        }

        return sb.ToReadOnlySeString();
    }

    private static bool IfConditionResolver(string condition, int lnum4 = 0)
    {
        var splitCondition = condition.Split(" ");
        int leftSide = 0;
        int rightSide = Int32.Parse(splitCondition[2]);

        switch (splitCondition[0])
        {
            case "lnum(4)":
                leftSide = lnum4;
                break;
            default:
                Services.PluginLog.Info($"[ERROR] Unknown left side: {splitCondition[0]}");
                break;
        }

        switch (splitCondition[1])
        {
            case "[eq]":
                return leftSide == rightSide;
            default:
                Services.PluginLog.Info($"[ERROR] Unknown comparison: {splitCondition[1]}");
                break;
        }

        return false;
    }
}
