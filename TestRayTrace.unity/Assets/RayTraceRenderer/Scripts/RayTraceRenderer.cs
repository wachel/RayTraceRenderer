using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;

public static class RayTraceTool
{
    public static Vector2 Lerp(Vector2 t0, Vector2 t1, Vector2 t2, float u, float v)
    {
        return (1 - u - v) * t0 + u * t1 + v * t2;
    }

    public static Vector3 Lerp(Vector3 t0, Vector3 t1, Vector3 t2, float u, float v)
    {
        return (1 - u - v) * t0 + u * t1 + v * t2;
    }

    public static Color GammaToLinearSpace(Color color)
    {
        return color.linear;
    }

    public static Color LinearToGammaSpace(Color color)
    {
        return color.gamma;
    }

    public static Color LinearSample(Color[] colors, int width, int height, Vector2 uv)
    {
        int x = (int)(uv.x * width);
        int y = (int)(uv.y * height);
        float fx = uv.x * width - x;
        float fy = uv.y * height - y;
        int x0 = ((x % width) + width) % width;
        int x1 = (((x + 1) % width) + width) % width;
        int y0 = ((y % height) + height) % height;
        int y1 = (((y + 1) % height) + height) % height;
        Color c00 = colors[y0 * width + x0];
        Color c01 = colors[y1 * width + x0];
        Color c10 = colors[y0 * width + x1];
        Color c11 = colors[y1 * width + x1];
        Color c0 = Color.LerpUnclamped(c00, c01, fy);
        Color c1 = Color.LerpUnclamped(c10, c11, fy);
        return Color.LerpUnclamped(c0, c1, fx);
    }

    public static Color PointSample(Color[] colors, int width, int height, Vector2 uv)
    {
        int x = (int)(uv.x * width + 0.5f);
        int y = (int)(uv.y * height + 0.5f);
        int x0 = ((x % width) + width) % width;
        int y0 = ((y % height) + height) % height;
        return colors[y0 * width + x0];
    }

    public static float BilinearInterpolation(float[,] values,int width,int height,Vector2 uv)
    {
        int x = (int)(uv.x * width);
        int y = (int)(uv.y * height);
        float fx = uv.x * width - x;
        float fy = uv.y * height - y;
        int x0 = ((x % width) + width) % width;
        int x1 = (((x + 1) % width) + width) % width;
        int y0 = ((y % height) + height) % height;
        int y1 = (((y + 1) % height) + height) % height;
        float c00 = values[x0, y0];
        float c01 = values[x0, y1];
        float c10 = values[x1, y0];
        float c11 = values[x1, y1];
        float c0 = Mathf.LerpUnclamped(c00, c01, fy);
        float c1 = Mathf.LerpUnclamped(c10, c11, fy);
        return Mathf.LerpUnclamped(c0, c1, fx);
    }

