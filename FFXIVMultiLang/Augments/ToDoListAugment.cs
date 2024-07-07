using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Linq;
using Lumina.Excel.GeneratedSheets;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVMultiLang.Sheets;
using Dalamud.Utility;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace FFXIVMultiLang.Augments;

public unsafe class ToDoListAugment
{
    private FFXIVMultiLang plugin;
    private Configuration configuration;

    public ToDoListAugment(FFXIVMultiLang Plugin)
    {
        plugin = Plugin;
        configuration = Plugin.Configuration;
        configuration = Plugin.Configuration;
    }

    public unsafe void OnToDoListPreRequestedUpdate(AddonEvent type, AddonArgs args)
    {
        HandleLanguageChanged();
    }

    public void HandleLanguageChanged()
    {
        List<(Quest, QuestText?)> trackedQuestData = new List<(Quest, QuestText?)>();

        var normalQuests = QuestManager.Instance()->NormalQuests;
        var trackedQuests = QuestManager.Instance()->TrackedQuests;

        Service.PluginLog.Info(QuestManager.Instance()->NumAcceptedQuests.ToString());

        for (var i = 0; i < trackedQuests.Length; i++)
        {
            var trackedQuest = trackedQuests[i];
            if (trackedQuest.QuestType == 0) continue;

            var questId = normalQuests[trackedQuest.Index].QuestId;
            var quest = Service.DataManager.GetExcelSheet<Quest>(configuration.ConfiguredLanguage)?.GetRow(questId + (uint)65535 + (uint)1);

            var currentStep = ((QuestManager.GetQuestSequence(questId) - 1) % 255).ToString().PadLeft(2, '0');
            var currentStepKey = $"TEXT_{quest.Id.ToString().ToUpper()}_TODO_{currentStep}";
            var questFolder = String.Join("", quest.Id.ToString().Split("_").Last().Take(3));
            var questPath = $"quest/{questFolder}/{quest.Id}";

            var questText = Service.DataManager.Excel.GetSheet<QuestText>(configuration.ConfiguredLanguage.ToLumina(), questPath)?.FirstOrDefault(rowData => rowData.Id.ToString() == currentStepKey);

            trackedQuestData.Add((quest, questText));
        }

        trackedQuestData.Reverse();

        var stringArrayData = (AtkStage.Instance()->GetStringArrayData())[24];

        for (var i = 0; i < trackedQuestData.Count; i++)
        {
            var (q, t) = trackedQuestData[i];
            stringArrayData->SetValue(9 + i, q.Name, false, true, true);
            stringArrayData->SetValue(9 + trackedQuestData.Count + i, t?.Description ?? "<<ERR>>", false, true, false);
        }

        RaptureAtkUnitManager.Instance()->GetAddonByName("_ToDoList")->OnRequestedUpdate(
            AtkStage.Instance()->GetNumberArrayData(),
            AtkStage.Instance()->GetStringArrayData()
        );
    }
}
