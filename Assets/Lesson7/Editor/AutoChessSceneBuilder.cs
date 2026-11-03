using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AlgoLesson7.Editor
{
    [InitializeOnLoad]
    public static class AutoChessSceneBuilder
    {
        private const string ScenePath = "Assets/Lesson7/Scenes/Lesson07_AutoChessSynergyArena.unity";
        private const string HexMeshPath = "Assets/Lesson7/Generated/HexTile.asset";

        static AutoChessSceneBuilder()
        {
            EditorApplication.delayCall += CreateSceneIfMissingOrOutdated;
        }

        [MenuItem("AlgoLab/Lesson 7/Rebuild AutoChess Synergy Arena")]
        public static void RebuildScene()
        {
            BuildScene(true);
        }

        private static void CreateSceneIfMissingOrOutdated()
        {
            bool hasHexCoordinates = File.Exists(ScenePath) &&
                                     File.ReadAllText(ScenePath).Contains("row: 7");

            if (!hasHexCoordinates)
                BuildScene(false);
        }

        private static void BuildScene(bool openAfterBuild)
        {
            Directory.CreateDirectory("Assets/Lesson7/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject environment = CreateRoot("01_ENVIRONMENT_Arena");
            CreateFloor(environment.transform);

            GameObject gameplay = CreateRoot("02_GAMEPLAY_BoardCells");
            CreateBoard(gameplay.transform, GetOrCreateHexMesh());

            CreateRoot("03_INTERACTION_Placement");
            CreateCamera();
            CreateLighting();
            CreateRoot("05_AUDIO");
            CreateRoot("06_UI_RuntimeOverlay");

            GameObject manager = CreateRoot("07_GAME_MANAGER");
            manager.AddComponent<AutoChessArenaGame>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (openAfterBuild)
                EditorSceneManager.OpenScene(ScenePath);

            Debug.Log("Lesson 7 scene created: " + ScenePath);
        }

        private static void CreateFloor(Transform parent)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Arena_Floor";
            floor.transform.SetParent(parent);
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(1.35f, 1f, 1.55f);
            floor.GetComponent<Renderer>().material = CreateMaterial(new Color(0.055f, 0.09f, 0.11f));

            GameObject divider = GameObject.CreatePrimitive(PrimitiveType.Cube);
            divider.name = "Center_Divider";
            divider.transform.SetParent(parent);
            divider.transform.position = new Vector3(0f, 0.08f, 0f);
            divider.transform.localScale = new Vector3(12f, 0.08f, 0.1f);
            divider.GetComponent<Renderer>().material = CreateMaterial(new Color(1f, 0.72f, 0.18f));
            Object.DestroyImmediate(divider.GetComponent<Collider>());
        }

        private static void CreateBoard(Transform parent, Mesh hexMesh)
        {
            GameObject playerRoot = new GameObject("Player_Board_Blue");
            playerRoot.transform.SetParent(parent);

            GameObject enemyRoot = new GameObject("Enemy_Board_Red");
            enemyRoot.transform.SetParent(parent);

            const int columns = 7;
            const int rows = 8;
            const float columnSpacing = 1.66f;
            const float rowSpacing = 1.46f;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    int team = row < 4 ? 0 : 1;
                    int teamRow = team == 0 ? row : row - 4;
                    int index = teamRow * columns + column;
                    float rowOffset = row % 2 == 0 ? 0f : columnSpacing * 0.5f;
                    float x = (column - 3) * columnSpacing + rowOffset;
                    float z = (row - 3.5f) * rowSpacing;
                    Transform root = team == 0 ? playerRoot.transform : enemyRoot.transform;

                    CreateCell(root, hexMesh, index, team, row, column, new Vector3(x, 0.12f, z));
                }
            }
        }

        private static void CreateCell(
            Transform parent,
            Mesh hexMesh,
            int index,
            int team,
            int row,
            int column,
            Vector3 position)
        {
            GameObject cellObject = new GameObject();
            cellObject.transform.SetParent(parent);
            cellObject.transform.position = position;

            MeshFilter meshFilter = cellObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = hexMesh;

            MeshRenderer meshRenderer = cellObject.AddComponent<MeshRenderer>();
            meshRenderer.material = CreateMaterial(
                team == 0
                    ? new Color(0.06f, 0.34f, 0.68f)
                    : new Color(0.62f, 0.1f, 0.16f));

            MeshCollider meshCollider = cellObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = hexMesh;

            AutoChessBoardCell cell = cellObject.AddComponent<AutoChessBoardCell>();
            cell.Initialize(index, team, row, column);
        }

        private static Mesh GetOrCreateHexMesh()
        {
            Directory.CreateDirectory("Assets/Lesson7/Generated");
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(HexMeshPath);

            if (existing != null)
                return existing;

            const int sides = 6;
            const float radius = 0.93f;
            const float halfHeight = 0.1f;
            Vector3[] vertices = new Vector3[sides * 2 + 2];
            int topCenter = sides * 2;
            int bottomCenter = topCenter + 1;

            for (int i = 0; i < sides; i++)
            {
                float angle = Mathf.Deg2Rad * (60f * i + 30f);
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                vertices[i] = new Vector3(x, halfHeight, z);
                vertices[i + sides] = new Vector3(x, -halfHeight, z);
            }

            vertices[topCenter] = new Vector3(0f, halfHeight, 0f);
            vertices[bottomCenter] = new Vector3(0f, -halfHeight, 0f);

            int[] triangles = new int[sides * 12];
            int triangleIndex = 0;

            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;

                triangles[triangleIndex++] = topCenter;
                triangles[triangleIndex++] = next;
                triangles[triangleIndex++] = i;

                triangles[triangleIndex++] = bottomCenter;
                triangles[triangleIndex++] = i + sides;
                triangles[triangleIndex++] = next + sides;

                triangles[triangleIndex++] = i;
                triangles[triangleIndex++] = next;
                triangles[triangleIndex++] = next + sides;

                triangles[triangleIndex++] = i;
                triangles[triangleIndex++] = next + sides;
                triangles[triangleIndex++] = i + sides;
            }

            Mesh mesh = new Mesh
            {
                name = "Lesson7_HexTile",
                vertices = vertices,
                triangles = triangles
            };

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, HexMeshPath);
            AssetDatabase.SaveAssets();
            return mesh;
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = CreateRoot("04_CAMERA_Main");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 16.2f, -14.8f);
            cameraObject.transform.rotation = Quaternion.Euler(48f, 0f, 0f);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.018f, 0.028f, 0.04f);
            camera.fieldOfView = 55f;
        }

        private static void CreateLighting()
        {
            GameObject lightingRoot = CreateRoot("04_LIGHTING");

            GameObject keyLightObject = new GameObject("Key_DirectionalLight");
            keyLightObject.transform.SetParent(lightingRoot.transform);
            keyLightObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
            Light keyLight = keyLightObject.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.intensity = 1.35f;
            keyLight.color = new Color(0.82f, 0.9f, 1f);

            GameObject fillLightObject = new GameObject("Arena_PointLight");
            fillLightObject.transform.SetParent(lightingRoot.transform);
            fillLightObject.transform.position = new Vector3(0f, 5f, 0f);
            Light fillLight = fillLightObject.AddComponent<Light>();
            fillLight.type = LightType.Point;
            fillLight.range = 18f;
            fillLight.intensity = 10f;
            fillLight.color = new Color(1f, 0.55f, 0.25f);
        }

        private static GameObject CreateRoot(string name)
        {
            return new GameObject(name);
        }

        private static Material CreateMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");

            if (shader == null)
                shader = Shader.Find("Standard");

            Material material = new Material(shader);
            material.color = color;
            return material;
        }
    }
}
