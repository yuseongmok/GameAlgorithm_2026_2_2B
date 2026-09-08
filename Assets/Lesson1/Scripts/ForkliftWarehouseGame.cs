using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 제공 코드: 3D 표현, 지게차 상호작용, 컨베이어 이동을 담당합니다.
// Queue와 Stack의 실제 구현은 Assets/ToDo_01/Scripts/WarehouseData.cs에 있습니다.
public sealed class ForkliftWarehouseGame : MonoBehaviour
{
    private const int MaxStackHeight = 4;

    [Header("Player")]
    public ForkliftController forklift;

    [Header("Box")]
    public GameObject boxPrefab;
    public Material[] boxMaterials;

    [Header("Queue Slots")]
    public Transform[] incomingSlots;
    public Transform[] outgoingSlots;

    [Header("Stack Points")]
    public Transform[] stackPoints;

    private readonly Dictionary<ForkliftBox, Coroutine> activeMoves =
        new Dictionary<ForkliftBox, Coroutine>();

    private IWarehouseData warehouseData;
    private ForkliftBox heldBox;
    private float spawnTimer = 0.3f;
    private bool shipping;

    private void Start()
    {
        warehouseData = FindStudentData();

        if (warehouseData == null)
        {
            Debug.LogError(
                "WarehouseData가 없습니다. Assets/ToDo_01/Scripts/WarehouseData.cs를 작성하세요.");
            enabled = false;
            return;
        }

        warehouseData.Clear();
        forklift.game = this;

        for (int i = 0; i < 3; i++)
        {
            SpawnIncomingBox();
        }
    }

