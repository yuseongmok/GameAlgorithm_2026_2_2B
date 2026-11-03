using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace AlgoLesson7
{
    public sealed class AutoChessArenaGame : MonoBehaviour
    {
        private const int BoardColumns = 7;
        private const int TeamRows = 4;

        private enum GamePhase
        {
            Placement,
            Battle,
            Finished
        }

        private readonly List<AutoChessBoardCell> playerCells = new List<AutoChessBoardCell>();
        private readonly List<AutoChessBoardCell> enemyCells = new List<AutoChessBoardCell>();
        private readonly List<AutoChessBoardCell> allCells = new List<AutoChessBoardCell>();
        private readonly List<AutoChessBattleUnit> playerUnits = new List<AutoChessBattleUnit>();
        private readonly List<AutoChessBattleUnit> enemyUnits = new List<AutoChessBattleUnit>();
        private readonly List<UnitDefinition> roster = new List<UnitDefinition>();

        private ISynergyCalculator synergyCalculator;
        private ITargetSelector targetSelector;
        private IHexPathfinder hexPathfinder;
        private Camera mainCamera;
        private GamePhase phase;
        private SynergyResult playerSynergy;
        private string message;
        private int selectedRosterIndex;
        private int nextUnitId;

        private void Awake()
        {
            if (SceneManager.GetActiveScene().name != "Lesson07_AutoChessSynergyArena")
                return;

            mainCamera = Camera.main;
            synergyCalculator = Lesson7AlgorithmFactory.CreateSynergyCalculator();
            targetSelector = Lesson7AlgorithmFactory.CreateTargetSelector();
            hexPathfinder = Lesson7AlgorithmFactory.CreateHexPathfinder();
            BuildRoster();
            FindBoardCells();
            ResetGame();
        }

        private void Update()
        {
            if (Keyboard.current == null)
                return;

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                ResetGame();
                return;
            }

            if (phase == GamePhase.Placement)
            {
                ReadRosterHotkeys();
                ReadPlacementClick();

                if (Keyboard.current.enterKey.wasPressedThisFrame)
                    StartBattle();
            }

            if (phase == GamePhase.Battle)
                TickBattle();
        }

        private void BuildRoster()
        {
            roster.Clear();
            roster.Add(new UnitDefinition("Knight", UnitRace.Human, UnitJob.Warrior, 95f, 13f, 1.35f, new Color(0.25f, 0.55f, 1f)));
            roster.Add(new UnitDefinition("Archer", UnitRace.Human, UnitJob.Ranger, 65f, 16f, 4.2f, new Color(0.25f, 0.9f, 0.55f)));
            roster.Add(new UnitDefinition("Wolf", UnitRace.Beast, UnitJob.Warrior, 82f, 15f, 1.35f, new Color(1f, 0.55f, 0.2f)));
            roster.Add(new UnitDefinition("Shaman", UnitRace.Beast, UnitJob.Mage, 62f, 20f, 3.6f, new Color(0.75f, 0.35f, 1f)));
            roster.Add(new UnitDefinition("Robot", UnitRace.Machine, UnitJob.Warrior, 110f, 11f, 1.35f, new Color(0.5f, 0.7f, 0.75f)));
            roster.Add(new UnitDefinition("Cannon", UnitRace.Machine, UnitJob.Ranger, 72f, 18f, 4.6f, new Color(0.95f, 0.25f, 0.35f)));
        }

        private void FindBoardCells()
        {
            playerCells.Clear();
            enemyCells.Clear();
            allCells.Clear();

            foreach (AutoChessBoardCell cell in FindObjectsByType<AutoChessBoardCell>(FindObjectsSortMode.None))
            {
                int globalRow = cell.Team == 0
                    ? cell.Index / BoardColumns
                    : cell.Index / BoardColumns + TeamRows;
                cell.ConfigureCoordinates(globalRow, cell.Index % BoardColumns);
                allCells.Add(cell);

                if (cell.Team == 0)
                    playerCells.Add(cell);
                else
                    enemyCells.Add(cell);
            }

            playerCells.Sort((left, right) => left.Index.CompareTo(right.Index));
            enemyCells.Sort((left, right) => left.Index.CompareTo(right.Index));
        }

        private void ResetGame()
        {
            StopAllCoroutines();

            foreach (AutoChessBattleUnit unit in FindObjectsByType<AutoChessBattleUnit>(FindObjectsSortMode.None))
                Destroy(unit.gameObject);

            foreach (AutoChessBoardCell cell in playerCells.Concat(enemyCells))
                cell.Occupant = null;

            playerUnits.Clear();
            enemyUnits.Clear();
            nextUnitId = 1;
            selectedRosterIndex = 0;
            playerSynergy = SynergyResult.Empty;
            phase = GamePhase.Placement;
            message = "숫자키로 유닛을 고르고 파란 칸을 클릭하세요.";

            PlacePlayer(0, 0);
            PlacePlayer(1, 1);
            PlacePlayer(2, 2);
            PlaceEnemyTeam();
        }

        private void PlaceEnemyTeam()
        {
            int[] rosterIndices = { 2, 4, 5, 3, 0 };
            int[] cellIndices = { 0, 1, 2, 4, 5 };

            for (int i = 0; i < rosterIndices.Length; i++)
            {
                AutoChessBattleUnit unit = CreateUnit(roster[rosterIndices[i]], 1, enemyCells[cellIndices[i]]);
                enemyCells[cellIndices[i]].Occupant = unit;
                enemyUnits.Add(unit);
            }
        }

        private void ReadRosterHotkeys()
        {
            Key[] keys =
            {
                Key.Digit1,
                Key.Digit2,
                Key.Digit3,
                Key.Digit4,
                Key.Digit5,
                Key.Digit6
            };

            for (int i = 0; i < keys.Length; i++)
            {
                if (Keyboard.current[keys[i]].wasPressedThisFrame)
                {
                    selectedRosterIndex = i;
                    message = $"{roster[i].Name} 선택. 파란 칸을 클릭하세요.";
                }
            }
        }

        private void ReadPlacementClick()
        {
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
                return;

            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
                return;

            AutoChessBoardCell cell = hit.collider.GetComponent<AutoChessBoardCell>();

            if (cell == null || cell.Team != 0)
                return;

            PlacePlayer(selectedRosterIndex, cell.Index);
        }

        private void PlacePlayer(int rosterIndex, int cellIndex)
        {
            if (cellIndex < 0 || cellIndex >= playerCells.Count)
                return;

            AutoChessBoardCell cell = playerCells[cellIndex];

            if (cell.Occupant != null)
            {
                playerUnits.Remove(cell.Occupant);
                Destroy(cell.Occupant.gameObject);
            }

            AutoChessBattleUnit unit = CreateUnit(roster[rosterIndex], 0, cell);
            cell.Occupant = unit;
            playerUnits.Add(unit);
            PreviewSynergy();
        }

        private AutoChessBattleUnit CreateUnit(UnitDefinition definition, int team, AutoChessBoardCell cell)
        {
            GameObject unitObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            AutoChessBattleUnit unit = unitObject.AddComponent<AutoChessBattleUnit>();
            unit.Initialize(this, definition, nextUnitId++, team);
            unit.PlaceOnCell(cell, true);
            return unit;
        }

        private void PreviewSynergy()
        {
            playerSynergy = synergyCalculator.Calculate(CreateSnapshots(playerUnits));
        }

        private void StartBattle()
        {
            if (playerUnits.Count == 0)
            {
                message = "유닛을 한 명 이상 배치해야 합니다.";
                return;
            }

            playerSynergy = synergyCalculator.Calculate(CreateSnapshots(playerUnits));
            SynergyResult enemySynergy = synergyCalculator.Calculate(CreateSnapshots(enemyUnits));

            foreach (AutoChessBattleUnit unit in playerUnits)
                unit.ApplySynergy(playerSynergy);

            foreach (AutoChessBattleUnit unit in enemyUnits)
                unit.ApplySynergy(enemySynergy);

            phase = GamePhase.Battle;
            message = "자동 전투 중... R 키로 다시 배치할 수 있습니다.";
        }

        private void TickBattle()
        {
            foreach (AutoChessBattleUnit unit in playerUnits)
                unit.TickBattle(Time.deltaTime);

            foreach (AutoChessBattleUnit unit in enemyUnits)
                unit.TickBattle(Time.deltaTime);

            bool playerAlive = playerUnits.Any(unit => unit.IsAlive);
            bool enemyAlive = enemyUnits.Any(unit => unit.IsAlive);

            if (playerAlive && enemyAlive)
                return;

            phase = GamePhase.Finished;
            message = playerAlive ? "승리! 시너지와 타깃 선택을 확인하세요." : "패배! 배치와 시너지 조합을 바꿔보세요.";
        }

        public AutoChessBattleUnit FindTargetFor(AutoChessBattleUnit self)
        {
            List<AutoChessBattleUnit> candidates = self.Team == 0 ? enemyUnits : playerUnits;
            int targetId = targetSelector.SelectTarget(self.CreateSnapshot(), CreateSnapshots(candidates));
            return candidates.FirstOrDefault(unit => unit.Id == targetId && unit.IsAlive);
        }

        public int GetHexDistance(AutoChessBoardCell left, AutoChessBoardCell right)
        {
            return HexGridUtility.GetDistance(left, right);
        }

        public bool TryMoveOneHex(AutoChessBattleUnit unit, AutoChessBattleUnit target)
        {
            AutoChessBoardCell from = unit.CurrentCell;
            AutoChessBoardCell targetCell = target.CurrentCell;

            if (from == null || targetCell == null)
                return false;

            AutoChessBoardCell next = hexPathfinder.FindNextStep(from, targetCell, allCells);

            if (next == null || next.Occupant != null || GetHexDistance(from, next) != 1)
                return false;

            from.Occupant = null;
            next.Occupant = unit;
            unit.PlaceOnCell(next, false);
            return true;
        }

        public void ReleaseCell(AutoChessBattleUnit unit)
        {
            AutoChessBoardCell cell = unit.CurrentCell;

            if (cell != null && cell.Occupant == unit)
                cell.Occupant = null;
        }


        private static List<UnitSnapshot> CreateSnapshots(IEnumerable<AutoChessBattleUnit> units)
        {
            return units.Select(unit => unit.CreateSnapshot()).ToList();
        }

        public void SpawnAttackFlash(Vector3 start, Vector3 end, Color color)
        {
            StartCoroutine(AttackFlashRoutine(start, end, color));
        }

        private IEnumerator AttackFlashRoutine(Vector3 start, Vector3 end, Color color)
        {
            GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Cube);
            flash.name = "Attack_Flash";
            Destroy(flash.GetComponent<Collider>());
            flash.transform.position = (start + end) * 0.5f + Vector3.up * 0.6f;
            flash.transform.LookAt(end + Vector3.up * 0.6f);
            flash.transform.localScale = new Vector3(0.08f, 0.08f, Vector3.Distance(start, end));
            Renderer renderer = flash.GetComponent<Renderer>();
            renderer.material.color = color;
            yield return new WaitForSeconds(0.08f);
            Destroy(flash);
        }

        private void OnGUI()
        {
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            GUIStyle bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                normal = { textColor = Color.white }
            };

            GUI.Box(new Rect(16f, 16f, 520f, 178f), string.Empty);
            GUI.Label(new Rect(30f, 25f, 480f, 34f), "오토체스 시너지 아레나", titleStyle);
            GUI.Label(new Rect(30f, 66f, 480f, 24f), message, bodyStyle);
            GUI.Label(new Rect(30f, 94f, 480f, 24f), "1~6 유닛 선택 · 파란 칸 클릭 · Enter 전투 · R 재시작", bodyStyle);

            string selected = roster.Count == 0 ? "-" : roster[selectedRosterIndex].Name;
            GUI.Label(new Rect(30f, 122f, 480f, 24f), $"선택 유닛: {selected}", bodyStyle);

            string synergies = playerSynergy == null || playerSynergy.ActiveNames.Count == 0
                ? "활성 시너지: 없음 (TODO를 완성하세요)"
                : "활성 시너지: " + string.Join(", ", playerSynergy.ActiveNames);

            GUI.Label(new Rect(30f, 150f, 480f, 24f), synergies, bodyStyle);

            GUI.Box(new Rect(Screen.width - 360f, 16f, 344f, 178f), string.Empty);
            GUI.Label(new Rect(Screen.width - 344f, 28f, 315f, 150f),
                "1 Knight  Human / Warrior\n" +
                "2 Archer   Human / Ranger\n" +
                "3 Wolf     Beast / Warrior\n" +
                "4 Shaman   Beast / Mage\n" +
                "5 Robot    Machine / Warrior\n" +
                "6 Cannon   Machine / Ranger",
                bodyStyle);
        }
    }
}
