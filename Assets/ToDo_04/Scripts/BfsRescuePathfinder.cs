using System.Collections.Generic;
using AlgoCourse.Lesson4;

namespace AlgoCourse.StudentWork
{
    public sealed class BfsRescuePathfinder : IRescuePathfinder
    {
        private static readonly GridPosition[] Directions =
        {
        new GridPosition(1,0),
        new GridPosition(-1,0),
        new GridPosition(0,1),
        new GridPosition(0,-1)
        };


        public PathSearchResult FindPath(
            int width,
            int height,
            GridPosition start,
            GridPosition goal,
            IReadOnlyCollection<GridPosition> blockedCells)
        {
            // TODO 01: Queue, visited, cameFrom을 생성합니다.
            Queue<GridPosition> frontier = new Queue<GridPosition>();

            // TODO 02: 시작 칸을 Queue와 visited에 넣습니다.
            HashSet<GridPosition> visited = new HashSet<GridPosition>();

            // TODO 03: Queue가 빌 때까지 상하좌우 이웃을 탐색합니다.
            Dictionary<GridPosition, GridPosition> camForm = new Dictionary<GridPosition, GridPosition>();

            List<GridPosition> visitedOrder = new List<GridPosition>();
            HashSet<GridPosition> blocked = new HashSet<GridPosition>(blockedCells);

            frontier.Enqueue(start);
            visited.Add(start);

            while(frontier.Count > 0)
            {
                GridPosition current = frontier.Dequeue();
                visitedOrder.Add(current);

                if(current == goal)
                {
                    break;
                }

                foreach(GridPosition direction in Directions)
                {
                    GridPosition next = new GridPosition(current.X + direction.X, current.Y + direction.Y);

                    if(!IsInside(next, width, height))
                    {
                        continue;
                    }

                    if(blocked.Contains(next) || visited.Contains(next))
                    {
                        continue;
                    }

                    visited.Add(next);
                    camForm[next] = current;
                    frontier.Enqueue(next);

                }

                //목표를 방문하짐 못했다면 빈 경로를 반환합니다.
                if (!visited.Contains(goal))
                {
                    return new PathSearchResult(visitedOrder, new List<GridPosition>());
                }

                List<GridPosition>path = new List<GridPosition>();
                GridPosition pathCell = goal;
                path.Add(pathCell);

                while (pathCell != start)
                {
                    pathCell = camForm[pathCell];
                    path.Add(pathCell);
                }

                path.Reverse();

                return new PathSearchResult(visitedOrder, path);
            }

            // TODO 04: 범위, 장애물, 방문 여부를 검사합니다.
            // TODO 05: cameFrom을 따라 최단 경로를 복원합니다.
            return new PathSearchResult(
                new List<GridPosition>(),
                new List<GridPosition>());
        }

        private static bool IsInside(GridPosition position, int width, int height)
        {
            return position.X >= 0 && position.X < width &&
                position.Y >= 0 && position.Y < height;
        }
    }
}
