using System.Collections.Generic;
using AlgoLesson7;

namespace AlgoLesson7.Student
{
    /// <summary>
    /// 사거리, 거리, 체력, ID 순서로 공격 대상을 선택합니다.
    /// 시작 상태에서는 첫 번째 생존 적을 반환합니다.
    /// </summary>
    public sealed class TargetSelector : ITargetSelector
    {
        public int SelectTarget(
            UnitSnapshot self,
            IReadOnlyList<UnitSnapshot> enemies)
        {
            if (enemies == null || enemies.Count == 0)
                return -1;

            int bestId = -1;

            foreach (UnitSnapshot enemy in enemies)
            {
                // TODO 13: 죽은 적은 후보에서 제외하세요.

                // TODO 14: 거리 제곱과 사거리 안에 있는지 계산하세요.

                // TODO 15: 사거리 → 거리 → 체력 → ID 순서로 더 좋은 후보를 선택하세요.

                // 골격 상태에서도 전투가 멈추지 않도록 첫 생존 적을 반환합니다.
                if (bestId < 0 && enemy.IsAlive)
                    bestId = enemy.Id;
            }

            return bestId;
        }
    }
}
