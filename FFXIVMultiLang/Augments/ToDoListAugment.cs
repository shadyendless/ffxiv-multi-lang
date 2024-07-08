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
using FFXIVClientStructs.FFXIV.Application.Network.WorkDefinitions;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System.Runtime.InteropServices;

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

        var stringArrayData = (AtkStage.Instance()->GetStringArrayData())[24];
        stringArrayData->SetValue(6, Service.DataManager.GetExcelSheet<Addon>(configuration.ConfiguredLanguage)?.GetRow(10037)?.Text ?? "", false, true, false);

        var queueInfo = UIState.Instance()->ContentsFinder.QueueInfo;

        if (queueInfo.QueueState != ContentsFinderQueueInfo.QueueStates.None)
        {
            var positionInQueue = queueInfo.PositionInQueue;
            var waitingText = Service.DataManager.GetExcelSheet<Addon>(configuration.ConfiguredLanguage)?.GetRow(10038)?.Text ?? "";
            var estimatingText = Service.DataManager.GetExcelSheet<Addon>(configuration.ConfiguredLanguage)?.GetRow(10044)?.Text ?? "";
            var waitTimeSuffix = Service.DataManager.GetExcelSheet<Addon>(configuration.ConfiguredLanguage)?.GetRow(1014)?.Text ?? "";

            if (waitTimeSuffix.Contains("Addon"))
            {
                var addonId = UInt32.Parse(waitTimeSuffix.Split("Addon").Last());
                waitTimeSuffix = Service.DataManager.GetExcelSheet<Addon>(configuration.ConfiguredLanguage)?.GetRow(addonId)?.Text ?? "";
            }

            stringArrayData->SetValue(7, $"{waitingText}: {(positionInQueue != -1 ? $"#{positionInQueue}" : estimatingText)}", false, true, false);

            var contentRoulette = Service.DataManager.GetExcelSheet<ContentRoulette>(configuration.ConfiguredLanguage)?.GetRow(queueInfo.QueuedContentRouletteId);
            if (contentRoulette != null)
            {
                stringArrayData->SetValue(0, contentRoulette.Name.ToString(), false, true, false);
            }

            var timeElapsed = (DateTime.Now - DateTimeOffset.FromUnixTimeSeconds(queueInfo.EnteredQueueTimestamp));
            var timeElapsedString = new DateTime(timeElapsed.Ticks).ToString("m:ss");
            var timeElapsedText = Service.DataManager.GetExcelSheet<Addon>(configuration.ConfiguredLanguage)?.GetRow(10817)?.Text ?? "";

            var splitTimeElapsedString = timeElapsedText.Replace(":/", ">>>>/").Replace(": :", ": >>>>").Replace(": )", ": >>>>)").Split(">>>>").ToList();
            splitTimeElapsedString.Insert(1, $"{timeElapsedString}");
            splitTimeElapsedString.Insert(3, $"{queueInfo.AverageWaitTime}{waitTimeSuffix}");
            var waitingString = String.Join("", splitTimeElapsedString);

            stringArrayData->SetValue(8, waitingString, false, true, false);
        }
    }

    private void UpdateQuestLog()
    {
        List<(Quest, QuestText?, ushort)> trackedQuestData = new List<(Quest, QuestText?, ushort)>();

        var normalQuests = QuestManager.Instance()->NormalQuests;
        var trackedQuests = QuestManager.Instance()->TrackedQuests;

        for (var i = 0; i < trackedQuests.Length; i++)
        {
            var trackedQuest = trackedQuests[i];
            if (trackedQuest.QuestType == 0) continue;

            var questId = normalQuests[trackedQuest.Index].QuestId;
            var quest = Service.DataManager.GetExcelSheet<Quest>(configuration.ConfiguredLanguage)?.GetRow(questId + (uint)65535 + (uint)1);

            if (quest == null) continue;

            var questStep = quest.ToDoCompleteSeq.ToList().IndexOf(QuestManager.GetQuestSequence(questId));
            var currentStep = questStep.ToString().PadLeft(2, '0');
            var currentStepKey = $"TEXT_{quest.Id.ToString().ToUpper()}_TODO_{currentStep}";
            var questFolder = String.Join("", quest.Id.ToString().Split("_").Last().Take(3));
            var questPath = $"quest/{questFolder}/{quest.Id}";

            var questText = Service.DataManager.Excel.GetSheet<QuestText>(configuration.ConfiguredLanguage.ToLumina(), questPath)?.FirstOrDefault(rowData => rowData.Id.ToString() == currentStepKey);

            trackedQuestData.Add((quest, questText, questId));
        }

        trackedQuestData.Reverse();

        var stringArrayData = (AtkStage.Instance()->GetStringArrayData())[24];

        for (var i = 0; i < trackedQuestData.Count; i++)
        {
            var (q, t, id) = trackedQuestData[i];
            stringArrayData->SetValue(9 + i, q.Name, false, true, true);
            stringArrayData->SetValue(9 + trackedQuestData.Count + i, BuildQuestDescription(q, t, id), false, true, false);
        }
    }

    private unsafe string BuildQuestDescription(Quest quest, QuestText? text, ushort questId)
    {
        if (text == null) return "<<ERR>>";

        var questStep = quest.ToDoCompleteSeq.ToList().IndexOf(QuestManager.GetQuestSequence(questId));
        bool showItemCounter = quest.ToDoQty[0] > 1;
        string description = text.Description;

        if (description.Contains("EObj") || description.Contains("EventItem"))
        {
            var itemId = quest.ScriptArg[1];
            var eventItem = Service.DataManager.GetExcelSheet<EObjName>(configuration.ConfiguredLanguage)?.GetRow(itemId);
            
            if (eventItem != null)
            {
                string replacementItem = (showItemCounter && eventItem.Plural.ToString() != "" ? eventItem.Plural : eventItem.Singular).ToString();

                if (eventItem != null)
                {
                    description = description.Replace("EObj", replacementItem).Replace("EventItem", replacementItem);
                }
            }
        }

        if (showItemCounter)
        {
            description += $" {QuestManager.Instance()->GetQuestById(questId)->Variables.ToArray()[0]}/{quest.ToDoQty[0]}";
        }

        return description;
    }
}
