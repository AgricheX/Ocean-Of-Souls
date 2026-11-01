using System;
using System.Collections.Generic;
using NeoModLoader.api;
using NeoModLoader.General;
using NeoModLoader.General.UI.Prefabs;
using UnityEngine;
using UnityEngine.UI;

namespace OceanOfSouls
{
    internal class PantheonWindow : AbstractWindow<PantheonWindow>
    {
        public const string Id = "oos_pantheon";

        public static Sprite AvatarBackground;

        private const int PageSize = 7;
        private const float RowWidth = 214f;
        private const float RowHeight = 30f;
        private const float ButtonWidth = 40f;
        private const float AvatarSlot = 28f;
        private const float AvatarScale = 0.6f;
        private const float InfoWidth = 92f;

        private sealed class Row
        {
            public GameObject Root;
            public Text Info;
            public Image Background;
            public Image Badge;
            public UiUnitAvatarElement Avatar;
            public SimpleButton Revive;
            public SimpleButton Reincarnate;
        }

        private readonly List<Row> rows = new List<Row>();
        private readonly SimpleButton[] filterButtons = new SimpleButton[SoulLevels.Count + 1];
        private readonly List<SoulEntry> filtered = new List<SoulEntry>();
        private readonly List<string> worldIds = new List<string>();
        private readonly List<string> worldNames = new List<string>();
        private readonly List<int> worldCounts = new List<int>();

        private readonly List<SoulEntry> shown = new List<SoulEntry>();
        private List<SoulEntry> all = new List<SoulEntry>();
        private int levelFilter = -1;
        private int worldIndex;
        private Text worldLabel;
        private SimpleButton previousWorldButton;
        private SimpleButton nextWorldButton;
        private string query = string.Empty;
        private int page;
        private Text countLabel;
        private Text pageLabel;
        private SimpleButton previousButton;
        private SimpleButton nextButton;

        public static void Create()
        {
            CreateAndInit(Id);
        }

        protected override void Init()
        {
            VerticalLayoutGroup layout = ContentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
            OT.InitializeNoActionVerticalLayoutGroup(layout);
            layout.spacing = 4;
            layout.padding = new RectOffset(0, 0, 6, 6);

            ContentSizeFitter fitter = ContentTransform.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            SoulAmbience.Create(ContentTransform);

            BuildPicture("Banner", PantheonArt.Banner, 44f);
            BuildWorldSelector();
            BuildFilters();
            BuildSearch();
            countLabel = BuildLabel("Count", 14f, 9, TextAnchor.MiddleCenter);
            countLabel.color = new Color(0.62f, 0.9f, 1f);
            BuildPicture("WaveTop", PantheonArt.Wave, 6f);
            BuildRows();
            BuildPicture("WaveBottom", PantheonArt.Wave, 6f);
            BuildPaging();
            Text hint = BuildLabel("Hint", 40f, 8, TextAnchor.UpperCenter);
            hint.color = new Color(0.66f, 0.72f, 0.91f);
            hint.text = Localize("oos_pantheon_hint");
        }

        public override void OnNormalEnable()
        {
            try
            {
                all = SoulStorage.ListEntries(SoulWorld.CurrentId(), true);
            }
            catch (Exception e)
            {
                all = new List<SoulEntry>();
                OceanOfSoulsMod.LogWarning("Pantheon load failed: " + e.Message);
            }
            page = 0;
            worldIndex = 0;
            BuildWorldList();
            Refresh();
        }

        public override void OnNormalDisable()
        {
            ReleaseShown();
            all = new List<SoulEntry>();
            filtered.Clear();
        }

        private void ReleaseShown()
        {
            foreach (SoulEntry entry in shown) entry.Release();
            shown.Clear();
        }

        private static string Localize(string key)
        {
            return LocalizedTextManager.getText(key);
        }

        private RectTransform NewContainer(string name, float height)
        {
            GameObject container = new GameObject(name, typeof(RectTransform));
            container.transform.SetParent(ContentTransform, false);
            RectTransform rect = container.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(RowWidth, height);
            rect.localScale = Vector3.one;
            return rect;
        }

        private void BuildPicture(string name, Sprite sprite, float height)
        {
            if (sprite == null) return;

            RectTransform container = NewContainer(name, height);
            Image image = container.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
        }

