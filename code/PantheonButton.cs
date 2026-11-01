using System.IO;
using System;
using NeoModLoader.api;
using NeoModLoader.General;
using NeoModLoader.General.UI.Tab;
using NeoModLoader.ui;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.UI;

namespace OceanOfSouls
{
    internal static class PantheonButton
    {
        private const string IconFile = "GameResources/pantheon.png";
        private const string BackgroundFile = "GameResources/soul_bg.png";
        private const string ConfigIconFile = "GameResources/config.png";
        private const string ConfigButtonId = "oos_config";
        private const string TabIconFile = "GameResources/pantheon_tab.png";
        private const string TabId = "oos_tab";
        private const string TabEntryName = "Button_" + TabId;

        public static void Init(string modFolder, Func<ModConfig> getConfig)
        {
            PantheonArt.Load(modFolder);
            PantheonWindow.AvatarBackground = Load(Path.Combine(modFolder, BackgroundFile));
            PantheonWindow.Create();

            Sprite icon = Load(Path.Combine(modFolder, IconFile));
            Sprite tabIcon = Load(Path.Combine(modFolder, TabIconFile)) ?? icon;

            PowersTab tab = CreateTab(tabIcon) ?? PowerButtonCreator.GetTab(PowerTabNames.Other);
            if (tab == null) return;

            PowerButton button = PowerButtonCreator.CreateWindowButton(PantheonWindow.Id, PantheonWindow.Id, icon);
            PowerButtonCreator.AddButtonToTab(button, tab);

            Sprite configIcon = Load(Path.Combine(modFolder, ConfigIconFile)) ?? icon;
            PowerButton configButton = PowerButtonCreator.CreateSimpleButton(ConfigButtonId, () => OpenConfig(getConfig), configIcon);
            PowerButtonCreator.AddButtonToTab(configButton, tab);
        }

        private static void OpenConfig(Func<ModConfig> getConfig)
        {
            ModConfig config = getConfig?.Invoke();
            if (config == null) return;
            ModConfigureWindow.ShowWindow(config);
        }

        private static PowersTab CreateTab(Sprite icon)
        {
            try
            {
                PowersTab tab = TabManager.CreateTab(TabId, TabId, TabId + " Description", icon);

                GameObject entry = GameObject.Find(TabEntryName);
                Transform entryIcon = entry != null ? entry.transform.Find("Icon") : null;
                Image image = entryIcon != null ? entryIcon.GetComponent<Image>() : null;
                if (image != null)
                {
                    image.preserveAspect = true;
                }
                return tab;
            }
            catch (Exception e)
            {
                OceanOfSoulsMod.LogWarning("Tab setup failed, using the Other tab: " + e.Message);
                return null;
            }
        }

        private static Sprite Load(string path)
        {
            return File.Exists(path) ? SpriteLoadUtils.LoadSingleSprite(path) : null;
        }
    }
}
