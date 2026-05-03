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
    [BepInPlugin("harchytek.hudstar", "HudStar", "2.0.0")]
    public class HudStar : BaseUnityPlugin
    {
        public static ConfigEntry<float> OffsetX;
        public static ConfigEntry<float> OffsetY;
        public static ConfigEntry<float> Scale;

        private void Awake()
        {
            // Configuration binding
            OffsetX = Config.Bind("HUD", "StarOffsetX", 0f, "Horizontal offset for all stars.");
            OffsetY = Config.Bind("HUD", "StarOffsetY", 0f, "Vertical offset for all stars.");
            Scale = Config.Bind("HUD", "StarScale", 1f, "Star size scale.");

            // Apply patches
            new Harmony("harchytek.hudstar").PatchAll();
            Logger.LogInfo("HudStar Loaded: Group-based star alignment active.");
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

                    // Find or create the custom group container
                    GameObject starGroup = m_gui.transform.Find("HudStar_Group")?.gameObject;
                    if (starGroup == null)
                    {
                        starGroup = new GameObject("HudStar_Group", typeof(RectTransform));
                        starGroup.transform.SetParent(m_gui.transform, false);
                        starGroup.AddComponent<StarForceUpdater>();
                    }

                    // Move all star-related objects into the custom container
                    foreach (Transform child in m_gui.transform)
                    {
                        string n = child.name.ToLower();
                        // Capture vanilla stars, CLLC stars, and shadows
                        if ((n.Contains("level") || n.Contains("star") || n.Contains("darken") || n.Contains("hc_")) && n != "hudstar_group")
                        {
                            child.SetParent(starGroup.transform, false);
                        }
                    }
                }
            }
        }

        public class StarForceUpdater : MonoBehaviour
        {
            private RectTransform rt;
            private const float BaseX = 0f; // Standardized center position
            private const float BaseY = 0f;

            void Awake() { rt = GetComponent<RectTransform>(); }

            void LateUpdate()
            {
                if (rt == null || !gameObject.activeInHierarchy) return;

                // Force anchors and pivot to center for predictable positioning
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);

                // Apply position and scale from config
                rt.anchoredPosition = new Vector2(BaseX + OffsetX.Value, BaseY + OffsetY.Value);
                rt.localScale = Vector3.one * Scale.Value;
            }
        }
    }
}
