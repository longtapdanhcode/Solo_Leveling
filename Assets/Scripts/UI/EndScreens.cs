// Implements: design/game-brief.md MVP 7 + "Player goal & fail state", production/epics/micro-arena-survivor/story-007-hud-game-feel.md
// (victory screen after the Void Warden, defeat screen when the revive is spent, both with a replay choice).
//
// Owner decision (2026-11-05): no automatic restart after a defeat; replay by keyboard or a mouse click, and it must work
// while the game is paused (time scale 0). Keys: Enter / keypad Enter / R (not Space, the Dash key).

using System.Collections;
using MicroArena.Gameplay.Combat;
using MicroArena.Gameplay.Config;
using MicroArena.Gameplay.Waves;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MicroArena.UI
{
    /// <summary>
    /// Runtime-built victory and defeat screens. When <see cref="RunState"/> enters Victory or Defeat, the matching screen
    /// appears after <see cref="FeelConfig.EndScreenDelay"/> real seconds and pauses the game through <see cref="GamePause"/>.
    /// Replay (after <see cref="FeelConfig.ReplayInputDelay"/>): Enter, keypad Enter or R, a click on the button, or
    /// the UI submit action (gamepad). Replay releases the pause, then calls <see cref="WaveDirector.RestartRun"/>.
    /// Everything runs on real time and unscaled UI input, so it works at time scale 0.
    /// <code>
    /// Transition table
    ///   Hidden  --phase becomes Victory / Defeat--------> Waiting (real-time delay)
    ///   Waiting --delay over----------------------------> Shown   (game paused, input locked for the replay delay)
    ///   Shown   --replay key / click / submit, unlocked-> Hidden  (pause released, RestartRun)
    ///   any     --OnRunReset / phase leaves the end------> Hidden  (pause released)
    /// </code>
    /// </summary>
    public sealed class EndScreens : MonoBehaviour
    {
        // Space is left out on purpose: it is the Dash key, so a replay on Space would also dash on the first frame of the new run.
        private static readonly Key[] ReplayKeys = { Key.Enter, Key.NumpadEnter, Key.R };

        [SerializeField] private FeelConfig _feel;
        [SerializeField] private RunState _runState;
        [SerializeField] private WaveDirector _director;
        [SerializeField] private TMP_FontAsset _font;
        [SerializeField] private TMP_FontAsset _boldFont;

        [Header("Layout (presentation only)")]
        [SerializeField] private Vector2 _referenceResolution = new Vector2(1920f, 1080f);
        [SerializeField] private int _sortingOrder = 200;

        [Header("Text")]
        [SerializeField] private string _victoryTitle = "CHIẾN THẮNG";
        [SerializeField] private string _victoryMessage = "Chúa Tể Hư Vô đã gục ngã. Đấu trường thuộc về Kaelen.";
        [SerializeField] private string _defeatTitle = "THẤT BẠI";
        [SerializeField] private string _defeatMessage = "Kaelen đã ngã xuống — không còn lượt hồi sinh.";
        [SerializeField] private string _replayLabel = "CHƠI LẠI";
        [SerializeField] private string _replayHint = "Enter / R hoặc bấm nút";

        [Header("Colours")]
        [SerializeField] private Color _dimColor = new Color(0f, 0f, 0f, 0.78f);
        [SerializeField] private Color _victoryColor = new Color(1f, 0.82f, 0.3f);
        [SerializeField] private Color _defeatColor = new Color(0.9f, 0.2f, 0.2f);
        [SerializeField] private Color _textColor = new Color(0.95f, 0.95f, 0.95f);
        [SerializeField] private Color _buttonColor = new Color(0.16f, 0.16f, 0.22f, 1f);

        private GameObject _root;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _message;
        private Button _replayButton;
        private Coroutine _pending;
        private bool _acceptInput;

        /// <summary>The phase whose screen is up (Victory or Defeat), or Playing when none is shown.</summary>
        public RunPhase ShownPhase { get; private set; } = RunPhase.Playing;

        /// <summary>True while an end screen is up.</summary>
        public bool IsShown => ShownPhase != RunPhase.Playing;

        /// <summary>Injects dependencies when building the scene. Call before the object is activated.</summary>
        public void Configure(FeelConfig feel, RunState runState, WaveDirector director, TMP_FontAsset font, TMP_FontAsset boldFont)
        {
            _feel = feel;
            _runState = runState;
            _director = director;
            _font = font;
            _boldFont = boldFont;
        }

        /// <summary>Shows the screen for <paramref name="phase"/> (Victory or Defeat) at once and pauses the game.</summary>
        public void Show(RunPhase phase)
        {
            _pending = null;
            ShownPhase = phase;
            bool victory = phase == RunPhase.Victory;
            _title.text = victory ? _victoryTitle : _defeatTitle;
            _title.color = victory ? _victoryColor : _defeatColor;
            _message.text = victory ? _victoryMessage : _defeatMessage;
            _root.SetActive(true);
            GamePause.Acquire(this);
            UiKit.EnsureEventSystem();
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(_replayButton.gameObject); // gamepad / arrows + submit
            }
            _acceptInput = _feel.ReplayInputDelay <= 0f;
            if (!_acceptInput && isActiveAndEnabled)
            {
                StartCoroutine(UnlockInput());
            }
        }

        /// <summary>Starts a new run from the end screen: hides it, releases the pause, then restarts. Ignored while hidden or locked.</summary>
        public void Replay()
        {
            if (!IsShown || !_acceptInput)
            {
                return;
            }
            Hide();
            _director.RestartRun();
        }

        private void Awake()
        {
            Build();
        }

        private void OnEnable()
        {
            _runState.OnPhaseChanged += HandlePhaseChanged;
            _director.OnRunReset += Hide;
        }

        private void OnDisable()
        {
            _runState.OnPhaseChanged -= HandlePhaseChanged;
            _director.OnRunReset -= Hide;
            Hide();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (!IsShown || !_acceptInput || keyboard == null)
            {
                return;
            }
            foreach (Key key in ReplayKeys)
            {
                if (keyboard[key].wasPressedThisFrame)
                {
                    Replay();
                    return;
                }
            }
        }

        private void HandlePhaseChanged(RunPhase previous, RunPhase next)
        {
            if (next != RunPhase.Victory && next != RunPhase.Defeat)
            {
                Hide();
                return;
            }
            if (_pending != null)
            {
                StopCoroutine(_pending);
            }
            _pending = StartCoroutine(ShowAfterDelay(next));
        }

        private IEnumerator ShowAfterDelay(RunPhase phase)
        {
            yield return new WaitForSecondsRealtime(_feel.EndScreenDelay);
            Show(phase);
        }

        private IEnumerator UnlockInput()
        {
            yield return new WaitForSecondsRealtime(_feel.ReplayInputDelay);
            _acceptInput = true;
        }

        private void Hide()
        {
            StopAllCoroutines();
            _pending = null;
            _acceptInput = false;
            ShownPhase = RunPhase.Playing;
            if (_root != null)
            {
                _root.SetActive(false);
            }
            GamePause.Release(this);
        }

        private void Build()
        {
            _root = UiKit.CreateCanvas("EndScreenCanvas", transform, _sortingOrder, _referenceResolution, true);
            Transform root = _root.transform;
            RectTransform dim = UiKit.CreateRect("Dim", root);
            UiKit.Stretch(dim);
            UiKit.AddImage(dim, _dimColor).raycastTarget = true; // blocks clicks to anything below
            var centre = new Vector2(0.5f, 0.5f);

            _title = UiKit.CreateText("Title", root, string.Empty, _boldFont, 110f, _textColor, TextAlignmentOptions.Center);
            Place(_title.rectTransform, centre, new Vector2(0f, 170f), new Vector2(1400f, 140f));
            _message = UiKit.CreateText("Message", root, string.Empty, _font, 36f, _textColor, TextAlignmentOptions.Center);
            Place(_message.rectTransform, centre, new Vector2(0f, 50f), new Vector2(1400f, 100f));

            RectTransform button = UiKit.Place("ReplayButton", root, centre, new Vector2(0f, -90f), new Vector2(420f, 96f));
            Image background = UiKit.AddImage(button, _buttonColor);
            background.raycastTarget = true;
            _replayButton = button.gameObject.AddComponent<Button>();
            _replayButton.targetGraphic = background;
            _replayButton.onClick.AddListener(Replay);
            TextMeshProUGUI label = UiKit.CreateText("Label", button, _replayLabel, _boldFont, 44f, _textColor, TextAlignmentOptions.Center);
            UiKit.Stretch(label.rectTransform);

            TextMeshProUGUI hint = UiKit.CreateText("Hint", root, _replayHint, _font, 26f, new Color(_textColor.r, _textColor.g, _textColor.b, 0.7f), TextAlignmentOptions.Center);
            Place(hint.rectTransform, centre, new Vector2(0f, -180f), new Vector2(1000f, 40f));
            _root.SetActive(false);
        }

        private static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
