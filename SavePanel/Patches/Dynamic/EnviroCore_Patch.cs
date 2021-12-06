using HarmonyLib;
using SavePanel.Managers;
using UnityEngine;

namespace SavePanel.Patches.Dynamic
{
    [HarmonyPatch(typeof(EnviroCore))]
    class EnviroCore_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnviroCore), "UpdateTime")]
        public static bool UpdateTime_Prefix(EnviroCore __instance, int daysInYear)
        {
            if (GameObjectManager.loadedIntoGame && !GameObjectManager.setTimeValue)
            {
                __instance.GameTime.dayLengthModifier = 0.005f;
                __instance.GameTime.nightLengthModifier = 0.005f;
                GameObjectManager.setTimeValue = true;
            }
            return true;
        }
    }
}
