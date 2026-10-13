using System.Collections.Generic;

namespace AlgoCourse.Lesson5
{
    public sealed class RunePuzzle
    {
        public IReadOnlyList<RuneData> Runes { get; }
        public int SlotCount { get; }
        public int TargetSum { get; }
        public bool RequireLightLast { get; }
        public bool ForbidFireWaterAdjacency { get; }

        public RunePuzzle(IReadOnlyList<RuneData> runes, int slotCount, int targetSum,
            bool requireLightLast, bool forbidFireWaterAdjacency)
        {
            Runes = runes;
            SlotCount = slotCount;
            TargetSum = targetSum;
            RequireLightLast = requireLightLast;
            ForbidFireWaterAdjacency = forbidFireWaterAdjacency;
        }
    }
}
