using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public static class RayTrace
{
    public static readonly uint Invalid = 0xffffffff;
    public struct RST
    {
        public RST(Quaternion rot, Vector3 scale, Vector3 pos)
        {
            this.rot = rot; this.scale = scale; this.pos = pos;
        }
        public Quaternion rot;
        public Vector3 scale;
        public Vector3 pos;
    }

    public struct Ray
    {
        public Ray(Vector3 pos, Vector3 dir, float length)
        {
            this.pos = pos; this.dir = dir; this.length = length;
            geomID = 0xffffffff; primID = 0xffffffff;
            mask = 0xffffffff; align1 = 0;
            u = 0; v = 0; normal = Vector3.zero;
        }
        public Vector3 pos;
        public uint mask;

        public Vector3 dir;
        public float length;

        public uint geomID;
        public uint primID;
        public float u;
        public float v;

        public Vector3 normal;
        public float align1;
    }



    [DllImport("raytrace")]
    public static extern void Init();

    [DllImport("raytrace", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Release();

    [DllImport("raytrace", CallingConvention = CallingConvention.Cdecl)]
    public static extern void AddMesh([MarshalAs(UnmanagedType.LPStruct)] RST rst, int vertexCount, int indexCount, [MarshalAs(UnmanagedType.LPArray)] Vector3[] vertices, [MarshalAs(UnmanagedType.LPArray)] int[] triangles, uint mask);

    [DllImport("raytrace", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Commit();

    [DllImport("raytrace", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Render(int width, int height, [MarshalAs(UnmanagedType.LPStruct)]Vector3 camPos, [MarshalAs(UnmanagedType.LPStruct)]Vector3 camDir, [MarshalAs(UnmanagedType.LPStruct)]Vector3 camUp, float fov, float length);

    [DllImport("raytrace", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Intersect(ref RayTrace.Ray ray);

    [DllImport("raytrace", CallingConvention = CallingConvention.Cdecl)]
    public static extern void BatchIntersect(int rayCount, [MarshalAs(UnmanagedType.LPArray)] RayTrace.Ray[] rays);

    public static void AddMesh(Transform transform, int vertexCount, int triangleCount, Vector3[] vertices, int[] triangles, uint mask)
    {
        RST rst = new RST(transform.rotation, transform.lossyScale, transform.position);
        AddMesh(rst, vertexCount, triangleCount, vertices, triangles, mask);
    }

}