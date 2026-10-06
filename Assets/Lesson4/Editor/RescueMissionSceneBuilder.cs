#if UNITY_EDITOR
using System.Collections.Generic;
using AlgoCourse.Lesson4;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AlgoCourse.Lesson4.Editor
{
    [InitializeOnLoad]
    internal static class RescueMissionSceneAutoBuilder
    {
        static RescueMissionSceneAutoBuilder()
        {
            EditorApplication.delayCall += BuildOnce;
        }

        private static void BuildOnce()
        {
            const string key = "AlgoCourse.Lesson4.RescueMissionBuilt.V1";
            if (SessionState.GetBool(key, false)) return;
            SessionState.SetBool(key, true);
            RescueMissionSceneBuilder.BuildScene();
        }
    }

    public static class RescueMissionSceneBuilder
    {
        private const int Width = 12;
        private const int Height = 8;
        private const string Root = "Assets/Lesson4";
        private const string ScenePath = Root + "/Scenes/Lesson04_BurningLabRescue.unity";
        private const string MaterialsPath = Root + "/Materials";

        private static readonly HashSet<GridPosition> Walls = new HashSet<GridPosition>
        {
            new GridPosition(2, 0), new GridPosition(2, 1), new GridPosition(2, 2),
            new GridPosition(4, 2), new GridPosition(5, 2), new GridPosition(6, 2),
            new GridPosition(8, 0), new GridPosition(8, 1), new GridPosition(8, 2),
            new GridPosition(1, 5), new GridPosition(2, 5), new GridPosition(3, 5),
            new GridPosition(5, 4), new GridPosition(5, 5), new GridPosition(5, 6),
            new GridPosition(8, 5), new GridPosition(9, 5), new GridPosition(10, 5),
            new GridPosition(10, 2), new GridPosition(10, 3)
        };

        private static readonly HashSet<GridPosition> InitialFires = new HashSet<GridPosition>
        {
            new GridPosition(3, 3), new GridPosition(7, 4), new GridPosition(9, 1)
        };

        [MenuItem("AlgoLab/Lesson 4/Rebuild Burning Lab Rescue Scene")]
        public static void BuildScene()
        {
            EnsureFolder("Assets", "Lesson4");
            EnsureFolder(Root, "Scenes");
            EnsureFolder(Root, "Materials");

            Material floorMaterial = MakeMaterial("Lab_Floor", new Color(0.18f, 0.23f, 0.27f));
            Material firefighterMaterial = MakeMaterial("Firefighter", new Color(0.96f, 0.62f, 0.05f));
            Material survivorMaterial = MakeMaterial("Survivor", new Color(0.72f, 0.24f, 0.88f));
            Material entranceMaterial = MakeMaterial("Entrance", new Color(0.05f, 0.82f, 0.74f));

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateEnvironment(floorMaterial, entranceMaterial);

            GameObject gridRoot = NewRoot("02_LAB_GRID");
            List<RescueTile> tiles = new List<RescueTile>();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    GridPosition position = new GridPosition(x, y);
                    GameObject tileObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tileObject.name = $"Tile_{x:00}_{y:00}";
                    tileObject.transform.SetParent(gridRoot.transform);
                    tileObject.transform.position = WorldPosition(position, 0f);
                    Renderer renderer = tileObject.GetComponent<Renderer>();
                    renderer.sharedMaterial = floorMaterial;
                    RescueTile tile = tileObject.AddComponent<RescueTile>();
                    tile.Configure(x, y, renderer);
                    if (Walls.Contains(position)) tile.SetWall();
                    if (InitialFires.Contains(position)) tile.SetFire();
                    tile.CaptureInitialState();
                    tiles.Add(tile);
                }
            }

            GameObject actorsRoot = NewRoot("03_RESCUE_ACTORS");
            FirefighterAgent firefighter = CreateFirefighter(actorsRoot.transform, firefighterMaterial);
            List<RescueSurvivor> survivors = new List<RescueSurvivor>
            {
                CreateSurvivor(actorsRoot.transform, "연구원", 100, new GridPosition(4, 1), survivorMaterial),
                CreateSurvivor(actorsRoot.transform, "부상자", 200, new GridPosition(7, 6), survivorMaterial),
                CreateSurvivor(actorsRoot.transform, "핵심 기술자", 300, new GridPosition(11, 7), survivorMaterial)
            };

            GameObject managerRoot = NewRoot("04_GAME_MANAGER");
            RescueMissionGame game = new GameObject("Rescue Mission Game").AddComponent<RescueMissionGame>();
            game.transform.SetParent(managerRoot.transform);
            game.gameObject.AddComponent<RescueMissionUI>();

            GameObject cameraRoot = NewRoot("05_CAMERA_AND_LIGHT");
            Camera camera = CreateCamera(cameraRoot.transform);
            CreateLights(cameraRoot.transform);

            Assign(game, "firefighter", firefighter);
            Assign(game, "worldCamera", camera);
            AssignArray(game, "tiles", tiles);
            AssignArray(game, "survivors", survivors);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Lesson 4 scene created: {ScenePath}");
        }

        private static void CreateEnvironment(Material floor, Material entrance)
        {
            GameObject environment = NewRoot("01_ENVIRONMENT");
            CreateCube("Lab Foundation", environment.transform, new Vector3(0f, -0.4f, 0f),
                new Vector3(14f, 0.5f, 10f), floor);
            CreateCube("Safe Zone", environment.transform, WorldPosition(new GridPosition(0, 0), 0.12f),
                new Vector3(1.2f, 0.08f, 1.2f), entrance);
        }

        private static FirefighterAgent CreateFirefighter(Transform parent, Material material)
        {
            GameObject actor = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            actor.name = "Firefighter";
            actor.transform.SetParent(parent);
            actor.transform.position = WorldPosition(new GridPosition(0, 0), 0.75f);
            actor.transform.localScale = new Vector3(0.55f, 0.75f, 0.55f);
            actor.GetComponent<Renderer>().sharedMaterial = material;
            return actor.AddComponent<FirefighterAgent>();
        }

        private static RescueSurvivor CreateSurvivor(
            Transform parent, string displayName, int score, GridPosition position, Material material)
        {
            GameObject actor = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            actor.name = $"Survivor_{displayName}";
            actor.transform.SetParent(parent);
            actor.transform.position = WorldPosition(position, 0.7f);
            actor.transform.localScale = new Vector3(0.48f, 0.65f, 0.48f);
            Renderer renderer = actor.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            RescueSurvivor survivor = actor.AddComponent<RescueSurvivor>();
            survivor.Configure(displayName, score, position, renderer);
            CreateLabel(actor.transform, $"{displayName}\n{score}점");
            return survivor;
        }

        private static Camera CreateCamera(Transform parent)
        {
            GameObject cameraObject = new GameObject("Mission Camera");
            cameraObject.transform.SetParent(parent);
            cameraObject.transform.position = new Vector3(0f, 12.5f, -10.5f);
            cameraObject.transform.rotation = Quaternion.Euler(48f, 0f, 0f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.fieldOfView = 48f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.035f, 0.05f);
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        private static void CreateLights(Transform parent)
        {
            GameObject keyObject = new GameObject("Directional Light");
            keyObject.transform.SetParent(parent);
            keyObject.transform.rotation = Quaternion.Euler(55f, -35f, 0f);
            Light key = keyObject.AddComponent<Light>();
            key.type = LightType.Directional;
            key.intensity = 1.25f;
            key.shadows = LightShadows.Soft;

            GameObject alarmObject = new GameObject("Emergency Red Light");
            alarmObject.transform.SetParent(parent);
            alarmObject.transform.position = new Vector3(0f, 6f, 1f);
            Light alarm = alarmObject.AddComponent<Light>();
            alarm.type = LightType.Point;
            alarm.color = new Color(1f, 0.12f, 0.05f);
            alarm.range = 16f;
            alarm.intensity = 3f;
        }

        private static void CreateLabel(Transform parent, string value)
        {
            GameObject labelObject = new GameObject("World Label");
            labelObject.transform.SetParent(parent);
            labelObject.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            labelObject.transform.rotation = Quaternion.Euler(48f, 0f, 0f);
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text = value;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 48;
            label.characterSize = 0.06f;
            label.color = Color.white;
        }

        private static Vector3 WorldPosition(GridPosition position, float y)
        {
            return new Vector3(position.X - (Width - 1) * 0.5f, y, position.Y - (Height - 1) * 0.5f);
        }

        private static GameObject NewRoot(string name) => new GameObject(name);

        private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject result = GameObject.CreatePrimitive(PrimitiveType.Cube);
            result.name = name;
            result.transform.SetParent(parent);
            result.transform.position = position;
            result.transform.localScale = scale;
            result.GetComponent<Renderer>().sharedMaterial = material;
            return result;
        }

        private static Material MakeMaterial(string name, Color color)
        {
            string path = $"{MaterialsPath}/{name}.mat";
            Material result = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (result == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                result = new Material(shader);
                AssetDatabase.CreateAsset(result, path);
            }
            result.color = color;
            EditorUtility.SetDirty(result);
            return result;
        }

        private static void Assign(Object target, string field, Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignArray<T>(Object target, string field, IReadOnlyList<T> values) where T : Object
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field);
            property.arraySize = values.Count;
            for (int index = 0; index < values.Count; index++)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }

        private static void AddToBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (scenes.TrueForAll(item => item.path != scenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }
    }
}
#endif
