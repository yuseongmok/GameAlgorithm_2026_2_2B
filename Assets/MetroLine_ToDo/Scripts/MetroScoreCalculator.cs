using UnityEngine;

namespace MetroLine.Student
{
    public sealed class MetroScoreCalculator : IMetroScoreCalculator
    {
        public int Calculate(int delivered, int transfers, float elapsedSeconds, float maxCongestion)
        {
            // TODO 23: 운송 승객 한 명마다 100점을 계산하세요.
            int deliveryScore = Mathf.Max(0, delivered) * 100;

            // TODO 24: 환승 한 번마다 15점을 계산하세요.
            int transferScore = Mathf.Max(0, transfers) * 15;

            // TODO 25: 생존 10초마다 20점을 계산하세요.
            int survivalScore = Mathf.FloorToInt(Mathf.Max(0f, elapsedSeconds) / 10f * 20);


            // TODO 26: 최대 혼잡도에 따라 최대 300점을 감점하세요.
            int congestionPenalty = Mathf.RoundToInt(Mathf.Clamp01(maxCongestion) * 300);


            // TODO 27: 최종 점수가 음수가 되지 않게 반환하세요.
            return Mathf.Max(0, deliveryScore + transferScore + survivalScore - congestionPenalty;
        }
    }
}
