using UnityEngine;

namespace AlgoCourse.Lesson5
{
    public sealed class RuneVaultUI : MonoBehaviour
    {
        private string status = "Space로 첫 탐색 단계를 실행하세요.";
        private int step;
        private int totalSteps;
        private int currentSum;
        private bool autoMode;

        public void SetState(string message, int currentStep, int stepCount, int sum, bool auto)
        {
            status = message;
            step = currentStep;
            totalSteps = stepCount;
            currentSum = sum;
            autoMode = auto;
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(18, 18, 560, 156), "마법 금고 침투 · Backtracking");
            GUI.Label(new Rect(36, 50, 520, 24), status);
            GUI.Label(new Rect(36, 76, 520, 24), $"단계 {step}/{totalSteps}   현재 합계 {currentSum}/14");
            GUI.Label(new Rect(36, 102, 520, 24), "조건: 4개 · 중복 금지 · 불/물 인접 금지 · 마지막은 빛");
            GUI.Label(new Rect(36, 128, 520, 24), $"Space 한 단계   A 자동   R 리셋   자동 모드 {(autoMode ? "ON" : "OFF")}");
        }
    }
}
