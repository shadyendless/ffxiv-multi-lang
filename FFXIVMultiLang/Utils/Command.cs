using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text;
using Dalamud.Game;
using System.Collections.Generic;

namespace FFXIVMultiLang.Utils;

public static class Command
{
    public static ClientLanguage GetClientLanguageFromInput(string language)
    {
        ClientLanguage newLanguage = Service.ClientState.ClientLanguage;

        switch (language.ToUpper())
        {
            case "JAPANESE":
            case "JAPANESE (JP)":
            case "JP":
                newLanguage = ClientLanguage.Japanese;
                break;
            case "ENGLISH":
            case "ENGLISH (EN)":
            case "EN":
                newLanguage = ClientLanguage.English;
                break;
            case "FRENCH":
            case "FRENCH (FR)":
            case "FR":
                newLanguage = ClientLanguage.French;
                break;
            case "GERMAN":
            case "GERMAN (DE)":
            case "DE":
                newLanguage = ClientLanguage.German;
                break;
            case "RESET":
                newLanguage = Service.ClientState.ClientLanguage;
                break;
            default:
                Service.ChatGui.Print(new XivChatEntry
                {
                    Message = new SeString(new List<Payload>
                    {
                        new UIForegroundPayload(0),
                        new TextPayload($"The language "),
                        new UIForegroundPayload(539),
                        new TextPayload(language),
                        new UIForegroundPayload(0),
                        new TextPayload($" is not supported."),
                        new UIForegroundPayload(0),
                        new TextPayload($"\nThe supported languages are: "),
                        new TextPayload($"\n    - Japanese (JP)"),
                        new TextPayload($"\n    - English (EN)"),
                        new TextPayload($"\n    - German (DE)"),
                        new TextPayload($"\n    - French (FR)"),
                        new UIForegroundPayload(0),
                    }),
                    Type = XivChatType.Echo
                });
                return Service.ClientState.ClientLanguage;
        }

        Service.ChatGui.Print(new XivChatEntry
        {
            Message = new SeString(new List<Payload>
                    {
                        new UIForegroundPayload(0),
                        newLanguage == Service.ClientState.ClientLanguage ?
                            new TextPayload($"Resetting your language to ") :
                            new TextPayload($"Setting your language to "),
                        new UIForegroundPayload(34),
                        new TextPayload(newLanguage.ToString()),
                        new UIForegroundPayload(0),
                        new TextPayload($"."),
                        new UIForegroundPayload(0),
                    }),
            Type = XivChatType.Echo
        });

        return newLanguage;
    }
}
