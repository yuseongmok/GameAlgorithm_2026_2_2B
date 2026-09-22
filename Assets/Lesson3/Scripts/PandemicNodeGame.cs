using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AlgoCourse.Lesson3
{
    public sealed class PandemicNodeGame : MonoBehaviour
    {
        [SerializeField] private PandemicCityNode[] cities;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private float autoDelay = 0.5f;

        private readonly Dictionary<int, PandemicCityNode> cityMap = new Dictionary<int, PandemicCityNode>();
        private ICityInfectionAlgorithm algorithm;
        private PandemicNodeUI gameUI;
        private Coroutine autoRoutine;
        private int processedSteps;

        private void Start()
        {
            cityMap.Clear();
            foreach (PandemicCityNode city in cities)
            {
                cityMap[city.CityId] = city;
            }

            gameUI = GetComponent<PandemicNodeUI>();
            if (gameUI == null)
            {
                gameUI = gameObject.AddComponent<PandemicNodeUI>();
            }
            gameUI.Initialize();
            ResetSimulation();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.spaceKey.wasPressedThisFrame) ProcessOneRequest();
                if (keyboard.aKey.wasPressedThisFrame) ToggleAuto();
                if (keyboard.rKey.wasPressedThisFrame) ResetSimulation();
            }

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                QueueClickedCity(mouse.position.ReadValue());
            }
        }

        public void ResetSimulation()
        {
            StopAuto();
            processedSteps = 0;
            algorithm = CreateAlgorithm();

            foreach (PandemicCityNode city in cities)
            {
                city.SetInfectionLevel(0);
            }

            if (algorithm == null)
            {
                SetStatus("ToDo_03의 PandemicInfectionAlgorithm을 확인하세요.");
                return;
            }

            Dictionary<int, int[]> graph = cities.ToDictionary(city => city.CityId, city => city.NeighborIds);
            algorithm.Initialize(graph);
            UpdateUI("도시 노드를 클릭해 감염 요청을 Queue에 넣으세요.");
        }

        public void ProcessOneRequest()
        {
            if (algorithm == null || algorithm.PendingCount == 0)
            {
                StopAuto();
                UpdateUI("처리할 감염 요청이 없습니다. 도시를 클릭하세요.");
                return;
            }

            CityInfectionStep step = algorithm.ProcessNext();
            processedSteps++;

            if (cityMap.TryGetValue(step.CityId, out PandemicCityNode city))
            {
                city.SetInfectionLevel(step.InfectionLevel);
                if (step.IsOutbreak)
                {
                    city.ShowOutbreak();
                }
            }

            RefreshQueuedColors();
            string result = step.IsOutbreak
                ? $"{city.CityName}에서 Outbreak! 연결 도시가 Queue에 추가됩니다."
                : $"{city.CityName} 감염 단계가 {step.InfectionLevel}로 증가했습니다.";
            UpdateUI(result);
        }

        private void QueueClickedCity(Vector2 screenPosition)
        {
            if (algorithm == null || worldCamera == null)
            {
                return;
            }

            Ray ray = worldCamera.ScreenPointToRay(screenPosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 200f))
            {
                return;
            }

            PandemicCityNode city = hit.collider.GetComponent<PandemicCityNode>();
            if (city == null || !algorithm.QueueInfection(city.CityId))
            {
                return;
            }

            city.SetQueued(true);
            UpdateUI($"{city.CityName} 감염 요청을 Enqueue했습니다.");
        }

        private void RefreshQueuedColors()
        {
            foreach (PandemicCityNode city in cities)
            {
                city.SetInfectionLevel(algorithm.GetInfectionLevel(city.CityId));
            }
        }

        private void ToggleAuto()
        {
            if (autoRoutine == null)
            {
                autoRoutine = StartCoroutine(AutoProcess());
                gameUI.SetAutoMode(true);
            }
            else
            {
                StopAuto();
            }
        }

        private IEnumerator AutoProcess()
        {
            while (algorithm != null && algorithm.PendingCount > 0)
            {
                ProcessOneRequest();
                yield return new WaitForSeconds(autoDelay);
            }
            autoRoutine = null;
            gameUI.SetAutoMode(false);
        }

        private void StopAuto()
        {
            if (autoRoutine != null)
            {
                StopCoroutine(autoRoutine);
                autoRoutine = null;
            }
            gameUI?.SetAutoMode(false);
        }

        private ICityInfectionAlgorithm CreateAlgorithm()
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(GetLoadableTypes)
                .FirstOrDefault(candidate =>
                    typeof(ICityInfectionAlgorithm).IsAssignableFrom(candidate) &&
                    !candidate.IsInterface && !candidate.IsAbstract);
            return type == null ? null : Activator.CreateInstance(type) as ICityInfectionAlgorithm;
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try { return assembly.GetTypes(); }
            catch (ReflectionTypeLoadException exception) { return exception.Types.Where(type => type != null); }
        }

        private void UpdateUI(string message)
        {
            gameUI?.SetState(message, processedSteps, algorithm?.PendingCount ?? 0, algorithm?.OutbreakCount ?? 0);
            Debug.Log($"[도시 감염] {message}");
        }

        private void SetStatus(string message)
        {
            gameUI?.SetState(message, 0, 0, 0);
            Debug.LogWarning(message);
        }
    }
}
