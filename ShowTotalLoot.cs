using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace ShowTotalLoot;

[BepInPlugin(ModGuid, ModName, ModVersion)]
public class ShowTotalLoot : BaseUnityPlugin
{
    // Mod Details
    private const string ModGuid = "ItsAGeBa.ShowTotalLoot";
    private const string ModName = "Show Total Loot";
    private const string ModVersion = "1.0.2";
    
    internal static ShowTotalLoot Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger => Instance._logger;
    private ManualLogSource _logger => base.Logger;
    internal Harmony? Harmony { get; set; }

    private void Awake()
    {
        Instance = this;
        
        Logger.LogInfo("Hallo This is the Show All Loot Mod");
        
        // Prevent the plugin from being deleted
        this.gameObject.transform.parent = null;
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;
        
        Patch();

        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");
        
    }

    internal void Patch()
    {
        Harmony ??= new Harmony(Info.Metadata.GUID);
        Harmony.PatchAll();
    }

    internal void Unpatch()
    {
        Harmony?.UnpatchSelf();
    }

    private void Update()
    {
        // Code that runs every frame goes here
    }
}