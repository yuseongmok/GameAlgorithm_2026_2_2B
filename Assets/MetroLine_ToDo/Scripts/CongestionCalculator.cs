using UnityEngine;

namespace MetroLine.Student
{
    public sealed class CongestionCalculator : ICongestionCalculator
    {
        public float Calculate(int waitingCount, int safeCapacity, float oldestWaitSeconds)
        {
            // TODO 17: waitingCount가 음수가 되지 않게 보정하세요.
            waitingCount = Mathf.Max(0, waitingCount);

            // TODO 18: safeCapacity가 최소 1이 되게 보정하세요.
            safeCapacity = Mathf.Max(1, safeCapacity);

            oldestWaitSeconds = Mathf.Max(0, oldestWaitSeconds);

            // TODO 19: 대기 인원 비율을 계산하세요.
            float countRatio = waitingCount / (float)safeCapacity;

            // TODO 20: 가장 오래 기다린 시간의 비율을 계산하세요. 기준은 30초입니다.
            float waitRatio = oldestWaitSeconds / 30f;

            // TODO 21: 두 비율 중 더 위험한 값을 선택하세요.
            float worstRatio = Mathf.Max(countRatio, waitRatio);

            // TODO 22: 결과를 0~1 범위로 제한해 반환하세요.
            return Mathf.Clamp01(worstRatio);
        }
    }
}
