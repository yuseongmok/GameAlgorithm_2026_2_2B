using System.Collections.Generic;

namespace AlgoCourse.Lesson4
{
    public interface IRescuePathfinder
    {
        PathSearchResult FindPath(
            int width,
            int height,
            GridPosition start,
            GridPosition goal,
            IReadOnlyCollection<GridPosition> blockedCells);
    }
}
