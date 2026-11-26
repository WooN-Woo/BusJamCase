using UnityEngine;

namespace BusJam
{
    // game flow: menu -> tap to start -> play -> win / fail
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private LevelDatabase levelDatabase;
        [SerializeField] private Board board;
        [SerializeField] private LevelTimer levelTimer;
        [SerializeField] private InputHandler inputHandler;
        [SerializeField] private CameraFitter cameraFitter;
        [SerializeField] private UIManager uiManager;

        private LevelData currentLevel;

        public GameState State { get; private set; }

        private int LevelNumber => SaveSystem.LevelIndex + 1;

        private void Awake()
        {
            Application.targetFrameRate = 60;
        }

        private void OnEnable()
        {
            board.Completed += OnLevelCompleted;
            board.Failed += OnLevelFailed;
            levelTimer.Expired += OnTimeExpired;
            uiManager.PlayClicked += OnPlayClicked;
            uiManager.StartClicked += StartLevel;
            uiManager.NextClicked += OnNextClicked;
            uiManager.RetryClicked += LoadCurrentLevel;
        }

        private void OnDisable()
        {
            board.Completed -= OnLevelCompleted;
            board.Failed -= OnLevelFailed;
            levelTimer.Expired -= OnTimeExpired;
            uiManager.PlayClicked -= OnPlayClicked;
            uiManager.StartClicked -= StartLevel;
            uiManager.NextClicked -= OnNextClicked;
            uiManager.RetryClicked -= LoadCurrentLevel;
        }

        private void Start()
        {
            ShowMenu();
        }

        private void Update()
        {
            if (State == GameState.Playing)
                uiManager.SetTimer(levelTimer.RemainingTime);
        }

        private void ShowMenu()
        {
            SetState(GameState.Menu);
            uiManager.ShowMenu(LevelNumber, levelDatabase.Count);
        }

        private void OnPlayClicked()
        {
            if (!levelDatabase.HasLevel(SaveSystem.LevelIndex))
                SaveSystem.LevelIndex = 0;

            LoadCurrentLevel();
        }

        private void OnNextClicked()
        {
            if (levelDatabase.HasLevel(SaveSystem.LevelIndex))
                LoadCurrentLevel();
            else
                ShowMenu();
        }

        private void LoadCurrentLevel()
        {
            currentLevel = levelDatabase.GetLevel(SaveSystem.LevelIndex);
            board.Build(currentLevel);
            cameraFitter.Fit(board.GetPlayAreaBounds());
            SetState(GameState.Start);
            uiManager.ShowStart(LevelNumber, currentLevel.TimeLimit);
        }

        private void StartLevel()
        {
            SetState(GameState.Playing);
            levelTimer.Begin(currentLevel.TimeLimit);
            uiManager.ShowGameplay(LevelNumber);
        }

        private void OnLevelCompleted()
        {
            levelTimer.Stop();
            SetState(GameState.Complete);
            uiManager.ShowComplete(LevelNumber);
            SaveSystem.LevelIndex++;
        }

        private void OnLevelFailed(string reason)
        {
            levelTimer.Stop();
            SetState(GameState.Fail);
            uiManager.ShowFail(reason);
        }

        private void OnTimeExpired()
        {
            board.Stop();
            OnLevelFailed("Time's up!");
        }

        private void SetState(GameState state)
        {
            State = state;
            inputHandler.enabled = state == GameState.Playing;
        }
    }
}
