﻿using System;
using System.Collections;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;
using MenuLib;
using MenuLib.MonoBehaviors;

namespace ShowTotalLoot.Patches;

[HarmonyPatch(typeof(RoundDirector))]
public class ShowMaxHaulPatch
{
    private static int _currentMaxHaul;
    private static Transform? _gameHud;
    private static REPOLabel? _maxHaulGoalLabel;
    private static bool isBreakRunning = false;
    
    [HarmonyPostfix, HarmonyPatch(typeof(ExtractionPoint), nameof(ExtractionPoint.ActivateTheFirstExtractionPointAutomaticallyWhenAPlayerLeaveTruck))]
    private static void StartRound_Postfix(ExtractionPoint __instance)
    {
        ShowTotalLoot.Logger.LogDebug($"StartRound_Postfix(): {__instance} Start Postfix");
        _currentMaxHaul = 0;
        
        if (!LevelGenerator.Instance.Generated)
        {
            ShowTotalLoot.Logger.LogError("AddEnemyValuable_Postfix(): LevelGenerator has not finished generating the level");
            return;
        }
        if (_gameHud == null)
        {
            ShowTotalLoot.Logger.LogError("AddEnemyValuable_Postfix(): _gameHud is null");
            return;
        }
        if (_maxHaulGoalLabel == null)
        {
            ShowTotalLoot.Logger.LogError("AddEnemyValuable_Postfix(): _maxHaulGoalLabel is null");
            return;
        }
        
        _maxHaulGoalLabel.gameObject.SetActive(true);
        
        foreach (ValuableObject valuable in ValuableDirector.instance.valuableList)
        {
            _currentMaxHaul += (int)valuable.dollarValueCurrent;
        }
        ShowTotalLoot.Logger.LogDebug($"StartRound_Postfix(): Total valuables in map: {ValuableDirector.instance.valuableList.Count}");
        UpdateLabel(_currentMaxHaul);
        ShowTotalLoot.Logger.LogDebug($"Round Max Haul: {_currentMaxHaul}");

    }


    [HarmonyPrefix, HarmonyPatch(typeof(PhysGrabObjectImpactDetector), nameof(PhysGrabObjectImpactDetector.BreakRPC))]
    private static void BreakRPC_Postfix()
    {
        isBreakRunning = true;
    }
    
    [HarmonyPostfix, HarmonyPatch(typeof(PhysGrabObjectImpactDetector), nameof(PhysGrabObjectImpactDetector.BreakRPC))]
    private static void BreakRPC_Postfix(PhysGrabObjectImpactDetector __instance, float valueLost, bool _loseValue)
    {
        isBreakRunning = false;
        if (!__instance.valuableObject) return;
        
        ShowTotalLoot.Logger.LogDebug($"BreakRPC_Postfix(): {__instance} Start Postfix");
        ShowTotalLoot.Logger.LogDebug($"BreakRPC_Postfix(): Item loseValue?: {_loseValue}");
        
        if (!_loseValue) return;
        
        // Check if condition in BreakRPC() has been met (DestoryObject() has been called)
        if (__instance.valuableObject.dollarValueCurrent < __instance.valuableObject.dollarValueOriginal * 0.15f)
        {
            ShowTotalLoot.Logger.LogDebug($"BreakRPC_Postfix(): {__instance.valuableObject.name} Has been destroyed ${valueLost}");
        }
        else
        {
            ShowTotalLoot.Logger.LogDebug($"BreakRPC_Postfix(): {__instance.valuableObject.name} lost ${valueLost}");
        }
            
        _currentMaxHaul -= (int)(valueLost);
        
        UpdateLabel(_currentMaxHaul);
    }
    
    [HarmonyPostfix, HarmonyPatch(typeof(PhysGrabObjectImpactDetector), nameof(PhysGrabObjectImpactDetector.DestroyObjectRPC))]
    private static void DestroyObjectRPC_Postfix(PhysGrabObjectImpactDetector __instance)
    {
        if (!__instance.valuableObject) return;
        if (isBreakRunning) return;
        
        ShowTotalLoot.Logger.LogDebug($"DestroyObjectRPC_Postfix(): {__instance} Start Postfix");
        
        ShowTotalLoot.Logger.LogDebug($"DestroyObjectRPC_Postfix(): {__instance.valuableObject.name} Has been destroyed ${__instance.valuableObject.dollarValueCurrent}");
        _currentMaxHaul -= (int)(__instance.valuableObject.dollarValueCurrent);
        
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
    
    [HarmonyPostfix, HarmonyPatch(typeof(EnemyDirector), nameof(EnemyDirector.AddEnemyValuable))]
    private static void AddEnemyValuable_Postfix(EnemyDirector __instance, EnemyValuable _newValuable)
    {
        ShowTotalLoot.Logger.LogDebug("AddEnemyValuable_Postfix(): Start Postfix");


        if (!LevelGenerator.Instance.Generated)
        {
            ShowTotalLoot.Logger.LogError("AddEnemyValuable_Postfix(): LevelGenerator has not finished generating the level");
            return;
        }
        if (_gameHud == null)
        {
            ShowTotalLoot.Logger.LogError("AddEnemyValuable_Postfix(): _gameHud is null");
            return;
        }
        if (_maxHaulGoalLabel == null)
        {
            ShowTotalLoot.Logger.LogError("AddEnemyValuable_Postfix(): _maxHaulGoalLabel is null");
            return;
        }
        ShowTotalLoot.Instance.StartCoroutine(DelayedAddEnemyValuable(_newValuable));
    }
    
    private static IEnumerator DelayedAddEnemyValuable(EnemyValuable enemyValuable)
    {
        yield return new WaitForSeconds(0.5f);
        ValuableObject valuableObjectComponent = enemyValuable.GetComponent<ValuableObject>();
        
        ShowTotalLoot.Logger.LogDebug($"DelayedAddEnemyValuable2(): EnemyValuable name: {enemyValuable.name}");
        ShowTotalLoot.Logger.LogDebug($"DelayedAddEnemyValuable2(): EnemyValuable name: {valuableObjectComponent.dollarValueOriginal}");

        int addedValue = (int)valuableObjectComponent.dollarValueOriginal;
        _currentMaxHaul += addedValue;
        UpdateLabel(_currentMaxHaul);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(ClownTrap), nameof(ClownTrap.TrapStop))]
    private static void ClowTrapStop_Postfix(ClownTrap __instance)
    {
        ShowTotalLoot.Logger.LogDebug($"ClowTrapStop_Postfix(): {__instance} Start Postfix");

        int clownCurrentValue = (int)__instance.GetComponentInParent<ValuableObject>().dollarValueCurrent;
        
        ShowTotalLoot.Logger.LogDebug($"ClowTrapStop_Postfix(): Clown Current Value: ${clownCurrentValue}");
        _currentMaxHaul -= clownCurrentValue;
        
        UpdateLabel(_currentMaxHaul);
    }
    
    private static void UpdateLabel(int newMaxHaulGoal, [CallerMemberName] string callerName = "")
    {
        ShowTotalLoot.Logger.LogDebug($"{callerName}(): running UpdateLabel()");

        if (_maxHaulGoalLabel == null)
        {
            ShowTotalLoot.Logger.LogError("UpdateLabel(): _maxHaulGoalLabel is null");
            return;
        }
        
        ShowTotalLoot.Logger.LogDebug(_maxHaulGoalLabel);
        ShowTotalLoot.Logger.LogDebug($"New Max Haul Goal: {newMaxHaulGoal}");
        _maxHaulGoalLabel.labelTMP.text = $"Total Value: {newMaxHaulGoal}";
        
    }
}