using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace MetroLine
{
    public sealed class MetroStationCanvasUI : MonoBehaviour
    {
        private sealed class LabelEntry
        {
            public MetroStationView Station;
            public RectTransform Rect;
            public Text Text;
        }

        private readonly List<LabelEntry> labels = new();
        private RectTransform canvasRect;
        private Text statusText;
        private Text addTrainButtonText;
        private Camera worldCamera;
        private Font font;

        public event Action AddTrainRequested;

        public void Build(IReadOnlyList<MetroStationView> stations)
        {
            EnsureEventSystem();
            worldCamera = Camera.main;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject canvasObject = new("Runtime_MetroStationCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            canvasRect = canvasObject.GetComponent<RectTransform>();
            CreateStatusPanel(canvasObject.transform);
            CreateAddTrainButton(canvasObject.transform);

            foreach (MetroStationView station in stations)
                labels.Add(CreateLabel(canvasObject.transform, station));
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            GameObject eventSystemObject = new("Runtime_EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
        }

        public void SetStatus(string value)
        {
            if (statusText != null) statusText.text = value;
        }

        public void SetTrainStock(int stock, bool placementMode)
        {
            if (addTrainButtonText == null) return;
            addTrainButtonText.text = placementMode
                ? "역을 선택하세요\n우클릭: 취소"
                : $"🚇 지하철 추가\n보유 {stock}대";
            addTrainButtonText.color = stock > 0 || placementMode
                ? Color.white
                : new Color(0.55f, 0.6f, 0.68f);
        }

        public void Refresh(IReadOnlyList<MetroStationState> states, int activeCount)
        {
            if (worldCamera == null) worldCamera = Camera.main;
            if (worldCamera == null || canvasRect == null) return;

            for (int i = 0; i < labels.Count; i++)
            {
                LabelEntry entry = labels[i];
                bool visible = i < activeCount && entry.Station.gameObject.activeInHierarchy;
                entry.Text.gameObject.SetActive(visible);
                if (!visible) continue;

                MetroStationState state = states.First(s => s.Id == entry.Station.StationId);
                int circles = state.Waiting.Count(p => p.DestinationShape == MetroShape.Circle);
                int triangles = state.Waiting.Count(p => p.DestinationShape == MetroShape.Triangle);
                int squares = state.Waiting.Count(p => p.DestinationShape == MetroShape.Square);

                entry.Text.text = $"{state.Name}  대기 {state.Waiting.Count}\n○{circles}  △{triangles}  □{squares}";
                entry.Text.color = state.Waiting.Count >= 7
                    ? new Color(1f, 0.25f, 0.2f)
                    : state.Waiting.Count >= 4
                        ? new Color(1f, 0.82f, 0.2f)
                        : Color.white;

                Vector3 screen = worldCamera.WorldToScreenPoint(entry.Station.transform.position + Vector3.up * 0.9f);
                bool onScreen = screen.z > 0f && screen.x >= 0f && screen.x <= Screen.width
                    && screen.y >= 0f && screen.y <= Screen.height;
                entry.Text.gameObject.SetActive(onScreen);
                if (!onScreen) continue;

                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect,
                    new Vector2(screen.x, screen.y), null, out Vector2 localPoint);
                entry.Rect.anchoredPosition = localPoint + Vector2.up * 24f;
            }
        }

        private LabelEntry CreateLabel(Transform parent, MetroStationView station)
        {
            GameObject labelObject = new($"StationHUD_{station.StationId:00}");
            labelObject.transform.SetParent(parent, false);
            RectTransform rect = labelObject.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(250f, 76f);

            Text text = labelObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = 23;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            Outline outline = labelObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.95f);
            outline.effectDistance = new Vector2(2f, -2f);
            return new LabelEntry { Station = station, Rect = rect, Text = text };
        }

        private void CreateStatusPanel(Transform parent)
        {
            GameObject panelObject = new("MetroStatusPanel");
            panelObject.transform.SetParent(parent, false);
            RectTransform panelRect = panelObject.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.zero;
            panelRect.pivot = Vector2.zero;
            panelRect.anchoredPosition = new Vector2(18f, 18f);
            panelRect.sizeDelta = new Vector2(570f, 286f);
            Image panel = panelObject.AddComponent<Image>();
            panel.color = new Color(0.015f, 0.03f, 0.055f, 0.9f);
            panel.raycastTarget = false;

            GameObject textObject = new("StatusText");
            textObject.transform.SetParent(panelObject.transform, false);
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(18f, 14f);
            textRect.offsetMax = new Vector2(-18f, -14f);
            statusText = textObject.AddComponent<Text>();
            statusText.font = font;
            statusText.fontSize = 24;
            statusText.fontStyle = FontStyle.Bold;
            statusText.alignment = TextAnchor.UpperLeft;
            statusText.color = Color.white;
            statusText.raycastTarget = false;
            statusText.horizontalOverflow = HorizontalWrapMode.Wrap;
            statusText.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private void CreateAddTrainButton(Transform parent)
        {
            GameObject buttonObject = new("AddTrainButton");
            buttonObject.transform.SetParent(parent, false);
            RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
            buttonRect.anchorMin = Vector2.one;
            buttonRect.anchorMax = Vector2.one;
            buttonRect.pivot = Vector2.one;
            buttonRect.anchoredPosition = new Vector2(-24f, -24f);
            buttonRect.sizeDelta = new Vector2(330f, 108f);

            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.06f, 0.42f, 0.68f, 0.96f);
            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.12f, 0.58f, 0.9f);
            colors.pressedColor = new Color(0.04f, 0.28f, 0.5f);
            button.colors = colors;
            button.onClick.AddListener(() => AddTrainRequested?.Invoke());

            GameObject textObject = new("Text");
            textObject.transform.SetParent(buttonObject.transform, false);
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10f, 6f);
            textRect.offsetMax = new Vector2(-10f, -6f);
            addTrainButtonText = textObject.AddComponent<Text>();
            addTrainButtonText.font = font;
            addTrainButtonText.fontSize = 29;
            addTrainButtonText.fontStyle = FontStyle.Bold;
            addTrainButtonText.alignment = TextAnchor.MiddleCenter;
            addTrainButtonText.color = Color.white;
            addTrainButtonText.raycastTarget = false;
        }
    }
}
