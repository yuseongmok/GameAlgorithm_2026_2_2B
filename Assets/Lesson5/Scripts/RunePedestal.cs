using UnityEngine;

namespace AlgoCourse.Lesson5
{
    public sealed class RunePedestal : MonoBehaviour
    {
        [SerializeField] private int runeId;
        [SerializeField] private Renderer runeRenderer;
        private Color baseColor;

        public int RuneId => runeId;

        public void Configure(int id, Renderer renderer, Color color)
        {
            runeId = id;
            runeRenderer = renderer;
            baseColor = color;
            runeRenderer.material.color = color;
        }

        public void SetState(bool selected, bool rejected)
        {
            if (runeRenderer == null) return;
            runeRenderer.material.color = rejected
                ? new Color(0.9f, 0.12f, 0.12f)
                : selected ? Color.white : baseColor;
        }
    }
}
