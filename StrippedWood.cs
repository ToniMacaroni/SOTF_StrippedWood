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

        // convert normal (no pun intended) textures to hdrp normal textures where r is in the alpha channel
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
    
    private static readonly int PropBaseColorMap = Shader.PropertyToID("_BaseColorMap");
    private static readonly int PropNormalMap = Shader.PropertyToID("_NormalMap");
    private static readonly int PropNormalScale = Shader.PropertyToID("_NormalScale");
    private static readonly int PropOcclusion = Shader.PropertyToID("_Occlusion");

    protected override void OnInitializeMod()
    {
        Config.Init();
    }

    protected override void OnSdkInitialized()
    {
        SettingsRegistry.CreateSettings(this, null, typeof(Config));

        // Your mod class has access to the "DataPath" property which is the <gamedir>/Mods/<yourmod>/ directory
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
        // OnGameActived is usually late enough so the scene has loaded and most systems are instantiated
        // but early enough that you can modify prefabs before they get instantiated
        
        foreach (var (id, def) in _mappings)
        {
            foreach (var comp in ConstructionTools.GetProfile(id)._prefab.GetComponentsInChildren<MeshRenderer>())
            {
                comp.sharedMaterial.SetTexture(PropBaseColorMap, def.Diffuse);
                comp.sharedMaterial.SetTexture(PropNormalMap, def.Normal);
                comp.sharedMaterial.SetFloat(PropNormalScale, def.NormalScale);
                comp.sharedMaterial.SetFloat(PropOcclusion, def.Occlusion);
            }
        }
    }
}