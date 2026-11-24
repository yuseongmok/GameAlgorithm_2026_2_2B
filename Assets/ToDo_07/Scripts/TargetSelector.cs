using System.Collections.Generic;
using AlgoLesson7;
using UnityEngine;

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
            bool bestInRange = false;
            float bestDistance = float.PositiveInfinity;
            float bestHealth = float.PositiveInfinity;

            foreach (UnitSnapshot enemy in enemies)
            {
                // TODO 13: 죽은 적은 후보에서 제외하세요.
                if (!enemy.IsAlive)
                    continue;

                // TODO 14: 거리 제곱과 사거리 안에 있는지 계산하세요.
                float distance = (enemy.Position - self.Position).sqrMagnitude;
                bool inRange = distance <= self.AttackRange * self.AttackRange;

                // TODO 15: 사거리 → 거리 → 체력 → ID 순서로 더 좋은 후보를 선택하세요.
                bool better = false;
                
                if (inRange != bestInRange)
                {
                    better = inRange;
                }
                else if (!Mathf.Approximately(distance, bestDistance))
                {
                    better = distance < bestDistance;
                }
                else if (!Mathf.Approximately(enemy.CurrentHealth, bestHealth))
                {
                    better = enemy.CurrentHealth < bestHealth;
                }
                else
                {
                    better = bestId < 0 || enemy.Id < bestId;
                }

                bestId = enemy.Id;
                bestInRange = inRange;
                bestDistance = distance;
                bestHealth = enemy.CurrentHealth;


            }

            return bestId;
        }
    }
}
