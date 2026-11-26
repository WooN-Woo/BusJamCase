using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BusJam
{
    public class UIManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject startPanel;
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private GameObject completePanel;
        [SerializeField] private GameObject failPanel;

        [Header("Menu")]
        [SerializeField] private LevelPathView levelPath;

        [Header("Texts")]
        [SerializeField] private TMP_Text menuLevelText;
        [SerializeField] private TMP_Text startLevelText;
        [SerializeField] private TMP_Text startTimeText;
        [SerializeField] private TMP_Text gameplayLevelText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text completeLevelText;
        [SerializeField] private TMP_Text failReasonText;
        [SerializeField] private Transform tapText;

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button startButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button retryButton;

        [Header("Timer")]
        [SerializeField] private float warningTime = 10f;
        [SerializeField] private Color normalTimerColor = Color.white;
        [SerializeField] private Color warningTimerColor = new Color(1f, 0.3f, 0.3f);

        [Header("Animation")]
        [SerializeField] private float fadeTime = 0.25f;
        [SerializeField] private float popTime = 0.35f;
        [SerializeField] private float popDelayStep = 0.07f;

        private int shownSeconds = -1;

        public event Action PlayClicked;
        public event Action StartClicked;
        public event Action NextClicked;
        public event Action RetryClicked;

        private void Awake()
        {
            playButton.onClick.AddListener(() => PlayClicked?.Invoke());
            startButton.onClick.AddListener(() => StartClicked?.Invoke());
            nextButton.onClick.AddListener(() => NextClicked?.Invoke());
            retryButton.onClick.AddListener(() => RetryClicked?.Invoke());
        }

        public void ShowMenu(int levelNumber, int levelCount)
        {
            ShowPanel(menuPanel);

            bool allLevelsDone = levelNumber > levelCount;
            menuLevelText.text = allLevelsDone ? "PLAY AGAIN" : $"LEVEL {levelNumber}";
            levelPath.Show(levelNumber, levelCount);

            // button and current level circle breathe together
            Breathe(playButton.transform);
            if (levelPath.CurrentNode != null)
                Breathe(levelPath.CurrentNode, levelPath.CurrentNode.localScale.x);
        }

        public void ShowStart(int levelNumber, float timeLimit)
        {
            ShowPanel(startPanel);
            startLevelText.text = $"Level {levelNumber}";
            startTimeText.text = $"Time: {FormatTime(timeLimit)}";
            Breathe(tapText);
        }

        public void ShowGameplay(int levelNumber)
        {
            ShowPanel(gameplayPanel);
            gameplayLevelText.text = $"Level {levelNumber}";
            shownSeconds = -1;
        }

        public void ShowComplete(int levelNumber)
        {
            ShowPanel(completePanel);
            completeLevelText.text = $"Level {levelNumber}\nComplete!";
            Breathe(nextButton.transform);
        }

        public void ShowFail(string reason)
        {
            ShowPanel(failPanel);
            failReasonText.text = reason;
            Breathe(retryButton.transform);
        }

        public void SetTimer(float remainingTime)
        {
            int seconds = Mathf.CeilToInt(remainingTime);
            if (seconds == shownSeconds)
                return;

            shownSeconds = seconds;
            timerText.text = FormatTime(seconds);
            bool lowTime = seconds <= warningTime;
            timerText.color = lowTime ? warningTimerColor : normalTimerColor;

            if (lowTime && seconds > 0)
            {
                timerText.transform.DOComplete();
                timerText.transform.DOPunchScale(Vector3.one * 0.25f, 0.3f, 6);
            }
        }

        private void ShowPanel(GameObject panel)
        {
            menuPanel.SetActive(panel == menuPanel);
            startPanel.SetActive(panel == startPanel);
            gameplayPanel.SetActive(panel == gameplayPanel);
            completePanel.SetActive(panel == completePanel);
            failPanel.SetActive(panel == failPanel);
            PlayOpenAnimation(panel);
        }

        private void PlayOpenAnimation(GameObject panel)
        {
            CanvasGroup group = panel.GetComponent<CanvasGroup>();
            group.DOKill();
            group.alpha = 0f;
            group.DOFade(1f, fadeTime);

            Transform root = panel.transform;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform item = root.GetChild(i);
                item.DOKill();
                item.localScale = Vector3.zero;
                item.DOScale(1f, popTime).SetEase(Ease.OutBack).SetDelay(i * popDelayStep);
            }
        }

        private static void Breathe(Transform item, float normalScale = 1f)
        {
            item.DOScale(normalScale * 1.07f, 0.6f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(1f)
                .SetLink(item.gameObject, LinkBehaviour.KillOnDisable);
        }

        private static string FormatTime(float seconds)
        {
            int total = Mathf.CeilToInt(seconds);
            return $"{total / 60}:{total % 60:00}";
        }
    }
}
