using System.Collections.Generic;
using UnityEngine;

namespace SavePanel.Managers
{
    class GameObjectManager
    {
        public static bool setTimeValue = false;

        public static bool loadedIntoGame = false;
        public static bool loadedIntoSavegame = false;
        public static AssetBundle bundle { get; set; }
        public static List<GameObject> saveMeridians { get; set; }
        public static int nearestMeridian { get; set; }
        public static string saveName { get; set; }
        public static int lockSaving { get; set; }
        public static bool isNewGame { get; set; }

        public static GameObject FindGameObject(string name, int index = 0)
        {
            GameObject[] allObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
            int foundTicker = 0;

            foreach (GameObject go in allObjects)
            {
                if (go.name == name)
                {
                    if(foundTicker < index)
                    {
                        foundTicker++;
                        continue;
                    }
                    return go;
                }
            }
            return null;
        }
    }
}
