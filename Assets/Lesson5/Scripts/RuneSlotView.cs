using UnityEngine;

namespace AlgoCourse.Lesson5
{
    public sealed class RuneSlotView : MonoBehaviour
    {
        [SerializeField] private Renderer slotRenderer;
        [SerializeField] private TextMesh label;

        public void Configure(Renderer renderer, TextMesh text)
        {
            slotRenderer = renderer;
            label = text;
            Clear();
        }

        public void ShowRune(RuneData rune, Color color)
        {
            slotRenderer.material.color = color;
            label.text = $"{rune.DisplayName}\n{rune.Value}";
        }

        public void Clear()
        {
            if (slotRenderer != null) slotRenderer.material.color = new Color(0.08f, 0.11f, 0.14f);
            if (label != null) label.text = "?";
        }
    }
}
