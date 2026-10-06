using UnityEngine;

namespace AlgoCourse.Lesson4
{
    public sealed class RescueMissionUI : MonoBehaviour
    {
        private string status = "구조 대상을 클릭하세요.";
        private int score;
        private int rescued;
        private int pathLength;
        private bool autoMode;

        public void SetState(string message, int currentScore, int rescuedCount, int currentPathLength, bool isAuto)
        {
            status = message;
            score = currentScore;
            rescued = rescuedCount;
            pathLength = currentPathLength;
            autoMode = isAuto;
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(18, 18, 500, 142), "긴급 구조 작전 · BFS 최단 경로");
            GUI.Label(new Rect(36, 50, 460, 24), status);
            GUI.Label(new Rect(36, 76, 460, 24), $"점수 {score}   구조 {rescued}/3   남은 경로 {pathLength}");
            GUI.Label(new Rect(36, 102, 470, 24), "클릭: 대상 선택   B: BFS   Space: 한 칸 이동   A: 자동 이동");
            GUI.Label(new Rect(36, 126, 470, 24), $"F: 화재 확산   R: 리셋   자동 모드: {(autoMode ? "ON" : "OFF")}");
        }
    }
}
