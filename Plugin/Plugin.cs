using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace LordAshes
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency(LordAshes.StatMessaging.Guid)]
    [BepInDependency(LordAshes.CustomStatsPlugin.Guid)]
    public partial class ShowStatsPlugin : BaseUnityPlugin
    {
        // Plugin info
        public const string Name = "Show Stats Plug-In";              
        public const string Guid = "org.lordashes.plugins.showstats";
        public const string Version = "3.3.0.0";

        // External plugins
        public enum ContentType
        {
            player = 0,
            gm = 1,
        }

        private static Dictionary<string, string> links = new Dictionary<string, string>()
        {
            {"GM INFO","org.lordashes.plugins.gminfo" },
            {"STATES","org.lordashes.plugins.states" },
            {"OVERHEADNOTESPLAYER","org.lordashes.plugins.overheadnotes."+ContentType.player },
            {"OVERHEADNOTESGM","org.lordashes.plugins.overheadnotes."+ContentType.gm },
        };

        // Configuration
        private static string displayPluginName { get; set; }
        private static bool makeDiagnsticLogs { get; set; }

        // Variables
        private static string lastJson = "";
        private static bool pluginOkay = false;

        void Awake()
        {
            UnityEngine.Debug.Log("Show Stats Plugin: "+this.GetType().AssemblyQualifiedName+" Active. (Diagnostics="+makeDiagnsticLogs.ToString()+")");

            displayPluginName = Config.Bind("Settings", "Show Stats Via Plugin", "GM Info").Value.ToUpper();
            makeDiagnsticLogs = Config.Bind("Settings", "Make Diagnostic Logs", false).Value;

            if (!links.ContainsKey(displayPluginName))
            {
                pluginOkay = false;
                if (makeDiagnsticLogs)
                {
                    foreach (string option in links.Keys)
                    {
                        Debug.LogError(new Exception("Show Stats Plugin: Known Display Option = '" + option + "'"));
                    }
                }
                Debug.LogException(new Exception("Show Stats Plugin: Selected Display '" + displayPluginName + "' Plugin Unknown."));
            }
            else
            {
                pluginOkay = true;
                var harmony = new Harmony(Guid);
                harmony.PatchAll();
                Utility.PostOnMainPage(this.GetType());
            }
        }

        void Update()
        {
            if (pluginOkay && Utility.isBoardLoaded())
            {
                if (lastSelected != null)
                {
                    try
                    {
                        string json = "";
                        try { json = StatMessaging.ReadInfo(lastSelected, CustomStatsPlugin.Guid + ".stats"); } catch {; }
                        if (json != null && json != "" && json!=lastJson)
                        {
                            Dictionary<string, object> stats = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                            Debug.Log("Show Stats Plugin: Update States For Active Mini ("+json+")");
                            string msg = "";
                            foreach (KeyValuePair<string, object> stat in stats)
                            {
                                msg += stat.Key + " = " + stat.Value + ",";
                            }
                            if (makeDiagnsticLogs) { Debug.Log("Show Stats Plugin: Processor = " + links[displayPluginName] + ", Displaying = " + msg); }
                            StatMessaging.SetInfo(lastSelected, links[displayPluginName], msg);
                            lastJson = json;
                        }
                    }
                    catch
                    {
                        Debug.Log("Show Stats Plugin: Previously Active Mini Is Not Avaialble");
                        lastSelected = CreatureGuid.Empty;
                    }
                }
            }
        }
    }
}
