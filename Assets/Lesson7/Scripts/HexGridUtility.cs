using System.Collections.Generic;
using UnityEngine;

namespace AlgoLesson7
{
    /// <summary>
    /// 육각 좌표 변환과 인접 칸 조회는 수업 기반 코드에서 제공합니다.
    /// 학생은 이 기능을 이용해 Queue 기반 BFS를 작성합니다.
    /// </summary>
    public static class HexGridUtility
    {
        public static int GetDistance(
            AutoChessBoardCell left,
            AutoChessBoardCell right)
        {
            if (left == null || right == null)
                return int.MaxValue;

            Vector3Int leftCube = OffsetToCube(left.Row, left.Column);
            Vector3Int rightCube = OffsetToCube(right.Row, right.Column);

            return Mathf.Max(
                Mathf.Abs(leftCube.x - rightCube.x),
                Mathf.Abs(leftCube.y - rightCube.y),
                Mathf.Abs(leftCube.z - rightCube.z));
        }

        public static IEnumerable<AutoChessBoardCell> GetOpenNeighbors(
            AutoChessBoardCell current,
            IReadOnlyList<AutoChessBoardCell> cells)
        {
            foreach (AutoChessBoardCell cell in cells)
            {
                if (cell.Occupant == null && GetDistance(current, cell) == 1)
                    yield return cell;
            }
        }

        private static Vector3Int OffsetToCube(int row, int column)
        {
            int x = column - (row - (row & 1)) / 2;
            int z = row;
            int y = -x - z;
            return new Vector3Int(x, y, z);
        }
    }
}
