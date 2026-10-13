#if UNITY_EDITOR
using System.Collections.Generic;
using AlgoCourse.Lesson5;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AlgoCourse.Lesson5.Editor
{
    [InitializeOnLoad]
    internal static class RuneVaultSceneAutoBuilder
    {
        static RuneVaultSceneAutoBuilder()
        {
            EditorApplication.delayCall += BuildOnce;
        }

        private static void BuildOnce()
        {
            const string key = "AlgoCourse.Lesson5.RuneVaultSceneBuilt.V1";
            if (SessionState.GetBool(key, false)) return;
            SessionState.SetBool(key, true);
            RuneVaultSceneBuilder.BuildScene();
        }
    }

    public static class RuneVaultSceneBuilder
    {
        private const string Root = "Assets/Lesson5";
        private const string ScenePath = Root + "/Scenes/Lesson05_RuneVault.unity";
        private const string MaterialsPath = Root + "/Materials";
        private static readonly string[] Names = { "불", "물", "바람", "땅", "빛", "어둠" };
        private static readonly int[] Values = { 4, 2, 3, 5, 6, 1 };
        private static readonly Color[] Colors =
        {
            new Color(0.95f,0.16f,0.08f), new Color(0.08f,0.45f,0.95f),
            new Color(0.25f,0.9f,0.68f), new Color(0.55f,0.32f,0.12f),
            new Color(1f,0.85f,0.18f), new Color(0.55f,0.2f,0.75f)
        };

        [MenuItem("AlgoLab/Lesson 5/Rebuild Rune Vault Scene")]
        public static void BuildScene()
        {
            EnsureFolder("Assets", "Lesson5");
            EnsureFolder(Root, "Scenes");
            EnsureFolder(Root, "Materials");
            Material stone = MakeMaterial("Vault_Stone", new Color(0.10f,0.12f,0.15f));
            Material gold = MakeMaterial("Vault_Gold", new Color(0.72f,0.48f,0.08f));
            Material slotMaterial = MakeMaterial("Rune_Slot", new Color(0.08f,0.11f,0.14f));

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject environment = NewRoot("01_ENVIRONMENT");
            CreateCube("Floor", environment.transform, new Vector3(0f,-0.35f,0f), new Vector3(16f,0.5f,11f), stone);
            CreateCube("Back Wall", environment.transform, new Vector3(0f,3.2f,4.7f), new Vector3(16f,7f,0.5f), stone);

            GameObject vaultRoot = NewRoot("02_MAGIC_VAULT");
            CreateCube("Vault Frame", vaultRoot.transform, new Vector3(0f,2.4f,4.15f), new Vector3(7.4f,5.2f,0.65f), gold);
            GameObject door = CreateCube("Vault Door", vaultRoot.transform, new Vector3(0f,2.4f,3.75f), new Vector3(5.8f,4.2f,0.55f), stone);
            CreateLabel(door.transform, "RUNE VAULT", new Vector3(0f,0f,-0.31f), 0.11f);

            GameObject slotsRoot = NewRoot("03_RUNE_SLOTS");
            List<RuneSlotView> slots = new List<RuneSlotView>();
            for (int index = 0; index < 4; index++)
            {
                GameObject slotObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                slotObject.name = $"Rune Slot {index + 1}";
                slotObject.transform.SetParent(slotsRoot.transform);
                slotObject.transform.position = new Vector3(-3f + index * 2f, 0.25f, 1.25f);
                slotObject.transform.localScale = new Vector3(0.72f,0.15f,0.72f);
                Renderer renderer = slotObject.GetComponent<Renderer>();
                renderer.sharedMaterial = slotMaterial;
                TextMesh label = CreateLabel(slotObject.transform, "?", new Vector3(0f,1.2f,0f), 0.09f);
                RuneSlotView view = slotObject.AddComponent<RuneSlotView>();
                view.Configure(renderer, label);
                slots.Add(view);
            }

            GameObject runesRoot = NewRoot("04_AVAILABLE_RUNES");
            List<RunePedestal> pedestals = new List<RunePedestal>();
            for (int index = 0; index < Names.Length; index++)
            {
                float x = -5f + (index % 3) * 5f;
                float z = -1.3f - (index / 3) * 2.1f;
                GameObject runeObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                runeObject.name = $"Rune_{index}_{Names[index]}";
                runeObject.transform.SetParent(runesRoot.transform);
                runeObject.transform.position = new Vector3(x,0.25f,z);
                runeObject.transform.localScale = new Vector3(0.85f,0.28f,0.85f);
                Renderer renderer = runeObject.GetComponent<Renderer>();
                renderer.sharedMaterial = MakeMaterial($"Rune_{index}_{Names[index]}", Colors[index]);
                RunePedestal pedestal = runeObject.AddComponent<RunePedestal>();
                pedestal.Configure(index, renderer, Colors[index]);
                CreateLabel(runeObject.transform, $"{Names[index]}  {Values[index]}", new Vector3(0f,1.1f,0f), 0.085f);
                pedestals.Add(pedestal);
            }

            GameObject managerRoot = NewRoot("05_GAME_MANAGER");
            RuneVaultGame game = new GameObject("Rune Vault Game").AddComponent<RuneVaultGame>();
            game.transform.SetParent(managerRoot.transform);
            game.gameObject.AddComponent<RuneVaultUI>();
            Assign(game,"vaultDoor",door.transform);
            AssignArray(game,"slots",slots);
            AssignArray(game,"pedestals",pedestals);

            GameObject cameraRoot = NewRoot("06_CAMERA_AND_LIGHT");
            GameObject cameraObject = new GameObject("Vault Camera");
            cameraObject.transform.SetParent(cameraRoot.transform);
            cameraObject.transform.position = new Vector3(0f,9.5f,-10.5f);
            cameraObject.transform.rotation = Quaternion.Euler(32f,0f,0f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.fieldOfView = 50f;
            camera.backgroundColor = new Color(0.02f,0.025f,0.04f);
            cameraObject.AddComponent<AudioListener>();
            GameObject lightObject = new GameObject("Moon Light");
            lightObject.transform.SetParent(cameraRoot.transform);
            lightObject.transform.rotation = Quaternion.Euler(50f,-25f,0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.4f;
            light.color = new Color(0.65f,0.75f,1f);
            light.shadows = LightShadows.Soft;

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Lesson 5 scene created: {ScenePath}");
        }

        private static TextMesh CreateLabel(Transform parent, string text, Vector3 localPosition, float size)
        {
            GameObject labelObject = new GameObject("Label");
            labelObject.transform.SetParent(parent);
            labelObject.transform.localPosition = localPosition;
            labelObject.transform.rotation = Quaternion.Euler(45f,0f,0f);
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 64;
            label.characterSize = size;
            label.color = Color.white;
            return label;
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
            string path=$"{MaterialsPath}/{name}.mat"; Material result=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(result==null){Shader shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");result=new Material(shader);AssetDatabase.CreateAsset(result,path);}
            result.color=color; EditorUtility.SetDirty(result); return result;
        }
        private static void Assign(Object target,string field,Object value){SerializedObject so=new SerializedObject(target);so.FindProperty(field).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
        private static void AssignArray<T>(Object target,string field,IReadOnlyList<T> values) where T:Object{SerializedObject so=new SerializedObject(target);SerializedProperty p=so.FindProperty(field);p.arraySize=values.Count;for(int i=0;i<values.Count;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=values[i];so.ApplyModifiedPropertiesWithoutUndo();}
        private static void EnsureFolder(string parent,string child){string path=$"{parent}/{child}";if(!AssetDatabase.IsValidFolder(path))AssetDatabase.CreateFolder(parent,child);}
        private static void AddToBuildSettings(string scenePath){List<EditorBuildSettingsScene> scenes=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);if(scenes.TrueForAll(item=>item.path!=scenePath)){scenes.Add(new EditorBuildSettingsScene(scenePath,true));EditorBuildSettings.scenes=scenes.ToArray();}}
    }
}
#endif
