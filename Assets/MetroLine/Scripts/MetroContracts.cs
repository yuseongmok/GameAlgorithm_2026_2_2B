using System;
using System.Collections.Generic;

namespace MetroLine
{
    [Serializable]
    public sealed class MetroStationState
    {
        public int Id;
        public string Name;
        public MetroShape Shape;
        public readonly List<MetroPassenger> Waiting = new();
    }

    [Serializable]
    public sealed class MetroPassenger
    {
        public int Id;
        public int OriginId;
        public MetroShape DestinationShape;
        public float WaitSeconds;
    }

    public enum MetroShape { Circle, Triangle, Square }

    public interface IMetroRouter
    {
        List<int> FindShortestPath(int startId, MetroShape goal,
            IReadOnlyList<MetroStationState> stations,
            IReadOnlyDictionary<int, List<int>> graph);
    }

    public interface IPassengerQueue
    {
        List<MetroPassenger> Board(List<MetroPassenger> waiting,
            IReadOnlyCollection<int> reachableStationIds,
            IReadOnlyList<MetroStationState> stations, int capacity);
    }

    public interface ICongestionCalculator
    {
        float Calculate(int waitingCount, int safeCapacity, float oldestWaitSeconds);
    }

    public interface IMetroScoreCalculator
    {
        int Calculate(int delivered, int transfers, float elapsedSeconds, float maxCongestion);
    }

    public interface ILineAnalyzer
    {
        float CalculateQuality(IReadOnlyList<int> line,
            IReadOnlyList<MetroStationState> stations,
            IReadOnlyDictionary<int, List<int>> graph);
    }
}
