using System.Collections;
using UnityEngine;

namespace AlgoCourse.Lesson4
{
    public sealed class FirefighterAgent : MonoBehaviour
    {
        [SerializeField] private float moveDuration = 0.18f;
        public GridPosition Position { get; private set; }
        public bool IsMoving { get; private set; }

        public void ResetAt(GridPosition position, Vector3 worldPosition)
        {
            StopAllCoroutines();
            IsMoving = false;
            Position = position;
            transform.position = worldPosition;
        }

        public void MoveTo(GridPosition position, Vector3 worldPosition, System.Action onComplete)
        {
            if (!IsMoving)
            {
                StartCoroutine(MoveRoutine(position, worldPosition, onComplete));
            }
        }

        private IEnumerator MoveRoutine(GridPosition position, Vector3 destination, System.Action onComplete)
        {
            IsMoving = true;
            Vector3 start = transform.position;
            float elapsed = 0f;
            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(start, destination, elapsed / moveDuration);
                yield return null;
            }
            transform.position = destination;
            Position = position;
            IsMoving = false;
            onComplete?.Invoke();
        }
    }
}
