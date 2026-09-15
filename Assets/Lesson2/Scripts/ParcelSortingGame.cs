using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AlgoCourse.Lesson2
{
    /// <summary>
    /// 택배 생성, 스캔 판정과 분류 이동을 담당하는 제공 코드입니다.
    /// 학생은 IParcelCatalog 구현에만 집중합니다.
    /// </summary>
    public sealed class ParcelSortingGame : MonoBehaviour
    {
        private const int UnknownParcelId = 1999;

        [Header("Scene References")]
        [SerializeField] private ParcelScannerPlayer player;
        [SerializeField] private ParcelBox parcelTemplate;
        [SerializeField] private Transform parcelContainer;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform scanPoint;
        [SerializeField] private Transform[] destinationPoints;
        [SerializeField] private TextMesh statusText;

        [Header("Game Rules")]
        [SerializeField] private float scanDistance = 2.8f;
        [SerializeField] private float spawnInterval = 4f;
        [SerializeField] private int maximumWaitingParcels = 5;
        [SerializeField] private float conveyorSpeed = 3.5f;

        private readonly List<ParcelBox> activeParcels = new List<ParcelBox>();
        private readonly int[] parcelIds = { 1001, 1002, 1003, 1004, 1005, 1006, UnknownParcelId };
        private readonly int[] guaranteedFirstIds = { 1001, 1004, 1006, UnknownParcelId, 1002 };
        private IParcelCatalog catalog;
        private ParcelSortingUI gameUI;
        private float nextSpawnTime;
        private int guaranteedSpawnIndex;

        private void Start()
        {
            HideLegacyWorldText();
            gameUI = gameObject.GetComponent<ParcelSortingUI>();
            if (gameUI == null)
            {
                gameUI = gameObject.AddComponent<ParcelSortingUI>();
            }

            gameUI.Initialize();
            catalog = CreateCatalog();
            TryInitializeCatalog();
            player.SetGame(this);
            parcelTemplate.gameObject.SetActive(false);

            SpawnParcel();
            SetStatus($"택배 번호를 확인하고 E로 스캔하세요. 등록 상품: {catalog?.Count ?? 0}개");
        }

        private static void HideLegacyWorldText()
        {
            TextMesh[] worldTexts = FindObjectsByType<TextMesh>(FindObjectsSortMode.None);

            foreach (TextMesh worldText in worldTexts)
            {
                worldText.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            activeParcels.RemoveAll(parcel => parcel == null);

            if (Time.time >= nextSpawnTime && CountWaitingParcels() < maximumWaitingParcels)
            {
                SpawnParcel();
            }
        }

        public void TryScanNearestParcel(Vector3 scannerPosition)
        {
            ParcelBox nearest = FindNearestWaitingParcel(scannerPosition);
            if (nearest == null)
            {
                SetStatus("스캔 실패: 가까운 택배가 없습니다.");
                return;
            }

            if (catalog == null)
            {
                SetStatus("ProductCatalog를 찾을 수 없습니다. ToDo_02 코드를 확인하세요.");
                return;
            }

            if (!catalog.TryFind(nearest.ParcelId, out ParcelProductInfo product))
            {
                StartCoroutine(MoveUnknownParcel(nearest));
                SetStatus($"번호 {nearest.ParcelId}: 미등록 택배! 검사 구역에 남깁니다.");
                return;
            }

            if (product.DestinationIndex < 0 || product.DestinationIndex >= destinationPoints.Length)
            {
                SetStatus($"번호 {nearest.ParcelId}: 잘못된 목적지 값입니다.");
                return;
            }

            nearest.IsBeingSorted = true;
            bool hasCollision = catalog.HasCollision(nearest.ParcelId);
            StartCoroutine(MoveToDestination(nearest, destinationPoints[product.DestinationIndex]));
            SetStatus(
                $"ID {nearest.ParcelId} | Hash {catalog.GetHash(nearest.ParcelId)} | " +
                $"Bucket {catalog.GetBucketIndex(nearest.ParcelId)}/{catalog.BucketCount - 1} | " +
                $"충돌 {(hasCollision ? "있음" : "없음")} | {product.ProductName} → {DestinationName(product.DestinationIndex)}");
        }

        private void SpawnParcel()
        {
            ParcelBox parcel = Instantiate(parcelTemplate, spawnPoint.position, Quaternion.identity, parcelContainer);
            parcel.gameObject.SetActive(true);
            int parcelId = guaranteedSpawnIndex < guaranteedFirstIds.Length
                ? guaranteedFirstIds[guaranteedSpawnIndex++]
                : parcelIds[UnityEngine.Random.Range(0, parcelIds.Length)];
            parcel.Initialize(parcelId);
            activeParcels.Add(parcel);
            nextSpawnTime = Time.time + spawnInterval;

            int waitingIndex = Mathf.Max(0, CountWaitingParcels() - 1);
            Vector3 waitingPosition = scanPoint.position + Vector3.back * (waitingIndex * 1.35f);
            StartCoroutine(MoveParcel(parcel, waitingPosition));
        }

        private IEnumerator MoveToDestination(ParcelBox parcel, Transform destination)
        {
            Vector3 mergePoint = new Vector3(0f, parcel.transform.position.y, 3.2f);
            yield return MoveParcel(parcel, mergePoint);
            yield return MoveParcel(parcel, destination.position);

            activeParcels.Remove(parcel);
            Destroy(parcel.gameObject, 0.5f);
        }

        private IEnumerator MoveUnknownParcel(ParcelBox parcel)
        {
            parcel.IsBeingSorted = true;
            Vector3 inspectionPosition = scanPoint.position + Vector3.right * 2.2f;
            yield return MoveParcel(parcel, inspectionPosition);
            parcel.IsBeingSorted = false;
            parcel.IsRejected = true;
        }

        private IEnumerator MoveParcel(ParcelBox parcel, Vector3 targetPosition)
        {
            while (parcel != null && Vector3.Distance(parcel.transform.position, targetPosition) > 0.03f)
            {
                parcel.transform.position = Vector3.MoveTowards(
                    parcel.transform.position,
                    targetPosition,
                    conveyorSpeed * Time.deltaTime);
                yield return null;
            }

            if (parcel != null)
            {
                parcel.transform.position = targetPosition;
            }
        }

        private ParcelBox FindNearestWaitingParcel(Vector3 scannerPosition)
        {
            ParcelBox nearest = null;
            float nearestDistance = scanDistance;

            foreach (ParcelBox parcel in activeParcels)
            {
                if (parcel == null || parcel.IsBeingSorted || parcel.IsRejected)
                {
                    continue;
                }

                float distance = Vector3.Distance(scannerPosition, parcel.transform.position);
                if (distance <= nearestDistance)
                {
                    nearest = parcel;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        private int CountWaitingParcels()
        {
            return activeParcels.Count(parcel =>
                parcel != null &&
                !parcel.IsBeingSorted &&
                !parcel.IsRejected);
        }

        private IParcelCatalog CreateCatalog()
        {
            Type catalogType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(GetLoadableTypes)
                .FirstOrDefault(type =>
                    typeof(IParcelCatalog).IsAssignableFrom(type) &&
                    !type.IsInterface &&
                    !type.IsAbstract);

            if (catalogType == null)
            {
                return null;
            }

            return Activator.CreateInstance(catalogType) as IParcelCatalog;
        }

        private void TryInitializeCatalog()
        {
            try
            {
                catalog?.Initialize();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"상품 데이터 초기화 실패: {exception.Message}");
            }
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return exception.Types.Where(type => type != null);
            }
        }

        private static string DestinationName(int index)
        {
            return index switch
            {
                0 => "A 생활용품",
                1 => "B 장비",
                2 => "C 귀중품",
                _ => "알 수 없음"
            };
        }

        private void SetStatus(string message)
        {
            if (gameUI != null)
            {
                gameUI.SetStatus(message);
            }

            if (statusText != null)
            {
                statusText.text = message;
            }

            Debug.Log($"[택배 분류] {message}");
        }
    }
}
