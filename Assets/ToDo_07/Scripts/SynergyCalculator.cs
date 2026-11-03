using System.Collections.Generic;
using AlgoLesson7;

namespace AlgoLesson7.Student
{
    /// <summary>
    /// 배치된 유닛의 종족과 직업을 집계해 활성 시너지를 계산합니다.
    /// 시작 상태에서는 빈 결과를 반환하므로 게임은 실행되지만 보너스는 없습니다.
    /// </summary>
    public sealed class SynergyCalculator : ISynergyCalculator
    {
        public SynergyResult Calculate(IReadOnlyList<UnitSnapshot> allies)
        {
            SynergyResult result = new SynergyResult();

            if (allies == null || allies.Count == 0)
                return result;

            Dictionary<UnitRace, int> raceCounts = result.RaceCounts;
            Dictionary<UnitJob, int> jobCounts = result.JobCounts;
            HashSet<string> countedNames = new HashSet<string>();

            foreach (UnitSnapshot unit in allies)
            {
                // TODO 01: 죽은 유닛은 시너지 계산에서 제외하세요.
                if (!unit.IsAlive)
                    continue;

                // TODO 02: 같은 이름의 유닛은 한 번만 집계하세요.
                if (!countedNames.Add(unit.UnitName))
                    continue;


                // TODO 03: raceCounts에 종족별 유닛 수를 기록하세요.
                raceCounts[unit.Race] = GetCount(raceCounts, unit.Race) + 1;


                // TODO 04: jobCounts에 직업별 유닛 수를 기록하세요.
                jobCounts[unit.Job] = GetCount(jobCounts, unit.Job) + 1;

            }

            int warriorCount = GetCount(jobCounts, UnitJob.Warrior);
            int rangerCount = GetCount(jobCounts, UnitJob.Ranger);
            int mageCount = GetCount(jobCounts, UnitJob.Mage);
            int humanCount = GetCount(raceCounts, UnitRace.Human);
            int beastCount = GetCount(raceCounts, UnitRace.Beast);
            int machineCount = GetCount(raceCounts, UnitRace.Machine);

            // TODO 05: Warrior 4명은 방어력 +5를 적용하세요.
            // TODO 06: Warrior 2명은 방어력 +2를 적용하세요.

            if(warriorCount >= 4)
            {
                result.WarriorArmor = 5f;
                result.ActiveNames.Add("Warrior 4");
            }
            else if (warriorCount > 2)
            {
                result.WarriorArmor = 2f;
                result.ActiveNames.Add("Warrior 2");
                //강한 Warriro 단계만 적용
            }

            // TODO 07: Ranger 2명은 공격속도 배율을 1.2로 설정하세요.
            if (rangerCount >= 2)
            {
                result.RangerAttackSpeedMultiplier = 1.2f;
                result.ActiveNames.Add("Ranger 2");
            }


            // TODO 08: Mage 2명은 공격력 배율을 1.25로 설정하세요.

            if (mageCount >= 2)
            {
                result.MageAttackMultiplier = 1.25f;
                result.ActiveNames.Add("Mage 2");
            }

            // TODO 09: Human 3명은 체력 배율을 1.2로 설정하세요.
            if (humanCount >= 3)
            {
                result.HumanHealthMultiplier = 1.2f;
                result.ActiveNames.Add("Human 3");
            }


            // TODO 10: Beast 3명은 공격력 배율을 1.2로 설정하세요.
            if (beastCount >= 3)
            {
                result.BeastAttackMultiplier = 1.2f;
                result.ActiveNames.Add("Beats 3");
            }


            // TODO 11: Machine 2명은 보호막 15를 적용하세요.
            if(machineCount >= 2)
            {
                result.MachineShield = 15f;
                result.ActiveNames.Add("Machine 2");
            }



            return result;
        }

        private static int GetCount<TKey>(
            Dictionary<TKey, int> counts,
            TKey key)
        {
            // TODO 12: key가 없을 때 예외 대신 0을 반환하세요.
            return counts.TryGetValue(key, out int count) ? count : 0;  
        }
    }
}
