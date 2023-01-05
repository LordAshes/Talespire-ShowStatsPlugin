using BepInEx;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Mathematics;
using UnityEngine;

namespace LordAshes
{
    public partial class ShowStatsPlugin : BaseUnityPlugin
    {
        private static CreatureGuid lastSelected = CreatureGuid.Empty;

        [HarmonyPatch(typeof(LocalClient), "SetSelectedCreatureId")]
        public static class Patches
        {
            public static bool Prefix(CreatureGuid newSelectedCreatureId)
            {
                if (lastSelected != null)
                {
                    if (makeDiagnsticLogs) { Debug.Log("Show Stats Plugin: Clearing Display On Previous Mini"); }
                    StatMessaging.ClearInfo(lastSelected, links[displayPluginName]);
                }
                if (newSelectedCreatureId != null)
                {
                    if (makeDiagnsticLogs) { Debug.Log("Show Stats Plugin: Setting Active Mini"); }
                    lastSelected = newSelectedCreatureId;
                    lastJson = "";
                }
                return true;
            }
        }
    }
}
