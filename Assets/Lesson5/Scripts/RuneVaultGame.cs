using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AlgoCourse.Lesson5
{
    public sealed class RuneVaultGame : MonoBehaviour
    {
        [SerializeField] private RunePedestal[] pedestals;
        [SerializeField] private RuneSlotView[] slots;
        [SerializeField] private Transform vaultDoor;
        [SerializeField] private float autoDelay = 0.38f;

        private readonly List<RuneData> runes = new List<RuneData>();
        private readonly List<int> selectedRuneIds = new List<int>();
        private RuneSolveResult result;
        private RuneVaultUI gameUI;
        private Coroutine autoRoutine;
        private int stepIndex;
        private int currentSum;
        private Vector3 closedDoorPosition;

        private void Start()
        {
            runes.Add(new RuneData(0, "불", 4, RuneElement.Fire));
            runes.Add(new RuneData(1, "물", 2, RuneElement.Water));
            runes.Add(new RuneData(2, "바람", 3, RuneElement.Wind));
            runes.Add(new RuneData(3, "땅", 5, RuneElement.Earth));
            runes.Add(new RuneData(4, "빛", 6, RuneElement.Light));
            runes.Add(new RuneData(5, "어둠", 1, RuneElement.Dark));
            gameUI = GetComponent<RuneVaultUI>() ?? gameObject.AddComponent<RuneVaultUI>();
            closedDoorPosition = vaultDoor.position;
            ResetPuzzle();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.spaceKey.wasPressedThisFrame) ProcessOneStep();
            if (keyboard.aKey.wasPressedThisFrame) ToggleAuto();
            if (keyboard.rKey.wasPressedThisFrame) ResetPuzzle();
        }

        public void ResetPuzzle()
        {
            StopAuto();
            selectedRuneIds.Clear();
            stepIndex = 0;
            currentSum = 0;
            vaultDoor.position = closedDoorPosition;
            foreach (RuneSlotView slot in slots) slot.Clear();
            foreach (RunePedestal pedestal in pedestals) pedestal.SetState(false, false);

            IRuneVaultSolver solver = CreateSolver();
            if (solver == null)
            {
                result = null;
                UpdateUI("ToDo_05의 RuneVaultSolver를 확인하세요.");
                return;
            }

            result = solver.Solve(new RunePuzzle(runes, 4, 14, true, true));
            UpdateUI(result.Steps.Count == 0
                ? "탐색 단계가 없습니다. 학생 코드를 확인하세요."
                : "Space로 선택과 되돌리기를 한 단계씩 확인하세요.");
        }

        public void ProcessOneStep()
        {
            if (result == null || stepIndex >= result.Steps.Count)
            {
                StopAuto();
                return;
            }

            RuneSearchStep step = result.Steps[stepIndex++];
            RuneData rune = runes.FirstOrDefault(item => item.Id == step.RuneId);
            currentSum = step.CurrentSum;

            if (step.Action == RuneSearchAction.Choose && rune != null)
            {
                selectedRuneIds.Add(rune.Id);
                RefreshSlots();
                FindPedestal(rune.Id)?.SetState(true, false);
            }
            else if (step.Action == RuneSearchAction.Backtrack && selectedRuneIds.Count > 0)
            {
                int removedId = selectedRuneIds[selectedRuneIds.Count - 1];
                selectedRuneIds.RemoveAt(selectedRuneIds.Count - 1);
                FindPedestal(removedId)?.SetState(false, false);
                RefreshSlots();
            }
            else if (step.Action == RuneSearchAction.Reject && rune != null)
            {
                FindPedestal(rune.Id)?.SetState(false, true);
                StartCoroutine(RestorePedestal(rune.Id));
            }
            else if (step.Action == RuneSearchAction.Solved)
            {
                vaultDoor.position = closedDoorPosition + Vector3.up * 4.2f;
                StopAuto();
            }

            UpdateUI(step.Reason);
        }

        private void RefreshSlots()
        {
            for (int index = 0; index < slots.Length; index++)
            {
                if (index < selectedRuneIds.Count)
                {
                    RuneData rune = runes.First(item => item.Id == selectedRuneIds[index]);
                    slots[index].ShowRune(rune, ElementColor(rune.Element));
                }
                else slots[index].Clear();
            }
        }

        private IEnumerator RestorePedestal(int runeId)
        {
            yield return new WaitForSeconds(0.22f);
            bool selected = selectedRuneIds.Contains(runeId);
            FindPedestal(runeId)?.SetState(selected, false);
        }

        private void ToggleAuto()
        {
            if (autoRoutine == null && result != null && stepIndex < result.Steps.Count)
                autoRoutine = StartCoroutine(AutoPlay());
            else StopAuto();
            UpdateUI(autoRoutine == null ? "자동 탐색을 멈췄습니다." : "자동 탐색을 시작합니다.");
        }

        private IEnumerator AutoPlay()
        {
            while (result != null && stepIndex < result.Steps.Count)
            {
                ProcessOneStep();
                yield return new WaitForSeconds(autoDelay);
            }
            autoRoutine = null;
        }

        private void StopAuto()
        {
            if (autoRoutine == null) return;
            StopCoroutine(autoRoutine);
            autoRoutine = null;
        }

        private RunePedestal FindPedestal(int id) => pedestals.FirstOrDefault(item => item.RuneId == id);

        private void UpdateUI(string message)
        {
            gameUI.SetState(message, stepIndex, result?.Steps.Count ?? 0, currentSum, autoRoutine != null);
            Debug.Log($"[마법 금고] {message}");
        }

        private static Color ElementColor(RuneElement element)
        {
            switch (element)
            {
                case RuneElement.Fire: return new Color(0.95f, 0.16f, 0.08f);
                case RuneElement.Water: return new Color(0.08f, 0.45f, 0.95f);
                case RuneElement.Wind: return new Color(0.25f, 0.9f, 0.68f);
                case RuneElement.Earth: return new Color(0.55f, 0.32f, 0.12f);
                case RuneElement.Light: return new Color(1f, 0.85f, 0.18f);
                default: return new Color(0.55f, 0.2f, 0.75f);
            }
        }

        private static IRuneVaultSolver CreateSolver()
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetLoadableTypes).FirstOrDefault(candidate =>
                typeof(IRuneVaultSolver).IsAssignableFrom(candidate) && !candidate.IsInterface && !candidate.IsAbstract);
            return type == null ? null : Activator.CreateInstance(type) as IRuneVaultSolver;
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try { return assembly.GetTypes(); }
            catch (ReflectionTypeLoadException exception) { return exception.Types.Where(type => type != null); }
        }
    }
}
