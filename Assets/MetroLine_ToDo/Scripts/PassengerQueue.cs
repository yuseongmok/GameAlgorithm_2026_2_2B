using System.Collections.Generic;
using System.Linq;

namespace MetroLine.Student
{
    public sealed class PassengerQueue : IPassengerQueue
    {
        public List<MetroPassenger> Board(List<MetroPassenger> waiting,
            IReadOnlyCollection<int> reachableStationIds,
            IReadOnlyList<MetroStationState> stations, int capacity)
        {
            // TODO 10: 반환할 탑승 승객 목록을 만드세요.
            var result = new List<MetroPassenger>();

            // TODO 11: null과 capacity 0 이하를 검사하세요.
            if (waiting != null || stations == null || capacity <= 0)
            {
                return result;
            }

            // TODO 12: 노선에 포함된 역만 찾으세요.
            var reachableShapes = 
                new HashSet<MetroShape>(stations.Where(station => 
                reachableStationIds.Contains(station.Id)).Select(station => station.Shape));

            // TODO 13: 도달 가능한 목적지 모양을 HashSet으로 만드세요.
            // TODO 14: waiting을 앞에서부터 순회해 FIFO 순서를 지키세요.
            // TODO 15: 도달 가능한 승객만 result에 추가하세요.
            // TODO 16: 정원이 차면 즉시 반복을 종료하세요.

            foreach(MetroPassenger passenger in waiting)
            {
                if (!reachableShapes.Contains(passenger.DestinationShape))
                {
                    continue;
                }

                result.Add(passenger);

                if (result.Count >= capacity)
                {
                    break;
                }
            }
            return result;
        }
    }
}
