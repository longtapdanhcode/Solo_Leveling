// Implements: production/epics/micro-arena-survivor/story-004-upgrade-cards.md (card pick screen: name, description,
// rarity; pick with 1/2/3 or a click). Story 007: TextMeshPro with Be Vietnam Pro (Vietnamese diacritics), no legacy Text.

using System;
using System.Collections.Generic;
using MicroArena.Gameplay.Cards;
using MicroArena.Gameplay.Combat;
using MicroArena.Gameplay.Config;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MicroArena.UI
{
    /// <summary>
    /// Runtime-built UGUI card pick screen. Pauses the game through <see cref="GamePause"/> while shown and works at
    /// time scale 0 (no scaled-time animation). Keys 1/2/3 or a click pick a card; keys pressed on the frame the screen
    /// opens are ignored. Creates an EventSystem through <see cref="UiKit.EnsureEventSystem"/> when none is current.
    /// </summary>
    public sealed class CardPickScreen : MonoBehaviour, ICardPickView
    {
        [Header("Fonts (Be Vietnam Pro TMP font assets)")]
        [SerializeField] private TMP_FontAsset _font;
        [SerializeField] private TMP_FontAsset _boldFont;

        [Header("Layout (presentation only)")]
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
        private readonly List<GameObject> _cards = new List<GameObject>();
        private Action<int> _onPicked;
        private int _shownFrame = -1;
        private int _count;

        /// <inheritdoc />
        public bool IsShown { get; private set; }

        /// <summary>Sets the TextMeshPro font assets when building the scene. Call before the screen is first shown.</summary>
        public void Configure(TMP_FontAsset font, TMP_FontAsset boldFont)
        {
            _font = font;
            _boldFont = boldFont;
        }

        /// <inheritdoc />
        public void Show(IReadOnlyList<CardConfig> cards, Action<int> onPicked)
        {
            EnsureBuilt();
            UiKit.EnsureEventSystem();
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
            _root = UiKit.CreateCanvas("CardPickCanvas", transform, _sortingOrder, _referenceResolution, true);

            RectTransform dim = UiKit.CreateRect("Dim", _root.transform);
            UiKit.Stretch(dim);
            dim.gameObject.AddComponent<Image>().color = _dimColor;

            TextMeshProUGUI title = UiKit.CreateText("Title", _root.transform, _title, _boldFont, 56f, _textColor, TextAlignmentOptions.Center);
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -110f);
            titleRect.sizeDelta = new Vector2(0f, 90f);

            _row = UiKit.CreateRect("Cards", _root.transform);
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
            RectTransform rect = UiKit.CreateRect($"Card{index + 1}", _row);
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

            Place(UiKit.CreateText("Rarity", rect, RarityLabel(card.Rarity), _boldFont, 30f, rarityColor, TextAlignmentOptions.Center), 30f, 50f);
            Place(UiKit.CreateText("Name", rect, card.DisplayName, _boldFont, 42f, _textColor, TextAlignmentOptions.Center), 100f, 110f);
            Place(UiKit.CreateText("Description", rect, card.Description, _font, 30f, _textColor, TextAlignmentOptions.Top), 240f, 220f);
            Place(UiKit.CreateText("Key", rect, $"[{index + 1}]", _boldFont, 36f, rarityColor, TextAlignmentOptions.Center), _cardSize.y - 80f, 50f);
            return rect.gameObject;
        }

        // Top-anchored block inside a card, with side padding.
        private static void Place(TMP_Text text, float top, float height)
        {
            RectTransform r = text.rectTransform;
            r.anchorMin = new Vector2(0f, 1f);
            r.anchorMax = new Vector2(1f, 1f);
            r.pivot = new Vector2(0.5f, 1f);
            r.anchoredPosition = new Vector2(0f, -top);
            r.sizeDelta = new Vector2(-48f, height);
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
    }
}