    public static Vector3[] GetRandomDirs_RoundProj(Vector3 faceNormal, int num)
    {
        Vector3[] rlt = new Vector3[num];
        Vector3 axisTemp = Mathf.Abs(Vector3.Dot(faceNormal, Vector3.up)) > 0.9f ? Vector3.left : Vector3.up;
        Vector3 axis0 = Vector3.Cross(faceNormal, axisTemp);
        Quaternion finalRot = Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), faceNormal);
        for (int i = 0; i < num; i++) {
            float lenght0 = Mathf.Sqrt(HaltonTool.Halton2(i, 0f, 1f));
            //float lenght0 = Mathf.Sin(Mathf.PI * 0.5f * Mathf.Sqrt(HaltonTool.Halton2(i, 0f, 1f)));
            Vector3 pos0 = axis0 * lenght0;
            Vector3 pos1 = Quaternion.AngleAxis(HaltonTool.Halton3(i, 0f, 360f), faceNormal) * pos0;
            Vector3 dir = pos1 + faceNormal * (Mathf.Sqrt(1 - lenght0 * lenght0));
            //Debug.DrawLine(dir + Vector3.up * 5, dir + Vector3.up * 5 + faceNormal * 0.02f, Color.red, 10000);
            dir = finalRot * dir;
            rlt[i] = dir;
        }
        return rlt;
    }

    //尝试瞎凑的公式
    public static Vector3[] GetRandomDirs_BRDF(Vector3 faceNormal ,Vector3 reflectDir,float smoothness, int num)
    {
        Vector3[] rlt = new Vector3[num];

        //Vector3 mainOutDir = Vector3.Lerp(faceNormal, reflectDir, smoothness).normalized;
        float reflectAngle = Mathf.Acos(Vector3.Dot(reflectDir,faceNormal));//镜面反射线与法线夹角
        float mainAngle = reflectAngle * smoothness;//粗糙反射线平均方向与法线夹角
        float distributionAngle = (Mathf.PI * 0.5f * (1-smoothness));


        Vector3 axisTemp = Mathf.Abs(Vector3.Dot(faceNormal, Vector3.up)) > 0.9f ? Vector3.left : Vector3.up;
        Vector3 axis0 = Vector3.Cross(faceNormal, axisTemp);//任意垂直于faceNormal的方向
        Vector3 axis2 = -Vector3.Cross(reflectDir, faceNormal);
        if (axis2.sqrMagnitude == 0) {
            axis2 = axis0;
        }

        Quaternion finalRot = Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), faceNormal);
        for (int i = 0; i < num; i++) {
            float angle0 = Mathf.Asin(Mathf.Sqrt(HaltonTool.Halton2(i, 0f, 1f))) * distributionAngle / (Mathf.PI/2);
            Vector3 dir0 = Quaternion.AngleAxis(angle0 * Mathf.Rad2Deg, axis0) * faceNormal;
            Vector3 dir1 = Quaternion.AngleAxis(HaltonTool.Halton3(i, 0f, 360f), faceNormal) * dir0;
            Vector3 dir2 = Quaternion.AngleAxis(mainAngle * Mathf.Rad2Deg, axis2) * dir1;
            rlt[i] = dir2;
            //Debug.DrawLine(dir2 + Vector3.up * 5, dir2 + Vector3.up * 5 + dir2 * 0.02f, Color.red, 10000);
        }
        return rlt;
    }

    //http://www.klayge.org/?p=3006
    public static Vector3[] GetRandomDirs_BlinnPhong_Importance(Vector3 faceNormal, Vector3 reflectDir, float smoothness, int num)
    {
        Vector3[] rlt = new Vector3[num];

        float reflectAngle = Mathf.Acos(Vector3.Dot(reflectDir, faceNormal));//镜面反射线与法线夹角
        float mainAngle = reflectAngle * smoothness;//粗糙反射线平均方向与法线夹角

        Vector3 axisTemp = Mathf.Abs(Vector3.Dot(faceNormal, Vector3.up)) > 0.9f ? Vector3.left : Vector3.up;
        Vector3 axis0 = Vector3.Cross(faceNormal, axisTemp);//任意垂直于faceNormal的方向
        Vector3 axis2 = -Vector3.Cross(reflectDir, faceNormal);
        if (axis2.sqrMagnitude == 0) {
            axis2 = axis0;
        }

        float shininess = Mathf.Pow(8192, smoothness);

        Quaternion finalRot = Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), faceNormal);
        for (int i = 0; i < num; i++) {
            float angle0 = Mathf.Acos(Mathf.Pow(1 - HaltonTool.Halton2(i, 0f, 1f) * (shininess + 1) / (shininess + 2), 1 / (shininess + 1)));
            angle0 *= Mathf.PI * 0.5f;//看起来上面公式的结果范围是0～1，所以加上这个修正一下，不知道原因
            Vector3 dir0 = Quaternion.AngleAxis(angle0 * Mathf.Rad2Deg, axis0) * faceNormal;
            Vector3 dir1 = Quaternion.AngleAxis(HaltonTool.Halton3(i, 0f, 360f), faceNormal) * dir0;
            Vector3 dir2 = Quaternion.AngleAxis(mainAngle * Mathf.Rad2Deg, axis2) * dir1;
            rlt[i] = dir2;
            //Debug.DrawLine(dir2 + Vector3.up * 5, dir2 + Vector3.up * 5 + dir2 * 0.02f, Color.red, 10000);
        }
        return rlt;
    }

    //http://www.klayge.org/?p=3006
    public static Vector3[] GetRandomDirs_BlinnPhong_Importance2(Vector3 faceNormal, Vector3 reflectDir, float smoothness, int num)
    {
        Vector3[] rlt = new Vector3[num];

        Vector3 w = Vector3.Lerp(faceNormal,reflectDir,smoothness).normalized;
        Vector3 u = Vector3.Cross(new Vector3(w.y,w.z,w.x), w);
        Vector3 v = Vector3.Cross(w, u);

        float shininess = Mathf.Pow(8192, smoothness);
        
        for (int i = 0; i < num; i++)
        {
            float a = Mathf.Acos(Mathf.Pow(1 - HaltonTool.Halton2(i, 0f, 1f) * (shininess + 1) / (shininess + 2), 1 / (shininess + 1)));
            a *= Mathf.PI * 0.5f;//看起来上面公式的结果范围是0～1，所以加上这个修正一下，不知道原因
            float phi = HaltonTool.Halton3(i, 0f, Mathf.PI * 2);
            rlt[i] = (u * Mathf.Cos(phi) + v * Mathf.Sin(phi)) * Mathf.Sin(a) + w * Mathf.Cos(a);
            //Debug.DrawLine(rlt[i] + Vector3.up * 5, rlt[i] + Vector3.up * 5 + rlt[i] * 0.02f, Color.red, 10000);
        }
        return rlt;
    }

    public static Vector3[] GetRandomDirs(Vector3 faceNormal, int num)
    {
        Vector3[] rlt = new Vector3[num];
        Vector3 axisTemp = Mathf.Abs(Vector3.Dot(faceNormal, Vector3.up)) > 0.9f ? Vector3.left : Vector3.up;
        Vector3 axis0 = Vector3.Cross(faceNormal, axisTemp);
        for (int i = 0; i < num; i++) {
            float angle0 = Mathf.Acos(HaltonTool.Halton2(i,0f,1f)) * Mathf.Rad2Deg;
            Vector3 dir = Quaternion.AngleAxis(angle0, axis0) * faceNormal;
            Quaternion finalRot = Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), faceNormal);
            dir = finalRot * dir;
            //Debug.DrawLine(dir + Vector3.up * 5, dir + Vector3.up * 5 + dir * 0.02f, Color.red, 10000);
            rlt[i] = dir;
        }
        return rlt;
    }

    public static Texture2D[] ReadTextures(List<Texture2D> textures)
    {
        Texture2D[] result = new Texture2D[textures.Count];
        RenderTexture[] rts = new RenderTexture[textures.Count];
        for (int i = 0; i < textures.Count; i++) {
            if (textures[i] != null) {
                rts[i] = new RenderTexture(textures[i].width, textures[i].height, 0);
                RenderTexture.active = rts[i];
                Graphics.Blit(textures[i], rts[i]);
            }
        }
        for (int i = 0; i < textures.Count; i++) {
            if (rts[i] != null) {
                RenderTexture.active = rts[i];
                result[i] = new Texture2D(textures[i].width, textures[i].height);
                result[i].ReadPixels(new Rect(0, 0, textures[i].width, textures[i].height), 0, 0);
                result[i].Apply();
            }
        }
        RenderTexture.active = null;
        return result;
    }

    public static List<Light> GetActivityLights(Renderer renderer, SimpleScene scene)
    {

        List<Light> rlt = new List<Light>();
        foreach (Light light in scene.lights) {
            if (light.isActiveAndEnabled) {
                if (light.type != LightType.Directional) {
                    Bounds bounds = renderer.bounds;
                    bounds.Expand(light.range);
                    Vector3 lightPos = light.transform.position;
                    if (bounds.Contains(lightPos)) {
                        rlt.Add(light);
                    }
                }
                else {
                    rlt.Add(light);
                }
            }
        }
        return rlt;
    }
}

