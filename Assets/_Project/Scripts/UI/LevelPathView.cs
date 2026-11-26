using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BusJam
{
    // level road on the menu. current level at the bottom, next ones above it
    public class LevelPathView : MonoBehaviour
    {
        [SerializeField] private Image[] nodes;
        [SerializeField] private TMP_Text[] labels;
        [SerializeField] private RectTransform road;

        [Header("Colors")]
        [SerializeField] private Color currentColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] private Color doneColor = new Color(0.3f, 0.8f, 0.4f);
        [SerializeField] private Color lockedColor = new Color(0.55f, 0.52f, 0.68f);

        [SerializeField] private float currentScale = 1.25f;

        public Transform CurrentNode { get; private set; }

        public void Show(int currentLevelNumber, int levelCount)
        {
            bool allLevelsDone = currentLevelNumber > levelCount;
            int firstNumber = allLevelsDone ? levelCount : currentLevelNumber;
            int lastVisible = 0;

            for (int i = 0; i < nodes.Length; i++)
            {
                int number = firstNumber + i;
                bool exists = number <= levelCount;
                bool isMoreSign = number == levelCount + 1;

                nodes[i].gameObject.SetActive(exists || isMoreSign);
                nodes[i].transform.DOKill();
                nodes[i].transform.localScale = Vector3.one;
                if (!exists && !isMoreSign)
                    continue;

                lastVisible = i;
                labels[i].text = exists ? number.ToString() : "...";

                if (!exists)
                    nodes[i].color = lockedColor;
                else if (allLevelsDone)
                    nodes[i].color = doneColor;
                else
                    nodes[i].color = i == 0 ? currentColor : lockedColor;
            }

            FitRoad(lastVisible);

            CurrentNode = allLevelsDone ? null : nodes[0].transform;
            if (CurrentNode != null)
                CurrentNode.localScale = Vector3.one * currentScale;
        }

        private void FitRoad(int lastVisible)
        {
            Vector2 bottom = ((RectTransform)nodes[0].transform).anchoredPosition;
            Vector2 top = ((RectTransform)nodes[lastVisible].transform).anchoredPosition;

            road.anchoredPosition = (bottom + top) * 0.5f;
            road.sizeDelta = new Vector2(road.sizeDelta.x, top.y - bottom.y);
        }
    }
}
