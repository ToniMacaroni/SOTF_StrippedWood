using RedLoader;
using SonsSdk;
using SonsSdk.Attributes;
using SUI;
using UnityEngine;

namespace StrippedWood;

public class StrippedWood : SonsMod, IOnGameActivatedReceiver
{
    public class MaterialDef
    {
        public Texture2D Diffuse;
        public Texture2D Normal;

        public float NormalScale = 0.3f;
        public float Occlusion = 1.0f;

        public Texture2D LoadAndConvertNormalTex(string path)
        {
            var nTex = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            nTex.LoadImage(File.ReadAllBytes(path));
            var px = nTex.GetPixels();
            for (var i = 0; i < px.Count; i++)
            {
                px[i] = new(1, px[i].g, 1, px[i].r);
            }
            nTex.SetPixels(px);
            nTex.Apply();
            return nTex;
        }
        
        public Texture2D LoadNormalTex(string path)
        {
            var nTex = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            nTex.LoadImage(File.ReadAllBytes(path));
            return nTex;
        }
        
        public void Load(string diffusePath, string normalPath)
        {
            Diffuse = AssetLoaders.LoadTexture(diffusePath);
            // Normal = LoadAndConvertNormalTex(normalPath);
            Normal = LoadNormalTex(normalPath);

            Diffuse.hideFlags = HideFlags.HideAndDontSave;
            Normal.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    private readonly Dictionary<int, MaterialDef> _mappings = new();
    
    private static readonly int BaseColorMap = Shader.PropertyToID("_BaseColorMap");
    private static readonly int NormalMap = Shader.PropertyToID("_NormalMap");
    private static readonly int NormalScale = Shader.PropertyToID("_NormalScale");
    private static readonly int Occlusion = Shader.PropertyToID("_Occlusion");

    protected override void OnInitializeMod()
    {
        Config.Init();
    }

    protected override void OnSdkInitialized()
    {
        SettingsRegistry.CreateSettings(this, null, typeof(Config));

        var dataPath = DataPath;

        var beamLogDef = new MaterialDef();
        beamLogDef.Load(dataPath / "beamLog_Diffuse.png", dataPath / "beamLog_Normal.png");
        
        var pillarLogDef = new MaterialDef();
        pillarLogDef.Diffuse = beamLogDef.Diffuse;
        pillarLogDef.Normal = beamLogDef.Normal;
        pillarLogDef.Occlusion = 0.5f;
        
        var quarterLogDef = new MaterialDef();
        quarterLogDef.Load(dataPath / "quarterLog_Diffuse.png", dataPath / "quarterLog_Normal.png");
        quarterLogDef.Occlusion = 0.5f;
        
        // File.WriteAllBytes("beamLog_Normal.png", beamLogDef.Normal.EncodeToPNG());
        // File.WriteAllBytes("quarterLog_Normal.png", quarterLogDef.Normal.EncodeToPNG());

        _mappings[1] = beamLogDef;
        _mappings[2] = pillarLogDef;
        _mappings[40] = quarterLogDef;
        _mappings[41] = quarterLogDef;
        _mappings[42] = quarterLogDef;
    }

    public void OnGameActivated()
    {
        foreach (var (id, def) in _mappings)
        {
            foreach (var comp in ConstructionTools.GetProfile(id)._prefab.GetComponentsInChildren<MeshRenderer>())
            {
                comp.sharedMaterial.SetTexture(BaseColorMap, def.Diffuse);
                comp.sharedMaterial.SetTexture(NormalMap, def.Normal);
                comp.sharedMaterial.SetFloat(NormalScale, def.NormalScale);
                comp.sharedMaterial.SetFloat(Occlusion, def.Occlusion);
            }
        }
    }
}