class Halton
{
    double value, inv_base;
    public void Number(int i, int b)
    {
        double f = inv_base = 1.0 / b;
        value = 0.0;
        while (i > 0) {
            value += f * (double)(i % b);
            i /= b;
            f *= inv_base;
        }
    }
    public void Next()
    {
        double r = 1.0 - value - 0.0000001;
        if (inv_base < r) value += inv_base;
        else {
            double h = inv_base, hh;
            do { hh = h; h *= inv_base; } while (h >= r);
            value += hh + h - 1.0;
        }
    }
    public double Get() { return value; }
};

public static class HaltonTool
{
    private static readonly int HaltonNum = 2000;
    private static float[] halton2;
    private static float[] halton3;
    private static float[] halton5;

    public static void InitHaltonSequence()
    {
        halton2 = new float[HaltonNum];
        halton3 = new float[HaltonNum];
        halton5 = new float[HaltonNum];
        {
            Halton halton = new Halton();
            halton.Number(0, 2);
            for (int i = 0; i < HaltonNum; i++) {
                halton2[i] = (float)halton.Get();
                halton.Next();
            }
        }
        {
            Halton halton = new Halton();
            halton.Number(0, 3);
            for (int i = 0; i < HaltonNum; i++) {
                halton3[i] = (float)halton.Get();
                halton.Next();
            }
        }
        {
            Halton halton = new Halton();
            halton.Number(0, 5);
            for (int i = 0; i < HaltonNum; i++) {
                halton5[i] = (float)halton.Get();
                halton.Next();
            }
        }
    }

    public static float Halton2(int index, float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
        //return halton2[index] * (max - min) + min;
    }

    public static float Halton3(int index, float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
        //return halton3[index] * (max - min) + min;
    }

    public static float Halton5(int index, float min, float max)
    {
        return halton5[index] * (max - min) + min;
    }
}