        private Text BuildLabel(string name, float height, int fontSize, TextAnchor anchor)
        {
            RectTransform container = NewContainer(name, height);
            Text text = container.gameObject.AddComponent<Text>();
            OT.InitializeCommonText(text);
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private void BuildWorldSelector()
        {
            RectTransform container = NewContainer("Worlds", 22f);

            previousWorldButton = SimpleButton.Instantiate(container, false, "PreviousWorld");
            previousWorldButton.Setup(() => ChangeWorld(-1), null, "<", new Vector2(24f, 20f));
            previousWorldButton.transform.localPosition = new Vector3(-RowWidth / 2f + 12f, 0f, 0f);
            previousWorldButton.transform.localScale = Vector3.one;

            nextWorldButton = SimpleButton.Instantiate(container, false, "NextWorld");
            nextWorldButton.Setup(() => ChangeWorld(1), null, ">", new Vector2(24f, 20f));
            nextWorldButton.transform.localPosition = new Vector3(RowWidth / 2f - 12f, 0f, 0f);
            nextWorldButton.transform.localScale = Vector3.one;

            GameObject labelObject = new GameObject("WorldLabel", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(container, false);
            labelObject.GetComponent<RectTransform>().sizeDelta = new Vector2(RowWidth - 60f, 20f);
            labelObject.transform.localPosition = Vector3.zero;
            worldLabel = labelObject.GetComponent<Text>();
            OT.InitializeCommonText(worldLabel);
            worldLabel.alignment = TextAnchor.MiddleCenter;
            worldLabel.color = new Color(0.8f, 0.92f, 1f);
            worldLabel.resizeTextForBestFit = true;
            worldLabel.resizeTextMinSize = 6;
            worldLabel.resizeTextMaxSize = 10;
        }

        private void BuildWorldList()
        {
            worldIds.Clear();
            worldNames.Clear();
            worldCounts.Clear();

            string currentId = SoulWorld.CurrentId();
            worldIds.Add(currentId);
            worldNames.Add(SoulWorld.CurrentName());
            worldCounts.Add(0);

            foreach (SoulEntry entry in all)
            {
                string id = entry.WorldId ?? string.Empty;
                int index = worldIds.IndexOf(id);
                if (index < 0)
                {
                    worldIds.Add(id);
                    worldNames.Add(string.IsNullOrEmpty(entry.WorldName) ? id : entry.WorldName);
                    worldCounts.Add(0);
                    index = worldIds.Count - 1;
                }
                worldCounts[index]++;
            }

            for (int i = 1; i < worldNames.Count; i++)
            {
                for (int j = 0; j < worldNames.Count; j++)
                {
                    if (i != j && worldNames[i] == worldNames[j] && worldIds[i].Length >= 4)
                    {
                        worldNames[i] = worldNames[i] + " #" + worldIds[i].Substring(0, 4);
                        break;
                    }
                }
            }
        }

        private bool AllWorldsSelected()
        {
            return worldIndex >= worldIds.Count;
        }

        private void ChangeWorld(int delta)
        {
            int options = worldIds.Count + 1;
            worldIndex = ((worldIndex + delta) % options + options) % options;
            page = 0;
            Refresh();
        }

        private void BuildFilters()
        {
            RectTransform container = NewContainer("Filters", 20f);
            const float width = 22f;
            const float gap = 2f;
            float start = -RowWidth / 2f + width / 2f;

            for (int i = 0; i < filterButtons.Length; i++)
            {
                int level = i - 1;
                SimpleButton button = SimpleButton.Instantiate(container, false, "Filter" + i);
                string label = level < 0 ? Localize("oos_pantheon_all") : level.ToString();
                button.Setup(() => SetFilter(level), null, label, new Vector2(width, 20f));
                button.transform.localPosition = new Vector3(start + i * (width + gap), 0f, 0f);
                button.transform.localScale = Vector3.one;
                filterButtons[i] = button;
            }
        }

        private void BuildSearch()
        {
            RectTransform container = NewContainer("Search", 22f);
            TextInput input = TextInput.Instantiate(container, false, "SearchInput");
            input.SetSize(new Vector2(RowWidth, 22f));
            input.Setup(string.Empty, OnQueryChanged);
            input.transform.localPosition = Vector3.zero;
            input.transform.localScale = Vector3.one;
        }

        private void BuildRows()
        {
            for (int i = 0; i < PageSize; i++)
            {
                int index = i;
                RectTransform container = NewContainer("Row" + i, RowHeight);
                Image background = container.gameObject.AddComponent<Image>();
                if (PantheonArt.RowBackground != null)
                {
                    background.sprite = PantheonArt.RowBackground;
                }
                else
                {
                    background.color = new Color(0f, 0f, 0f, 0.3f);
                }

                UiUnitAvatarElement avatar = CreateAvatar(container);

                GameObject badgeObject = new GameObject("Badge", typeof(RectTransform), typeof(Image));
                badgeObject.transform.SetParent(container, false);
                RectTransform badgeRect = badgeObject.GetComponent<RectTransform>();
                badgeRect.sizeDelta = new Vector2(11f, 11f);
                badgeRect.localPosition = new Vector3(-RowWidth / 2f + AvatarSlot + 1f, -RowHeight / 2f + 8f, 0f);
                Image badge = badgeObject.GetComponent<Image>();
                badge.raycastTarget = false;

                GameObject infoObject = new GameObject("Info", typeof(RectTransform), typeof(Text));
                infoObject.transform.SetParent(container, false);
                RectTransform infoRect = infoObject.GetComponent<RectTransform>();
                infoRect.sizeDelta = new Vector2(InfoWidth, RowHeight - 2f);
                infoRect.localPosition = new Vector3(-RowWidth / 2f + AvatarSlot + 4f + InfoWidth / 2f, 0f, 0f);
                Text info = infoObject.GetComponent<Text>();
                OT.InitializeCommonText(info);
                info.alignment = TextAnchor.MiddleLeft;
                info.color = Color.white;
                info.resizeTextForBestFit = true;
                info.resizeTextMinSize = 6;
                info.resizeTextMaxSize = 10;

                SimpleButton reincarnate = SimpleButton.Instantiate(container, false, "Reincarnate");
                reincarnate.Setup(() => OnReincarnate(index), null, Localize("oos_pantheon_reincarnate"), new Vector2(ButtonWidth, 22f));
                reincarnate.transform.localPosition = new Vector3(RowWidth / 2f - 4f - ButtonWidth - 2f - ButtonWidth / 2f, 0f, 0f);
                reincarnate.transform.localScale = Vector3.one;
                reincarnate.Background.color = new Color(0.78f, 0.82f, 1f);

                SimpleButton revive = SimpleButton.Instantiate(container, false, "Revive");
                revive.Setup(() => OnRevive(index), null, Localize("oos_pantheon_revive"), new Vector2(ButtonWidth, 22f));
                revive.transform.localPosition = new Vector3(RowWidth / 2f - 4f - ButtonWidth / 2f, 0f, 0f);
                revive.transform.localScale = Vector3.one;
                revive.Background.color = new Color(0.78f, 1f, 0.9f);

                rows.Add(new Row { Root = container.gameObject, Info = info, Background = background, Badge = badge, Avatar = avatar, Revive = revive, Reincarnate = reincarnate });
            }
        }

        private UiUnitAvatarElement CreateAvatar(RectTransform row)
        {
            try
            {
                UiUnitAvatarElement prefab = Resources.Load<UiUnitAvatarElement>("ui/UnitAvatarElement");
                if (prefab == null) return null;

                UiUnitAvatarElement element = Instantiate(prefab, row);
                element.show_banner_kingdom = false;
                element.show_banner_clan = false;
                if (element.kingdomBanner != null) element.kingdomBanner.gameObject.SetActive(false);
                if (element.clanBanner != null) element.clanBanner.gameObject.SetActive(false);
                if (element.unit_type_bg != null) element.unit_type_bg.gameObject.SetActive(false);

                TipButton tip = element.GetComponent<TipButton>();
                if (tip != null) tip.enabled = false;

                StyleAvatarBackground(element);

                element.transform.localScale = Vector3.one * AvatarScale;
                element.transform.localPosition = new Vector3(-RowWidth / 2f + AvatarSlot / 2f + 2f, 0f, 0f);
                return element;
            }
            catch (Exception e)
            {
                OceanOfSoulsMod.LogWarning("Pantheon avatar setup failed: " + e.Message);
                return null;
            }
        }

        private static void StyleAvatarBackground(UiUnitAvatarElement element)
        {
            if (AvatarBackground == null) return;

            Image[] keep = element.avatarLoader != null ? element.avatarLoader.GetComponentsInChildren<Image>(true) : new Image[0];
            foreach (Image image in element.GetComponentsInChildren<Image>(true))
            {
                if (System.Array.IndexOf(keep, image) >= 0) continue;

                Mask mask = image.GetComponent<Mask>();
                if (mask != null)
                {
                    mask.showMaskGraphic = false;
                    continue;
                }
                image.enabled = false;
            }

            GameObject background = new GameObject("OceanOfSoulsBackground", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(element.transform, false);
            RectTransform rect = background.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image backgroundImage = background.GetComponent<Image>();
            backgroundImage.sprite = AvatarBackground;
            backgroundImage.raycastTarget = false;
            background.transform.SetAsFirstSibling();
        }

        private static void ShowAvatar(Row row, SoulData data)
        {
            if (row.Avatar == null) return;
            if (data == null)
            {
                row.Avatar.gameObject.SetActive(false);
                return;
            }

            try
            {
                ActorAsset asset = string.IsNullOrEmpty(data.AssetId) ? null : AssetManager.actor_library.get(data.AssetId);
                if (asset == null)
                {
                    row.Avatar.gameObject.SetActive(false);
                    return;
                }

                row.Avatar.gameObject.SetActive(true);
                row.Avatar.avatarLoader.load(SoulLook.ToAvatarData(data.Look, asset, data.SoulId));
            }
            catch (Exception e)
            {
                row.Avatar.gameObject.SetActive(false);
                OceanOfSoulsMod.LogWarning("Pantheon avatar failed: " + e.Message);
            }
        }

        private void BuildPaging()
        {
            RectTransform container = NewContainer("Paging", 22f);

            previousButton = SimpleButton.Instantiate(container, false, "Previous");
            previousButton.Setup(() => ChangePage(-1), null, "<", new Vector2(30f, 20f));
            previousButton.transform.localPosition = new Vector3(-60f, 0f, 0f);
            previousButton.transform.localScale = Vector3.one;

            nextButton = SimpleButton.Instantiate(container, false, "Next");
            nextButton.Setup(() => ChangePage(1), null, ">", new Vector2(30f, 20f));
            nextButton.transform.localPosition = new Vector3(60f, 0f, 0f);
            nextButton.transform.localScale = Vector3.one;

            GameObject labelObject = new GameObject("PageLabel", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(container, false);
            labelObject.GetComponent<RectTransform>().sizeDelta = new Vector2(80f, 20f);
            labelObject.transform.localPosition = Vector3.zero;
            pageLabel = labelObject.GetComponent<Text>();
            OT.InitializeCommonText(pageLabel);
            pageLabel.alignment = TextAnchor.MiddleCenter;
            pageLabel.fontSize = 10;
            pageLabel.color = Color.white;
        }

        private void SetFilter(int level)
        {
            levelFilter = level;
            page = 0;
            Refresh();
        }

        private void OnQueryChanged(string value)
        {
            query = value ?? string.Empty;
            page = 0;
            Refresh();
        }

        private void ChangePage(int delta)
        {
            page += delta;
            Refresh();
        }

        private int PageCount()
        {
            return Math.Max(1, (filtered.Count + PageSize - 1) / PageSize);
        }

        private void ApplyFilter()
        {
            filtered.Clear();
            string needle = query.Trim();
            bool allWorlds = AllWorldsSelected();
            string selectedWorld = allWorlds ? null : worldIds[worldIndex];
            foreach (SoulEntry entry in all)
            {
                if (!allWorlds && (entry.WorldId ?? string.Empty) != selectedWorld) continue;
                if (levelFilter >= 0 && entry.Level != levelFilter) continue;
                if (needle.Length > 0 && (entry.Name ?? string.Empty).IndexOf(needle, StringComparison.OrdinalIgnoreCase) < 0) continue;
                filtered.Add(entry);
            }

            filtered.Sort((a, b) =>
            {
                int byLevel = b.Level.CompareTo(a.Level);
                if (byLevel != 0) return byLevel;
                int byPeak = b.PeakLevel.CompareTo(a.PeakLevel);
                if (byPeak != 0) return byPeak;
                return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });
        }

        private void Refresh()
        {
            ReleaseShown();
            ApplyFilter();

            int pages = PageCount();
            if (page < 0) page = 0;
            if (page >= pages) page = pages - 1;

            for (int i = 0; i < rows.Count; i++)
            {
                int index = page * PageSize + i;
                bool visible = index < filtered.Count;
                rows[i].Root.SetActive(visible);
                if (!visible) continue;

                SoulEntry entry = filtered[index];
                shown.Add(entry);
                SoulData data = entry.Data;
                string lives = string.Format(Localize(entry.Lives == 1 ? "oos_pantheon_lives_one" : "oos_pantheon_lives_many"), entry.Lives);
                string details = string.Format(Localize("oos_pantheon_row"), entry.Level, entry.PeakLevel, lives);
                if (AllWorldsSelected() && !string.IsNullOrEmpty(entry.WorldName))
                {
                    details += " | " + entry.WorldName;
                }
                string title = data != null && !string.IsNullOrEmpty(data.Name) ? data.Name : entry.DisplayName;
                rows[i].Info.text = "<color=#" + PantheonArt.LevelHex(entry.Level) + ">" + title + "</color>\n<color=#9fb4c8>" + details + "</color>";
                if (PantheonArt.RowBackground != null)
                {
                    rows[i].Background.color = PantheonArt.LevelColor(entry.Level);
                }
                Sprite orb = PantheonArt.Orbs[Math.Max(SoulLevels.MinLevel, Math.Min(SoulLevels.MaxLevel, entry.Level))];
                rows[i].Badge.sprite = orb;
                rows[i].Badge.enabled = orb != null;
                ShowAvatar(rows[i], data);
            }

            for (int i = 0; i < filterButtons.Length; i++)
            {
                bool selected = i - 1 == levelFilter;
                filterButtons[i].Background.color = selected ? Color.white : new Color(0.6f, 0.6f, 0.7f, 1f);
            }

            worldLabel.text = WorldLabelText();
            countLabel.text = string.Format(Localize("oos_pantheon_count"), filtered.Count);
            pageLabel.text = (page + 1) + " / " + pages;
            previousButton.gameObject.SetActive(page > 0);
            nextButton.gameObject.SetActive(page < pages - 1);
        }

        private string WorldLabelText()
        {
            if (AllWorldsSelected())
            {
                return string.Format(Localize("oos_pantheon_world"), Localize("oos_pantheon_world_all"), all.Count);
            }

            string name = worldNames[worldIndex];
            if (worldIds[worldIndex] == SoulWorld.CurrentId())
            {
                name = string.Format(Localize("oos_pantheon_world_current"), name);
            }
            return string.Format(Localize("oos_pantheon_world"), name, worldCounts[worldIndex]);
        }

        private void OnRevive(int rowIndex)
        {
            Act(rowIndex, SoulSpawner.Revive, "oos_pantheon_revived");
        }

        private void OnReincarnate(int rowIndex)
        {
            Act(rowIndex, SoulSpawner.Reincarnate, "oos_pantheon_reincarnated");
        }

        private void Act(int rowIndex, Func<SoulEntry, bool> action, string successKey)
        {
            int index = page * PageSize + rowIndex;
            if (index < 0 || index >= filtered.Count) return;

            SoulEntry entry = filtered[index];
            bool done = false;
            try
            {
                done = action(entry);
            }
            catch (Exception e)
            {
                OceanOfSoulsMod.LogWarning("Pantheon action failed: " + e.Message);
            }

            if (done)
            {
                all.Remove(entry);
                int removedIndex = worldIds.IndexOf(entry.WorldId ?? string.Empty);
                if (removedIndex >= 0 && worldCounts[removedIndex] > 0)
                {
                    worldCounts[removedIndex]--;
                }
                WorldTip.showNow(string.Format(Localize(successKey), entry.DisplayName), false, "top", 3f);
            }
            else
            {
                WorldTip.showNow(Localize("oos_pantheon_failed"), false, "top", 3f);
            }
            Refresh();
        }
    }
}
