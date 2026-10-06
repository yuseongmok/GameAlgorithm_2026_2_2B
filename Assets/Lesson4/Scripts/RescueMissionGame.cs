using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AlgoCourse.Lesson4
{
    public sealed class RescueMissionGame : MonoBehaviour
    {
        [SerializeField] private int width = 12;
        [SerializeField] private int height = 8;
        [SerializeField] private RescueTile[] tiles;
        [SerializeField] private RescueSurvivor[] survivors;
        [SerializeField] private FirefighterAgent firefighter;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private float autoStepDelay = 0.28f;

        private readonly Dictionary<GridPosition, RescueTile> tileMap = new Dictionary<GridPosition, RescueTile>();
        private readonly List<GridPosition> currentPath = new List<GridPosition>();
        private IRescuePathfinder pathfinder;
        private RescueMissionUI gameUI;
        private RescueSurvivor selectedSurvivor;
        private Coroutine autoRoutine;
        private int nextPathIndex;
        private int score;
        private int rescuedCount;

        private void Start()
        {
            foreach (RescueTile tile in tiles) tileMap[tile.Position] = tile;
            gameUI = GetComponent<RescueMissionUI>() ?? gameObject.AddComponent<RescueMissionUI>();
            pathfinder = CreatePathfinder();
            ResetMission();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.bKey.wasPressedThisFrame) CalculatePath();
                if (keyboard.spaceKey.wasPressedThisFrame) MoveOneStep();
                if (keyboard.aKey.wasPressedThisFrame) ToggleAuto();
                if (keyboard.fKey.wasPressedThisFrame) SpreadFire();
                if (keyboard.rKey.wasPressedThisFrame) ResetMission();
            }

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                SelectClickedSurvivor(mouse.position.ReadValue());
            }
        }

        public void ResetMission()
        {
            StopAuto();
            score = 0;
            rescuedCount = 0;
            selectedSurvivor = null;
            currentPath.Clear();
            nextPathIndex = 0;
            foreach (RescueTile tile in tiles) tile.RestoreInitialState();
            foreach (RescueSurvivor survivor in survivors) survivor.ResetSurvivor();
            GridPosition entrance = new GridPosition(0, 0);
            firefighter.ResetAt(entrance, WorldPosition(entrance, 0.75f));
            UpdateUI(pathfinder == null
                ? "ToDo_04의 BfsRescuePathfinder를 확인하세요."
                : "구조 대상을 클릭하고 B를 눌러 BFS를 시작하세요.");
        }

        public void CalculatePath()
        {
            StopAuto();
            if (pathfinder == null || selectedSurvivor == null || selectedSurvivor.IsRescued)
            {
                UpdateUI("먼저 구조 대상을 클릭하세요.");
                return;
            }

            ClearSearchColors();
            HashSet<GridPosition> blocked = new HashSet<GridPosition>(tiles.Where(tile => tile.IsBlocked).Select(tile => tile.Position));
            PathSearchResult result = pathfinder.FindPath(width, height, firefighter.Position, selectedSurvivor.Position, blocked);
            foreach (GridPosition position in result.VisitedOrder)
            {
                if (tileMap.TryGetValue(position, out RescueTile tile)) tile.ShowVisited();
            }

            currentPath.Clear();
            currentPath.AddRange(result.Path);
            nextPathIndex = currentPath.Count > 0 && currentPath[0] == firefighter.Position ? 1 : 0;
            foreach (GridPosition position in currentPath)
            {
                if (tileMap.TryGetValue(position, out RescueTile tile)) tile.ShowPath();
            }
            UpdateUI(result.Found ? $"최단 경로 발견: {Math.Max(0, currentPath.Count - 1)}칸" : "경로를 찾지 못했습니다.");
        }

        public void MoveOneStep()
        {
            if (firefighter.IsMoving || nextPathIndex >= currentPath.Count)
            {
                StopAuto();
                return;
            }

            GridPosition next = currentPath[nextPathIndex];
            if (!tileMap.TryGetValue(next, out RescueTile tile) || tile.IsBlocked)
            {
                StopAuto();
                currentPath.Clear();
                UpdateUI("화재가 경로를 막았습니다. B를 눌러 다시 탐색하세요.");
                return;
            }

            nextPathIndex++;
            firefighter.MoveTo(next, WorldPosition(next, 0.75f), CheckRescue);
            UpdateUI($"경로를 따라 {next}로 이동합니다.");
        }

        public void SpreadFire()
        {
            List<RescueTile> candidates = tiles.Where(tile => !tile.IsBlocked && tile.Position != firefighter.Position &&
                survivors.All(survivor => survivor.IsRescued || survivor.Position != tile.Position)).ToList();
            if (candidates.Count == 0) return;

            RescueTile fire = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            fire.SetFire();
            if (nextPathIndex < currentPath.Count && currentPath.Skip(nextPathIndex).Contains(fire.Position))
            {
                StopAuto();
                UpdateUI($"{fire.Position}에 불이 번져 경로가 차단됐습니다. 다시 BFS를 실행하세요.");
            }
            else
            {
                UpdateUI($"{fire.Position}에 불이 번졌습니다.");
            }
        }

        private void CheckRescue()
        {
            if (selectedSurvivor != null && !selectedSurvivor.IsRescued && firefighter.Position == selectedSurvivor.Position)
            {
                selectedSurvivor.Rescue();
                score += selectedSurvivor.Score;
                rescuedCount++;
                selectedSurvivor = null;
                currentPath.Clear();
                StopAuto();
                UpdateUI("구조 성공! 다음 구조 대상을 선택하세요.");
            }
        }

        private void SelectClickedSurvivor(Vector2 screenPosition)
        {
            if (worldCamera == null) return;
            Ray ray = worldCamera.ScreenPointToRay(screenPosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 200f)) return;
            RescueSurvivor clicked = hit.collider.GetComponent<RescueSurvivor>();
            if (clicked == null || clicked.IsRescued) return;
            foreach (RescueSurvivor survivor in survivors) survivor.SetSelected(survivor == clicked);
            selectedSurvivor = clicked;
            currentPath.Clear();
            UpdateUI($"{clicked.SurvivorName} 선택 · {clicked.Score}점 · B로 경로 탐색");
        }

        private void ToggleAuto()
        {
            if (autoRoutine == null && nextPathIndex < currentPath.Count)
            {
                autoRoutine = StartCoroutine(AutoMove());
            }
            else
            {
                StopAuto();
            }
            UpdateUI(autoRoutine == null ? "자동 이동을 멈췄습니다." : "자동 이동을 시작합니다.");
        }

        private IEnumerator AutoMove()
        {
            while (nextPathIndex < currentPath.Count)
            {
                MoveOneStep();
                yield return new WaitForSeconds(autoStepDelay);
                if (currentPath.Count == 0) break;
            }
            autoRoutine = null;
            UpdateUI("자동 이동이 끝났습니다.");
        }

        private void StopAuto()
        {
            if (autoRoutine != null)
            {
                StopCoroutine(autoRoutine);
                autoRoutine = null;
            }
        }

        private void ClearSearchColors()
        {
            foreach (RescueTile tile in tiles) tile.ClearSearchColor();
        }

        private Vector3 WorldPosition(GridPosition position, float y)
        {
            return new Vector3(position.X - (width - 1) * 0.5f, y, position.Y - (height - 1) * 0.5f);
        }

        private void UpdateUI(string message)
        {
            gameUI.SetState(message, score, rescuedCount, Math.Max(0, currentPath.Count - nextPathIndex), autoRoutine != null);
            Debug.Log($"[구조 작전] {message}");
        }

        private static IRescuePathfinder CreatePathfinder()
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetLoadableTypes).FirstOrDefault(candidate =>
                typeof(IRescuePathfinder).IsAssignableFrom(candidate) && !candidate.IsInterface && !candidate.IsAbstract);
            return type == null ? null : Activator.CreateInstance(type) as IRescuePathfinder;
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try { return assembly.GetTypes(); }
            catch (ReflectionTypeLoadException exception) { return exception.Types.Where(type => type != null); }
        }
    }
}
