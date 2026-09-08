using UnityEngine;

public sealed class ForkliftBox : MonoBehaviour
{
    // 제공 코드: 화면에 보이는 상자의 현재 소속 상태입니다.
    public int kind;
    [HideInInspector] public bool queued;
    [HideInInspector] public bool held;
    [HideInInspector] public int stackIndex=-1;
}
