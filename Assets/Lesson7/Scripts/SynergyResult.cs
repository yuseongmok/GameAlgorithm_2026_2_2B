using System.Collections.Generic;

namespace AlgoLesson7
{
    public sealed class SynergyResult
    {
        public readonly Dictionary<UnitRace, int> RaceCounts = new Dictionary<UnitRace, int>();
        public readonly Dictionary<UnitJob, int> JobCounts = new Dictionary<UnitJob, int>();
        public readonly List<string> ActiveNames = new List<string>();

        public float WarriorArmor { get; set; }
        public float RangerAttackSpeedMultiplier { get; set; } = 1f;
        public float MageAttackMultiplier { get; set; } = 1f;
        public float HumanHealthMultiplier { get; set; } = 1f;
        public float BeastAttackMultiplier { get; set; } = 1f;
        public float MachineShield { get; set; }

        public static SynergyResult Empty => new SynergyResult();
    }
}