public class SimpleTexture
{
    public Color[] colors;
    public int width;
    public int height;
    public Vector2 scale = Vector2.one;
    bool bFirst = true;
    public SimpleTexture(Texture2D texture)
    {
        if (texture != null) {
            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(texture);
            var tImporter = UnityEditor.AssetImporter.GetAtPath(assetPath) as UnityEditor.TextureImporter;
            if (tImporter == null || !tImporter.isReadable) {
                RenderTexture rt = new RenderTexture(texture.width, texture.height, 0);
                RenderTexture.active = rt;
                Graphics.Blit(texture, rt);
                texture = new Texture2D(texture.width, texture.height);
                texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                texture.Apply();
                RenderTexture.active = null;

                if (bFirst) {
                    bFirst = false;
                    RayTraceRenderer.Instance.previewTexture = texture;
                }
            }
        }
        if (texture) {
            width = texture.width;
            height = texture.height;
            colors = texture.GetPixels();
            for (int i = 0; i < colors.Length; i++) {
                colors[i] = RayTraceTool.GammaToLinearSpace(colors[i]);
            }
        }

        if (colors == null || colors.Length == 0) {
            width = 1;
            height = 1;
            colors = new Color[1];
            colors[0] = Color.white;
        }
    }

    public SimpleTexture(int width, int height)
    {
        colors = new Color[width * height];
        for (int i = 0; i < colors.Length; i++) {
            colors[i] = Color.black;
        }
        this.width = width;
        this.height = height;
    }
    public void SetColor(int x, int y, Color color)
    {
        colors[y * width + x] = color;
    }
}

public abstract class SimpleMaterialBase
{
    public float glossiness;
    public float metallic;
    public enum RenderMode
    {
        Opaque,
        Alphablend,
        Multiply,
        Additive,
    }
    public abstract Color PointSample(Vector2 uv);
    public abstract Color LinearSample(Vector2 uv);
    public abstract Color GetColor();
    public abstract RenderMode GetRenderMode();
}

public class SimpleMaterial : SimpleMaterialBase
{
    public static Dictionary<Int64, SimpleTexture> textures = new Dictionary<long, SimpleTexture>();
    public SimpleTexture texture;
    public Texture2D unityTexture;
    public Color color = Color.white;
    public RenderMode renderMode;
    public SimpleMaterial(Material mat)
    {
        unityTexture = mat.mainTexture as Texture2D;

        if (mat.mainTexture != null) {
            Int64 key = mat.mainTexture.GetNativeTexturePtr().ToInt64();
            if (!textures.TryGetValue(key, out texture)) {
                texture = new SimpleTexture(mat.mainTexture as Texture2D);
                textures[key] = texture;
            }
        }
        else {
            texture = new SimpleTexture(null);
        }
        if (mat.HasProperty("_Color")) {
            this.color = RayTraceTool.GammaToLinearSpace(mat.color);
        }
        if (mat.HasProperty("_Glossiness")) {
            glossiness = mat.GetFloat("_Glossiness");
        }
        if (mat.HasProperty("_Metallic")) {
            metallic = mat.GetFloat("_Metallic");
        }
        if(mat.HasProperty("_Cutoff")){
            renderMode = RenderMode.Alphablend;
        }
        else if (mat.shader.name.ToLower().Contains("standard")) {
            if (mat.HasProperty("_Mode")) {
                if (mat.GetInt("_Mode") == 0) {
                    renderMode = RenderMode.Opaque;
                }
            }
        }
        else if (mat.shader.name.ToLower().Contains("multiply")) {
            renderMode = RenderMode.Multiply;
        }
        else if (mat.shader.name.ToLower().Contains("transparent")) {
            renderMode = RenderMode.Alphablend;
        }
    }

    public override Color PointSample(Vector2 uv)
    {
        return RayTraceTool.PointSample(texture.colors, texture.width, texture.height, uv);
    }
    public override Color LinearSample(Vector2 uv)
    {
        return RayTraceTool.LinearSample(texture.colors, texture.width, texture.height, uv);
    }
    public override Color GetColor()
    {
        return color;
    }
    public override RenderMode GetRenderMode()
    {
        return renderMode;
    }
}

