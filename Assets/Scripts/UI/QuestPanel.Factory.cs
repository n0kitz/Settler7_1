using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    public partial class QuestPanel
    {
        // ---- Static factory ---------------------------------------------

        public static QuestPanel Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var go = new GameObject("QuestPanel");
            go.transform.SetParent(canvasTransform, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.05f, 0.15f);
            rect.anchorMax = new Vector2(0.45f, 0.85f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.08f, 0.05f, 0.95f);

            var panel = go.AddComponent<QuestPanel>();
            panel._font = font;
            panel.BuildLayout(go.transform);

            go.SetActive(false);
            return panel;
        }

        // ---- Layout -----------------------------------------------------

        private void BuildLayout(Transform root)
        {
            var outerLayout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            outerLayout.padding = new RectOffset(10, 10, 10, 10);
            outerLayout.spacing = 8f;
            outerLayout.childForceExpandWidth = true;
            outerLayout.childForceExpandHeight = false;
            outerLayout.childAlignment = TextAnchor.UpperLeft;

            var title = UIFactory.CreateLabel(root, "Title", "Quests  [Q]",
                20, FontStyles.Bold, _font);
            title.color = UIColors.TEXT_HEADER_GOLD;
            AddLayoutElement(title.gameObject, 28f);

            var activeHeader = UIFactory.CreateLabel(root, "ActiveHeader", "Active Quests",
                14, FontStyles.Bold, _font);
            activeHeader.color = UIColors.ACCENT_ORANGE;
            AddLayoutElement(activeHeader.gameObject, 20f);

            _activeContainer = CreateScrollContainer(root, "ActiveContainer", 200f);

            var availHeader = UIFactory.CreateLabel(root, "AvailHeader", "Available Quests",
                14, FontStyles.Bold, _font);
            availHeader.color = UIColors.TEXT_GOLD;
            AddLayoutElement(availHeader.gameObject, 20f);

            _availableContainer = CreateScrollContainer(root, "AvailableContainer", 150f);
        }

        // ---- Entry builder ----------------------------------------------

        private GameObject BuildQuestEntry(Transform parent, Quest quest,
            GameState state, bool isActive)
        {
            var entryGo = new GameObject($"Quest_{quest.Id}");
            entryGo.transform.SetParent(parent, false);
            entryGo.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 10f);

            var entryLayout = entryGo.AddComponent<VerticalLayoutGroup>();
            entryLayout.padding = new RectOffset(6, 6, 4, 4);
            entryLayout.spacing = 2f;
            entryLayout.childForceExpandWidth = true;
            entryLayout.childForceExpandHeight = false;

            entryGo.AddComponent<Image>().color = UIColors.PANEL_GRAY_MEDIUM;
            entryGo.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var nameLabel = UIFactory.CreateLabel(entryGo.transform,
                "Name", quest.DisplayName, 13, FontStyles.Bold, _font);
            nameLabel.color = isActive ? UIColors.TEXT_HEADER_GOLD : UIColors.TEXT_GOLD;
            AddLayoutElement(nameLabel.gameObject, 18f);

            var descLabel = UIFactory.CreateLabel(entryGo.transform,
                "Desc", quest.Description, 11, FontStyles.Normal, _font);
            descLabel.color = UIColors.TEXT_GRAY_DIM;
            descLabel.textWrappingMode = TextWrappingModes.Normal;
            AddLayoutElement(descLabel.gameObject, 0f, flexible: 1f);

            if (isActive && quest.Objectives != null)
            {
                var sb = new StringBuilder("Objectives:\n");
                foreach (var obj in quest.Objectives)
                    sb.AppendLine($"  • {DescribeObjective(obj, state)}");

                var objLabel = UIFactory.CreateLabel(entryGo.transform,
                    "Objectives", sb.ToString().TrimEnd(), 11, FontStyles.Normal, _font);
                objLabel.color = UIColors.TEXT_GREEN_LIGHT;
                objLabel.textWrappingMode = TextWrappingModes.Normal;
                AddLayoutElement(objLabel.gameObject, 0f, flexible: 1f);
            }

            if (quest.Rewards != null && quest.Rewards.Count > 0)
            {
                var sb = new StringBuilder("Rewards: ");
                for (int i = 0; i < quest.Rewards.Count; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.Append(DescribeReward(quest.Rewards[i]));
                }
                var rewardLabel = UIFactory.CreateLabel(entryGo.transform,
                    "Rewards", sb.ToString(), 11, FontStyles.Italic, _font);
                rewardLabel.color = UIColors.ACCENT_ORANGE;
                AddLayoutElement(rewardLabel.gameObject, 16f);
            }

            if (!isActive && state.Quests.AvailableQuests != null)
            {
                var questId = quest.Id;
                var btn = UIFactory.CreateButton(entryGo.transform, "Accept Quest",
                    _font, UIColors.BUTTON_GREEN,
                    () => AcceptQuest(questId),
                    new Vector2(0f, 26f), 11f);
                AddLayoutElement(btn.gameObject, 26f);
            }

            return entryGo;
        }

        // ---- Static helpers ---------------------------------------------

        private static string DescribeObjective(QuestObjective obj, GameState state)
        {
            return obj.Type switch
            {
                QuestObjectiveType.DeliverResource =>
                    $"Gather {obj.Amount}x {obj.ResourceType} " +
                    $"(have {state.PlayerResources[0].Get(obj.ResourceType)})",
                QuestObjectiveType.OwnSectors     => $"Own {obj.Amount} sectors",
                QuestObjectiveType.HaveArmy       =>
                    $"Have army of {obj.Amount} soldiers " +
                    $"(have {state.Army.GetTotalArmySize(0)})",
                QuestObjectiveType.HavePrestigeLevel =>
                    $"Reach prestige level {obj.Amount} " +
                    $"(current: {state.Prestige.GetLevel(0)})",
                QuestObjectiveType.ResearchTech   => $"Research tech: {obj.TechId}",
                _                                 => obj.Type.ToString()
            };
        }

        private static string DescribeReward(QuestReward reward)
        {
            return reward.Type switch
            {
                QuestRewardType.Resource       => $"{reward.Amount}x {reward.ResourceType}",
                QuestRewardType.PrestigePoints => $"{reward.Amount} prestige pts",
                QuestRewardType.VictoryPoint   => "Victory Point",
                _                              => reward.Type.ToString()
            };
        }

        private Transform CreateScrollContainer(Transform parent, string name, float height)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, height);

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 4f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperLeft;

            var le = go.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;
            le.preferredHeight = height;

            return go.transform;
        }

        private static void ClearContainer(Transform container)
        {
            if (container == null) return;
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }

        private static void AddLayoutElement(GameObject go, float preferredHeight,
            float flexible = -1f)
        {
            var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            if (preferredHeight > 0f) le.preferredHeight = preferredHeight;
            if (flexible >= 0f) le.flexibleHeight = flexible;
            le.flexibleWidth = 1f;
        }
    }
}