    private void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f && warehouseData.IncomingCount < incomingSlots.Length)
        {
            SpawnIncomingBox();
            spawnTimer = UnityEngine.Random.Range(1.5f, 2.8f);
        }
    }

    // 학생 코드가 Lesson1 패키지 밖에 있어도 자동으로 찾아 연결합니다.
    private IWarehouseData FindStudentData()
    {
        MonoBehaviour existing = GetComponents<MonoBehaviour>()
            .FirstOrDefault(component => component is IWarehouseData);

        if (existing != null)
        {
            return (IWarehouseData)existing;
        }

        Type dataType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(GetLoadableTypes)
            .FirstOrDefault(type =>
                typeof(MonoBehaviour).IsAssignableFrom(type) &&
                typeof(IWarehouseData).IsAssignableFrom(type) &&
                !type.IsAbstract);

        return dataType == null
            ? null
            : (IWarehouseData)gameObject.AddComponent(dataType);
    }

    private static IEnumerable<Type> GetLoadableTypes(System.Reflection.Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (System.Reflection.ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type != null);
        }
    }

    private void SpawnIncomingBox()
    {
        int kind = UnityEngine.Random.Range(1, 4);
        Vector3 spawnPosition = incomingSlots[incomingSlots.Length - 1].position;
        ForkliftBox box = CreateBox(kind, spawnPosition);

        box.queued = true;
        warehouseData.EnqueueIncoming(box);
        Reflow(warehouseData.GetIncomingItems(), incomingSlots);
    }

    // E 키를 누르면 호출됩니다.
    public void Interact()
    {
        if (heldBox != null)
        {
            DropHeldBox();
            return;
        }

        ForkliftBox candidate = FindPickupCandidate();

        if (candidate == null)
        {
            return;
        }

        CancelMove(candidate);
        RemoveFromDataStructure(candidate);

        heldBox = candidate;
        heldBox.held = true;
        heldBox.transform.SetParent(forklift.holdPoint, false);
        heldBox.transform.localPosition = Vector3.zero;
        heldBox.transform.localRotation = Quaternion.identity;
    }

    private ForkliftBox FindPickupCandidate()
    {
        ForkliftBox candidate = null;
        float bestDistance = 2.4f;

        CheckCandidate(warehouseData.PeekIncoming(), ref candidate, ref bestDistance);

        for (int stackIndex = 0; stackIndex < stackPoints.Length; stackIndex++)
        {
            // Stack은 반드시 Peek 결과인 맨 위 상자만 후보가 됩니다.
            CheckCandidate(
                warehouseData.PeekStack(stackIndex),
                ref candidate,
                ref bestDistance);
        }

        foreach (ForkliftBox box in FindObjectsByType<ForkliftBox>(FindObjectsSortMode.None))
        {
            bool isLooseBox = !box.queued && !box.held && box.stackIndex < 0;

            if (isLooseBox)
            {
                CheckCandidate(box, ref candidate, ref bestDistance);
            }
        }

        return candidate;
    }

    private void RemoveFromDataStructure(ForkliftBox box)
    {
        if (warehouseData.PeekIncoming() == box)
        {
            warehouseData.DequeueIncoming();
            box.queued = false;
            Reflow(warehouseData.GetIncomingItems(), incomingSlots);
        }

        for (int stackIndex = 0; stackIndex < stackPoints.Length; stackIndex++)
        {
            if (warehouseData.PeekStack(stackIndex) == box)
            {
                warehouseData.PopStack(stackIndex);
                box.stackIndex = -1;
                break;
            }
        }
    }

    private void DropHeldBox()
    {
        Vector3 forkPosition = heldBox.transform.position;

        if (Vector3.Distance(forkPosition, outgoingSlots[0].position) < 2.2f)
        {
            EnqueueOutgoing();
            return;
        }

        int stackIndex = FindNearestStack(forkPosition);

        if (stackIndex >= 0)
        {
            // 가득 찬 Stack에서는 상자를 떨어뜨리지 않고 포크에 유지합니다.
            if (warehouseData.GetStackCount(stackIndex) >= MaxStackHeight)
            {
                return;
            }

            PushOnStack(stackIndex);
            return;
        }

        DropOnFloor();
    }

    private void EnqueueOutgoing()
    {
        ForkliftBox box = ReleaseHeldBox();
        box.queued = true;
        box.stackIndex = -1;

        warehouseData.EnqueueOutgoing(box);
        Reflow(warehouseData.GetOutgoingItems(), outgoingSlots);

        if (!shipping)
        {
            StartCoroutine(ShipQueue());
        }
    }

    private void PushOnStack(int stackIndex)
    {
        ForkliftBox box = ReleaseHeldBox();
        box.stackIndex = stackIndex;
        warehouseData.PushStack(stackIndex, box);

        int height = warehouseData.GetStackCount(stackIndex) - 1;
        Vector3 target = stackPoints[stackIndex].position +
                         Vector3.up * (0.42f + height * 0.72f);

        StartMove(box, target);
    }

    private void DropOnFloor()
    {
        ForkliftBox box = ReleaseHeldBox();
        box.stackIndex = -1;
        box.transform.position = forklift.transform.position +
                                 forklift.transform.forward * 1.4f +
                                 Vector3.up * 0.35f;
    }

    private ForkliftBox ReleaseHeldBox()
    {
        ForkliftBox box = heldBox;
        heldBox = null;
        box.transform.SetParent(null);
        box.held = false;
        return box;
    }

    private int FindNearestStack(Vector3 forkPosition)
    {
        int nearestIndex = -1;
        float bestDistance = 2.4f;

        for (int i = 0; i < stackPoints.Length; i++)
        {
            float distance = Vector3.Distance(forkPosition, stackPoints[i].position);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    private void CheckCandidate(
        ForkliftBox box,
        ref ForkliftBox candidate,
        ref float bestDistance)
    {
        if (box == null)
        {
            return;
        }

        // Stack의 높이는 집기 가능 거리에 영향을 주지 않습니다.
        // 지게차와 상자의 바닥 좌표(XZ)만 비교합니다.
        Vector3 direction = box.transform.position - forklift.transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;
        float facing = direction.sqrMagnitude > 0.001f
            ? Vector3.Dot(forklift.transform.forward, direction.normalized)
            : 1f;

        if (distance < bestDistance && facing > 0.1f)
        {
            bestDistance = distance;
            candidate = box;
        }
    }

    private ForkliftBox CreateBox(int kind, Vector3 position)
    {
        GameObject instance = Instantiate(boxPrefab, position, Quaternion.identity);
        instance.name = "Box " + (char)('A' + kind - 1);
        instance.SetActive(true);
        instance.GetComponent<Renderer>().sharedMaterial = boxMaterials[kind - 1];

        ForkliftBox box = instance.GetComponent<ForkliftBox>();
        box.kind = kind;
        return box;
    }

    private void Reflow(ForkliftBox[] boxes, Transform[] slots)
    {
        for (int i = 0; i < boxes.Length && i < slots.Length; i++)
        {
            StartMove(boxes[i], slots[i].position);
        }
    }

    private void StartMove(ForkliftBox box, Vector3 target)
    {
        CancelMove(box);
        activeMoves[box] = StartCoroutine(MoveTo(box, target));
    }

    private void CancelMove(ForkliftBox box)
    {
        if (box != null && activeMoves.TryGetValue(box, out Coroutine movement))
        {
            StopCoroutine(movement);
            activeMoves.Remove(box);
        }
    }

    private IEnumerator MoveTo(ForkliftBox box, Vector3 target)
    {
        Vector3 start = box.transform.position;
        float progress = 0f;

        while (progress < 1f && box != null)
        {
            progress += Time.deltaTime * 2.5f;
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            box.transform.position = Vector3.Lerp(start, target, smoothProgress);
            yield return null;
        }

        if (box != null)
        {
            activeMoves.Remove(box);
        }
    }

    private IEnumerator ShipQueue()
    {
        shipping = true;

        while (warehouseData.OutgoingCount > 0)
        {
            ForkliftBox box = warehouseData.PeekOutgoing();
            CancelMove(box);

            foreach (Transform slot in outgoingSlots)
            {
                yield return MoveTo(box, slot.position);
                yield return new WaitForSeconds(0.12f);
            }

            warehouseData.DequeueOutgoing();
            Destroy(box.gameObject);
            Reflow(warehouseData.GetOutgoingItems(), outgoingSlots);
        }

        shipping = false;
    }
}
