using UnityEngine;
using UnityEngine.UI;

namespace AlgoCourse.Lesson3
{
    public sealed class PandemicNodeUI : MonoBehaviour
    {
        private Canvas canvas;
        private Font font;
        private Text status;
        private Text stats;
        private Text auto;

        public void Initialize()
        {
            if (canvas != null) return;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GameObject root = new GameObject("Pandemic Node UI");
            root.transform.SetParent(transform, false);
            canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            root.AddComponent<GraphicRaycaster>();

            status = Create("Status", new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(1240f, 82f), 30);
            stats = Create("Stats", new Vector2(1f, 1f), new Vector2(-28f, -145f), new Vector2(340f, 200f), 27);
            auto = Create("Auto", new Vector2(0f, 1f), new Vector2(28f, -85f), new Vector2(300f, 64f), 24);
            Text guide = Create("Guide", new Vector2(0.5f, 0f), new Vector2(0f, 25f), new Vector2(1150f, 68f), 24);
            guide.text = "도시 클릭  Enqueue     Space  Dequeue 1회     A  자동 처리     R  초기화";
            auto.text = "자동 처리: OFF";
        }

        public void SetState(string message, int steps, int queue, int outbreaks)
        {
            if (status != null) status.text = message;
            if (stats != null) stats.text = $"처리 횟수  {steps}\n\nQueue  {queue}\nOutbreak  {outbreaks}";
        }

        public void SetAutoMode(bool enabled)
        {
            if (auto == null) return;
            auto.text = enabled ? "자동 처리: ON" : "자동 처리: OFF";
            auto.color = enabled ? new Color(1f, 0.55f, 0.15f) : Color.white;
        }

        private Text Create(string name, Vector2 anchor, Vector2 position, Vector2 size, int fontSize)
        {
            GameObject panel = new GameObject(name + " Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas.transform, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchor; rect.anchorMax = anchor; rect.pivot = anchor;
            rect.anchoredPosition = position; rect.sizeDelta = size;
            panel.GetComponent<Image>().color = new Color(0.03f, 0.06f, 0.10f, 0.90f);
            GameObject labelObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(panel.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(18f, 10f); labelRect.offsetMax = new Vector2(-18f, -10f);
            Text label = labelObject.GetComponent<Text>();
            label.font = font; label.fontSize = fontSize; label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white; label.raycastTarget = false;
            return label;
        }
    }
}
