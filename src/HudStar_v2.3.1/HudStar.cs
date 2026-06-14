using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;

namespace HudStar
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency("redseiko.valheim.enhuddlement", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("org.bepinex.plugins.creaturelevelcontrol", BepInDependency.DependencyFlags.SoftDependency)]
    public class HudStarPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "Harchytek.HudStar";
        public const string ModName = "HudStar";
        public const string ModVersion = "2.3.1";
        
        public static ConfigEntry<float> OffsetX;
        public static ConfigEntry<float> OffsetY;
        public static ConfigEntry<float> Scale;

        private static readonly HashSet<string> BossBlacklist = new HashSet<string>
        {
            "Eikthyr", "gd_king", "Bonemass", "Dragon", "GoblinKing", "Queen", "SeekerQueen", "Fader"
        };

        private void Awake()
        {
            OffsetX = Config.Bind("HUD", "StarOffsetX", 0f, "Horizontal offset for all stars.");
            OffsetY = Config.Bind("HUD", "StarOffsetY", 0f, "Vertical offset for all stars.");
            Scale = Config.Bind("HUD", "StarScale", 1f, "Star size scale.");

            new Harmony("harchytek.hudstar").PatchAll();
        }

        [HarmonyPatch(typeof(EnemyHud), "UpdateHuds")]
        public static class EnemyHud_UpdateHuds_Patch
        {
            [HarmonyPriority(Priority.Last)]
            [HarmonyPostfix]
            private static void Postfix(EnemyHud __instance)
            {
                var hudsField = Traverse.Create(__instance).Field("m_huds").GetValue<IDictionary>();
                if (hudsField == null) return;

                foreach (DictionaryEntry entry in hudsField)
                {
                    object hudData = entry.Value;

                    Character character = Traverse.Create(hudData).Field("m_character").GetValue<Character>();
                    if (character != null)
                    {
                        string prefabName = character.gameObject.name.Replace("(Clone)", "").Trim();
                        if (BossBlacklist.Contains(prefabName)) continue;
                    }

                    GameObject m_gui = Traverse.Create(hudData).Field("m_gui").GetValue<GameObject>();
                    if (m_gui == null || !m_gui.activeInHierarchy) continue;

                    for (int i = 0; i < m_gui.transform.childCount; i++)
                    {
                        Transform child = m_gui.transform.GetChild(i);
                        string n = child.name.ToLower();

                        if (n.Contains("level") || n.Contains("star") || n.Contains("darken") || n.Contains("hc_"))
                        {
                            RectTransform rt = child.GetComponent<RectTransform>();
                            if (rt != null)
                            {
                                HudStarMarker marker = child.GetComponent<HudStarMarker>();
                                
                                if (marker == null)
                                {
                                    marker = child.gameObject.AddComponent<HudStarMarker>();
                                    marker.originalPos = rt.anchoredPosition;
                                    marker.originalScale = rt.localScale;
                                }

                                rt.anchoredPosition = marker.originalPos + new Vector2(OffsetX.Value, OffsetY.Value);
                                rt.localScale = marker.originalScale * Scale.Value;
                            }
                        }
                    }
                }
            }
        }

        public class HudStarMarker : MonoBehaviour
        {
            public Vector2 originalPos;
            public Vector3 originalScale;
        }
    }
}