using UnityEngine;
using UnityEngine.InputSystem;

namespace AlgoCourse.Lesson2
{
    /// <summary>
    /// 수업 주제가 아닌 이동과 스캔 입력을 미리 제공합니다.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class ParcelScannerPlayer : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5.5f;
        [SerializeField] private float turnSpeed = 14f;
        [SerializeField] private ParcelSortingGame game;

        private CharacterController controller;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            Vector2 input = ReadMoveInput(keyboard);
            Vector3 direction = new Vector3(input.x, 0f, input.y);

            if (direction.sqrMagnitude > 1f)
            {
                direction.Normalize();
            }

            controller.Move(direction * (moveSpeed * Time.deltaTime));

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    turnSpeed * Time.deltaTime);
            }

            if (keyboard.eKey.wasPressedThisFrame)
            {
                game.TryScanNearestParcel(transform.position);
            }
        }

        public void SetGame(ParcelSortingGame sortingGame)
        {
            game = sortingGame;
        }

        private static Vector2 ReadMoveInput(Keyboard keyboard)
        {
            float horizontal = 0f;
            float vertical = 0f;

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                horizontal -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                horizontal += 1f;
            }

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                vertical -= 1f;
            }

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                vertical += 1f;
            }

            return new Vector2(horizontal, vertical);
        }
    }
}
