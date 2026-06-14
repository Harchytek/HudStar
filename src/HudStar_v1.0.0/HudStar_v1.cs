// Project: HudStar | Author: Harchytek | License: GNU GPL v3
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace HudStarHarchytek
{
    [BepInPlugin("harchytek.hudstar", "HudStar", "1.0.0")]
    public class HudStar : BaseUnityPlugin
    {
        // Internal reference values
        private const float BaseX = -33f;
        private const float BaseY = -7f;
        private const float BaseExtraX = -8.5f;

        public ConfigEntry<float> OffsetX;
        public ConfigEntry<float> OffsetY;
        public ConfigEntry<float> Scale;
        public ConfigEntry<float> ExtraX_1Star;

        private float _timer = 0f;
        private const float ScanInterval = 0.5f; // Reduced to 0.2s for greater responsiveness.
        private GameObject _enemyHudBase;
        private readonly List<RectTransform> _starRoots = new List<RectTransform>();

        private void Awake()
        {
            // Configuration
            OffsetX = Config.Bind("HUD", "StarOffsetX", 1f, "Global horizontal adjustment.");
            OffsetY = Config.Bind("HUD", "StarOffsetY", 1f, "Global vertical adjustment.");
            ExtraX_1Star = Config.Bind("HUD", "StarOnly1_ExtraX", 1f, "Extra X adjustment only for 1-star enemies.");
            Scale = Config.Bind("HUD", "StarScale", 1f, "Star size scale.");
        }

        private void Update()
        {
            // 1. Fast detection in Update
            _timer += Time.deltaTime;
            if (_timer >= ScanInterval)
            {
                _timer = 0f;
                RefreshCache();
            }
        }

        private void LateUpdate()
        {
            // 2. Forced application in LateUpdate to overwrite game moves
            if (_starRoots.Count == 0) return;

            float finalCommonX = BaseX + (OffsetX.Value - 1f);
            float finalCommonY = BaseY + (OffsetY.Value - 1f);
            float finalSpecialX = finalCommonX + BaseExtraX + (ExtraX_1Star.Value - 1f);
            Vector3 targetScale = Vector3.one * Scale.Value;

            for (int i = _starRoots.Count - 1; i >= 0; i--)
            {
                RectTransform rt = _starRoots[i];
                if (rt == null)
                {
                    _starRoots.RemoveAt(i);
                    continue;
                }

                if (rt.gameObject.activeInHierarchy)
                {
                    // Check if it's a 1* enemy (one star child in the root)
                    bool isSingleStar = (rt.childCount <= 1);
                    float x = isSingleStar ? finalSpecialX : finalCommonX;

                    rt.anchoredPosition = new Vector2(x, finalCommonY);
                    rt.localScale = targetScale;
                }
            }
        }

        private void RefreshCache()
        {
            if (_enemyHudBase == null)
            {
                _enemyHudBase = GameObject.Find("EnemyHud");
                if (_enemyHudBase == null) return;
            }

            _starRoots.Clear();
            RectTransform[] all = _enemyHudBase.GetComponentsInChildren<RectTransform>(true);

            foreach (var rt in all)
            {
                if (rt == null) continue;
                string name = rt.name;

                // Identification of star roots
                if (name.IndexOf("hc_", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.IndexOf("star", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.IndexOf("level", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    bool isRootParent = true;
                    if (rt.parent != null)
                    {
                        string pName = rt.parent.name;
                        if (pName.IndexOf("hc_", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                            pName.IndexOf("star", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                            pName.IndexOf("level", System.StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            isRootParent = false;
                        }
                    }

                    if (isRootParent)
                    {
                        // Disabling Unity components that force the position
                        if (rt.TryGetComponent<ContentSizeFitter>(out var fitter)) fitter.enabled = false;
                        if (rt.TryGetComponent<HorizontalLayoutGroup>(out var layout)) layout.enabled = false;
                        _starRoots.Add(rt);
                    }
                }
            }
        }
    }
}