using HarmonyLib;
using SavePanel.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace SavePanel.Patches.Dynamic
{
    [HarmonyPatch(typeof(StoreManager))]
    class StoreManager_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StoreManager), "EnableLoading")]
        public static bool EnableLoading_Prefix(bool enable)
        {
            if (GameObjectManager.saveMeridians != null)
            {
                GameObjectManager.saveMeridians.Clear();
            }
            else
            {
                GameObjectManager.saveMeridians = new List<GameObject>();
            }
            GameObjectManager.nearestMeridian = -1;
            GameObjectManager.saveName = "noneSelectedYet";
            GameObjectManager.lockSaving = 0;
            GameObjectManager.loadedIntoGame = false;
            GameObjectManager.loadedIntoSavegame = false;

            GameObjectManager.setTimeValue = false;

            return true;
        }
    }
}
