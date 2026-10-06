using UnityEngine;

namespace AlgoCourse.Lesson4
{
    public sealed class RescueSurvivor : MonoBehaviour
    {
        [SerializeField] private string survivorName = "연구원";
        [SerializeField] private int score = 100;
        [SerializeField] private int gridX;
        [SerializeField] private int gridY;
        [SerializeField] private Renderer bodyRenderer;

        public string SurvivorName => survivorName;
        public int Score => score;
        public GridPosition Position => new GridPosition(gridX, gridY);
        public bool IsRescued { get; private set; }

        public void Configure(string displayName, int rescueScore, GridPosition position, Renderer renderer)
        {
            survivorName = displayName;
            score = rescueScore;
            gridX = position.X;
            gridY = position.Y;
            bodyRenderer = renderer;
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (bodyRenderer != null)
            {
                bodyRenderer.material.color = selected
                    ? new Color(1f, 0.88f, 0.12f)
                    : new Color(0.72f, 0.24f, 0.88f);
            }
        }

        public void Rescue()
        {
            IsRescued = true;
            gameObject.SetActive(false);
        }

        public void ResetSurvivor()
        {
            IsRescued = false;
            gameObject.SetActive(true);
            SetSelected(false);
        }
    }
}
