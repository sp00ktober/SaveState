using HarmonyLib;
using SavePanel.Managers;
using JNM.Storage;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace SavePanel.Patches.Dynamic
{
    [HarmonyPatch(typeof(LoaderButton))]
    class LoaderButton_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LoaderButton), "LoadGame")]
        public static bool LoadGame_Prefix(LoaderButton __instance)
        {
            GameObjectManager.isNewGame = false;
            __instance.loadingPanel.SetActive(true);
            __instance.Invoke("StartLoad", 0.8f);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LoaderButton), "StartLoad")]
        public static bool StartLoad_Prefix(LoaderButton __instance)
        {
            SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LoaderButton), "NewGame")]
        public static bool NewGame_Prefix()
        {
            GameObjectManager.isNewGame = true;
            GameObjectManager.saveName = GetNextSaveName();
            return true;
        }

        public static string GetNextSaveName()
        {
            List<string> saves = LevelManager.Instance.SavedGames;
            if(saves.Count == 0)
            {
                // this is critical, player never had a save before so we need to use the vanilla naming in case the mod is uninstalled
                return "level_storage.xml";
            }
            return "Save" + (saves.Count - 1).ToString() + ".xml";
        }
    }
}
