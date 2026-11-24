using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MetroLine
{
    public sealed class MetroLineGame : MonoBehaviour
    {
        private sealed class Train
        {
            public GameObject View;
            public int Segment;
            public float T;
            public int Direction = 1;
            public readonly List<MetroPassenger> Passengers = new();
        }

        [Header("Simulation")]
        [SerializeField] private float passengerSpawnInterval = 2.2f;
        [SerializeField] private float trainSpeed = 0.34f;
        [SerializeField] private int trainCapacity = 6;
        [SerializeField] private int stationSafeCapacity = 7;
        [SerializeField] private int initialStationCount = 8;
        [SerializeField] private float newStationInterval = 12f;

        private readonly List<MetroStationView> views = new();
        private readonly List<MetroStationState> stations = new();
        private readonly List<int> line = new();
        private readonly Dictionary<int, List<int>> graph = new();
        private readonly List<GameObject> rails = new();
        private readonly List<Train> trains = new();
        private GameObject selectionRing;
        private GameObject selectionMarker;
        private Vector3 selectionMarkerBasePosition;
        private IMetroRouter router;
        private IPassengerQueue passengerQueue;
        private ICongestionCalculator congestion;
        private IMetroScoreCalculator scoreCalculator;
        private ILineAnalyzer lineAnalyzer;
        private MetroStationCanvasUI stationCanvasUI;
        private float spawnTimer;
        private float stationUnlockTimer;
        private int activeStationCount;
        private float elapsed;
        private float maxCongestion;
        private int passengerId;
        private int delivered;
        private int transfers;
        private bool gameOver;
        private bool hasRouteSelection;
        private bool trainPlacementMode;
        private int trainStock = 2;
        private int nextTrainReward = 20;
        private int simulationSpeed = 1;
        private string message = "역을 클릭해 노선을 만드세요";

        private void Awake()
        {
            EnsureAlgorithms();

            views.AddRange(FindObjectsByType<MetroStationView>(FindObjectsInactive.Include,
                FindObjectsSortMode.None).OrderBy(v => v.StationId));
            foreach (MetroStationView view in views)
            {
                MetroShapeMeshUtility.Apply(view);
                stations.Add(new MetroStationState { Id = view.StationId, Name = view.StationName, Shape = view.Shape });
                graph[view.StationId] = new List<int>();
            }

            activeStationCount = Mathf.Clamp(initialStationCount, 2, views.Count);
            for (int i = 0; i < views.Count; i++)
                views[i].gameObject.SetActive(i < activeStationCount);

            foreach (MetroStationLabel worldLabel in FindObjectsByType<MetroStationLabel>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
                worldLabel.gameObject.SetActive(false);

            stationCanvasUI = gameObject.AddComponent<MetroStationCanvasUI>();
            stationCanvasUI.Build(views);
            stationCanvasUI.AddTrainRequested += BeginTrainPlacement;
        }

        private void EnsureAlgorithms()
        {
            router = MetroAlgorithmFactory.Create<IMetroRouter>(new FallbackRouter());
            passengerQueue = MetroAlgorithmFactory.Create<IPassengerQueue>(new FallbackPassengerQueue());
            congestion = MetroAlgorithmFactory.Create<ICongestionCalculator>(new FallbackCongestion());
            scoreCalculator = MetroAlgorithmFactory.Create<IMetroScoreCalculator>(new FallbackScore());
            lineAnalyzer = MetroAlgorithmFactory.Create<ILineAnalyzer>(new FallbackLineAnalyzer());
        }

        private void Update()
        {
            HandleSpeedInput();
            if (Keyboard.current.rKey.wasPressedThisFrame) ResetSimulation();
            HandleCancelInput();
            AnimateSelection();
            UpdateStationLabels();
            if (gameOver) return;
            elapsed += Time.deltaTime * simulationSpeed;
            UnlockStationsOverTime();
            HandleStationClick();
            SpawnPassengers();
            MoveTrain();
            UpdateCongestion();
        }

        private void HandleCancelInput()
        {
            bool keyboardCancel = Keyboard.current != null
                && Keyboard.current.escapeKey.wasPressedThisFrame;
            bool mouseCancel = Mouse.current != null
                && Mouse.current.rightButton.wasPressedThisFrame;
            if (!keyboardCancel && !mouseCancel) return;

            if (trainPlacementMode)
            {
                trainPlacementMode = false;
                message = "지하철 추가를 취소했습니다";
                return;
            }

            if (line.Count == 0)
            {
                message = "취소할 역 선택이 없습니다";
                return;
            }

            HideSelection();
            message = "연장 선택 해제 — 노선과 열차는 그대로 유지됩니다";
        }

        private void RemoveTrainsSafely()
        {
            foreach (Train train in trains)
            {
                if (train.Passengers.Count > 0 && line.Count > 0)
                {
                    MetroStationState returnStation = stations.First(s => s.Id == line[train.Segment]);
                    returnStation.Waiting.AddRange(train.Passengers);
                }
                if (train.View != null) Destroy(train.View);
            }
            trains.Clear();
        }

        private void HideSelection()
        {
            if (selectionRing != null) selectionRing.SetActive(false);
            if (selectionMarker != null) selectionMarker.SetActive(false);
            hasRouteSelection = false;
        }

        private void UpdateStationLabels()
        {
            if (scoreCalculator == null || lineAnalyzer == null)
                EnsureAlgorithms();
            stationCanvasUI?.Refresh(stations, activeStationCount);
            if (stationCanvasUI == null) return;

            int trainCircles = trains.Sum(t => t.Passengers.Count(p => p.DestinationShape == MetroShape.Circle));
            int trainTriangles = trains.Sum(t => t.Passengers.Count(p => p.DestinationShape == MetroShape.Triangle));
            int trainSquares = trains.Sum(t => t.Passengers.Count(p => p.DestinationShape == MetroShape.Square));
            int onboard = trains.Sum(t => t.Passengers.Count);
            int score = scoreCalculator.Calculate(delivered, transfers, elapsed, maxCongestion);
            float quality = lineAnalyzer.CalculateQuality(line, stations, graph);
            stationCanvasUI.SetStatus(
                $"METRO LINE\n{message}\n" +
                $"운송 {delivered}명  |  운행 {trains.Count}대  |  탑승 {onboard}/{Mathf.Max(1, trains.Count) * trainCapacity}\n" +
                $"탑승 목적지  ○{trainCircles}  △{trainTriangles}  □{trainSquares}\n" +
                $"점수 {score:N0}  |  최고 혼잡 {maxCongestion:P0}\n" +
                $"노선 품질 {quality:P0}  |  활성 역 {activeStationCount}/{views.Count}  |  연결 {line.Count}\n" +
                $"배속 ×{simulationSpeed}  [1] 보통  [2] 빠르게  [3] 매우 빠르게\n" +
                "우클릭/Esc 취소  |  R 재시작");
            stationCanvasUI.SetTrainStock(trainStock, trainPlacementMode);
        }

        private void BeginTrainPlacement()
        {
            if (gameOver) return;
            if (line.Count < 2)
            {
                message = "먼저 역 두 개 이상을 연결하세요";
                return;
            }
            if (trainStock <= 0)
            {
                message = $"보유 지하철이 없습니다 — 승객 {nextTrainReward}명 운송 시 1대 지급";
                return;
            }
            trainPlacementMode = true;
            HideSelection();
            message = "새 지하철이 출발할 노선의 역을 선택하세요";
        }

        private void HandleStationClick()
        {
            if (!Mouse.current.leftButton.wasPressedThisFrame || Camera.main == null) return;
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out RaycastHit hit)) return;
            MetroStationView selected = hit.collider.GetComponent<MetroStationView>();
            if (selected == null) return;

            if (trainPlacementMode)
            {
                if (!line.Contains(selected.StationId))
                {
                    message = "노선에 연결된 역을 선택해야 합니다";
                    return;
                }
                int stationIndex = line.IndexOf(selected.StationId);
                CreateTrain(stationIndex, trains.Count % 2 == 0 ? 1 : -1);
                trainStock--;
                trainPlacementMode = false;
                message = $"{selected.StationName} 역에 지하철 추가 — 현재 {trains.Count}대 운행 중";
                return;
            }

            if (line.Contains(selected.StationId))
            {
                if (selected.StationId == line[^1])
                {
                    ShowSelection(selected);
                    message = $"{selected.StationName} 역에서 노선 연장을 다시 시작합니다";
                }
                else
                    message = "노선은 마지막 역에서만 이어갈 수 있습니다";
                return;
            }

            if (line.Count > 0 && !hasRouteSelection)
            {
                message = "노선의 마지막 역을 먼저 선택한 뒤 새 역을 선택하세요";
                return;
            }

            line.Add(selected.StationId);
            ShowSelection(selected);
            if (line.Count == 1)
                message = $"{selected.StationName} 선택 — 다음 역을 선택하면 열차가 생성됩니다";
            else if (line.Count == 2)
                message = $"열차 생성: {views.First(v => v.StationId == line[0]).StationName} ↔ {selected.StationName}";
            else
                message = $"노선 확장: {selected.StationName} 역 연결";
            RebuildGraphAndRails();
            if (line.Count == 2) CreateTrain(0, 1);
        }

        private void HandleSpeedInput()
        {
            if (Keyboard.current == null) return;
            if (Keyboard.current.digit1Key.wasPressedThisFrame
                || Keyboard.current.numpad1Key.wasPressedThisFrame)
                simulationSpeed = 1;
            if (Keyboard.current.digit2Key.wasPressedThisFrame
                || Keyboard.current.numpad2Key.wasPressedThisFrame)
                simulationSpeed = 2;
            if (Keyboard.current.digit3Key.wasPressedThisFrame
                || Keyboard.current.numpad3Key.wasPressedThisFrame)
                simulationSpeed = 3;
        }

        private void ShowSelection(MetroStationView station)
        {
            if (selectionRing == null)
            {
                selectionRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                selectionRing.name = "Runtime_SelectedStation_Ring";
                selectionRing.transform.localScale = new Vector3(1.35f, 0.025f, 1.35f);
                selectionRing.GetComponent<Renderer>().material.color = new Color(1f, 0.9f, 0.15f);
                Destroy(selectionRing.GetComponent<Collider>());

                selectionMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                selectionMarker.name = "Runtime_SelectedStation_Marker";
                selectionMarker.transform.localScale = Vector3.one * 0.28f;
                selectionMarker.GetComponent<Renderer>().material.color = new Color(1f, 0.95f, 0.3f);
                Destroy(selectionMarker.GetComponent<Collider>());
            }

            Vector3 position = station.transform.position;
            selectionRing.transform.position = new Vector3(position.x, 0.08f, position.z);
            selectionMarkerBasePosition = position + Vector3.up * 1.25f;
            selectionMarker.transform.position = selectionMarkerBasePosition;
            selectionRing.SetActive(true);
            selectionMarker.SetActive(true);
            hasRouteSelection = true;
        }

        private void AnimateSelection()
        {
            if (selectionRing == null || !selectionRing.activeSelf) return;
            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 5f) * 0.12f;
            selectionRing.transform.localScale = new Vector3(1.35f * pulse, 0.025f, 1.35f * pulse);
            selectionMarker.transform.position = selectionMarkerBasePosition + Vector3.up
                * (Mathf.Sin(Time.unscaledTime * 4f) * 0.12f);
        }

        private void SpawnPassengers()
        {
            spawnTimer += Time.deltaTime * simulationSpeed;
            if (spawnTimer < passengerSpawnInterval) return;
            spawnTimer = 0f;
            MetroStationState origin = stations[UnityEngine.Random.Range(0, activeStationCount)];
            MetroShape[] goals = Enum.GetValues(typeof(MetroShape)).Cast<MetroShape>()
                .Where(shape => shape != origin.Shape).ToArray();
            origin.Waiting.Add(new MetroPassenger
            {
                Id = ++passengerId,
                OriginId = origin.Id,
                DestinationShape = goals[UnityEngine.Random.Range(0, goals.Length)]
            });
        }

        private void UnlockStationsOverTime()
        {
            if (activeStationCount >= views.Count) return;
            stationUnlockTimer += Time.deltaTime * simulationSpeed;
            if (stationUnlockTimer < newStationInterval) return;
            stationUnlockTimer = 0f;
            MetroStationView station = views[activeStationCount];
            station.gameObject.SetActive(true);
            activeStationCount++;
            message = $"새 역 등장: {station.StationName} — 노선을 확장하세요";
        }

        private void MoveTrain()
        {
            if (line.Count < 2) return;
            foreach (Train train in trains)
                MoveTrain(train);
        }

        private void MoveTrain(Train train)
        {
            if (train.View == null) return;
            int fromIndex = train.Segment;
            int toIndex = train.Segment + train.Direction;
            if (toIndex < 0 || toIndex >= line.Count)
            {
                train.Direction *= -1;
                toIndex = train.Segment + train.Direction;
            }

            Vector3 from = ViewFor(line[fromIndex]).transform.position + Vector3.up * 0.8f;
            Vector3 to = ViewFor(line[toIndex]).transform.position + Vector3.up * 0.8f;
            train.T += Time.deltaTime * trainSpeed * simulationSpeed;
            train.View.transform.position = Vector3.Lerp(from, to, train.T);
            if (train.T < 1f) return;

            train.T = 0f;
            train.Segment = toIndex;
            ProcessStation(train, line[toIndex]);
        }

        private void ProcessStation(Train train, int stationId)
        {
            MetroStationState station = stations.First(s => s.Id == stationId);
            for (int i = train.Passengers.Count - 1; i >= 0; i--)
            {
                if (station.Shape != train.Passengers[i].DestinationShape) continue;
                train.Passengers.RemoveAt(i);
                delivered++;
                if (delivered >= nextTrainReward)
                {
                    trainStock++;
                    nextTrainReward += 20;
                    message = $"승객 운송 보상: 지하철 1대 지급 — 보유 {trainStock}대";
                }
            }

            int remaining = trainCapacity - train.Passengers.Count;
            if (remaining <= 0) return;
            List<MetroPassenger> boarded = passengerQueue.Board(station.Waiting, line, stations, remaining);
            foreach (MetroPassenger passenger in boarded)
            {
                if (!station.Waiting.Remove(passenger)) continue;
                List<int> path = router.FindShortestPath(stationId, passenger.DestinationShape, stations, graph);
                if (path.Count == 0)
                {
                    station.Waiting.Add(passenger);
                    continue;
                }
                train.Passengers.Add(passenger);
                if (path.Count > 2) transfers++;
            }
        }

        private void UpdateCongestion()
        {
            foreach (MetroStationState station in stations)
            {
                foreach (MetroPassenger passenger in station.Waiting)
                    passenger.WaitSeconds += Time.deltaTime * simulationSpeed;
                float oldest = station.Waiting.Count == 0 ? 0f : station.Waiting.Max(p => p.WaitSeconds);
                float value = congestion.Calculate(station.Waiting.Count, stationSafeCapacity, oldest);
                maxCongestion = Mathf.Max(maxCongestion, value);
                if (value < 1f) continue;
                gameOver = true;
                message = $"{station.Name} 역 혼잡으로 운행 종료";
            }
        }

        private void RebuildGraphAndRails()
        {
            foreach (List<int> edges in graph.Values) edges.Clear();
            foreach (GameObject rail in rails) Destroy(rail);
            rails.Clear();
            for (int i = 0; i < line.Count - 1; i++)
            {
                int a = line[i];
                int b = line[i + 1];
                graph[a].Add(b);
                graph[b].Add(a);
                CreateRail(ViewFor(a).transform.position, ViewFor(b).transform.position);
            }
        }

        private void CreateRail(Vector3 a, Vector3 b)
        {
            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "Runtime_Rail";
            rail.transform.position = (a + b) * 0.5f + Vector3.up * 0.13f;
            rail.transform.LookAt(b + Vector3.up * 0.13f);
            rail.transform.localScale = new Vector3(0.16f, 0.08f, Vector3.Distance(a, b));
            rail.GetComponent<Renderer>().material.color = new Color(0.18f, 0.75f, 1f);
            rails.Add(rail);
        }

        private void CreateTrain(int stationIndex, int direction)
        {
            Train train = new();
            train.View = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            train.View.name = $"Runtime_Train_{trains.Count + 1:00}";
            train.View.transform.localScale = new Vector3(0.55f, 0.4f, 0.8f);
            train.View.GetComponent<Renderer>().material.color = new Color(1f, 0.82f, 0.15f);
            train.Segment = Mathf.Clamp(stationIndex, 0, line.Count - 1);
            train.Direction = direction;
            if (train.Segment == 0) train.Direction = 1;
            if (train.Segment == line.Count - 1) train.Direction = -1;
            train.T = 0f;
            train.View.transform.position = ViewFor(line[train.Segment]).transform.position + Vector3.up * 0.8f;
            trains.Add(train);
            ProcessStation(train, line[train.Segment]);
        }

        private MetroStationView ViewFor(int id) => views.First(v => v.StationId == id);

        private void ResetSimulation()
        {
            RemoveTrainsSafely();
            foreach (MetroStationState station in stations) station.Waiting.Clear();
            foreach (GameObject rail in rails) Destroy(rail);
            rails.Clear();
            HideSelection();
            line.Clear();
            hasRouteSelection = false;
            delivered = 0;
            transfers = 0;
            elapsed = 0f;
            stationUnlockTimer = 0f;
            maxCongestion = 0f;
            gameOver = false;
            trainPlacementMode = false;
            trainStock = 2;
            nextTrainReward = 20;
            simulationSpeed = 1;
            activeStationCount = Mathf.Clamp(initialStationCount, 2, views.Count);
            for (int i = 0; i < views.Count; i++)
                views[i].gameObject.SetActive(i < activeStationCount);
            message = "역을 클릭해 새 노선을 만드세요";
        }

        private void OnGUI()
        {
            // 모든 게임 글씨는 MetroStationCanvasUI의 Screen Space Overlay Canvas에서 표시합니다.
        }
    }
}
