using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using FFXIVMultiLang.Extensions;
using System.Linq;
using Dalamud.Game;

namespace FFXIVMultiLang.Augments;

public unsafe class ToDoList_DutyFinderAugment
{
    private FFXIVMultiLang plugin;
    private Configuration configuration;

    public ToDoList_DutyFinderAugment(FFXIVMultiLang Plugin)
    {
        plugin = Plugin;
        configuration = Plugin.Configuration;
    }

    public void Initialize()
    {
        Services.PluginLog.Info("Initializing ToDoList_DutyFinderAugment");
        Services.AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "_ToDoList", OnToDoListPreRequestedUpdate);
        Services.AddonLifecycle.RegisterListener(AddonEvent.PreDraw, "_ToDoList", OnToDoListPreDraw);
    }

    public void Cleanup()
    {
        Services.PluginLog.Info("Clearing ToDoList_DutyFinderAugment");
        Services.AddonLifecycle.UnregisterListener(AddonEvent.PreRequestedUpdate, "_ToDoList", OnToDoListPreRequestedUpdate);
        Services.AddonLifecycle.UnregisterListener(AddonEvent.PreDraw, "_ToDoList", OnToDoListPreDraw);
    }

    public void OnToDoListPreRequestedUpdate(AddonEvent type, AddonArgs args)
    {
        HandleLanguageChanged(configuration.SwapDutyFinderTrackerLanguage ? configuration.ConfiguredLanguage : Services.ClientState.ClientLanguage);
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
            ->SetText(Services.DataManager.GetExcelSheet<Addon>(configuration.ConfiguredLanguage)?.GetRow(2500)?.Text ?? "");
    }

    public void HandleLanguageChanged(ClientLanguage language)
    {
        UpdateDutyFinder(language);
        UIState.Instance()->DirectorTodo.IsFullUpdatePending = true;
    }

    private void UpdateDutyFinder(ClientLanguage language)
    {
        var addonSheet = Services.DataManager.GetExcelSheet<Addon>(language);
        var contentFinderConditionSheet = Services.DataManager.GetExcelSheet<ContentFinderCondition>(language);
        var contentRouletteSheet = Services.DataManager.GetExcelSheet<ContentRoulette>(language);

        if (addonSheet == null) return;

        var stringArrayData = (AtkStage.Instance()->GetStringArrayData())[24];
        stringArrayData->SetValue(6, addonSheet?.GetRow(10037)?.Text ?? "", false, true, false);

        var queueInfo = UIState.Instance()->ContentsFinder.QueueInfo;

        if (queueInfo.QueueState != ContentsFinderQueueInfo.QueueStates.None)
        {
            var positionInQueue = queueInfo.PositionInQueue;
            var waitingText = addonSheet?.GetRow(10038)?.Text ?? "";
            var estimatingText = addonSheet?.GetRow(10044)?.Text ?? "";
            var waitTimeSuffix = addonSheet?.GetRow(1014)?.Text ?? "";

            // Sometimes the Addon entry will reference another entry to use instead, this does that lookup.
            while (waitTimeSuffix.Contains("Addon"))
            {
                var addonId = UInt32.Parse(waitTimeSuffix.Split("Addon").Last());
                waitTimeSuffix = addonSheet?.GetRow(addonId)?.Text ?? "";
            }

            // Data Specific to a Duty Roulette
            var contentRoulette = contentRouletteSheet?.GetRow(queueInfo.QueuedContentRouletteId);
            if (contentRoulette != null && queueInfo.QueuedContentRouletteId != 0)
            {
                stringArrayData->SetValue(0, contentRoulette.Name.ToString(), false, true, false);
                stringArrayData->SetValue(7, $"{waitingText}: {(positionInQueue != -1 ? $"#{positionInQueue}" : estimatingText)}", false, true, false);
            }

            // Forming Party Text
            var formingPartyText = addonSheet?.GetRow(2536)?.Text ?? "";
            stringArrayData->SetValue(6, formingPartyText, false, true, false);

            // Time Elapsed
            var timeElapsed = (DateTime.Now - DateTimeOffset.FromUnixTimeSeconds(queueInfo.EnteredQueueTimestamp));
            var timeElapsedString = new DateTime(timeElapsed.Ticks).ToString("m:ss");
            var timeElapsedText = addonSheet?.GetRow(10817)?.Text ?? "";

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
}
