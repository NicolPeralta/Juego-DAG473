using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class AccuracyValidation
{
    const string Folder = "C:/Users/donen/Documents/Codex/2026-10-29/j/work/aim-fix/";
    const string Key = "DAG.AccuracyValidation";
    static int index;
    static double next;
    static GameObject wall, owner, cameraObject;
    static Camera camera;
    static CannonAimer aimer;
    static CodigoJugador shooter;
    static GameObject[] prefabs;
    static readonly Vector2[] Positions = {
        new Vector2(.2f,.2f), new Vector2(.5f,.2f), new Vector2(.8f,.2f),
        new Vector2(.2f,.5f), new Vector2(.5f,.5f), new Vector2(.8f,.5f),
        new Vector2(.2f,.8f), new Vector2(.5f,.8f), new Vector2(.8f,.8f) };
    static AccuracyValidation()
    {
        EditorApplication.delayCall += Begin;
        if(SessionState.GetBool(Key,false)) EditorApplication.playModeStateChanged += StateChanged;
    }
    static void Begin()
    {
        if(!File.Exists(Folder+"run.request") || EditorApplication.isPlayingOrWillChangePlaymode) return;
        File.Delete(Folder+"run.request");
        SessionState.SetString(Key+".start",AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
        EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/GameScene.unity");
        SessionState.SetBool(Key,true);
        EditorApplication.playModeStateChanged += StateChanged;
        EditorApplication.isPlaying=true;
    }
    static void StateChanged(PlayModeStateChange state)
    {
        if(state==PlayModeStateChange.EnteredPlayMode)
        {
            index=0; next=EditorApplication.timeSinceStartup+1;
            EditorApplication.update+=Tick;
        }
        if(state==PlayModeStateChange.EnteredEditMode)
        {
            EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(SessionState.GetString(Key+".start",""));
            SessionState.SetBool(Key,false);
        }
    }
    static void Tick()
    {
        if(EditorApplication.timeSinceStartup<next) return;
        next=EditorApplication.timeSinceStartup+.08;
        try
        {
            if(index==0) Setup();
            if(index<27) RunShot(index++);
            else
            {
                if(!AudioManager.Instance.musicSource.isPlaying) throw new Exception("Music stopped");
                File.AppendAllText(Folder+"validation.txt","ALL_27_ACCURACY_CHECKS_PASSED\n");
                Finish();
            }
        }
        catch(Exception ex)
        {
            File.AppendAllText(Folder+"validation.txt","FAIL "+ex+"\n");
            Debug.LogException(ex); Finish();
        }
    }
    static void Setup()
    {
        wall=new GameObject("Accuracy target"); wall.tag="Cubo";
        wall.transform.position=new Vector3(0,30,100);
        var collider=wall.AddComponent<BoxCollider>(); collider.size=new Vector3(40,30,.1f);
        cameraObject=new GameObject("Accuracy camera");
        camera=cameraObject.AddComponent<Camera>(); camera.enabled=false;
        camera.transform.position=new Vector3(0,30,80); camera.aspect=16f/9;
        owner=new GameObject("Accuracy launcher"); owner.transform.position=new Vector3(2,27,83);
        shooter=owner.AddComponent<CodigoJugador>(); shooter.enabled=false;
        aimer=UnityEngine.Object.FindFirstObjectByType<CannonAimer>();
        prefabs=new[]{"Cubo","Sphere","Capsule"}.Select(n=>AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/"+n+".prefab")).ToArray();
        Physics.SyncTransforms();
    }
    static void RunShot(int test)
    {
        Vector2 uv=Positions[test%9];
        Vector3 pixel=camera.ViewportToScreenPoint(new Vector3(uv.x,uv.y,1));
        Ray ray=camera.ScreenPointToRay(pixel);
        Vector3 point=aimer.GetAimPoint(ray,out Collider hit);
        if(hit==null || hit.gameObject!=wall) throw new Exception("Mouse ray missed test surface");
        var existing=UnityEngine.Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
        var existingShots=UnityEngine.Object.FindObjectsByType<LanzadorObjeto>(FindObjectsSortMode.None);
        shooter.prefabs=new[]{prefabs[test/9]};
        shooter.velocidadLanzamiento=(test%2==0)?18f:500f;
        shooter.velocidadMultiplicador=1;
        int score=shooter.Puntos;
        typeof(CodigoJugador).GetMethod("LanzarObjeto",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(shooter,new object[]{point,wall});
        var projectile=UnityEngine.Object.FindObjectsByType<LanzadorObjeto>(FindObjectsSortMode.None).Single(p=>p.codigoJugador==shooter && !existingShots.Contains(p));
        var body=projectile.GetComponent<Rigidbody>();
        if(!body.isKinematic || body.useGravity) throw new Exception("Gravity still affects aimed shots");
        // Advance the production fixed-step flight code, including its ray sweep.
        for(int i=0;i<300 && shooter.Puntos==score;i++)
        {
            projectile.SendMessage("FixedUpdate");
            Physics.SyncTransforms();
        }
        if(shooter.Puntos!=score+1) throw new Exception("Shot failed to hit exactly once");
        var burst=UnityEngine.Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None).Single(p=>!existing.Contains(p));
        float metres=Vector3.Distance(burst.transform.position,point);
        Vector3 projected=camera.WorldToScreenPoint(burst.transform.position);
        float pixels=Vector2.Distance(new Vector2(projected.x,projected.y),new Vector2(pixel.x,pixel.y));
        if(metres>.002f || pixels>1f) throw new Exception("Offset: "+metres+" metres / "+pixels+" pixels");
        File.AppendAllText(Folder+"validation.txt",$"PASS {prefabs[test/9].name} {uv} speed={shooter.velocidadLanzamiento}: error={metres:F6}m, {pixels:F4}px\n");
        UnityEngine.Object.Destroy(burst.gameObject);
    }
    static void Finish()
    {
        EditorApplication.update-=Tick;
        EditorApplication.isPlaying=false;
    }
}
