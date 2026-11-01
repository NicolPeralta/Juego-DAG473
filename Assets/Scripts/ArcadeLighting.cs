using UnityEngine;
using UnityEngine.Rendering;

public class ArcadeLighting : MonoBehaviour
{
    public Color ambient = new Color(.4f, .36f, .34f);
    void Awake()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = ambient;
        var probe = new SphericalHarmonicsL2(); probe.AddAmbientLight(ambient);
        RenderSettings.ambientProbe = probe;
    }
}
