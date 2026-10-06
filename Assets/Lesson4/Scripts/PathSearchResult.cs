using System.Collections.Generic;

namespace AlgoCourse.Lesson4
{
    public sealed class PathSearchResult
    {
        public IReadOnlyList<GridPosition> VisitedOrder { get; }
        public IReadOnlyList<GridPosition> Path { get; }
        public bool Found => Path.Count > 0;

        public PathSearchResult(List<GridPosition> visitedOrder, List<GridPosition> path)
        {
            VisitedOrder = visitedOrder ?? new List<GridPosition>();
            Path = path ?? new List<GridPosition>();
        }
    }
}
