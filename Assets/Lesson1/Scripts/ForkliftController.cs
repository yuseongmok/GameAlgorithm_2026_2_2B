using UnityEngine;
using UnityEngine.InputSystem;

public sealed class ForkliftController : MonoBehaviour
{
    // 제공 코드: 학생은 이동 코드를 작성하지 않습니다.
    public Transform holdPoint;
    public ForkliftWarehouseGame game;
    void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }
        bool moveForward = Keyboard.current.wKey.isPressed ||
                           Keyboard.current.upArrowKey.isPressed;
        bool moveBackward = Keyboard.current.sKey.isPressed ||
                            Keyboard.current.downArrowKey.isPressed;
        bool turnRight = Keyboard.current.dKey.isPressed ||
                         Keyboard.current.rightArrowKey.isPressed;
        bool turnLeft = Keyboard.current.aKey.isPressed ||
                        Keyboard.current.leftArrowKey.isPressed;

        float move = (moveForward ? 1f : 0f) - (moveBackward ? 1f : 0f);
        float turn = (turnRight ? 1f : 0f) - (turnLeft ? 1f : 0f);
        transform.Rotate(0,turn*110*Time.deltaTime,0);
        transform.position+=transform.forward*move*4*Time.deltaTime;
        transform.position=new Vector3(Mathf.Clamp(transform.position.x,-8,8),.05f,Mathf.Clamp(transform.position.z,-5,6));
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            game.Interact();
        }
    }
}
