using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class ForkliftSceneBuilder
{
    const string Scene="Assets/Lesson1/Scenes/Lesson01_ForkliftQueue.unity",Stamp="Assets/Lesson1/.forklift_v2";
    static ForkliftSceneBuilder(){EditorApplication.delayCall+=BuildWhenReady;}
    static void BuildWhenReady(){if(File.Exists(Stamp))return;if(EditorApplication.isPlayingOrWillChangePlaymode){EditorApplication.isPlaying=false;EditorApplication.delayCall+=BuildWhenReady;return;}Build();}
    [MenuItem("AlgoLab/Lesson 1/Rebuild Forklift Scene")]
    public static void Build()
    {
        AssetDatabase.DeleteAsset(Scene);
        AssetDatabase.DeleteAsset("Assets/Lesson1/Scenes/Lesson01_StackQueueWarehouse.unity");
        Directory.CreateDirectory("Assets/Lesson1/Scenes");Directory.CreateDirectory("Assets/Lesson1/Materials");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        Material floor=Mat("Floor",new Color(.12f,.15f,.18f)),metal=Mat("Metal",new Color(.34f,.38f,.42f)),yellow=Mat("Forklift",new Color(.95f,.58f,.08f)),dark=Mat("Dark",new Color(.06f,.07f,.08f));
        Material[] boxMats={Mat("Box_A",new Color(.1f,.5f,.95f)),Mat("Box_B",new Color(.95f,.3f,.12f)),Mat("Box_C",new Color(.15f,.7f,.3f))};
        GameObject env=Root("01_ENVIRONMENT");Cube(env,"Warehouse Floor",new Vector3(0,-.25f,.5f),new Vector3(18,.5f,13),floor);Light light=Child(env,"Directional Light").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.4f;light.transform.rotation=Quaternion.Euler(50,-30,0);
        GameObject inRoot=Root("02_INCOMING_QUEUE");Transform[] incoming=Belt(inRoot,-7,-2.8f,"INCOMING QUEUE",metal);
        GameObject stackRoot=Root("03_STACK_STORAGE");Transform[] stacks=new Transform[3];for(int i=0;i<3;i++){float x=-3+i*3;Cube(stackRoot,"Stack Pad "+(i+1),new Vector3(x,.05f,2),new Vector3(2.2f,.2f,2.2f),metal);stacks[i]=Point(stackRoot,"Stack "+(i+1)+" Point",new Vector3(x,.28f,2));Sign(stackRoot,"STACK "+(i+1),new Vector3(x,.18f,3.2f));}
        GameObject outRoot=Root("04_OUTGOING_QUEUE");Transform[] outgoing=Belt(outRoot,3.8f,-2.8f,"OUTGOING QUEUE",metal);
        GameObject truck=Root("05_FORKLIFT_PLAYER");truck.transform.position=new Vector3(0,.05f,-.2f);ForkliftController controller=truck.AddComponent<ForkliftController>();Cube(truck,"Body",new Vector3(0,.6f,0),new Vector3(1.2f,1.1f,1.55f),yellow);Cube(truck,"Cab",new Vector3(0,1.45f,.15f),new Vector3(1.05f,.8f,1),dark);for(int i=0;i<4;i++)Wheel(truck,new Vector3(i<2?-.68f:.68f,.35f,i%2==0?-.5f:.55f),dark);Cube(truck,"Fork Left",new Vector3(-.32f,.28f,1.18f),new Vector3(.16f,.12f,1.25f),metal);Cube(truck,"Fork Right",new Vector3(.32f,.28f,1.18f),new Vector3(.16f,.12f,1.25f),metal);controller.holdPoint=Point(truck,"Box Hold Point",new Vector3(0,.62f,1.35f));
        Camera cam=Child(env,"Follow Camera").AddComponent<Camera>();cam.tag="MainCamera";cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.04f,.06f,.09f);cam.transform.position=new Vector3(0,6,-8);ForkliftCamera follow=cam.gameObject.AddComponent<ForkliftCamera>();follow.target=truck.transform;
        GameObject prefab=Cube(Root("06_BOX_TEMPLATE"),"Box Prefab",new Vector3(0,-10,0),Vector3.one*.62f,boxMats[0]);prefab.AddComponent<ForkliftBox>();prefab.SetActive(false);
        ForkliftWarehouseGame game=Root("07_GAME_MANAGER").AddComponent<ForkliftWarehouseGame>();game.forklift=controller;game.boxPrefab=prefab;game.incomingSlots=incoming;game.outgoingSlots=outgoing;game.stackPoints=stacks;game.boxMaterials=boxMats;controller.game=game;
        Sign(env,"WASD / ARROW KEYS : DRIVE     E : PICK UP / DROP",new Vector3(0,.12f,5.5f),.1f);
        EditorSceneManager.SaveScene(scene,Scene);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Scene,true)};File.WriteAllText(Stamp,"Unity 6 forklift queue v1");AssetDatabase.Refresh();
    }
    static Transform[] Belt(GameObject root,float x,float z,string label,Material mat){Cube(root,label+" Belt",new Vector3(x+1.8f,.05f,z),new Vector3(4.6f,.2f,1.4f),mat);Transform[] slots=new Transform[6];for(int i=0;i<6;i++){slots[i]=Point(root,"Queue Slot "+i,new Vector3(x+i*.72f,.55f,z));Cube(root,"Roller "+i,new Vector3(x+i*.72f,.18f,z),new Vector3(.12f,.12f,1.2f),mat);}Sign(root,label,new Vector3(x+1.8f,.15f,z-1));return slots;}
    static GameObject Root(string n){return new GameObject(n);}static GameObject Child(GameObject p,string n){GameObject g=new GameObject(n);g.transform.SetParent(p.transform);return g;}static Transform Point(GameObject p,string n,Vector3 pos){Transform t=Child(p,n).transform;t.position=pos;return t;}
    static GameObject Cube(GameObject p,string n,Vector3 pos,Vector3 scale,Material m){GameObject g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(p.transform);g.transform.position=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=m;return g;}
    static void Wheel(GameObject p,Vector3 pos,Material m){GameObject g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name="Wheel";g.transform.SetParent(p.transform);g.transform.localPosition=pos;g.transform.localScale=new Vector3(.42f,.18f,.42f);g.transform.localRotation=Quaternion.Euler(0,0,90);g.GetComponent<Renderer>().sharedMaterial=m;}
    static Material Mat(string n,Color c){string path="Assets/Lesson1/Materials/"+n+".mat";Material m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.color=c;EditorUtility.SetDirty(m);return m;}
    static void Sign(GameObject p,string value,Vector3 pos,float size=.08f){TextMesh t=Child(p,value).AddComponent<TextMesh>();t.text=value;t.fontSize=48;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.color=Color.white;t.transform.position=pos;t.transform.rotation=Quaternion.Euler(90,0,0);}
}
