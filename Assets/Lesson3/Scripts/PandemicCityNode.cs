using UnityEngine;

namespace AlgoCourse.Lesson3
{
    public sealed class PandemicCityNode : MonoBehaviour
    {
        [SerializeField] private int cityId;
        [SerializeField] private string cityName;
        [SerializeField] private int[] neighborIds;
        [SerializeField] private GameObject[] infectionCubes;
        [SerializeField] private Renderer cityRenderer;

        private MaterialPropertyBlock propertyBlock;
        private Vector3 originalScale;

        public int CityId => cityId;
        public string CityName => cityName;
        public int[] NeighborIds => neighborIds;

        public void Configure(int id, string displayName, int[] neighbors, GameObject[] cubes, Renderer renderer)
        {
            cityId = id;
            cityName = displayName;
            neighborIds = neighbors;
            infectionCubes = cubes;
            cityRenderer = renderer;
            originalScale = transform.localScale;
            SetInfectionLevel(0);
        }

        private void Awake()
        {
            originalScale = transform.localScale;
        }

        public void SetInfectionLevel(int level)
        {
            for (int index = 0; index < infectionCubes.Length; index++)
            {
                if (infectionCubes[index] != null)
                {
                    infectionCubes[index].SetActive(index < level);
                }
            }

            SetNodeColor(level == 0
                ? new Color(0.10f, 0.52f, 0.72f)
                : new Color(0.72f, 0.12f, 0.10f));
        }

        public void SetQueued(bool queued)
        {
            if (queued)
            {
                SetNodeColor(new Color(1f, 0.62f, 0.08f));
            }
        }

        public void ShowOutbreak()
        {
            transform.localScale = originalScale * 1.35f;
            CancelInvoke(nameof(RestoreScale));
            Invoke(nameof(RestoreScale), 0.28f);
        }

        private void RestoreScale() => transform.localScale = originalScale;

        private void SetNodeColor(Color color)
        {
            if (cityRenderer == null)
            {
                cityRenderer = GetComponent<Renderer>();
            }

            if (cityRenderer == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            propertyBlock.SetColor("_BaseColor", color);
            propertyBlock.SetColor("_Color", color);
            cityRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
