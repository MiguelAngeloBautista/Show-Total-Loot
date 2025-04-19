using System;
using System.Runtime.CompilerServices;
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
        ShowTotalLoot.Logger.LogDebug($"StartRound_Postfix(): {__instance} Start Postfix");
        
        if (!LevelGenerator.Instance.Generated)
            throw new InvalidOperationException("StartRoundLogic_Postfix(): LevelGenerator has not finished generating the level");
        if (_gameHud == null)
            throw new NullReferenceException("StartRoundLogic_Postfix(): _gameHud is null");
        if (_maxHaulGoalLabel == null)
            throw new NullReferenceException("StartRoundLogic_Postfix(): _maxHaulGoalLabel is null");
        
        _maxHaulGoalLabel.gameObject.SetActive(true);
        
        foreach (ValuableObject valuable in ValuableDirector.instance.valuableList)
        {
            _currentMaxHaul += (int)valuable.dollarValueCurrent;
        }
        ShowTotalLoot.Logger.LogDebug($"StartRound_Postfix(): Total valuables in map: {ValuableDirector.instance.valuableList.Count}");

        _currentMaxHaul = RoundDirector.instance.haulGoalMax;

        UpdateLabel(_currentMaxHaul);
        ShowTotalLoot.Logger.LogDebug($"Round Max Haul: {_currentMaxHaul}");

    }

    [HarmonyPostfix, HarmonyPatch(typeof(PhysGrabObjectImpactDetector), nameof(PhysGrabObjectImpactDetector.BreakRPC))]
    private static void BreakRPC_Postfix(PhysGrabObjectImpactDetector __instance)
    {
        if (!__instance.valuableObject) return;
        
        ShowTotalLoot.Logger.LogDebug($"BreakRPC_Postfix(): {__instance} Start Postfix");
        
        _currentMaxHaul -= (int)(__instance.valuableObject.dollarValueOriginal - __instance.valuableObject.dollarValueCurrent);
        
        UpdateLabel(_currentMaxHaul);

    }

    [HarmonyPostfix, HarmonyPatch(typeof(LoadingUI), nameof(LoadingUI.StopLoading))]
    private static void StopLoading_Postfix(LoadingUI __instance)
    {
        ShowTotalLoot.Logger.LogDebug($"StopLoading_Postfix(): {__instance} Start Postfix");
        
        _gameHud = GameObject.Find("Game Hud").transform;
        _maxHaulGoalLabel = MenuAPI.CreateREPOLabel("Total Value", _gameHud, localPosition: new Vector2(146, -200));
        _maxHaulGoalLabel.gameObject.SetActive(false);
        ShowTotalLoot.Logger.LogDebug($"maxHaulGoalLabel {_maxHaulGoalLabel}");
    }
    
    private static void UpdateLabel(int newMaxHaulGoal, [CallerMemberName] string callerName = "")
    {
        ShowTotalLoot.Logger.LogDebug($"{callerName}(): running UpdateLabel()");
        
        if (_maxHaulGoalLabel == null)
            throw new NullReferenceException("UpdateLabel(): _maxHaulGoalLabel is null");
        
        ShowTotalLoot.Logger.LogDebug(_maxHaulGoalLabel);
        ShowTotalLoot.Logger.LogDebug($"New Max Haul Goal: {newMaxHaulGoal}");
        _maxHaulGoalLabel.labelTMP.text = $"Total Value: {newMaxHaulGoal}";
        
    }
}