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
using FFXIVMultiLang.Extensions;
using System.Linq;

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

    public void OnToDoListPreRequestedUpdate(AddonEvent type, AddonArgs args)
    {
        HandleLanguageChanged();
    }

    public void OnToDoListPreDraw(AddonEvent type, AddonArgs args)
    {
        RaptureAtkUnitManager.Instance()
            ->GetAddonByName("_ToDoList")
            ->GetNodeById(6)
            ->GetAsAtkComponentNode()
            ->Component
            ->UldManager
            .SearchNodeById(2)
            ->GetAsAtkTextNode()
            ->SetText(Service.DataManager.GetExcelSheet<Addon>(configuration.ConfiguredLanguage)?.GetRow(2500)?.Text ?? "");
    }

    public void HandleLanguageChanged()
    {
        UpdateQuestLog();
        UpdateDutyFinder();

        RaptureAtkUnitManager.Instance()->GetAddonByName("_ToDoList")->OnRequestedUpdate(
            AtkStage.Instance()->GetNumberArrayData(),
            AtkStage.Instance()->GetStringArrayData()
        );
    }

    private void UpdateDutyFinder()
    {

        //RaptureAtkUnitManager.Instance()->GetAddonByName("_ToDoList")->GetNodeById(6)->ChildCount->SetScale(1, 1);
        var addonSheet = Service.DataManager.GetExcelSheet<Addon>(configuration.ConfiguredLanguage);
        var contentFinderConditionSheet = Service.DataManager.GetExcelSheet<ContentFinderCondition>(configuration.ConfiguredLanguage);

        if (addonSheet == null) return;

        var stringArrayData = (AtkStage.Instance()->GetStringArrayData())[24];
        stringArrayData->SetValue(6, Service.DataManager.GetExcelSheet<Addon>(configuration.ConfiguredLanguage)?.GetRow(10037)?.Text ?? "", false, true, false);

        var queueInfo = UIState.Instance()->ContentsFinder.QueueInfo;

        if (queueInfo.QueueState != ContentsFinderQueueInfo.QueueStates.None)
        {
            var positionInQueue = queueInfo.PositionInQueue;
            var waitingText = addonSheet.GetRow(10038)?.Text ?? "";
            var estimatingText = addonSheet.GetRow(10044)?.Text ?? "";
            var waitTimeSuffix = addonSheet.GetRow(1014)?.Text ?? "";

            // Sometimes the Addon entry will reference another entry to use instead, this does that lookup.
            while (waitTimeSuffix.Contains("Addon"))
            {
                var addonId = UInt32.Parse(waitTimeSuffix.Split("Addon").Last());
                waitTimeSuffix = addonSheet.GetRow(addonId)?.Text ?? "";
            }

            // Data Specific to a Duty Roulette
            var contentRoulette = Service.DataManager.GetExcelSheet<ContentRoulette>(configuration.ConfiguredLanguage)?.GetRow(queueInfo.QueuedContentRouletteId);
            if (contentRoulette != null && queueInfo.QueuedContentRouletteId != 0)
            {
                stringArrayData->SetValue(0, contentRoulette.Name.ToString(), false, true, false);
                stringArrayData->SetValue(7, $"{waitingText}: {(positionInQueue != -1 ? $"#{positionInQueue}" : estimatingText)}", false, true, false);
            }

            // Forming Party Text
            var formingPartyText = addonSheet.GetRow(2536)?.Text ?? "";
            stringArrayData->SetValue(6, formingPartyText, false, true, false);

            // Time Elapsed
            var timeElapsed = (DateTime.Now - DateTimeOffset.FromUnixTimeSeconds(queueInfo.EnteredQueueTimestamp));
            var timeElapsedString = new DateTime(timeElapsed.Ticks).ToString("m:ss");
            var timeElapsedText = Service.DataManager.GetExcelSheet<Addon>(configuration.ConfiguredLanguage)?.GetRow(10817)?.Text ?? "";

            var splitTimeElapsedString = timeElapsedText.Replace(":/", ">>>>/").Replace(": :", ": >>>>").Replace(": )", ": >>>>)").Split(">>>>").ToList();
            splitTimeElapsedString.Insert(1, $"{timeElapsedString}");
            splitTimeElapsedString.Insert(3, $"{queueInfo.AverageWaitTime}{waitTimeSuffix}");
            var waitingString = String.Join("", splitTimeElapsedString);

            stringArrayData->SetValue(8, waitingString, false, true, false);
        }

        if (queueInfo.QueuedContentFinderConditionId1 > 0)
        {
            stringArrayData->SetValue(0, contentFinderConditionSheet?.GetRow(queueInfo.QueuedContentFinderConditionId1)?.Name?.ToString()?.FirstCharToUpper() ?? "", false, true, false);
        }

        if (queueInfo.QueuedContentFinderConditionId2 > 0)
        {
            stringArrayData->SetValue(1, contentFinderConditionSheet?.GetRow(queueInfo.QueuedContentFinderConditionId2)?.Name.ToString()?.FirstCharToUpper() ?? "", false, true, false);
        }

        if (queueInfo.QueuedContentFinderConditionId3 > 0)
        {
            stringArrayData->SetValue(2, contentFinderConditionSheet?.GetRow(queueInfo.QueuedContentFinderConditionId3)?.Name.ToString()?.FirstCharToUpper() ?? "", false, true, false);
        }

        if (queueInfo.QueuedContentFinderConditionId4 > 0)
        {
            stringArrayData->SetValue(3, contentFinderConditionSheet?.GetRow(queueInfo.QueuedContentFinderConditionId4)?.Name.ToString()?.FirstCharToUpper() ?? "", false, true, false);
        }

        if (queueInfo.QueuedContentFinderConditionId5 > 0)
        {
            stringArrayData->SetValue(4, contentFinderConditionSheet?.GetRow(queueInfo.QueuedContentFinderConditionId5)?.Name.ToString()?.FirstCharToUpper() ?? "", false, true, false);
        }
    }

    private void UpdateQuestLog()
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
            var quest = Service.DataManager.GetExcelSheet<Quest>(configuration.ConfiguredLanguage)?.GetRow(questId + (uint)65535 + (uint)1);

            if (quest == null) continue;

            Service.PluginLog.Info(normalQuests[i].AcceptClassJob.ToString());

            var questStep = quest.ToDoCompleteSeq.ToList().IndexOf(QuestManager.GetQuestSequence(questId));
            var currentStep = questStep.ToString().PadLeft(2, '0');
            var currentStepKey = $"TEXT_{quest.Id.ToString().ToUpper()}_TODO_{currentStep}";
            var questFolder = String.Join("", quest.Id.ToString().Split("_").Last().Take(3));
            var questPath = $"quest/{questFolder}/{quest.Id}";

            var questText = Service.DataManager.Excel.GetSheet<QuestText>(configuration.ConfiguredLanguage.ToLumina(), questPath)?.FirstOrDefault(rowData => rowData.Id.ToString() == currentStepKey);

            trackedQuestData.Add((quest, questText, questId, acceptClassJob));
        }

        trackedQuestData.Reverse();

        var stringArrayData = (AtkStage.Instance()->GetStringArrayData())[24];

        for (var i = 0; i < trackedQuestData.Count; i++)
        {
            var (q, t, id, lnum4) = trackedQuestData[i];
            stringArrayData->SetValue(9 + i, q.Name, false, true, true);
            stringArrayData->SetValue(9 + trackedQuestData.Count + i, BuildQuestDescription(q, t, id, lnum4), false, true, false);
        }
    }

    private unsafe byte[] BuildQuestDescription(Quest quest, QuestText? text, ushort questId, uint lnum4)
    {
        if (text == null) return [];

        var questStep = quest.ToDoCompleteSeq.ToList().IndexOf(QuestManager.GetQuestSequence(questId));
        var description = MacroString.ProcessMacroString(text.Description, configuration.ConfiguredLanguage, (int)lnum4);
        bool showItemCounter = quest.ToDoQty[0] > 1;

        return new SeString(
            MacroString.ProcessMacroString(text.Description, configuration.ConfiguredLanguage, (int)lnum4).Data.ToArray()
        ).ToDalamudString().Append(showItemCounter ? $" {QuestManager.Instance()->GetQuestById(questId)->Variables.ToArray()[0]}/{quest.ToDoQty[0]}" : "").Encode();
    }
}
