using System;
using HarmonyLib;
using UnityEngine;
using MenuLib;
using MenuLib.MonoBehaviors;
namespace ShowTotalLoot.Patches;

[HarmonyPatch(typeof(RoundDirector))]
public class ShowMaxHaulPatch
{
    private static int _currentMaxHaul = 0;
    private static Transform? _gameHud;
    private static REPOLabel? _maxHaulGoalLabel;
    
    [HarmonyPostfix, HarmonyPatch(typeof(ExtractionPoint), nameof(ExtractionPoint.ActivateTheFirstExtractionPointAutomaticallyWhenAPlayerLeaveTruck))]
    private static void StartRound_Postfix(ExtractionPoint __instance)
    {
        _maxHaulGoalLabel.gameObject.SetActive(true);
        
        foreach (ValuableObject valuable in ValuableDirector.instance.valuableList)
        {
            _currentMaxHaul += (int)valuable.dollarValueCurrent;
        }
        ShowTotalLoot.Logger.LogInfo($"{__instance} Start Postfix");

        _currentMaxHaul = RoundDirector.instance.haulGoalMax;

        UpdateLabel(_currentMaxHaul);
        
        ShowTotalLoot.Logger.LogInfo($"Round Max Haul: {_currentMaxHaul}");
    
    }

    [HarmonyPostfix, HarmonyPatch(typeof(PhysGrabObjectImpactDetector), nameof(PhysGrabObjectImpactDetector.BreakRPC))]
    private static void BreakRPC_Postfix(PhysGrabObjectImpactDetector __instance)
    {
        if (__instance.valuableObject)
        {
            ShowTotalLoot.Logger.LogInfo($"{__instance} Start Postfix");

            float valueLost = __instance.valuableObject.dollarValueOriginal - __instance.valuableObject.dollarValueCurrent;
            _currentMaxHaul -= (int)valueLost;
        _currentMaxHaul -= (int)(__instance.valuableObject.dollarValueOriginal - __instance.valuableObject.dollarValueCurrent);
        
            UpdateLabel(_currentMaxHaul);
        }
        else
        {
            ShowTotalLoot.Logger.LogInfo($"{__instance} is not a ValuableObject");
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(LoadingUI), nameof(LoadingUI.StopLoading))]
    private static void StopLoading_Postfix(LoadingUI __instance)
    {
        ShowTotalLoot.Logger.LogInfo($"{__instance} Start Postfix");
        
        _gameHud = GameObject.Find("Game Hud").transform;
        _maxHaulGoalLabel = MenuAPI.CreateREPOLabel("Total Value", _gameHud, localPosition: new Vector2(146, -200));
        ShowTotalLoot.Logger.LogInfo($"maxHaulGoalLabel {_maxHaulGoalLabel}");
        _maxHaulGoalLabel.gameObject.SetActive(false);
    }
    
    private static void UpdateLabel(int newMaxHaulGoal)
    {
        if (_maxHaulGoalLabel == null)
        {
            ShowTotalLoot.Logger.LogError("UpdateLabel(): MaxHaulGoalLabel is null");
        }

        ShowTotalLoot.Logger.LogInfo(_maxHaulGoalLabel);
        ShowTotalLoot.Logger.LogInfo($"New Max Haul Goal: {newMaxHaulGoal}");
        _maxHaulGoalLabel.labelTMP.text = $"Total Value: {newMaxHaulGoal}";
        
    }
}