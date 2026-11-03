using System.Collections.Generic;

namespace AlgoLesson7
{
    public interface ISynergyCalculator
    {
        SynergyResult Calculate(IReadOnlyList<UnitSnapshot> allies);
    }

    public interface ITargetSelector
    {
        int SelectTarget(UnitSnapshot self, IReadOnlyList<UnitSnapshot> enemies);
    }

    public interface IHexPathfinder
    {
        AutoChessBoardCell FindNextStep(
            AutoChessBoardCell start,
            AutoChessBoardCell target,
            IReadOnlyList<AutoChessBoardCell> cells);
    }
}
