// Implements: design/game-brief.md MVP 7 (HUD), production/epics/micro-arena-survivor/story-007-hud-game-feel.md.
//
// Owner decisions (2026-11-05): a rage indicator tells the player when Q is usable; a boss bar shows during Wave 5
// (Gargoyle Tyrant) and Wave 10 (Void Warden) and hides when the boss leaves; all text is TextMeshPro in Be Vietnam Pro;
// the revive has visual feedback (screen flash + message). The HUD only reads events and keeps no game state.

using System.Collections;
using System.Collections.Generic;
using MicroArena.Gameplay.Combat;
using MicroArena.Gameplay.Config;
using MicroArena.Gameplay.Player;
using MicroArena.Gameplay.Waves;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MicroArena.UI
{
    /// <summary>
    /// Runtime-built UGUI HUD. Top-left: HP bar, one pip per dash charge (bright = ready), the Soul Burst rage bar (pulses
    /// when Q is ready). Top-centre: "WAVE xx/10", the remaining-enemies bar and, during a boss wave, the boss name and HP bar.
    /// Revive: a full-screen flash and a message, timed on real time.
    /// <para>Every value shown arrives through an event of <see cref="Health"/>, <see cref="Dash"/>, <see cref="SoulBurst"/>
    /// or <see cref="WaveDirector"/>; the HUD stores nothing but its UI objects. On Start it asks the player components for
    /// their current values once (they raise no event before the first change).</para>
    /// </summary>
    public sealed class HudController : MonoBehaviour
    {
        [SerializeField] private FeelConfig _feel;
        [SerializeField] private WaveDirector _director;
        [Tooltip("Kaelen.")]
        [SerializeField] private Health _health;
        [SerializeField] private Dash _dash;
        [SerializeField] private SoulBurst _soulBurst;
        [SerializeField] private TMP_FontAsset _font;
        [SerializeField] private TMP_FontAsset _boldFont;

        [Header("Layout (presentation only)")]
        [SerializeField] private Vector2 _referenceResolution = new Vector2(1920f, 1080f);
        [SerializeField] private int _sortingOrder = 10;

        [Header("Text")]
        [SerializeField] private string _hpFormat = "{0:0} / {1:0}";
        [SerializeField] private string _dashLabel = "DASH";
        [SerializeField] private string _rageFormat = "NỘ {0:0}/{1:0}";
        [SerializeField] private string _rageReadyText = "[Q] SOUL BURST — SẴN SÀNG";
        [SerializeField] private string _waveFormat = "WAVE {0:00}/{1:00}";
        [SerializeField] private string _remainingFormat = "CÒN LẠI {0}/{1}";
        [SerializeField] private string _reviveText = "HỒI SINH!";

        [Header("Colours")]
        [SerializeField] private Color _panelColor = new Color(0f, 0f, 0f, 0.55f);
        [SerializeField] private Color _textColor = new Color(0.95f, 0.95f, 0.95f);
        [SerializeField] private Color _hpColor = new Color(0.25f, 0.8f, 0.35f);
        [SerializeField] private Color _hpLowColor = new Color(0.9f, 0.15f, 0.15f);
        [SerializeField] private Color _pipReadyColor = new Color(0.45f, 0.85f, 1f);
        [SerializeField] private Color _pipSpentColor = new Color(0.45f, 0.85f, 1f, 0.18f);
        [SerializeField] private Color _rageColor = new Color(0.55f, 0.2f, 0.9f);
        [SerializeField] private Color _rageReadyColor = new Color(0.85f, 0.45f, 1f);
        [SerializeField] private Color _remainingColor = new Color(0.95f, 0.75f, 0.2f);
        [SerializeField] private Color _bossColor = new Color(0.75f, 0.1f, 0.15f);
        [SerializeField] private Color _reviveTextColor = new Color(1f, 0.85f, 0.4f);

        private GameObject _root;
        private Image _hpFill;
        private TextMeshProUGUI _hpText;
        private RectTransform _pipRow;
        private readonly List<Image> _pips = new List<Image>();
        private Sprite _circle;
        private Image _rageFill;
        private TextMeshProUGUI _rageText;
        private TextMeshProUGUI _waveText;
        private Image _remainingFill;
        private TextMeshProUGUI _remainingText;
        private GameObject _bossPanel;
        private TextMeshProUGUI _bossName;
        private Image _bossFill;
        private Image _reviveFlash;
        private TextMeshProUGUI _reviveMessage;
        private Coroutine _ragePulse;
        private Coroutine _reviveFeedback;

        /// <summary>Injects dependencies when building the scene. Call before the object is activated.</summary>
        public void Configure(FeelConfig feel, WaveDirector director, Health health, Dash dash, SoulBurst soulBurst,
            TMP_FontAsset font, TMP_FontAsset boldFont)
        {
            _feel = feel;
            _director = director;
            _health = health;
            _dash = dash;
            _soulBurst = soulBurst;
            _font = font;
            _boldFont = boldFont;
        }

        private void Awake()
        {
            Build();
        }

        private void OnEnable()
        {
            _health.OnHealthChanged += ShowHealth;
            _health.OnRevived += ShowRevive;
            _dash.OnChargesChanged += ShowDashCharges;
            _soulBurst.OnRageChanged += ShowRage;
            _director.OnWaveStarted += ShowWave;
            _director.OnRemainingChanged += ShowRemaining;
            _director.OnBossSpawned += ShowBoss;
            _director.OnBossHealthChanged += ShowBossHealth;
            _director.OnBossCleared += HideBoss;
            _director.OnRunReset += HandleRunReset;
        }

        private void OnDisable()
        {
            _health.OnHealthChanged -= ShowHealth;
            _health.OnRevived -= ShowRevive;
            _dash.OnChargesChanged -= ShowDashCharges;
            _soulBurst.OnRageChanged -= ShowRage;
            _director.OnWaveStarted -= ShowWave;
            _director.OnRemainingChanged -= ShowRemaining;
            _director.OnBossSpawned -= ShowBoss;
            _director.OnBossHealthChanged -= ShowBossHealth;
            _director.OnBossCleared -= HideBoss;
            _director.OnRunReset -= HandleRunReset;
            StopFeedback();
        }

        // Every Awake has run by now, so the models exist. Wave values arrive by event when the director starts the run.
        private void Start()
        {
            ShowHealth(_health.Current, _health.Max);
            ShowDashCharges(_dash.Charges, _dash.MaxCharges);
            ShowRage(_soulBurst.Rage, _soulBurst.MaxRage);
        }

        private void OnDestroy()
        {
            if (_circle != null)
            {
                Destroy(_circle.texture);
                Destroy(_circle);
            }
        }

        private void ShowHealth(float current, float max)
        {
            float fraction = max > 0f ? current / max : 0f;
            UiKit.SetFill(_hpFill, fraction);
            _hpFill.color = fraction <= _feel.LowHealthFraction ? _hpLowColor : _hpColor;
            _hpText.text = string.Format(_hpFormat, Mathf.Ceil(current), max);
        }

        private void ShowDashCharges(int current, int max)
        {
            while (_pips.Count < max)
            {
                RectTransform pip = UiKit.CreateRect($"Pip{_pips.Count + 1}", _pipRow);
                pip.sizeDelta = new Vector2(26f, 26f);
                _pips.Add(UiKit.AddImage(pip, _pipReadyColor, _circle));
            }
            while (_pips.Count > max)
            {
                Destroy(_pips[_pips.Count - 1].gameObject);
                _pips.RemoveAt(_pips.Count - 1);
            }
            for (int i = 0; i < _pips.Count; i++)
            {
                _pips[i].color = i < current ? _pipReadyColor : _pipSpentColor;
            }
        }

        private void ShowRage(float current, float max)
        {
            UiKit.SetFill(_rageFill, max > 0f ? current / max : 0f);
            bool ready = max > 0f && current >= max;
            _rageText.text = ready ? _rageReadyText : string.Format(_rageFormat, current, max);
            if (_ragePulse != null)
            {
                StopCoroutine(_ragePulse);
                _ragePulse = null;
            }
            _rageFill.color = ready ? _rageReadyColor : _rageColor;
            _rageText.color = ready ? _rageReadyColor : _textColor;
            if (ready && isActiveAndEnabled && _feel.RageReadyPulseSpeed > 0f)
            {
                _ragePulse = StartCoroutine(PulseRage());
            }
        }

        private void ShowWave(int wave)
        {
            _waveText.text = string.Format(_waveFormat, wave, _director.WaveCount);
        }

        private void ShowRemaining(int remaining, int target)
        {
            UiKit.SetFill(_remainingFill, target > 0 ? (float)remaining / target : 0f);
            _remainingText.text = string.Format(_remainingFormat, remaining, target);
        }

        private void ShowBoss(string displayName, float current, float max)
        {
            _bossName.text = displayName;
            ShowBossHealth(current, max);
            _bossPanel.SetActive(true);
        }

        private void ShowBossHealth(float current, float max)
        {
            UiKit.SetFill(_bossFill, max > 0f ? current / max : 0f);
        }

        private void HideBoss()
        {
            _bossPanel.SetActive(false);
        }

        private void HandleRunReset()
        {
            HideBoss();
            StopFeedback();
        }

        private void ShowRevive()
        {
            if (_reviveFeedback != null)
            {
                StopCoroutine(_reviveFeedback);
            }
            if (isActiveAndEnabled)
            {
                _reviveFeedback = StartCoroutine(PlayRevive());
            }
        }

        // Real time: the revive's feedback plays the same during a hit-stop.
        private IEnumerator PlayRevive()
        {
            Color flash = _feel.ReviveFlashColor;
            float flashTime = _feel.ReviveFlashDuration;
            float messageTime = _feel.ReviveMessageDuration;
            float fadeStart = messageTime * (2f / 3f);
            _reviveFlash.gameObject.SetActive(true);
            _reviveMessage.gameObject.SetActive(true);
            float elapsed = 0f;
            while (elapsed < Mathf.Max(flashTime, messageTime))
            {
                Color f = flash;
                f.a = flash.a * (1f - Mathf.Clamp01(elapsed / flashTime));
                _reviveFlash.color = f;
                Color m = _reviveTextColor;
                m.a = elapsed < fadeStart ? 1f : 1f - Mathf.Clamp01((elapsed - fadeStart) / (messageTime - fadeStart));
                _reviveMessage.color = m;
                yield return null;
                elapsed += Time.unscaledDeltaTime;
            }
            _reviveFlash.gameObject.SetActive(false);
            _reviveMessage.gameObject.SetActive(false);
            _reviveFeedback = null;
        }

        private IEnumerator PulseRage()
        {
            while (true)
            {
                float t = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * _feel.RageReadyPulseSpeed * Mathf.PI * 2f);
                _rageFill.color = Color.Lerp(_rageColor, _rageReadyColor, t);
                yield return null;
            }
        }

        private void StopFeedback()
        {
            if (_reviveFeedback != null)
            {
                StopCoroutine(_reviveFeedback);
                _reviveFeedback = null;
            }
            if (_reviveFlash != null)
            {
                _reviveFlash.gameObject.SetActive(false);
                _reviveMessage.gameObject.SetActive(false);
            }
        }

        private void Build()
        {
            _circle = UiKit.CreateCircleSprite(64);
            _root = UiKit.CreateCanvas("HudCanvas", transform, _sortingOrder, _referenceResolution, false);
            Transform root = _root.transform;
            var topLeft = new Vector2(0f, 1f);
            var topCentre = new Vector2(0.5f, 1f);

            // Top-left: player resources.
            RectTransform panel = UiKit.Place("PlayerPanel", root, topLeft, new Vector2(24f, -24f), new Vector2(480f, 168f));
            UiKit.AddImage(panel, _panelColor);
            _hpFill = UiKit.CreateBar("HpBar", panel, topLeft, new Vector2(16f, -16f), new Vector2(448f, 34f), new Color(0f, 0f, 0f, 0.6f), _hpColor);
            _hpText = UiKit.CreateText("HpText", _hpFill.transform.parent, string.Empty, _boldFont, 24f, _textColor, TextAlignmentOptions.Center);
            UiKit.Stretch(_hpText.rectTransform);

            TextMeshProUGUI dash = UiKit.CreateText("DashLabel", panel, _dashLabel, _boldFont, 20f, _textColor, TextAlignmentOptions.MidlineLeft);
            Fit(dash.rectTransform, topLeft, new Vector2(16f, -60f), new Vector2(90f, 30f));
            _pipRow = UiKit.Place("DashPips", panel, topLeft, new Vector2(106f, -62f), new Vector2(200f, 26f));
            var row = _pipRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 10f;
            row.childAlignment = TextAnchor.MiddleLeft;
            row.childControlWidth = false;
            row.childControlHeight = false;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;

            _rageFill = UiKit.CreateBar("RageBar", panel, topLeft, new Vector2(16f, -104f), new Vector2(448f, 16f), new Color(0f, 0f, 0f, 0.6f), _rageColor);
            _rageText = UiKit.CreateText("RageText", panel, string.Empty, _boldFont, 20f, _textColor, TextAlignmentOptions.MidlineLeft);
            Fit(_rageText.rectTransform, topLeft, new Vector2(16f, -124f), new Vector2(448f, 30f));

            // Top-centre: wave progress and the boss.
            _waveText = UiKit.CreateText("WaveText", root, string.Empty, _boldFont, 46f, _textColor, TextAlignmentOptions.Center);
            Fit(_waveText.rectTransform, topCentre, new Vector2(0f, -18f), new Vector2(600f, 60f));
            _remainingFill = UiKit.CreateBar("RemainingBar", root, topCentre, new Vector2(0f, -84f), new Vector2(520f, 14f), new Color(0f, 0f, 0f, 0.6f), _remainingColor);
            _remainingText = UiKit.CreateText("RemainingText", root, string.Empty, _font, 22f, _textColor, TextAlignmentOptions.Center);
            Fit(_remainingText.rectTransform, topCentre, new Vector2(0f, -102f), new Vector2(520f, 30f));

            RectTransform boss = UiKit.Place("BossPanel", root, topCentre, new Vector2(0f, -146f), new Vector2(920f, 80f));
            UiKit.AddImage(boss, _panelColor);
            _bossPanel = boss.gameObject;
            _bossName = UiKit.CreateText("BossName", boss, string.Empty, _boldFont, 28f, _textColor, TextAlignmentOptions.Center);
            Fit(_bossName.rectTransform, topCentre, new Vector2(0f, -6f), new Vector2(900f, 36f));
            _bossFill = UiKit.CreateBar("BossBar", boss, topCentre, new Vector2(0f, -46f), new Vector2(900f, 24f), new Color(0f, 0f, 0f, 0.6f), _bossColor);
            _bossPanel.SetActive(false);

            // Revive feedback.
            RectTransform flash = UiKit.CreateRect("ReviveFlash", root);
            UiKit.Stretch(flash);
            _reviveFlash = UiKit.AddImage(flash, _feel.ReviveFlashColor);
            flash.gameObject.SetActive(false);
            _reviveMessage = UiKit.CreateText("ReviveMessage", root, _reviveText, _boldFont, 72f, _reviveTextColor, TextAlignmentOptions.Center);
            Fit(_reviveMessage.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 160f), new Vector2(900f, 100f));
            _reviveMessage.gameObject.SetActive(false);
        }

        private static void Fit(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
