using UnityEngine;

public sealed class ForkliftCamera : MonoBehaviour
{
    // 제공 코드: 지게차의 로컬 뒤쪽 위치를 부드럽게 따라갑니다.
    public Transform target;
    public Vector3 offset=new Vector3(0,15,-7);
    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.TransformPoint(offset);
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            Time.deltaTime * 6f);
        transform.LookAt(target.position + Vector3.up);
    }
}
