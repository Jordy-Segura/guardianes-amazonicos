using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GuardianesAmazonicos.Nivel1.HU03
{
    public static class JungleUi
    {
        public static readonly Color Ink = new Color32(9, 39, 30, 255);
        public static readonly Color Cream = new Color32(255, 248, 227, 255);
        public static readonly Color Gold = new Color32(255, 212, 56, 255);
        public static readonly Color Blue = new Color32(134, 217, 255, 255);
        public static readonly Color Purple = new Color32(213, 194, 255, 255);
        public static readonly Color Green = new Color32(11, 102, 35, 255);
        private static Sprite _rounded;
        private static TMP_FontAsset _font;

        public static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(w, h);
            return rect;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        public static Image Image(Transform parent, string name, Sprite sprite, Color color,
            float x, float y, float w, float h, bool preserve = false)
        {
            var image = Rect(name, parent, x, y, w, h).gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.preserveAspect = preserve;
            image.raycastTarget = false;
            return image;
        }

        public static RectTransform Panel(Transform parent, string name, Color color,
            float x, float y, float w, float h, bool goldEdge = true)
        {
            var rect = Rect(name, parent, x, y, w, h);
            var shadow = Image(rect, "Shadow", Rounded(), new Color32(3, 29, 18, 100), 0, 7, w, h);
            shadow.type = UnityEngine.UI.Image.Type.Sliced;
            var edge = Image(rect, "Edge", Rounded(), goldEdge ? new Color32(190, 124, 20, 255) : Ink, 0, 0, w, h);
            edge.type = UnityEngine.UI.Image.Type.Sliced;
            var inner = Image(rect, "Surface", Rounded(), color, 4, 4, w - 8, h - 8);
            inner.type = UnityEngine.UI.Image.Type.Sliced;
            var shine = Image(rect, "Highlight", Rounded(), new Color(1, 1, 1, .2f), 8, 8, w - 16, 12);
            shine.type = UnityEngine.UI.Image.Type.Sliced;
            return rect;
        }

        public static TextMeshProUGUI Text(Transform parent, string name, string value, float size,
            float x, float y, float w, float h, Color? color = null,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            if (_font == null) _font = Resources.Load<TMP_FontAsset>("HU03/NunitoExtraBold SDF");
            var text = Rect(name, parent, x, y, w, h).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = _font != null ? _font : TMP_Settings.defaultFontAsset;
            text.text = value;
            text.fontSize = size;
            text.color = color ?? Ink;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.raycastTarget = false;
            text.richText = false;
            text.margin = new Vector4(4, 0, 4, 0);
            return text;
        }

        public static Button Button(Transform parent, string name, string label, Color color,
            float x, float y, float w, float h, Action action, float fontSize = 32)
        {
            var rect = Panel(parent, name, color, x, y, w, h);
            var surface = rect.Find("Surface").GetComponent<Image>();
            surface.raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = surface;
            var colors = button.colors;
            colors.highlightedColor = new Color(1.06f, 1.06f, 1.06f);
            colors.pressedColor = new Color(.82f, .86f, .82f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(.68f, .68f, .68f, .8f);
            colors.fadeDuration = 0;
            button.colors = colors;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(() => action?.Invoke());
            rect.gameObject.AddComponent<AccessibleButton>();
            Text(rect, "Label", label, fontSize, 12, 8, w - 24, h - 16);
            return button;
        }

        public static Sprite Rounded()
        {
            if (_rounded != null) return _rounded;
            const int size = 64;
            const float radius = 20;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(radius - x, x - (size - 1 - radius), 0);
                float dy = Mathf.Max(radius - y, y - (size - 1 - radius), 0);
                float alpha = Mathf.Clamp01(radius - Mathf.Sqrt(dx * dx + dy * dy) + .5f);
                texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
            texture.Apply();
            _rounded = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * .5f,
                100, 0, SpriteMeshType.FullRect, new Vector4(22, 22, 22, 22));
            return _rounded;
        }
    }
}
