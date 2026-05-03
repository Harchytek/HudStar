// Project: HudStar | Author: Harchytek | License: GNU GPL v3
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;

namespace HudStarHarchytek
{
    [BepInPlugin("harchytek.hudstar", "HudStar", "2.1.0")]
    public class HudStar : BaseUnityPlugin
    {
        public static ConfigEntry<float> OffsetX;
        public static ConfigEntry<float> OffsetY;
        public static ConfigEntry<float> Scale;

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
            [HarmonyPostfix]
            private static void Postfix(EnemyHud __instance)
            {
                var hudsField = Traverse.Create(__instance).Field("m_huds").GetValue<IDictionary>();
                if (hudsField == null) return;

                foreach (DictionaryEntry entry in hudsField)
                {
                    object hudData = entry.Value;

                    GameObject m_gui = Traverse.Create(hudData).Field("m_gui").GetValue<GameObject>();
                    if (m_gui == null || !m_gui.activeInHierarchy) continue;

                    GameObject starGroup = m_gui.transform.Find("HudStar_Group")?.gameObject;
                    RectTransform groupRT;

                    if (starGroup == null)
                    {
                        starGroup = new GameObject("HudStar_Group", typeof(RectTransform));
                        starGroup.transform.SetParent(m_gui.transform, false);
                        groupRT = starGroup.GetComponent<RectTransform>();
                        
                        SetupRectTransform(groupRT);
                        
                        starGroup.AddComponent<StarForceUpdater>();
                    }
                    else
                    {
                        groupRT = starGroup.GetComponent<RectTransform>();
                    }

                    ApplyInstantPosition(groupRT);

                    foreach (Transform child in m_gui.transform)
                    {
                        string n = child.name.ToLower();
                        if ((n.Contains("level") || n.Contains("star") || n.Contains("darken") || n.Contains("hc_")) && n != "hudstar_group")
                        {
                            child.SetParent(starGroup.transform, false);
                        }
                    }
                }
            }

            private static void SetupRectTransform(RectTransform rt)
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
            }

            private static void ApplyInstantPosition(RectTransform rt)
            {
                rt.anchoredPosition = new Vector2(OffsetX.Value, OffsetY.Value);
                rt.localScale = Vector3.one * Scale.Value;
            }
        }

        public class StarForceUpdater : MonoBehaviour
        {
            private RectTransform rt;
            void Awake() { rt = GetComponent<RectTransform>(); }

            void LateUpdate()
            {
                if (rt == null || !gameObject.activeInHierarchy) return;

                rt.anchoredPosition = new Vector2(OffsetX.Value, OffsetY.Value);
                rt.localScale = Vector3.one * Scale.Value;
            }
        }
    }
}