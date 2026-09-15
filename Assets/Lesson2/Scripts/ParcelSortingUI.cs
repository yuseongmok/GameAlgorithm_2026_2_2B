using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AlgoCourse.Lesson2
{
    /// <summary>
    /// 화면 고정 안내문과 택배를 따라다니는 번호표를 관리합니다.
    /// Dictionary 수업과 관계없는 UI 코드는 제공 영역에 둡니다.
    /// </summary>
    public sealed class ParcelSortingUI : MonoBehaviour
    {
        private readonly Dictionary<ParcelBox, Text> parcelLabels =
            new Dictionary<ParcelBox, Text>();

        private Canvas canvas;
        private Font font;
        private Text statusText;
        private RectTransform labelLayer;
        private Camera worldCamera;

        public void Initialize()
        {
            if (canvas != null)
            {
                return;
            }

            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            worldCamera = Camera.main;
            CreateCanvas();
            CreateHeader();
            CreateControlGuide();
            CreateDestinationGuide();
            CreateLabelLayer();
        }

        public void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        private void LateUpdate()
        {
            if (canvas == null)
            {
                return;
            }

            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }

            SynchronizeParcelLabels();
            UpdateParcelLabelPositions();
        }

        private void CreateCanvas()
        {
            GameObject canvasObject = new GameObject("Parcel Sorting UI");
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        private void CreateHeader()
        {
            GameObject panel = CreatePanel(
                "Status Panel",
                canvas.transform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -24f),
                new Vector2(980f, 96f),
                new Color(0.04f, 0.07f, 0.11f, 0.90f));

            statusText = CreateText("Status", panel.transform, 30, TextAnchor.MiddleCenter, Color.white);
            statusText.text = "택배 분류 시스템 준비 중...";
        }

        private void CreateControlGuide()
        {
            GameObject panel = CreatePanel(
                "Control Guide",
                canvas.transform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 22f),
                new Vector2(760f, 70f),
                new Color(0.04f, 0.07f, 0.11f, 0.86f));

            Text guide = CreateText("Guide", panel.transform, 25, TextAnchor.MiddleCenter, Color.white);
            guide.text = "이동  WASD / 방향키     |     가까운 택배 스캔  E";
        }

        private void CreateDestinationGuide()
        {
            GameObject panel = CreatePanel(
                "Destination Guide",
                canvas.transform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-24f, -145f),
                new Vector2(330f, 230f),
                new Color(0.04f, 0.07f, 0.11f, 0.86f));

            Text guide = CreateText("Destinations", panel.transform, 24, TextAnchor.MiddleLeft, Color.white);
            guide.rectTransform.offsetMin = new Vector2(28f, 18f);
            guide.rectTransform.offsetMax = new Vector2(-20f, -18f);
            guide.text = "분류 구역\n\nA  생활용품\nB  장비\nC  귀중품\n\n미등록 → 검사 구역";
        }

        private void CreateLabelLayer()
        {
            GameObject layer = new GameObject("Parcel Number Labels", typeof(RectTransform));
            layer.transform.SetParent(canvas.transform, false);
            labelLayer = layer.GetComponent<RectTransform>();
            labelLayer.anchorMin = Vector2.zero;
            labelLayer.anchorMax = Vector2.one;
            labelLayer.offsetMin = Vector2.zero;
            labelLayer.offsetMax = Vector2.zero;
        }

        private void SynchronizeParcelLabels()
        {
            ParcelBox[] parcels = FindObjectsByType<ParcelBox>(FindObjectsSortMode.None);

            foreach (ParcelBox parcel in parcels)
            {
                if (!parcelLabels.ContainsKey(parcel))
                {
                    Text label = CreateParcelLabel(parcel.ParcelId);
                    parcelLabels.Add(parcel, label);
                }
            }

            List<ParcelBox> removed = new List<ParcelBox>();
            foreach (KeyValuePair<ParcelBox, Text> pair in parcelLabels)
            {
                if (pair.Key == null)
                {
                    Destroy(pair.Value.transform.parent.gameObject);
                    removed.Add(pair.Key);
                }
            }

            foreach (ParcelBox parcel in removed)
            {
                parcelLabels.Remove(parcel);
            }
        }

        private Text CreateParcelLabel(int parcelId)
        {
            GameObject background = new GameObject(
                $"Label {parcelId}",
                typeof(RectTransform),
                typeof(Image));
            background.transform.SetParent(labelLayer, false);
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.sizeDelta = new Vector2(112f, 42f);
            Image image = background.GetComponent<Image>();
            image.color = new Color(1f, 0.92f, 0.56f, 0.96f);
            image.raycastTarget = false;

            Text label = CreateText("Number", background.transform, 24, TextAnchor.MiddleCenter, Color.black);
            label.text = parcelId.ToString();
            return label;
        }

        private void UpdateParcelLabelPositions()
        {
            if (worldCamera == null)
            {
                return;
            }

            foreach (KeyValuePair<ParcelBox, Text> pair in parcelLabels)
            {
                if (pair.Key == null || pair.Value == null)
                {
                    continue;
                }

                Vector3 screenPosition = worldCamera.WorldToScreenPoint(
                    pair.Key.transform.position + Vector3.up * 0.85f);
                bool visible = screenPosition.z > 0f;
                GameObject labelObject = pair.Value.transform.parent.gameObject;
                labelObject.SetActive(visible);

                if (visible)
                {
                    labelObject.GetComponent<RectTransform>().position = screenPosition;
                    pair.Value.text = pair.Key.ParcelId.ToString();
                }
            }
        }

        private GameObject CreatePanel(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 position,
            Vector2 size,
            Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMin;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private Text CreateText(
            string name,
            Transform parent,
            int fontSize,
            TextAnchor alignment,
            Color color)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Text text = textObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }
    }
}
