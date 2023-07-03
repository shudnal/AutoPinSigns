﻿using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace AutoPinSigns
{
    [BepInPlugin(pluginID, pluginName, pluginVersion)]
    public class AutoPinSigns : BaseUnityPlugin
    {
        const string pluginID = "shudnal.AutoPinSigns";
        const string pluginName = "Auto Pin Signs";
        const string pluginVersion = "1.0.0";
        public static ManualLogSource logger;

        private Harmony _harmony;

        private static bool modEnabled;

        public static Dictionary<Vector3, string> itemsPins = new Dictionary<Vector3, string>();
        public static Dictionary<string, bool> FireList = new Dictionary<string, bool>();
        public static Dictionary<string, bool> BaseList = new Dictionary<string, bool>();
        public static Dictionary<string, bool> HammerList = new Dictionary<string, bool>();
        public static Dictionary<string, bool> PinList = new Dictionary<string, bool>();
        public static Dictionary<string, bool> PortalList = new Dictionary<string, bool>();

        private void Awake()
        {

            modEnabled = Config.Bind("General", "Enabled", defaultValue: true, "Enable the mod").Value;

            string section = "Signs";

            string configFireList = Config.Bind(section, "FireList", "fire", "List of the case-insensitive strings to add Fire pin.  Comma-separate each string.  Default: fire").Value;
            string configBaseList = Config.Bind(section, "BaseList", "base", "List of the case-insensitive strings to add Base pin.  Comma-separate each string.  Default: base").Value;
            string configHammerList = Config.Bind(section, "HammerList", "hammer", "List of the strings to add Hammer pin.  Comma-separate each string.  Default: hammer").Value;
            string configPinList = Config.Bind(section, "PinList", "pin,dot", "List of the strings to add Dot pin.  Comma-separate each string.  Default: pin,dot").Value;
            string configPortalList = Config.Bind(section, "PortalList", "portal", "List of the strings to add Portal pin.  Comma-separate each string.  Default: portal").Value;

            if (!modEnabled)
            {
                return;
            }

            AddToDict(configFireList, FireList);
            AddToDict(configBaseList, BaseList);
            AddToDict(configHammerList, HammerList);
            AddToDict(configPinList, PinList);
            AddToDict(configPortalList, PortalList);

            logger = Logger;

            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), pluginID);

        }
        private void OnDestroy()
        {
            Config.Save();
            _harmony?.UnpatchSelf();
        }

        static bool IsFireSign(string text)
        {
            return FireList.ContainsKey(text);
        }
        static bool IsBaseSign(string text)
        {
            return BaseList.ContainsKey(text);
        }
        static bool IsHammerSign(string text)
        {
            return HammerList.ContainsKey(text);
        }
        static bool IsPinSign(string text)
        {
            return PinList.ContainsKey(text);
        }
        static bool IsPortalSign(string text)
        {
            return PortalList.ContainsKey(text);
        }

        static bool IsPinnableSign(string text)
        {
            return IsFireSign(text) || IsBaseSign(text) || IsHammerSign(text) || IsPinSign(text) || IsPortalSign(text);
        }

        static Minimap.PinType GetIcon(string text)
        {
            if (IsFireSign(text))
                return Minimap.PinType.Icon0;
            if (IsBaseSign(text))
                return Minimap.PinType.Icon1;
            if (IsHammerSign(text))
                return Minimap.PinType.Icon2;
            if (IsPinSign(text))
                return Minimap.PinType.Icon3;
            if (IsPortalSign(text))
                return Minimap.PinType.Icon4;

            return Minimap.PinType.Icon3;
        }

        static void AddToDict(string text, Dictionary<string, bool> Dict)
        {
            char[] separator = new char[1] { ',' };
            foreach (string item in text.Replace(" ", "").Split(separator, StringSplitOptions.RemoveEmptyEntries))
            {
                Dict.Add(item, true);
            }
        }

        static public void DeleteClosestPins(Vector3 pos)
        {
            while (FindAndDeleteClosestPin(pos)) { };
        }

        static public bool FindAndDeleteClosestPin(Vector3 pos)
        {
            foreach (Minimap.PinData pin in Minimap.instance.m_pins)
            {
                if (IsPinnableSign(pin.m_name))
                {
                    if (Utils.DistanceXZ(pos, pin.m_pos) < 2.0f)
                    {
                        Minimap.instance.RemovePin(pin);
                        return true;
                    }
                }
            }
            itemsPins.Remove(pos);
            return false;
        }

        [HarmonyPatch(typeof(Minimap), nameof(Minimap.AddPin))]
        public static class Minimap_AddPin_Patch
        {
            static bool Prefix(ref Minimap __instance, List<Minimap.PinData> ___m_pins, Vector3 pos, Minimap.PinType type, string name, bool save)
            {
                if (__instance.HaveSimilarPin(pos, type, name, save))
                    return false;
                else
                    return true;
            }

        }

        [HarmonyPatch(typeof(Sign), nameof(Sign.UpdateText))]
        public static class Sign_UpdateText_patch
        {
            static void Postfix(ref Sign __instance)
            {
                string text = (string)AccessTools.Method(typeof(Sign), "GetText").Invoke(__instance, new object[] { });
                string lowertext = text.ToLower();

                Vector3 pos = __instance.transform.position;

                if (itemsPins.ContainsKey(pos))
                {
                    string currenttext = itemsPins[pos];

                    if (!IsPinnableSign(lowertext))
                    {
                        DeleteClosestPins(pos);
                        return;
                    }
                    else if (currenttext != lowertext)
                    {
                        DeleteClosestPins(pos);
                    }
                    else return;

                }

                if (!IsPinnableSign(lowertext))
                {
                    return;
                }

                Minimap.PinType icon = GetIcon(lowertext);

                Minimap.instance.AddPin(pos, icon, lowertext, true, false, 0L);
                itemsPins.Add(pos, lowertext);

                Sprite m_icon_sprite = Minimap.instance.GetSprite(icon);

                if (Player.m_localPlayer != null)
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "$msg_pin_added: " + text, 0, m_icon_sprite);
                }

            }
        }

        [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.Destroy))]
        public static class WearNTear_OnDestroy_patch
        {
            static void Postfix(ref WearNTear __instance)
            {
                Sign component = __instance.GetComponent<Sign>();

                if (component != null)
                {
                    string text = (string)AccessTools.Method(typeof(Sign), "GetText").Invoke(component, new object[] { });
                    text = text.ToLower();

                    if (IsPinnableSign(text))
                    {
                        Vector3 pos = __instance.transform.position;
                        if (itemsPins.ContainsKey(pos))
                        {
                            DeleteClosestPins(pos);
                        }
                    }
                }


            }

        }
    }
}