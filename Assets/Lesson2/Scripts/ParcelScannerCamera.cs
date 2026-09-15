using UnityEngine;

namespace AlgoCourse.Lesson2
{
    /// <summary>
    /// 분류장 전체와 플레이어를 함께 보여 주는 추적 카메라입니다.
    /// </summary>
    public sealed class ParcelScannerCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 11f, -9f);
        [SerializeField] private float smoothSpeed = 6f;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                smoothSpeed * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up * 0.8f);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
