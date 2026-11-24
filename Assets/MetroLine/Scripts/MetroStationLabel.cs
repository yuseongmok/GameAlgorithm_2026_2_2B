using UnityEngine;

namespace MetroLine
{
    public sealed class MetroStationLabel : MonoBehaviour
    {
        private TextMesh textMesh;

        public void Initialize(string stationName)
        {
            textMesh = GetComponent<TextMesh>();
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = 46;
            textMesh.characterSize = 0.16f;
            textMesh.color = Color.white;
            SetText(stationName, 0);
        }

        public void SetText(string stationName, int waitingCount)
        {
            if (textMesh == null) textMesh = GetComponent<TextMesh>();
            textMesh.text = $"{stationName}\n대기 {waitingCount}";
            textMesh.color = waitingCount >= 7
                ? new Color(1f, 0.25f, 0.2f)
                : waitingCount >= 4
                    ? new Color(1f, 0.8f, 0.2f)
                    : Color.white;
        }

        private void LateUpdate()
        {
            if (Camera.main == null) return;
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }
    }
}
