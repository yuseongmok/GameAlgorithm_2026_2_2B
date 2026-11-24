using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MetroLine.Student
{
    public sealed class LineAnalyzer : ILineAnalyzer
    {
        public float CalculateQuality(IReadOnlyList<int> line,
            IReadOnlyList<MetroStationState> stations,
            IReadOnlyDictionary<int, List<int>> graph)
        {
            // TODO 28: null과 역 2개 미만인 노선을 검사하세요.

            if (line == null || stations == null || graph == null || line.Count < 2)
            {
                return 0f;
            }

            // TODO 29: 노선에 포함된 서로 다른 역 모양 수를 구하세요.
            int shapeCount = stations.Where(station => line.Contains(station.Id))
                .Select(station => station.Shape)
                .Distinct()
                .Count();

            // TODO 30: 이웃이 하나 이상인 연결 역의 비율을 구하세요.
            // TODO 31: 노선 전체 역 수를 구하세요.
            float ShapeSocre = shapeCount / 3f;
            int connectedCount = 0;

            foreach (int stationId in line)
            {
                if (graph.TryGetValue(stationId, out List<int> neighbors) && neighbors.Count > 0)
                {
                    connectedCount++;
                }
            }
            float connectionScore = connectedCount / (float)line.Count;
            // TODO 32: 중복을 제거한 고유 역 수로 중복 비율을 구하세요.
            // TODO 33: 모양 다양성과 연결성을 각각 50%로 합산하세요.
            int uniqueCount = line.Distinct().Count();
            float duplicateRatio = 1f - uniqueCount / (float)line.Count;
            float duplicatePenalty = duplicateRatio * 0.5f;

            float qualtiy = ShapeSocre * 0.5f + connectionScore * 0.5f;
            // TODO 34: 중복 패널티를 빼고 0~1 범위로 반환하세요
            return Mathf.Clamp01(qualtiy - duplicatePenalty);
        }
    }
}
