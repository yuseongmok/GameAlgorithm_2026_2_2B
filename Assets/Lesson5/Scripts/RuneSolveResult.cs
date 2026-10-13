using System.Collections.Generic;

namespace AlgoCourse.Lesson5
{
    public sealed class RuneSolveResult
    {
        public IReadOnlyList<RuneSearchStep> Steps { get; }
        public IReadOnlyList<int> SolutionRuneIds { get; }
        public bool Solved => SolutionRuneIds.Count > 0;

        public RuneSolveResult(List<RuneSearchStep> steps, List<int> solutionRuneIds)
        {
            Steps = steps ?? new List<RuneSearchStep>();
            SolutionRuneIds = solutionRuneIds ?? new List<int>();
        }
    }
}
