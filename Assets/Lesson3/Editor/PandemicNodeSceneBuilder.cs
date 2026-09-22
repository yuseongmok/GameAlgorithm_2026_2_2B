#if UNITY_EDITOR
using System.Collections.Generic;
using AlgoCourse.Lesson3;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AlgoCourse.Lesson3.Editor
{
    [InitializeOnLoad]
    internal static class PandemicNodeSceneAutoBuilder
    {
        static PandemicNodeSceneAutoBuilder()
        {
            EditorApplication.delayCall += BuildOnce;
        }

        private static void BuildOnce()
        {
            const string key = "AlgoCourse.Lesson3.PandemicNodeSceneBuilt.V1";
            if (SessionState.GetBool(key, false)) return;
            SessionState.SetBool(key, true);
            PandemicNodeSceneBuilder.BuildScene();
        }
    }

    public static class PandemicNodeSceneBuilder
    {
        private const string Root = "Assets/Lesson3";
        private const string ScenePath = Root + "/Scenes/Lesson03_PandemicNodes.unity";
        private const string Materials = Root + "/Materials";

        private static readonly string[] Names =
        {
            "SEOUL", "TOKYO", "BEIJING", "BANGKOK", "DELHI", "DUBAI",
            "CAIRO", "PARIS", "LONDON", "NEW YORK", "MEXICO", "SAO PAULO"
        };

        private static readonly Vector3[] Positions =
        {
            new Vector3(4.7f,0f,2.8f), new Vector3(6.8f,0f,2.0f), new Vector3(3.4f,0f,4.2f),
            new Vector3(3.8f,0f,0.1f), new Vector3(1.1f,0f,1.0f), new Vector3(-1.0f,0f,1.5f),
            new Vector3(-2.8f,0f,2.1f), new Vector3(-4.6f,0f,3.5f), new Vector3(-6.4f,0f,4.4f),
            new Vector3(-5.8f,0f,0.2f), new Vector3(-4.0f,0f,-2.2f), new Vector3(-0.8f,0f,-3.5f)
        };

        private static readonly int[][] Neighbors =
        {
            new[]{1,2,3}, new[]{0,2,3}, new[]{0,1,4}, new[]{0,1,4,11},
            new[]{2,3,5}, new[]{4,6,11}, new[]{5,7,11}, new[]{6,8,9},
            new[]{7,9}, new[]{7,8,10}, new[]{9,11}, new[]{3,5,6,10}
        };

        [MenuItem("AlgoLab/Lesson 3/Rebuild Pandemic Node Scene")]
        public static void BuildScene()
        {
            EnsureFolder("Assets", "Lesson3");
            EnsureFolder(Root, "Scenes");
            EnsureFolder(Root, "Materials");
            Material board = MakeMaterial("Pandemic_Board", new Color(0.035f,0.08f,0.12f));
            Material node = MakeMaterial("Pandemic_Node", new Color(0.10f,0.52f,0.72f));
            Material route = MakeMaterial("Pandemic_Route", new Color(0.32f,0.62f,0.70f));
            Material cube = MakeMaterial("Pandemic_InfectionCube", new Color(0.92f,0.08f,0.06f));

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject environment = NewRoot("01_WORLD_BOARD");
            CreateCube("World Board", environment.transform, new Vector3(0f,-0.28f,0.4f), new Vector3(16f,0.4f,10.5f), board);

            GameObject routesRoot = NewRoot("02_CITY_ROUTES");
            HashSet<string> madeRoutes = new HashSet<string>();
            for (int city = 0; city < Neighbors.Length; city++)
            {
                foreach (int neighbor in Neighbors[city])
                {
                    string key = city < neighbor ? $"{city}-{neighbor}" : $"{neighbor}-{city}";
                    if (madeRoutes.Add(key)) CreateRoute(routesRoot.transform, Positions[city], Positions[neighbor], route);
                }
            }

            GameObject citiesRoot = NewRoot("03_CITY_NODES");
            List<PandemicCityNode> cities = new List<PandemicCityNode>();
            for (int id = 0; id < Names.Length; id++)
            {
                GameObject cityObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cityObject.name = $"City_{id:00}_{Names[id]}";
                cityObject.transform.SetParent(citiesRoot.transform);
                cityObject.transform.position = Positions[id] + Vector3.up * 0.12f;
                cityObject.transform.localScale = new Vector3(0.72f,0.16f,0.72f);
                Renderer renderer = cityObject.GetComponent<Renderer>();
                renderer.sharedMaterial = node;
                PandemicCityNode cityNode = cityObject.AddComponent<PandemicCityNode>();

                GameObject[] cubes = new GameObject[3];
                for (int level = 0; level < cubes.Length; level++)
                {
                    cubes[level] = CreateCube($"Infection Cube {level + 1}", cityObject.transform,
                        Positions[id] + new Vector3(-0.28f + level * 0.28f, 0.52f, 0f),
                        new Vector3(0.22f,0.22f,0.22f), cube);
                    Object.DestroyImmediate(cubes[level].GetComponent<Collider>());
                }

                CreateLabel(cityObject.transform, Names[id], Positions[id] + new Vector3(0f,0.72f,0f));
                cityNode.Configure(id, Names[id], Neighbors[id], cubes, renderer);
                cities.Add(cityNode);
            }

            GameObject managerRoot = NewRoot("04_GAME_MANAGER");
            PandemicNodeGame game = new GameObject("Pandemic Node Game").AddComponent<PandemicNodeGame>();
            game.transform.SetParent(managerRoot.transform);
            game.gameObject.AddComponent<PandemicNodeUI>();

            GameObject cameraRoot = NewRoot("05_CAMERA_AND_LIGHT");
            GameObject cameraObject = new GameObject("Board Camera");
            cameraObject.transform.SetParent(cameraRoot.transform);
            cameraObject.transform.position = new Vector3(0f,13.5f,-9.8f);
            cameraObject.transform.rotation = Quaternion.Euler(52f,0f,0f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera"; camera.fieldOfView = 48f;
            cameraObject.AddComponent<AudioListener>();
            GameObject lightObject = new GameObject("Directional Light");
            lightObject.transform.SetParent(cameraRoot.transform);
            lightObject.transform.rotation = Quaternion.Euler(55f,-35f,0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional; light.intensity = 1.4f; light.shadows = LightShadows.Soft;

            Assign(game,"worldCamera",camera);
            AssignArray(game,"cities",cities);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        }

        private static void CreateRoute(Transform parent, Vector3 from, Vector3 to, Material material)
        {
            Vector3 midpoint = (from + to) * 0.5f + Vector3.up * 0.02f;
            float length = Vector3.Distance(from,to);
            GameObject routeObject = CreateCube("Route",parent,midpoint,new Vector3(0.09f,0.06f,length),material);
            routeObject.transform.rotation = Quaternion.LookRotation(to - from);
            Object.DestroyImmediate(routeObject.GetComponent<Collider>());
        }

        private static void CreateLabel(Transform parent, string value, Vector3 position)
        {
            GameObject labelObject = new GameObject("City Label");
            labelObject.transform.SetParent(parent);
            labelObject.transform.position = position;
            labelObject.transform.rotation = Quaternion.Euler(65f,0f,0f);
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text = value; label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center;
            label.fontSize = 64; label.characterSize = 0.075f; label.color = Color.white;
        }

        private static GameObject NewRoot(string name) => new GameObject(name);
        private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject result = GameObject.CreatePrimitive(PrimitiveType.Cube);
            result.name=name; result.transform.SetParent(parent); result.transform.position=position; result.transform.localScale=scale;
            result.GetComponent<Renderer>().sharedMaterial=material; return result;
        }

        private static Material MakeMaterial(string name, Color color)
        {
            string path=$"{Materials}/{name}.mat"; Material result=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(result==null){Shader shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard"); result=new Material(shader); AssetDatabase.CreateAsset(result,path);}
            result.color=color; EditorUtility.SetDirty(result); return result;
        }

        private static void Assign(Object target,string field,Object value){SerializedObject so=new SerializedObject(target);so.FindProperty(field).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
        private static void AssignArray(Object target,string field,IReadOnlyList<PandemicCityNode> values){SerializedObject so=new SerializedObject(target);SerializedProperty p=so.FindProperty(field);p.arraySize=values.Count;for(int i=0;i<values.Count;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=values[i];so.ApplyModifiedPropertiesWithoutUndo();}
        private static void EnsureFolder(string parent,string child){string path=$"{parent}/{child}";if(!AssetDatabase.IsValidFolder(path))AssetDatabase.CreateFolder(parent,child);}
        private static void AddToBuildSettings(string scenePath){List<EditorBuildSettingsScene> scenes=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);if(scenes.TrueForAll(item=>item.path!=scenePath)){scenes.Add(new EditorBuildSettingsScene(scenePath,true));EditorBuildSettings.scenes=scenes.ToArray();}}
    }
}
#endif
