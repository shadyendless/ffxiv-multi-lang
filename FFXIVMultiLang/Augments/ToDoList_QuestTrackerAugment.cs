using Dalamud.Game;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVMultiLang.Sheets;
using FFXIVMultiLang.Utils;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;
using System.Collections.Generic;
using System.Linq;

namespace FFXIVMultiLang.Augments;

public unsafe class ToDoList_QuestTrackerAugment
{
    private FFXIVMultiLang plugin;
    private Configuration configuration;

    public ToDoList_QuestTrackerAugment(FFXIVMultiLang Plugin)
    {
        plugin = Plugin;
        configuration = Plugin.Configuration;
    }

    public void Initialize()
    {
        Services.PluginLog.Info("Initializing ToDoList_QuestTrackerAugment");
        Services.AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "_ToDoList", OnToDoListPreRequestedUpdate);

        HandleLanguageChanged(configuration.ConfiguredLanguage);
    }

    public void Cleanup()
    {
        Services.PluginLog.Info("Clearing ToDoList_QuestTrackerAugment");
        Services.AddonLifecycle.UnregisterListener(AddonEvent.PreRequestedUpdate, "_ToDoList", OnToDoListPreRequestedUpdate);
    }

    public void OnToDoListPreRequestedUpdate(AddonEvent type, AddonArgs args)
    {
        HandleLanguageChanged(configuration.ConfiguredLanguage);
    }

    public void HandleLanguageChanged(ClientLanguage language)
    {
        UpdateQuestLog(language);
        UIState.Instance()->DirectorTodo.IsFullUpdatePending = true;
    }

    private void UpdateQuestLog(ClientLanguage language)
    {
        List<(Quest, QuestText?, ushort, uint)> trackedQuestData = new List<(Quest, QuestText?, ushort, uint)>();

        var normalQuests = QuestManager.Instance()->NormalQuests;
        var trackedQuests = QuestManager.Instance()->TrackedQuests;

        for (var i = 0; i < trackedQuests.Length; i++)
        {
            var trackedQuest = trackedQuests[i];
            if (trackedQuest.QuestType == 0) continue;

            var questId = normalQuests[trackedQuest.Index].QuestId;
            var acceptClassJob = normalQuests[trackedQuest.Index].AcceptClassJob;
            var quest = Services.DataManager.GetExcelSheet<Quest>(language)?.GetRow(questId + (uint)65535 + (uint)1);

            if (quest == null) continue;

            var questStep = quest.ToDoCompleteSeq.ToList().IndexOf(QuestManager.GetQuestSequence(questId));
            var currentStep = questStep.ToString().PadLeft(2, '0');
            var currentStepKey = $"TEXT_{quest.Id.ToString().ToUpper()}_TODO_{currentStep}";
            var questFolder = String.Join("", quest.Id.ToString().Split("_").Last().Take(3));
            var questPath = $"quest/{questFolder}/{quest.Id}";

            var questText = Services.DataManager.Excel.GetSheet<QuestText>(language.ToLumina(), questPath)?.FirstOrDefault(rowData => rowData.Id.ToString() == currentStepKey);

            trackedQuestData.Add((quest, questText, questId, acceptClassJob));
        }

        trackedQuestData.Reverse();

        var stringArrayData = (AtkStage.Instance()->GetStringArrayData())[24];

        for (var i = 0; i < trackedQuestData.Count; i++)
        {
            var (q, t, id, lnum4) = trackedQuestData[i];
            stringArrayData->SetValue(9 + i, q.Name, false, true, true);
            stringArrayData->SetValue(9 + trackedQuestData.Count + i, BuildQuestDescription(language, q, t, id, lnum4), false, true, false);
        }
    }

    private unsafe byte[] BuildQuestDescription(ClientLanguage language, Quest quest, QuestText? text, ushort questId, uint lnum4)
    {
        if (text == null) return [];

        var questStep = quest.ToDoCompleteSeq.ToList().IndexOf(QuestManager.GetQuestSequence(questId));
        var description = MacroString.ProcessMacroString(text.Description, language, (int)lnum4);
        bool showItemCounter = quest.ToDoQty[0] > 1;

        return new SeString(
            MacroString.ProcessMacroString(text.Description, language, (int)lnum4).Data.ToArray()
        ).ToDalamudString().Append(showItemCounter ? $" {QuestManager.Instance()->GetQuestById(questId)->Variables.ToArray()[0]}/{quest.ToDoQty[0]}" : "").Encode();
    }
}
