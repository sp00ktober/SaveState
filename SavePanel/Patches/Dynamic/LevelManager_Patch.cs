using HarmonyLib;
using JNM.Storage;
using SavePanel.Managers;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace SavePanel.Patches.Dynamic
{
    [HarmonyPatch(typeof(LevelManager))]
    class LevelManager_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LevelManager), "SaveGame")]
        public static bool SaveGame_Prefix(LevelManager __instance, string name)
        {
            if (GameObjectManager.saveName == "noneSelectedYet")
            {
                // prevent saving in middle world
                return false;
            }
            else
            {
                if(GameObjectManager.lockSaving == 0)
                {
                    GameObjectManager.lockSaving++;
                    __instance.SaveGame(GameObjectManager.saveName);
                    GameObjectManager.lockSaving--;

                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LevelManager), "SavedGames", MethodType.Getter)]
        public static bool get_SavedGames_Prefix(LevelManager __instance, ref List<string> __result)
        {
            List<string> list = new List<string>();
            string text = Path.DirectorySeparatorChar.ToString();
            string persistentDataPath = Application.persistentDataPath;

            // up to 21 saves allowed for now, the first one is the vanilla save
            for(int i = -1; i < 20; i++)
            {
                string path = string.Concat(new string[]
                {
                    persistentDataPath,
                    text,
                    (string)AccessTools.Field(typeof(LevelManager), "_currentName").GetValue(__instance),
                    text,
                    (i == -1) ? "level_storage.xml" : "Save" + i.ToString() + ".xml"
                });
                if (File.Exists(path))
                {
                    list.Add(new DirectoryInfo(path).Name);
                }
            }

            __result = list;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LevelManager), "GetCurrentLevel")]
        public static bool GetCurrentLevel_Prefix(LevelManager __instance)
        {
            string text = Path.DirectorySeparatorChar.ToString();
            string path = string.Concat(new string[]
            {
                Application.persistentDataPath,
                text,
                (string)AccessTools.Field(typeof(LevelManager), "_currentName").GetValue(__instance),
                text,
                GameObjectManager.saveName
            });

            FileStream fs = null;

            if (!File.Exists(path))
            {
                fs = File.Create(path);
            }

            if(fs != null)
            {
                string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?><config><currentlevel name=\"\"/><levels></levels></config>";
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xml);
                xmlDocument.Save(fs);

                fs.Position = 0; // read file from start
                ((XmlDocument)AccessTools.Field(typeof(LevelManager), "_levelFile").GetValue(__instance)).Load(fs);
                fs.Close();
            }
            else
            {
                ((XmlDocument)AccessTools.Field(typeof(LevelManager), "_levelFile").GetValue(__instance)).Load(path);
            }

            XmlNode xmlNode = ((XmlDocument)AccessTools.Field(typeof(LevelManager), "_levelFile").GetValue(__instance)).SelectSingleNode("//config/currentlevel");
            if (xmlNode != null)
            {
                AccessTools.Field(typeof(LevelManager), "_currentLevel").SetValue(__instance, xmlNode.Attributes["name"].Value);
            }

            AccessTools.Field(typeof(LevelManager), "_currentFilePath").SetValue(__instance, path);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LevelManager), "EnsureStorageExists")]
        public static bool EnsureStorageExists_Prefix(LevelManager __instance, bool createFile)
        {
            // i check for the file in the patch above, altho its a less good check.
            // maybe improve that later but the main reason to skip here is because it sets _currentFilePath which leads to overwriting the wrong save.
            return false;
        }
    }
}