public class SimpleTerrainMaterial : SimpleMaterialBase
{
    int alphamapWidth;
    int alphamapHeight;
    int alphamapLayers;
    List<float[,]> alphamaps;
    SimpleTexture[] textures;
    public SimpleTerrainMaterial(TerrainData terrain)
    {
        alphamapWidth = terrain.alphamapWidth;
        alphamapHeight = terrain.alphamapHeight;
        alphamapLayers = terrain.alphamapLayers;
        alphamaps = new List<float[,]>();
        float[,,] terrainAlphamaps = terrain.GetAlphamaps(0,0,terrain.alphamapWidth,terrain.alphamapHeight);
        for (int l = 0; l < alphamapLayers; l++) {
            float[,] alphamap = new float[alphamapWidth, alphamapHeight];
            for (int i = 0; i < alphamapWidth; i++) {
                for (int j = 0; j < alphamapHeight; j++) {
                    alphamap[i, j] = terrainAlphamaps[i, j, l];
                }
            }
            alphamaps.Add(alphamap);
        }
        textures = new SimpleTexture[terrain.splatPrototypes.Length];
        for (int i = 0; i < textures.Length; i++) {
            textures[i] = new SimpleTexture(terrain.splatPrototypes[i].texture);
            Vector2 ts = terrain.splatPrototypes[i].tileSize;
            textures[i].scale = new Vector2(terrain.size.x / ts.x,terrain.size.z / ts.y);
        }
    }
    public override Color PointSample(Vector2 uv)
    {
        Color rlt = Color.black;
        for (int i = 0; i < alphamapLayers; i++) {
            float alpha = RayTraceTool.BilinearInterpolation(alphamaps[i], alphamapWidth, alphamapHeight, new Vector2(uv.y, uv.x));
            if (alpha > 0.004f) {
                rlt += RayTraceTool.PointSample(textures[i].colors, textures[i].width, textures[i].height, Vector2.Scale(uv, textures[i].scale)) * alpha;
            }
        }
        return rlt;
    }
    public override Color LinearSample(Vector2 uv)
    {
        Color rlt = Color.black;
        for (int i = 0; i < alphamapLayers; i++) {
            float alpha = RayTraceTool.BilinearInterpolation(alphamaps[i], alphamapWidth, alphamapHeight, new Vector2(uv.y,uv.x));
            if (alpha > 0.004f) {
                rlt += RayTraceTool.LinearSample(textures[i].colors, textures[i].width, textures[i].height, Vector2.Scale(uv,textures[i].scale)) * alpha;
            }
        }
        return rlt;
    }
    public override Color GetColor()
    {
        return Color.white;
    }
    public override RenderMode GetRenderMode()
    {
        return RenderMode.Opaque;
    }
}

public class SimpleMesh
{
    public SimpleMesh(Mesh mesh, int subMeshIndex)
    {
        triangles = mesh.GetTriangles(subMeshIndex); uv = mesh.uv; vertices = mesh.vertices; normals = mesh.normals;
    }
    public SimpleMesh(Int32[] triangles, Vector2[] uv, Vector3[] vertices, Vector3[] normals)
    {
        this.triangles = triangles;
        this.uv = uv;
        this.vertices = vertices;
        this.normals = normals;
    }
    public Int32[] triangles;
    public Vector2[] uv;
    public Vector3[] vertices;
    public Vector3[] normals;
}

public class SimpleModel
{
    public SimpleModel() { }
    public SimpleModel(Transform transform, Mesh mesh, int subMeshIndex, Material mat)
    {
        rst.pos = transform.position; rst.rot = transform.rotation; rst.scale = transform.lossyScale;
        this.mesh = new SimpleMesh(mesh, subMeshIndex);
        this.material = new SimpleMaterial(mat);
    }
    public RayTrace.RST rst;
    public SimpleMesh mesh;
    public SimpleMaterialBase material;
    public List<Light> lights = new List<Light>();
}

public class SimpleScene
{
    public Light[] lights;
    public Camera camera;
    public List<SimpleModel> models = new List<SimpleModel>();
}

[RequireComponent(typeof(Camera))]
public class RayTraceRenderer : MonoBehaviour
{
    public static RayTraceRenderer Instance;
    public Texture2D previewTexture;

    public Texture2D texture;
    public int width = 128;
    public int height = 128;
    public int sampleNum = 100;
    public bool ignoreMaterialColor;
    public bool lightRange;
    public bool antiAliasing;
    public Color skyColor = Color.black;

    SimpleScene scene;
    public delegate bool UpdateProgressFun(float progress);
    public UpdateProgressFun updateProgress;

