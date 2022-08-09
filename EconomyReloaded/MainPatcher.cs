using EconomyReloaded.lang;
using HarmonyLib;
using Helper;
using System;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace EconomyReloaded
{
    public class MainPatcher
    {
        private static Config.Options _cfg;

        public static void Patch()
        {
            try
            {
                var harmony = new Harmony("p1xel8ted.GraveyardKeeper.EconomyReloaded");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                _cfg = Config.GetOptions();
            }
            catch (Exception ex)
            {
                Log($"{ex.Message}, {ex.Source}, {ex.StackTrace}", true);
            }
        }

        private static void Log(string message, bool error = false)
        {
            if (_cfg.Debug || error)
            {
                Tools.Log("EconomyReloaded", $"{message}", error);
            }
        }

        [HarmonyPatch(typeof(Trading))]
        internal class TradingPatches
        {
            [HarmonyPatch(nameof(Trading.GetSingleItemCostInTraderInventory), typeof(Item), typeof(int))]
            [HarmonyPostfix]
            public static void TraderPostfix(ref float __result, Item item)
            {
                if (!_cfg.DisableInflation) return;
                if (__result != 0.0)
                {
                    __result = item.definition.base_price;
                }
            }

            [HarmonyPatch(nameof(Trading.GetSingleItemCostInPlayerInventory), typeof(Item), typeof(int))]
            [HarmonyPostfix]
            public static void PlayerPostfix(ref float __result, Item item)
            {
                if (!_cfg.DisableDeflation) return;
                if (__result != 0.0)
                {
                    __result = item.definition.base_price;
                }
            }
        }

        private static string GetLocalizedString(string content)
        {
            Thread.CurrentThread.CurrentUICulture = CrossModFields.Culture;
            return content;
        }

        [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.Update))]
        public static class TimeOfDayUpdatePatch
        {
            [HarmonyPrefix]
            public static void Prefix()
            {
                if (Input.GetKeyUp(KeyCode.F5))
                {
                    _cfg = Config.GetOptions();

                    if (!CrossModFields.ConfigReloadShown)
                    {
                        Tools.ShowMessage(GetLocalizedString(strings.ConfigMessage), Vector3.zero);
                        CrossModFields.ConfigReloadShown = true;
                    }
                }
            }
        }
    }
}