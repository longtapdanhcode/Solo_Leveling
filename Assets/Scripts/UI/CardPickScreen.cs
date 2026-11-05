// Implements: production/epics/micro-arena-survivor/story-004-upgrade-cards.md (card pick screen: name, description,
// rarity; pick with 1/2/3 or a click). Visual polish is story 007.

using System;
using System.Collections.Generic;
using MicroArena.Gameplay.Cards;
using MicroArena.Gameplay.Combat;
using MicroArena.Gameplay.Config;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace MicroArena.UI
{
    /// <summary>
    /// Runtime-built UGUI card pick screen. Pauses the game through <see cref="GamePause"/> while shown and works at
    /// time scale 0 (no scaled-time animation). Keys 1/2/3 or a click pick a card; keys pressed on the frame the screen
    /// opens are ignored. Creates an EventSystem with <see cref="InputSystemUIInputModule"/> when
    /// <see cref="EventSystem.current"/> is null (no scene search).
    /// </summary>
    public sealed class CardPickScreen : MonoBehaviour, ICardPickView
    {
        private const string BuiltinFontName = "LegacyRuntime.ttf";

        [Header("Layout (presentation only; polish in story 007)")]
        [SerializeField] private Vector2 _referenceResolution = new Vector2(1920f, 1080f);
        [SerializeField] private Vector2 _cardSize = new Vector2(440f, 560f);
        [SerializeField, Min(0f)] private float _cardSpacing = 60f;
        [SerializeField] private int _sortingOrder = 100;
        [SerializeField] private string _title = "CHỌN MỘT THẺ NÂNG CẤP";
        [SerializeField] private Color _dimColor = new Color(0f, 0f, 0f, 0.7f);
        [SerializeField] private Color _cardColor = new Color(0.12f, 0.12f, 0.16f, 0.97f);
        [SerializeField] private Color _textColor = new Color(0.95f, 0.95f, 0.95f);

        [Header("Rarity labels and colors")]
        [SerializeField] private string _commonLabel = "THƯỜNG";
        [SerializeField] private string _rareLabel = "HIẾM";
        [SerializeField] private string _legendaryLabel = "HUYỀN THOẠI";
        [SerializeField] private Color _commonColor = new Color(0.8f, 0.8f, 0.8f);
        [SerializeField] private Color _rareColor = new Color(0.3f, 0.6f, 1f);
        [SerializeField] private Color _legendaryColor = new Color(1f, 0.7f, 0.15f);

        private static readonly Key[] PickKeys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5 };
        private static readonly Key[] PickPadKeys = { Key.Numpad1, Key.Numpad2, Key.Numpad3, Key.Numpad4, Key.Numpad5 };

        private GameObject _root;
        private RectTransform _row;
        private Font _font;
        private readonly List<GameObject> _cards = new List<GameObject>();
        private Action<int> _onPicked;
        private int _shownFrame = -1;
        private int _count;

        /// <inheritdoc />
        public bool IsShown { get; private set; }

        /// <inheritdoc />
        public void Show(IReadOnlyList<CardConfig> cards, Action<int> onPicked)
        {
            EnsureBuilt();
            EnsureEventSystem();
            ClearCards();
            _count = cards.Count;
            for (int i = 0; i < cards.Count; i++)
            {
                _cards.Add(BuildCard(cards[i], i));
            }
            _onPicked = onPicked;
            _shownFrame = Time.frameCount;
            _root.SetActive(true);
            IsShown = true;
            GamePause.Acquire(this);
            if (EventSystem.current != null && _cards.Count > 0)
            {
                EventSystem.current.SetSelectedGameObject(_cards[0]); // gamepad/arrow navigation + submit
            }
        }

        /// <inheritdoc />
        public void Hide()
        {
            _onPicked = null;
            IsShown = false;
            if (_root != null)
            {
                _root.SetActive(false);
            }
            GamePause.Release(this);
        }

        private void OnDisable()
        {
            Hide();
        }

        private void Update()
        {
            if (!IsShown || Time.frameCount == _shownFrame)
            {
                return;
            }
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }
            int limit = Mathf.Min(_count, PickKeys.Length);
            for (int i = 0; i < limit; i++)
            {
                if (keyboard[PickKeys[i]].wasPressedThisFrame || keyboard[PickPadKeys[i]].wasPressedThisFrame)
                {
                    Pick(i);
                    return;
                }
            }
        }

        private void Pick(int index)
        {
            if (!IsShown || Time.frameCount == _shownFrame)
            {
                return;
            }
            Action<int> callback = _onPicked;
            _onPicked = null; // one pick per show, even if a key and a click land in the same frame
            callback?.Invoke(index);
        }

        private void EnsureBuilt()
        {
            if (_root != null)
            {
                return;
            }
            _font = Resources.GetBuiltinResource<Font>(BuiltinFontName);

            _root = new GameObject("CardPickCanvas", typeof(RectTransform));
            _root.transform.SetParent(transform, false);
            var canvas = _root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = _sortingOrder;
            var scaler = _root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = _referenceResolution;
            scaler.matchWidthOrHeight = 0.5f;
            _root.AddComponent<GraphicRaycaster>();

            RectTransform dim = CreateRect("Dim", _root.transform);
            Stretch(dim);
            dim.gameObject.AddComponent<Image>().color = _dimColor;

            Text title = CreateText("Title", _root.transform, _title, 56, FontStyle.Bold, _textColor, TextAnchor.MiddleCenter);
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -110f);
            titleRect.sizeDelta = new Vector2(0f, 90f);

            _row = CreateRect("Cards", _root.transform);
            // Full-width row: with zero width the layout group would start at the centre and overflow to the right.
            _row.anchorMin = new Vector2(0f, 0.5f);
            _row.anchorMax = new Vector2(1f, 0.5f);
            _row.anchoredPosition = new Vector2(0f, -40f);
            _row.sizeDelta = new Vector2(0f, _cardSize.y);
            var layout = _row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = _cardSpacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            _root.SetActive(false);
        }

        private GameObject BuildCard(CardConfig card, int index)
        {
            RectTransform rect = CreateRect($"Card{index + 1}", _row);
            rect.sizeDelta = _cardSize;
            var image = rect.gameObject.AddComponent<Image>();
            image.color = _cardColor;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            int captured = index;
            button.onClick.AddListener(() => Pick(captured));

            Color rarityColor = RarityColor(card.Rarity);
            var outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = rarityColor;
            outline.effectDistance = new Vector2(4f, -4f);

            Place(CreateText("Rarity", rect, RarityLabel(card.Rarity), 30, FontStyle.Bold, rarityColor, TextAnchor.MiddleCenter), 30f, 50f);
            Place(CreateText("Name", rect, card.DisplayName, 42, FontStyle.Bold, _textColor, TextAnchor.MiddleCenter), 100f, 110f);
            Place(CreateText("Description", rect, card.Description, 30, FontStyle.Normal, _textColor, TextAnchor.UpperCenter), 240f, 220f);
            Place(CreateText("Key", rect, $"[{index + 1}]", 36, FontStyle.Bold, rarityColor, TextAnchor.MiddleCenter), _cardSize.y - 80f, 50f);
            return rect.gameObject;
        }

        // Top-anchored block inside a card, with side padding.
        private static void Place(Text text, float top, float height)
        {
            RectTransform r = text.rectTransform;
            r.anchorMin = new Vector2(0f, 1f);
            r.anchorMax = new Vector2(1f, 1f);
            r.pivot = new Vector2(0.5f, 1f);
            r.anchoredPosition = new Vector2(0f, -top);
            r.sizeDelta = new Vector2(-48f, height);
        }

        private Text CreateText(string name, Transform parent, string content, int size, FontStyle style, Color color, TextAnchor anchor)
        {
            RectTransform rect = CreateRect(name, parent);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = _font;
            text.text = content;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void ClearCards()
        {
            foreach (GameObject card in _cards)
            {
                Destroy(card);
            }
            _cards.Clear();
        }

        private string RarityLabel(CardRarity rarity)
        {
            switch (rarity)
            {
                case CardRarity.Common: return _commonLabel;
                case CardRarity.Rare: return _rareLabel;
                default: return _legendaryLabel;
            }
        }

        private Color RarityColor(CardRarity rarity)
        {
            switch (rarity)
            {
                case CardRarity.Common: return _commonColor;
                case CardRarity.Rare: return _rareColor;
                default: return _legendaryColor;
            }
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) // set by any enabled EventSystem; no scene search
            {
                return;
            }
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            var module = go.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }
    }
}
