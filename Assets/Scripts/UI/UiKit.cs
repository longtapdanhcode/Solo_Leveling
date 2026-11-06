// Implements: production/epics/micro-arena-survivor/story-007-hud-game-feel.md (all UI on TextMeshPro with Be Vietnam Pro).
// Shared by HudController, EndScreens and CardPickScreen, which build their UGUI at runtime.

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace MicroArena.UI
{
    /// <summary>Runtime UGUI building helpers: overlay canvas, rects, TextMeshPro labels, bars, the event system.</summary>
    public static class UiKit
    {
        /// <summary>Creates a screen-space overlay canvas scaled from <paramref name="referenceResolution"/>, child of <paramref name="parent"/>.</summary>
        public static GameObject CreateCanvas(string name, Transform parent, int sortingOrder, Vector2 referenceResolution, bool raycasts)
        {
            var root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.matchWidthOrHeight = 0.5f;
            if (raycasts)
            {
                root.AddComponent<GraphicRaycaster>();
            }
            return root;
        }

        /// <summary>Creates an empty RectTransform under <paramref name="parent"/>.</summary>
        public static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        /// <summary>Creates a rect anchored and pivoted at <paramref name="anchor"/>, placed at <paramref name="position"/> with <paramref name="size"/>.</summary>
        public static RectTransform Place(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        /// <summary>Makes <paramref name="rect"/> fill its parent.</summary>
        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>Adds a plain coloured Image (no raycast) to <paramref name="rect"/>.</summary>
        public static Image AddImage(RectTransform rect, Color color, Sprite sprite = null)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        /// <summary>Creates a TextMeshPro label on a new child rect of <paramref name="parent"/>; wraps, never raycasts. Size the rect afterwards.</summary>
        public static TextMeshProUGUI CreateText(string name, Transform parent, string content, TMP_FontAsset font, float size,
            Color color, TextAlignmentOptions alignment)
        {
            RectTransform rect = CreateRect(name, parent);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Overflow;
            text.raycastTarget = false;
            return text;
        }

        /// <summary>
        /// Creates a horizontal bar: a background and a left-anchored fill; returns the fill image. Set the value with
        /// <see cref="SetFill"/> (anchor based, so it needs no sprite). The background is the fill's parent.
        /// </summary>
        public static Image CreateBar(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Color background, Color fill)
        {
            RectTransform back = Place(name, parent, anchor, position, size);
            AddImage(back, background);
            RectTransform fillRect = CreateRect("Fill", back);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            return AddImage(fillRect, fill);
        }

        /// <summary>Sets a bar made by <see cref="CreateBar"/> to <paramref name="fraction"/> (clamped to 0..1).</summary>
        public static void SetFill(Image fill, float fraction)
        {
            fill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(fraction), 1f);
        }

        /// <summary>Reads back the fraction set by <see cref="SetFill"/>.</summary>
        public static float GetFill(Image fill)
        {
            return fill.rectTransform.anchorMax.x;
        }

        /// <summary>Creates a filled-circle sprite of <paramref name="size"/> pixels (anti-aliased edge). The caller destroys texture and sprite.</summary>
        public static Sprite CreateCircleSprite(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "UiCircle", wrapMode = TextureWrapMode.Clamp };
            float radius = size * 0.5f;
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(radius, radius));
                    byte alpha = (byte)(Mathf.Clamp01(radius - distance) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        /// <summary>
        /// Creates an EventSystem with <see cref="InputSystemUIInputModule"/> (default actions, works at time scale 0) when
        /// <see cref="EventSystem.current"/> is null. No scene search.
        /// </summary>
        public static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
    }
}
