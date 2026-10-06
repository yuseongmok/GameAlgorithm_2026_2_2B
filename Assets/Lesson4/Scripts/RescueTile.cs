using UnityEngine;

namespace AlgoCourse.Lesson4
{
    public sealed class RescueTile : MonoBehaviour
    {
        [SerializeField] private int x;
        [SerializeField] private int y;
        [SerializeField] private Renderer tileRenderer;

        private static readonly Color FloorColor = new Color(0.18f, 0.23f, 0.27f);
        private static readonly Color WallColor = new Color(0.07f, 0.09f, 0.11f);
        private static readonly Color FireColor = new Color(0.95f, 0.16f, 0.03f);
        private static readonly Color VisitedColor = new Color(0.12f, 0.48f, 0.82f);
        private static readonly Color PathColor = new Color(0.12f, 0.86f, 0.38f);

        public GridPosition Position => new GridPosition(x, y);
        public bool IsWall { get; private set; }
        public bool IsOnFire { get; private set; }
        public bool IsBlocked => IsWall || IsOnFire;

        [SerializeField] private bool startsAsWall;
        [SerializeField] private bool startsOnFire;

        public void Configure(int gridX, int gridY, Renderer renderer)
        {
            x = gridX;
            y = gridY;
            tileRenderer = renderer;
            SetFloor();
        }

        public void SetFloor()
        {
            IsWall = false;
            IsOnFire = false;
            transform.localScale = new Vector3(0.92f, 0.12f, 0.92f);
            SetColor(FloorColor);
        }

        public void SetWall()
        {
            IsWall = true;
            IsOnFire = false;
            transform.localScale = new Vector3(0.92f, 1.4f, 0.92f);
            SetColor(WallColor);
        }

        public void SetFire()
        {
            if (IsWall) return;
            IsOnFire = true;
            transform.localScale = new Vector3(0.92f, 0.5f, 0.92f);
            SetColor(FireColor);
        }

        public void CaptureInitialState()
        {
            startsAsWall = IsWall;
            startsOnFire = IsOnFire;
        }

        public void RestoreInitialState()
        {
            if (startsAsWall)
            {
                SetWall();
            }
            else if (startsOnFire)
            {
                SetFloor();
                SetFire();
            }
            else
            {
                SetFloor();
            }
        }

        public void ShowVisited()
        {
            if (!IsBlocked) SetColor(VisitedColor);
        }

        public void ShowPath()
        {
            if (!IsBlocked) SetColor(PathColor);
        }

        public void ClearSearchColor()
        {
            if (!IsBlocked) SetColor(FloorColor);
        }

        private void SetColor(Color color)
        {
            if (tileRenderer != null)
            {
                tileRenderer.material.color = color;
            }
        }
    }
}