    public void Init()
    {
        Instance = this;
        updateProgress(0);
        HaltonTool.InitHaltonSequence();

        Debug.Log("Start Time : " + Time.realtimeSinceStartup);

        scene = new SimpleScene();

        RayTrace.Init();
        SimpleMaterial.textures.Clear();

        scene.lights = GameObject.FindObjectsOfType<Light>();

        Camera cam = GetComponent<Camera>();
        if (cam) {
            MeshRenderer[] renderers = GameObject.FindObjectsOfType<MeshRenderer>();
            foreach (MeshRenderer r in renderers) {
                bool isLayerVisible = ((1 << r.gameObject.layer) & cam.cullingMask) != 0;
                if (r.isVisible && isLayerVisible) {
                    MeshFilter filter = r.gameObject.GetComponent<MeshFilter>();
                    if (filter && filter.sharedMesh) {
                        for (int i = 0; i < filter.sharedMesh.subMeshCount; i++) {
                            Mesh mesh = filter.sharedMesh;
                            int[] triangles = filter.sharedMesh.GetTriangles(i);
                            RayTrace.AddMesh(filter.gameObject.transform, mesh.vertexCount, triangles.Length, mesh.vertices, triangles, 1);
                            SimpleModel model = new SimpleModel(r.gameObject.transform, filter.sharedMesh, i, r.sharedMaterials[i]);
                            model.lights = RayTraceTool.GetActivityLights(r, scene);
                            scene.models.Add(model);
                        }
                    }
                }
            }
            Terrain[] terrains = GameObject.FindObjectsOfType<Terrain>();
            foreach (Terrain terrain in terrains) {
                bool isLayerVisible = ((1 << terrain.gameObject.layer) & cam.cullingMask) != 0;
                if (terrain.gameObject.activeInHierarchy && isLayerVisible) {
                    int w = terrain.terrainData.heightmapWidth;
                    int h = terrain.terrainData.heightmapHeight;
                    float realWidth = terrain.terrainData.size.x * (1 + 1f / w);
                    float realHeight = terrain.terrainData.size.z * (1 + 1f / h);
                    float[,] heights = terrain.terrainData.GetHeights(0, 0, w, h);
                    Vector3[] vertices = new Vector3[w * h];
                    Vector2[] uvs = new Vector2[vertices.Length];
                    Vector3[] normals = new Vector3[vertices.Length];
                    int[] triangles = new int[(w - 1) * (h - 1) * 2 * 3];
                    for (int i = 0; i < w; i++) {
                        for (int j = 0; j < h; j++) {
                            Vector3 pos = new Vector3(i * realWidth / w, heights[j,i] * terrain.terrainData.size.y, j * realHeight / h);
                            vertices[j * w + i] = pos;
                            uvs[j * w + i] = new Vector2(i / (float)w, j / (float)h);
                            normals[j * w + i] = terrain.terrainData.GetInterpolatedNormal(i / (float)w, j / (float)h);
                        }
                    }

                    for (int i = 0; i < w - 1; i++) {
                        for (int j = 0; j < h - 1; j++) {
                            int index = (j * (w - 1) + i) * 6;
                            triangles[index + 0] = (i + 0) + (j + 0) * w;
                            triangles[index + 1] = (i + 1) + (j + 1) * w;
                            triangles[index + 2] = (i + 1) + (j + 0) * w;
                            triangles[index + 3] = (i + 0) + (j + 0) * w;
                            triangles[index + 4] = (i + 0) + (j + 1) * w;
                            triangles[index + 5] = (i + 1) + (j + 1) * w;
                        }
                    }

                    RayTrace.AddMesh(terrain.transform, vertices.Length, triangles.Length, vertices, triangles, 1);
                    SimpleModel model = new SimpleModel();
                    model.mesh = new SimpleMesh(triangles, uvs, vertices, normals);
                    model.material = new SimpleTerrainMaterial(terrain.terrainData);
                    model.rst = new RayTrace.RST(terrain.transform.rotation, terrain.transform.lossyScale, terrain.transform.position);
                    model.lights.AddRange(scene.lights);
                    scene.models.Add(model);
                }
            }
        }

        RayTrace.Commit();

        texture = new Texture2D(width, height, TextureFormat.ARGB32, true);
        //viewResult.sharedMaterial.mainTexture = texture;

    }


    public void Release()
    {
        RayTrace.Release(); 
        SimpleMaterial.textures.Clear();
        scene = null;
    }

    public void Clear()
    {
        texture = new Texture2D(texture.width, texture.height);
    }

