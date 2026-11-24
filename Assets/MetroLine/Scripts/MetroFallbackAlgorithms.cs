using System.Collections.Generic;
using System.Linq;

namespace MetroLine
{
    internal sealed class FallbackRouter : IMetroRouter
    {
        public List<int> FindShortestPath(int startId, MetroShape goal,
            IReadOnlyList<MetroStationState> stations,
            IReadOnlyDictionary<int, List<int>> graph)
        {
            if (stations.FirstOrDefault(s => s.Id == startId)?.Shape == goal)
                return new List<int> { startId };
            if (!graph.TryGetValue(startId, out List<int> next)) return new List<int>();
            int match = next.FirstOrDefault(id => stations.First(s => s.Id == id).Shape == goal);
            return match == 0 && startId != 0 ? new List<int>() : new List<int> { startId, match };
        }
    }

    internal sealed class FallbackPassengerQueue : IPassengerQueue
    {
        public List<MetroPassenger> Board(List<MetroPassenger> waiting,
            IReadOnlyCollection<int> reachable, IReadOnlyList<MetroStationState> stations, int capacity)
        {
            return waiting.Take(capacity).ToList();
        }
    }

    internal sealed class FallbackCongestion : ICongestionCalculator
    {
        public float Calculate(int count, int safe, float oldest) => safe <= 0 ? 1f : count / (float)safe;
    }

    internal sealed class FallbackScore : IMetroScoreCalculator
    {
        public int Calculate(int delivered, int transfers, float elapsed, float congestion) => delivered * 100;
    }

    internal sealed class FallbackLineAnalyzer : ILineAnalyzer
    {
        public float CalculateQuality(IReadOnlyList<int> line,
            IReadOnlyList<MetroStationState> stations,
            IReadOnlyDictionary<int, List<int>> graph) => line.Count < 2 ? 0f : 0.5f;
    }
}
