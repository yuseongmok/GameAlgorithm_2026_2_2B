#if UNITY_EDITOR
using System.Collections.Generic;
using AlgoCourse.Lesson2;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AlgoCourse.Lesson2.Editor
{
    /// <summary>
    /// Lesson2를 처음 임포트한 뒤 씬이 없으면 한 번만 자동 생성합니다.
    /// </summary>
    [InitializeOnLoad]
    internal static class ParcelScannerSceneAutoBuilder
    {
        static ParcelScannerSceneAutoBuilder()
        {
            EditorApplication.delayCall += BuildWhenMissing;
        }

        private static void BuildWhenMissing()
        {
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(ParcelScannerSceneBuilder.SceneAssetPath))
            {
                ParcelScannerSceneBuilder.BuildScene();
                Debug.Log("Lesson 2 택배 스캔 분류 씬을 자동 생성했습니다.");
            }
        }
    }

    /// <summary>
    /// 수업용 3D 택배 분류장을 실제 Hierarchy 오브젝트로 제작합니다.
    /// </summary>
    public static class ParcelScannerSceneBuilder
    {
        private const string LessonRoot = "Assets/Lesson2";
        private const string ScenePath = LessonRoot + "/Scenes/Lesson02_ParcelScanner.unity";
        private const string MaterialFolder = LessonRoot + "/Materials";

        public const string SceneAssetPath = ScenePath;

        [MenuItem("AlgoLab/Lesson 2/Rebuild Parcel Scanner Scene")]
        public static void BuildFromMenu()
        {
            BuildScene();
            Debug.Log($"Lesson 2 씬 생성 완료: {ScenePath}");
        }

        public static void BuildScene()
        {
            EnsureFolder("Assets", "Lesson2");
            EnsureFolder(LessonRoot, "Scenes");
            EnsureFolder(LessonRoot, "Materials");

            Material floor = CreateMaterial("Floor", new Color(0.16f, 0.20f, 0.24f));
            Material belt = CreateMaterial("Conveyor", new Color(0.08f, 0.10f, 0.13f));
            Material metal = CreateMaterial("Metal", new Color(0.34f, 0.39f, 0.43f));
            Material blue = CreateMaterial("ScannerPlayer", new Color(0.12f, 0.58f, 0.92f));
            Material parcel = CreateMaterial("Parcel", new Color(0.73f, 0.45f, 0.20f));
            Material zoneA = CreateMaterial("Zone_A", new Color(0.18f, 0.72f, 0.40f));
            Material zoneB = CreateMaterial("Zone_B", new Color(0.95f, 0.63f, 0.12f));
            Material zoneC = CreateMaterial("Zone_C", new Color(0.70f, 0.30f, 0.86f));
            Material warning = CreateMaterial("Inspection", new Color(0.88f, 0.20f, 0.20f));

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject environment = Root("01_ENVIRONMENT");
            CreateCube("Floor", environment.transform, new Vector3(0f, -0.25f, 1.5f), new Vector3(22f, 0.5f, 18f), floor);
            CreateCube("Back Wall", environment.transform, new Vector3(0f, 2f, 9f), new Vector3(22f, 4.5f, 0.4f), metal);
            CreateCube("Left Safety Wall", environment.transform, new Vector3(-10.8f, 1f, 1.5f), new Vector3(0.3f, 2f, 15f), metal);
            CreateWorldText("Lesson Title", environment.transform, "DICTIONARY PARCEL CENTER", new Vector3(0f, 3.8f, 8.7f), 0.22f, Color.white);

            GameObject incoming = Root("02_INCOMING_CONVEYOR");
            CreateConveyor("Incoming Belt", incoming.transform, new Vector3(0f, 0.15f, -3.2f), new Vector3(2.2f, 0.3f, 8.5f), belt, metal);
            Transform spawnPoint = CreatePoint("Parcel Spawn Point", incoming.transform, new Vector3(0f, 0.8f, -6.6f));
            Transform scanPoint = CreatePoint("Scan Waiting Point", incoming.transform, new Vector3(0f, 0.8f, -0.7f));

            GameObject scanArea = Root("03_SCAN_AREA");
            CreateCube("Scanner Left", scanArea.transform, new Vector3(-1.25f, 1.15f, -0.2f), new Vector3(0.25f, 2.3f, 0.25f), blue);
            CreateCube("Scanner Right", scanArea.transform, new Vector3(1.25f, 1.15f, -0.2f), new Vector3(0.25f, 2.3f, 0.25f), blue);
            CreateCube("Scanner Top", scanArea.transform, new Vector3(0f, 2.25f, -0.2f), new Vector3(2.7f, 0.25f, 0.25f), blue);
            CreateWorldText("Scan Sign", scanArea.transform, "SCAN  [ E ]", new Vector3(0f, 2.65f, -0.2f), 0.15f, Color.cyan);
            CreateCube("Inspection Zone", scanArea.transform, new Vector3(2.2f, 0.03f, -0.7f), new Vector3(2f, 0.06f, 2f), warning);
            CreateWorldText("Inspection Sign", scanArea.transform, "UNKNOWN ID", new Vector3(2.2f, 0.12f, 0.2f), 0.09f, Color.red);

            GameObject zones = Root("04_SORTING_ZONES");
            Material[] zoneMaterials = { zoneA, zoneB, zoneC };
            string[] zoneNames = { "A  LIVING", "B  EQUIPMENT", "C  VALUABLE" };
            float[] zoneX = { -6f, 0f, 6f };
            Transform[] destinations = new Transform[3];
            CreateConveyor("Main Diverter", zones.transform, new Vector3(0f, 0.15f, 3.2f), new Vector3(14f, 0.3f, 1.5f), belt, metal);

            for (int index = 0; index < destinations.Length; index++)
            {
                CreateConveyor($"Zone {index} Belt", zones.transform, new Vector3(zoneX[index], 0.15f, 5.5f), new Vector3(2.5f, 0.3f, 5f), belt, metal);
                CreateCube($"Zone {index} Pad", zones.transform, new Vector3(zoneX[index], 0.03f, 7.3f), new Vector3(3.4f, 0.08f, 2.2f), zoneMaterials[index]);
                CreateWorldText($"Zone {index} Sign", zones.transform, zoneNames[index], new Vector3(zoneX[index], 1.4f, 8.25f), 0.13f, zoneMaterials[index].color);
                destinations[index] = CreatePoint($"Zone {index} Destination", zones.transform, new Vector3(zoneX[index], 0.8f, 7.1f));
            }

            GameObject playerRoot = Root("05_PLAYER");
            GameObject playerObject = new GameObject("Scanner Worker");
            playerObject.transform.SetParent(playerRoot.transform);
            playerObject.transform.position = new Vector3(-3f, 0f, -1f);
            CharacterController controller = playerObject.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.42f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            ParcelScannerPlayer playerScript = playerObject.AddComponent<ParcelScannerPlayer>();
            CreateCapsule("Body", playerObject.transform, new Vector3(0f, 0.9f, 0f), new Vector3(0.8f, 0.9f, 0.8f), blue);
            CreateLocalCube("Hand Scanner", playerObject.transform, new Vector3(0.55f, 1.05f, 0.25f), new Vector3(0.22f, 0.22f, 0.55f), warning);

            GameObject templates = Root("06_PARCEL_TEMPLATE");
            GameObject templateObject = CreateCube("Parcel Template", templates.transform, new Vector3(0f, -10f, 0f), Vector3.one, parcel);
            ParcelBox parcelTemplate = templateObject.AddComponent<ParcelBox>();
            templateObject.SetActive(false);

            GameObject managerRoot = Root("07_GAME_MANAGER");
            Transform parcelContainer = new GameObject("Runtime Parcels").transform;
            parcelContainer.SetParent(managerRoot.transform);
            GameObject managerObject = new GameObject("Parcel Sorting Game");
            managerObject.transform.SetParent(managerRoot.transform);
            ParcelSortingGame game = managerObject.AddComponent<ParcelSortingGame>();
            TextMesh status = CreateWorldText("World Status", managerRoot.transform, "택배 분류 시스템 준비 중...", new Vector3(0f, 3.1f, -0.2f), 0.13f, Color.white);

            Assign(game, "player", playerScript);
            Assign(game, "parcelTemplate", parcelTemplate);
            Assign(game, "parcelContainer", parcelContainer);
            Assign(game, "spawnPoint", spawnPoint);
            Assign(game, "scanPoint", scanPoint);
            AssignArray(game, "destinationPoints", destinations);
            Assign(game, "statusText", status);
            Assign(playerScript, "game", game);

            GameObject cameraRoot = Root("08_CAMERA_AND_LIGHT");
            GameObject cameraObject = new GameObject("Tracking Camera");
            cameraObject.transform.SetParent(cameraRoot.transform);
            cameraObject.transform.position = playerObject.transform.position + new Vector3(0f, 11f, -9f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.fieldOfView = 55f;
            cameraObject.AddComponent<AudioListener>();
            ParcelScannerCamera followCamera = cameraObject.AddComponent<ParcelScannerCamera>();
            Assign(followCamera, "target", playerObject.transform);

            GameObject lightObject = new GameObject("Directional Light");
            lightObject.transform.SetParent(cameraRoot.transform);
            lightObject.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.shadows = LightShadows.Soft;

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static GameObject Root(string name) => new GameObject(name);

        private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent);
            cube.transform.position = position;
            cube.transform.localScale = scale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            return cube;
        }

        private static GameObject CreateLocalCube(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = position;
            cube.transform.localScale = scale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(cube.GetComponent<Collider>());
            return cube;
        }

        private static GameObject CreateCapsule(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = name;
            capsule.transform.SetParent(parent, false);
            capsule.transform.localPosition = position;
            capsule.transform.localScale = scale;
            capsule.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(capsule.GetComponent<Collider>());
            return capsule;
        }

        private static void CreateConveyor(string name, Transform parent, Vector3 position, Vector3 scale, Material belt, Material metal)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent);
            CreateCube("Belt", root.transform, position, scale, belt);
            Vector3 railScale = new Vector3(0.16f, 0.3f, scale.z);
            CreateCube("Left Rail", root.transform, position + Vector3.left * (scale.x * 0.5f), railScale, metal);
            CreateCube("Right Rail", root.transform, position + Vector3.right * (scale.x * 0.5f), railScale, metal);
        }

        private static TextMesh CreateWorldText(string name, Transform parent, string value, Vector3 position, float size, Color color)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent);
            textObject.transform.position = position;
            textObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            TextMesh text = textObject.AddComponent<TextMesh>();
            text.text = value;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontSize = 64;
            text.characterSize = size;
            text.color = color;
            return text;
        }

        private static Transform CreatePoint(string name, Transform parent, Vector3 position)
        {
            GameObject point = new GameObject(name);
            point.transform.SetParent(parent);
            point.transform.position = position;
            return point.transform;
        }

        private static Material CreateMaterial(string name, Color color)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void Assign(Object target, string propertyName, Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignArray(Object target, string propertyName, IReadOnlyList<Transform> values)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            property.arraySize = values.Count;

            for (int index = 0; index < values.Count; index++)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string parent, string child)
        {
            string fullPath = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!scenes.Exists(item => item.path == scenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }
    }
}
#endif