    public void Render()
    {
        //RayTraceTool.GetRandomDirs_BlinnPhong_Importance2(Vector3.up, new Vector3(1, 0.1f, 0).normalized, 0.7f, 1000);
        //return;

        updateProgress(0);
        Camera cam = GetComponent<Camera>();
        if (cam) {
            Vector3 left = Vector3.Cross(cam.transform.forward, cam.transform.up);
            Vector3 up = Vector3.Cross(left, cam.transform.forward);
            float lengthY = Mathf.Tan(Mathf.Deg2Rad * cam.fieldOfView / 2) * 2;
            float lengthX = lengthY * width / height;
            int waitNum = width * height;
            int curIndex = 0;
            SimpleTexture targetTexture = new SimpleTexture(width, height);

            float startTime = Time.realtimeSinceStartup;
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    int x = i - width / 2;
                    int y = height / 2 - j;

                    Color color = Color.black;
                    int splitPixel = antiAliasing ? 3 : 1;
                    for (int ax = 0; ax < splitPixel; ax++) {
                        for (int ay = 0; ay < splitPixel; ay++) {
                            float fx = (lengthX * (x + ax / (float)splitPixel)) / width;
                            float fy = (lengthY * (y + ay / (float)splitPixel)) / height;
                            Vector3 temp = cam.transform.position + cam.transform.forward + left * fx + up * fy;
                            Vector3 tempDir = Vector3.Normalize(temp - cam.transform.position);
                            int subSampleNum = sampleNum / (splitPixel * splitPixel);
                            color += TraceColor(cam.transform.position, tempDir, subSampleNum,3);
                        }
                    }
                    color /= splitPixel * splitPixel;
                    color.a = 1;
                    targetTexture.SetColor(width - i - 1, height - j - 1, RayTraceTool.LinearToGammaSpace(color));

                    curIndex += 1;
                }
                if (updateProgress(curIndex / (float)waitNum)) {
                    break;
                }
            }
            //TaskPipeLine.Flush();
            Debug.Log("render time : " + (Time.realtimeSinceStartup - startTime));
            texture.SetPixels(targetTexture.colors);
            texture.Apply(true);
        }
    }

    float Fresnel_Schlick(float InCosine, float normalReflectance)
    {
        // InCosine是入射光线和法线的夹角，normalReflectance是入射光线和法线垂直时的反射能量大小
	    float oneMinusCos = 1.0f - InCosine;
	    float oneMinusCosSqr = oneMinusCos * oneMinusCos;
	    float fresnel = normalReflectance +
		    (1.0f - normalReflectance) * oneMinusCosSqr * oneMinusCosSqr * oneMinusCos;

	    return fresnel;
    }


    Color TraceColor(Vector3 startPos, Vector3 rayDir, int sample,int depth)
    {
        
        if (depth <= 0) {
            return Color.black;
        }
        RayTrace.Ray ray = new RayTrace.Ray(startPos, rayDir, 1000);
        RayTrace.Intersect(ref ray);
        if (ray.geomID == RayTrace.Invalid) {
            return skyColor;
        }
        else {
            SimpleModel model = scene.models[(int)ray.geomID];
            SimpleMesh mesh = model.mesh;
            int t0 = mesh.triangles[ray.primID * 3 + 0];
            int t1 = mesh.triangles[ray.primID * 3 + 1];
            int t2 = mesh.triangles[ray.primID * 3 + 2];
            Vector2 uv = RayTraceTool.Lerp(mesh.uv[t0], mesh.uv[t1], mesh.uv[t2], ray.u, ray.v);
            Color texColor = model.material.LinearSample(uv);

            if (!ignoreMaterialColor) {
                texColor *= model.material.GetColor();
            }

            if (model.material.GetRenderMode() == SimpleMaterial.RenderMode.Opaque) {
                texColor.a = 1;
            }

            Vector3 hitPos = ray.pos + ray.dir * ray.length;
            Vector3 hitNormal = RayTraceTool.Lerp(mesh.normals[t0], mesh.normals[t1], mesh.normals[t2], ray.u, ray.v);
            hitNormal = (model.rst.rot * hitNormal);


            float transFactor = 1 - texColor.a;
            float glossiness = model.material.glossiness;
            float reflectFactor = Mathf.Lerp(0.2f * glossiness,1f,model.material.metallic);
            if (reflectFactor > 0.01f) {
                reflectFactor = Fresnel_Schlick(Vector3.Dot(-rayDir, hitNormal), reflectFactor);
            }
            float grayscale = texColor.grayscale;
            float diffuseFactor = Mathf.Clamp01(1 - transFactor - reflectFactor);

            int transSample = (int)(sample * grayscale * transFactor);
            int reflectSample = (int)(sample * reflectFactor);
            int diffuseSample = (int)(sample * grayscale * diffuseFactor);


            Color finaleColor = Color.black;

            Color reflectColor = Color.black;
            {
                Color finalLightColor = Color.black;// GetAllLightsColor(model, hitPos, hitNormal) * (reflectFactor + scene.lights.Length);//光源直接贡献
                if (reflectSample > 0) {
                    Vector3 reflectDir = Vector3.Reflect(rayDir, hitNormal);
                    Vector3[] dirs = RayTraceTool.GetRandomDirs_BlinnPhong_Importance2(hitNormal, reflectDir, glossiness, reflectSample);
                    for (int i = 0; i < reflectSample; i++) {
                        finalLightColor += TraceColor(hitPos + hitNormal * 0.01f, dirs[i], 1,depth - 1);//间接光贡献
                    }
                    finalLightColor /= (reflectSample);
                } 
                reflectColor = Color.Lerp(Color.white,texColor,model.material.metallic) * finalLightColor;
            }

            Color diffuseColor = Color.black;
            {
                Color finalLightColor = GetAllLightsColor(model, hitPos, hitNormal) * (diffuseSample + scene.lights.Length);//光源直接贡献
                if (diffuseSample > 0) {
                    Vector3 reflectDir = Vector3.Reflect(rayDir, hitNormal);
                    Vector3[] dirs = RayTraceTool.GetRandomDirs_RoundProj(hitNormal, diffuseSample);
                    for (int i = 0; i < diffuseSample; i++) {
                        finalLightColor += TraceColor(hitPos + hitNormal * 0.01f, dirs[i], 1, depth - 1);//间接光贡献
                    }
                }
                finalLightColor /= (diffuseSample + scene.lights.Length);
                diffuseColor = texColor * finalLightColor;
            }

            //Color reflectColor = Color.black;
            //if(reflectFactor > 0.01f){
            //    Vector3 refDir = Vector3.Reflect(rayDir,hitNormal);
            //    Color finalLightColor = TraceColor(hitPos + refDir * 0.01f, refDir,reflectSample ,depth - 1);
            //    reflectColor = finalLightColor; 
            //}

            finaleColor = reflectColor * reflectFactor + diffuseColor * diffuseFactor;

            if (texColor.a < 0.99f) {
                Color transColor = TraceColor(hitPos + rayDir * 0.01f, rayDir, transSample,depth);
                if (model.material.GetRenderMode() == SimpleMaterial.RenderMode.Alphablend) {
                    finaleColor = finaleColor * texColor.a + transColor * (1 - texColor.a);
                }
                else if (model.material.GetRenderMode() == SimpleMaterial.RenderMode.Multiply) {
                    finaleColor = finaleColor * transColor;
                }
                else if (model.material.GetRenderMode() == SimpleMaterial.RenderMode.Additive) {
                    finaleColor = finaleColor + transColor;
                }
            }

            return finaleColor;
        }
    }

    Color GetAllLightsColor(SimpleModel model, Vector3 pos, Vector3 normal)
    {
        Color finalLightColor = Color.black;
        foreach (Light light in scene.lights) {
            Vector3 lightDir = light.type == LightType.Directional ? (-light.transform.forward) : (light.transform.position - pos).normalized;
            if (Vector3.Dot(lightDir, normal) > 0) {
                float lightLength = light.type == LightType.Directional ? 10000 : ((light.transform.position - pos).magnitude + 0.01f);
                float power = 0;
                //float power = light.type == LightType.Directional ? light.intensity : light.intensity / (lightLength * lightLength);
                if (light.type == LightType.Directional) {
                    power = light.intensity;
                }
                else {
                    if (lightRange) {
                        float d = (1 - lightLength / light.range);
                        if (d > 0) {
                            power = light.intensity * (d * d);
                        }
                    }
                    else {
                        power = light.intensity / (lightLength * lightLength);
                    }
                }

                Color lightColor = light.color * Vector3.Dot(lightDir, normal) * power;
                if (lightColor.grayscale > 0.004f) {
                    finalLightColor += GetLightPathColor(pos + lightDir * 0.01f, lightDir, lightLength) * lightColor;
                }
            }
        }
        return finalLightColor;
    }

    Color GetLightPathColor(Vector3 pos, Vector3 dir, float length)
    {
        RayTrace.Ray ray = new RayTrace.Ray(pos, dir, length);
        RayTrace.Intersect(ref ray);
        if (ray.geomID == RayTrace.Invalid) {
            return Color.white;
        }
        else {
            SimpleModel model = scene.models[(int)ray.geomID];
            SimpleMesh mesh = model.mesh;
            int t0 = mesh.triangles[ray.primID * 3 + 0];
            int t1 = mesh.triangles[ray.primID * 3 + 1];
            int t2 = mesh.triangles[ray.primID * 3 + 2];
            Vector2 uv = RayTraceTool.Lerp(mesh.uv[t0], mesh.uv[t1], mesh.uv[t2], ray.u, ray.v);
            Vector3 hitPos = ray.pos + ray.dir * ray.length;
            Color texColor = model.material.PointSample(uv);
            if (texColor.a < 0.99f) {
                Color aheadColor = GetLightPathColor(hitPos, dir * 0.01f, length - ray.length);
                Color blendColor = (aheadColor * (1 - texColor.a)) + (texColor * texColor.a);
                return blendColor;
            }
        }
        return Color.black;
    }
}
