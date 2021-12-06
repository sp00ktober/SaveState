using BepInEx;
using HarmonyLib;
using JNM.Storage;
using SavePanel.Managers;
using System.Collections;
using UnityEngine;

/*
 * huge thanks to all who helped me with the meridian model, especially essu from the BepInEx server who actually solved every issue in the end!
 */

namespace SavePanel.Patches.Dynamic
{
    [HarmonyPatch(typeof(GeneralManager))]
    class GeneralManager_Patch
    {
        private static float nearbyTimer = 0;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GeneralManager), "Update")]
        public static void Update_Postfix(GeneralManager __instance)
        {
            if (GameObjectManager.isNewGame)
            {
                return;
            }

            if (!GameObjectManager.loadedIntoGame)
            {
                GameObjectManager.bundle = AssetBundle.LoadFromFile(Paths.PluginPath + "\\meridian");

                GameObject meridianBase = GameObjectManager.bundle.LoadAsset<GameObject>("Assets/Prefabs/blender.prefab");
                GameObject orig = GameObjectManager.FindGameObject("meridian_02", 2);

                GameObjectManager.bundle.Unload(false);

                int offsetCounter = 0;

                for (int i = 0; i < LevelManager.Instance.SavedGames.Count; i++)
                {
                    GameObject meridian = GameObject.Instantiate(meridianBase);

                    meridian.transform.Find("meridian_02").GetComponent<MeshRenderer>().sharedMaterial = orig.GetComponent<MeshRenderer>().sharedMaterial;

                    meridian.transform.position = __instance.MainPlayer.transform.position + new Vector3((i % 2 == 0) ? 10 : -10, 0, 20 + (7 * offsetCounter));
                    meridian.transform.rotation = new Quaternion(0, -0.35f, 0, 1);

                    if ((i - 1) % 2 == 0)
                    {
                        offsetCounter++;
                    }

                    GameObjectManager.saveMeridians.Add(meridian);
                }

                GameObjectManager.nearestMeridian = -1;
                GameObjectManager.saveName = "noneSelectedYet";
                GameObjectManager.lockSaving = 0;
                GameObjectManager.loadedIntoGame = true;
            }
            else if(GameObjectManager.saveName == "noneSelectedYet")
            {
                float nearest = 0;
                for(int i = 0; i < GameObjectManager.saveMeridians.Count; i++)
                {
                    float distanceFromCamera = Vector3.Distance(GameObjectManager.saveMeridians[i].transform.position, EnviroSky.instance.PlayerCamera.transform.position);
                    if(nearest == 0)
                    {
                        nearest = distanceFromCamera;
                        GameObjectManager.nearestMeridian = i;
                    }
                    else if(distanceFromCamera < nearest)
                    {
                        nearest = distanceFromCamera;
                        GameObjectManager.nearestMeridian = i;
                    }
                }

                if(nearest < 3)
                {
                    nearbyTimer += Time.deltaTime;
                }
                else
                {
                    nearbyTimer = 0;
                }

                // wait 5 secs until loading save
                if(nearbyTimer > 5f)
                {
                    GameObjectManager.saveName = LevelManager.Instance.SavedGames[GameObjectManager.nearestMeridian];
                    SpawnDeath(__instance);
                }
            }
        }

        private static void SpawnDeath(GeneralManager manager)
        {
            Vector3 playerPos = manager.MainPlayer.transform.position;

            ScoloManager sm = (ScoloManager)AccessTools.Field(typeof(GeneralManager), "GestScolo").GetValue(GameObject.FindGameObjectWithTag("manager").GetComponent<GeneralManager>());
            CameraShake cs = (CameraShake)AccessTools.Field(typeof(ScoloManager), "cameraShake").GetValue(sm);

            cs.StartShake(10f, 1f);
            sm.GetComponent<AudioSource>().Play();

            manager.StartCoroutine(LoadGame());
        }

        private static IEnumerator LoadGame()
        {
            yield return new WaitForSecondsRealtime(8);
            GameObjectManager.loadedIntoSavegame = true;
            LevelManager.Instance.LoadGame(GameObjectManager.saveName);
        }
    }
}
