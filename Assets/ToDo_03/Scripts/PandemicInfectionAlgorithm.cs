using System.Collections.Generic;
using AlgoCourse.Lesson3;

namespace AlgoCourse.StudentWork
{
    public sealed class PandemicInfectionAlgorithm : ICityInfectionAlgorithm
    {

        private const int MaximumInfectionLevel = 3;                    //한 도시가 가질 수 있는 최대 감염 단게를 3으로 지정

        private readonly Dictionary<int, int[]> graph = new Dictionary<int, int[]>();   //도시 번호를 key로 하고 연결된 아웃 도시 목록을 Value로 저장. _
        private readonly Dictionary<int , int> infectionLevels = new Dictionary<int , int>(); //각 도시 번호별 현재 감염 단계를 저장합니다.
        private readonly Queue<int> infectionQueue = new Queue<int>();   //감염 처리를 기다리고 있는 도시들을 순서대로 저장하는 큐.
        private readonly HashSet<int> outbreakCities = new HashSet<int>(); //현재 연쇄 감염 과정에서 이미 아웃브레이크가 발생한 도시를 저장.

        public int PendingCount => infectionQueue.Count;        //감염 큐에 대기중인 도시의 개수를 반환
        public int OutbreakCount { get; private set; }          //지금까지 발생한 전체 아웃브레이크 횟수를 저장.

        public void Initialize(IReadOnlyDictionary<int, int[]> cityGraph)
        {
            graph.Clear();           //도시 연결 정보를 모두 삭제
            infectionLevels.Clear(); //감염 단계 정보를 모두 삭제
            infectionQueue.Clear();  //감염 대기 큐도 초기화
            outbreakCities.Clear();  //아웃브레이크 도시 기록을 제거

            OutbreakCount = 0;

            //전달 받은 모든 도시 정보를 하나씩 확인합니다.
            foreach(KeyValuePair<int, int[]> city in cityGraph)
            {
                graph.Add(city.Key, city.Value );   //독 번호와 해당 도시의 이웃 도시 목록을 그래프에 저장
                infectionLevels.Add(city.Key, 0);   //해당 도시의 초기 감염 단계를 0으로 설정
            }
        }

        public bool QueueInfection(int cityId)          //지정한 도시를 감염 처리 대기 큐에 추가
        {
            if (!graph.ContainsKey(cityId))         //그래프에 존재하지 않는 도시 번호 인지 확인
            {
                return false;                       //존재하지 않는 도시라면 감염 요청에 실패 했음을 반환.
            }

            if(infectionQueue.Count == 0)           //현재 감염 큐가 비어있다면 새로운 연쇄 감염이 시작되는 상태    
            {
                outbreakCities.Clear();             //새로운 연쇄 감염을 위해 이전 아웃브레이크 도시 기록을 초기화
            }
            infectionQueue.Enqueue(cityId);         //지정한 도시 감염 처리 대기 큐의 마지막에 추가


            return true;
        }

        public CityInfectionStep ProcessNext()
        {
            if (infectionQueue.Count == 0)      //처리할 도시가 감염 큐에 남아있는지 확인 
            {
                return new CityInfectionStep(-1, 0, false, false);              //처리할 도시가 없다면 아무 변화가 없는 결과를 반환 
            }

            int cityId = infectionQueue.Dequeue();
            int currentLevel = infectionLevels[cityId];

            if (currentLevel < MaximumInfectionLevel)
            {
                int nextLevel = currentLevel + 1;
                infectionLevels[cityId] = nextLevel;
                return new CityInfectionStep(cityId, nextLevel, false, true);           //도시가 존재하면 감염단계를 반환하고 존재하지않으면 0을 반환 
            }

            if (!outbreakCities.Add(cityId))                                        //이미 이번 연쇄 감염에서 아웃브레이크가 발생했던 도시인지확인
            {
                return new CityInfectionStep(cityId, currentLevel, false, false);   //발생한 도시라면 추가 확산 없음
            }

            OutbreakCount++;                                                        //새로운 아웃브레이크가 발생 했으므로 전체 발생 횟수를 1증가

            foreach (int neighborld in graph[cityId])                              //도시의 연결된 모든 이웃 도시를 확인
            {
                infectionQueue.Enqueue(neighborld);                               //각 이웃 도시를 감염 처리 대기 큐에 추가
            }
            return new CityInfectionStep(cityId, currentLevel, true, true);        //현재 도시에서 아웃브레이크가 발생하고 상태가 변경 되었다는 결과 반환
        }

        public int GetInfectionLevel(int cityId)
        {
            return infectionLevels.TryGetValue(cityId, out int level) ? level : 0;          //도시가 존재하면 감염단계를 반환하고 존재하지 않으면 0을 반환
        }
    }
}
