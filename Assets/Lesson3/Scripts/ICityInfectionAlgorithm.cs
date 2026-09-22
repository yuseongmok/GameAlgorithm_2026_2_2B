using System.Collections.Generic;

namespace AlgoCourse.Lesson3
{
    public interface ICityInfectionAlgorithm
    {
        int PendingCount { get; }
        int OutbreakCount { get; }
        void Initialize(IReadOnlyDictionary<int, int[]> cityGraph);
        bool QueueInfection(int cityId);
        CityInfectionStep ProcessNext();
        int GetInfectionLevel(int cityId);
    }
}
