using Kingmaker.Blueprints;
using Kingmaker.Kingdom;
using Kingmaker.UI.SettingsUI;
using UnityModManagerNet;
using static KingdomResolution.GUIHelper;
using static KingdomResolution.Labels;
using static UnityEngine.GUILayout;

namespace KingdomResolution;

public static class Main
{
    internal static Harmony HarmonyInstance;
    internal static UnityModManager.ModEntry.ModLogger Log;
	public static bool enabled;
	public static Settings settings;
	static string modId;
	static int SavedCustomLeaderPenalty; // Unused?

    public static bool Load(UnityModManager.ModEntry modEntry) {
        Log = modEntry.Logger;
        modEntry.OnGUI = OnGUI;
		modEntry.OnSaveGUI = OnSaveGUI;
		modEntry.OnToggle = OnToggle;
        HarmonyInstance = new Harmony(modEntry.Info.Id);
		modId = modEntry.Info.Id;
		settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
        
		try
		{
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
			KingdomStash.Init();
        }
		catch
		{
            HarmonyInstance.UnpatchAll(modId);
            throw;
        }
        
		return true;
    }

	// Called when the mod is turned to on/off.
	public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
	{
		enabled = value;
		return true; // Permit or not.
	}

	public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
	{
		settings.Save(modEntry);
	}

    public static void OnGUI(UnityModManager.ModEntry modEntry)
	{
		if (!enabled) return;
		try
		{
			string percentFormatter(float value) => Math.Round(value * 100, 0) == 0 ? " 1 day" : Math.Round(value * 100, 0) + " %";
			Label("Kingdom Options", Util.BoldLabel);
			ChooseFactor(EventTimeFactorLabel, EventTimeFactorTooltip, settings.eventTimeFactor, 1,
				(value) => settings.eventTimeFactor = (float)Math.Round(value, 2), percentFormatter);
			ChooseFactor(ProjectTimeFactorLabel, ProjectTimeFactorTooltip, settings.projectTimeFactor, 1,
				(value) => settings.projectTimeFactor = (float)Math.Round(value, 2), percentFormatter);
			ChooseFactor(RulerTimeFactorLabel, RulerTimeFactorTooltip, settings.baronTimeFactor, 1,
				(value) => settings.baronTimeFactor = (float)Math.Round(value, 2), percentFormatter);
			ChooseFactor(EventPriceFactorLabel, EventPriceFactorTooltip, settings.eventPriceFactor, 1,
				(value) => settings.eventPriceFactor = (float)Math.Round(value, 2), (value) => " " + Math.Round(Math.Round(value, 2) * 100, 0) + " %");
            ChooseFactor(KingdomEventDCModLabel, KingdomEventDCModTooltip, settings.eventDCFactor, 1,
                (value) => settings.eventDCFactor = (float)Math.Round(value, 2), percentFormatter);
			Toggle(ref settings.easyEvents, EasyEventsLabel, EasyEventsTooltip);
			Toggle(ref settings.alwaysManageKingdom, AlwaysManageKingdomLabel, AlwaysManageKingdomTooltip);
			Toggle(ref settings.alwaysAdvanceTime, AlwaysAdvanceTimeLabel, AlwaysAdvanceTimeTooltip);
			Toggle(ref settings.skipPlayerTime, SkipPlayerTimeLabel, SkipPlayerTimeTooltip);
			Toggle(ref settings.alwaysBaronProcurement, AlwaysBaronProcurementLabel, AlwaysBaronProcurementTooltip);
			Toggle(ref settings.overrideIgnoreEvents, OverrideIgnoreEventsLabel, OverrideIgnoreEventsTooltip);
			Toggle(ref settings.disableAutoAssignLeaders, DisableAutoAssignLeadersLabel, DisableAutoAssignLeadersTooltip);
			Toggle(ref settings.disableMercenaryPenalty, DisableMercenaryPenaltyLabel, DisableMercenaryPenaltyTooltip);
			Toggle(ref settings.currencyFallback, CurrencyFallbackLabel, CurrencyFallbackTooltip);
			ChooseInt(ref settings.currencyFallbackExchangeRate, CurrencyFallbackExchangeRateLabel, CurrencyFallbackExchangeRateTooltip);
			BeginHorizontal();
			Toggle(ref settings.pauseKingdomTimeline, PauseKingdomTimelineLabel, PauseKingdomTimelineTooltip);
			if (settings.pauseKingdomTimeline)
			{
				Toggle(ref settings.enablePausedKingdomManagement, EnablePausedKingdomManagementLabel, EnablePausedKingdomManagementTooltip);
				if (settings.enablePausedKingdomManagement)
				{
					Toggle(ref settings.enablePausedRandomEvents, EnablePausedRandomEventsLabel, EnablePausedRandomEventsTooltip);
				}
			}
			EndHorizontal();
			if (ResourcesLibrary.LibraryObject != null && SettingsRoot.Instance.KingdomManagementMode.CurrentValue == KingdomDifficulty.Auto)
			{
				if (Button("Disable Auto Kingdom Management Mode"))
				{
					SettingsRoot.Instance.KingdomManagementMode.CurrentValue = KingdomDifficulty.Easy;
					SettingsRoot.Instance.KingdomDifficulty.CurrentValue = KingdomDifficulty.Easy;
				}
			}
			ChooseKingdomUnreset();
			Label("Preview Options", Util.BoldLabel);
			Toggle(ref settings.previewEventResults, PreviewEventResultsLabel, PreviewEventResultsTooltip);
			Toggle(ref settings.previewDialogResults, PreviewDialogResultsLabel, PreviewDialogResultsTooltip);
			Toggle(ref settings.previewAlignmentRestrictedDialog, PreviewAlignmentRestrictedDialogLabel, PreviewAlignmentRestrictedDialogTooltip);
			Toggle(ref settings.previewRandomEncounters, PreviewRandomEncountersLabel, PreviewRandomEncountersTooltip);
			Label("Misc Options", Util.BoldLabel);
			Toggle(ref settings.highlightObjectsToggle, HighlightObjectToggleLabel, HighLightObjectToggleTooltip);
			KingdomStash.OnGUI();
			KingdomInfo.OnGUI();
			ShowTooltip();
		}
		catch (Exception ex)
		{
            Log.Log($"Caught exception:\n{ex}");
			throw ex;
		}
    }
	
	public static void ChooseKingdomUnreset()
	{
		KingdomState instance = KingdomState.Instance;
		
		if (instance == null) return;
		
		var kingdomUnrestName = instance.Unrest == KingdomStatusType.Metastable ? " Serene" : " " + instance.Unrest;
		
		BeginHorizontal();
		Label("Kingdom Unrest: " + kingdomUnrestName, Width(300));
		
		if (Button("More Unrest"))
		{
			if (instance.Unrest != KingdomStatusType.Crumbling)
			{
				instance.SetUnrest(instance.Unrest - 1, KingdomStatusChangeReason.None, modId);
			}
		}
		
		if (Button("Less Unrest"))
		{
			if (instance.Unrest == KingdomStatusType.Metastable) return;
			instance.SetUnrest(instance.Unrest + 1, KingdomStatusChangeReason.None, modId);
		}
		
		EndHorizontal();
	}

    public static void LogDebug(string message)
    {
#if DEBUG
        Log.Log($"DEBUG: {message}");
#endif
    }
}
