// raytrace.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "render.h"

using namespace embree;

RTCDevice g_device = NULL;


int main()
{
	RTCDevice g_device = rtcNewDevice();
	RTCScene scene = rtcDeviceNewScene(g_device, RTC_SCENE_STATIC, RTC_INTERSECT1 | RTC_INTERSECT4 | RTC_INTERSECT8);

	unsigned int geomID = rtcNewTriangleMesh(scene, RTC_GEOMETRY_STATIC, 1, 3);
	Vertex* vertices = (Vertex*)rtcMapBuffer(scene, geomID, RTC_VERTEX_BUFFER);
	vertices[0].x = 0;vertices[0].y = 0;vertices[0].z = 0;vertices[0].a = 0;
	vertices[1].x = 0;vertices[1].y = 1;vertices[1].z = 0;vertices[1].a = 0;
	vertices[2].x = 1;vertices[2].y = 0;vertices[2].z = 0;vertices[2].a = 0;
	rtcUnmapBuffer(scene, geomID, RTC_VERTEX_BUFFER);
	Triangle* triangles = (Triangle*)rtcMapBuffer(scene, geomID, RTC_INDEX_BUFFER);
	triangles[0].v0 = 0;triangles[0].v1 = 1;triangles[0].v2 = 2;
	rtcUnmapBuffer(scene, geomID, RTC_INDEX_BUFFER);
	rtcCommit(scene);

	RTCRay ray;
	ray.org[0] = 0.1; ray.org[1] = 0.1; ray.org[2] = -1;
	ray.dir[0] = 0; ray.dir[1] = 0; ray.dir[2] = 1;
	ray.tnear = 0;
	ray.tfar = 2;
	ray.geomID = RTC_INVALID_GEOMETRY_ID;
	ray.primID = RTC_INVALID_GEOMETRY_ID;
	ray.instID = RTC_INVALID_GEOMETRY_ID;
	ray.mask = 0xFFFFFFFF;
	ray.time = 0.f;
	rtcIntersect(scene, ray);

	RenderToImage(scene, 128, 128, Vec3<float>(0.5,0.5, -6), Vec3f(0,0,1),Vec3f(0,1,0),50 * 3.1416/180,10);

	rtcDeleteScene(scene);
    return 0;
}



