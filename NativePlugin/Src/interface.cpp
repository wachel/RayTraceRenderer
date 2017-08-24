#include "stdafx.h"
#include "interface.h"
#include "render.h"
#include "Convert.h"

using namespace embree;

static RTCDevice g_device = NULL;
static RTCScene scene = NULL;

DLLEXPORT void Init()
{
	g_device = rtcNewDevice();
	scene = rtcDeviceNewScene(g_device, RTC_SCENE_STATIC, RTC_INTERSECT1 | RTC_INTERSECT4 | RTC_INTERSECT8);
}

DLLEXPORT void Release()
{
	rtcDeleteScene(scene);
	rtcDeleteDevice(g_device);
}

DLLEXPORT unsigned int AddMesh(RST* rst, int vertexCount, int indexCount, float * vertices, int * triangles, int mask)
{
	unsigned int geomID = rtcNewTriangleMesh(scene, RTC_GEOMETRY_STATIC, indexCount / 3, vertexCount);
	Vertex* verticesBuf = (Vertex*)rtcMapBuffer(scene, geomID, RTC_VERTEX_BUFFER);
	for (int i = 0; i < vertexCount; i++) {
		Vec3f localPos = ToVec3f(&vertices[i*3]);
		Quaternion3f rot = Quaternion3f(rst->rot[3], rst->rot[0], rst->rot[1], rst->rot[2]);
		Vec3f worldPos = rot * (localPos * rst->scale) + rst->pos;
		verticesBuf[i].x = worldPos.x;
		verticesBuf[i].y = worldPos.y;
		verticesBuf[i].z = worldPos.z;
		verticesBuf[i].a = 0;
	}
	rtcUnmapBuffer(scene, geomID, RTC_VERTEX_BUFFER);
	Triangle* trianglesBuf = (Triangle*)rtcMapBuffer(scene, geomID, RTC_INDEX_BUFFER);
	for (int i = 0; i < indexCount / 3; i++) {
		trianglesBuf[i].v0 = triangles[i * 3 + 0];
		trianglesBuf[i].v1 = triangles[i * 3 + 2];
		trianglesBuf[i].v2 = triangles[i * 3 + 1];
	}
	rtcUnmapBuffer(scene, geomID, RTC_INDEX_BUFFER);
	rtcSetMask(scene, geomID, mask);
	return geomID;
}

DLLEXPORT void Commit()
{
	rtcCommit(scene);
}

DLLEXPORT void Render(int width, int height, float * camPos, float * camDir, float * camUp, float fovDegree, float length)
{
	Vec3f dir = ToVec3f(camDir);
	Vec3f pos = ToVec3f(camPos);
	Vec3f up = ToVec3f(camUp);
	float fov = fovDegree * 3.1415926f / 180;
	RenderToImage(scene, width, height, pos, dir, up, fov, length);
}


DLLEXPORT void Intersect(Ray* input)
{
	RTCRay ray;
	ray.org[0] = input->pos.x; ray.org[1] = input->pos.y; ray.org[2] = input->pos.z;
	ray.dir[0] = input->dir.x; ray.dir[1] = input->dir.y; ray.dir[2] = input->dir.z;
	ray.tnear = 0;
	ray.tfar = input->length;
	ray.geomID = RTC_INVALID_GEOMETRY_ID;
	ray.primID = RTC_INVALID_GEOMETRY_ID;
	ray.instID = RTC_INVALID_GEOMETRY_ID;
	ray.mask = input->mask;
	ray.time = 0.f;
	rtcIntersect(scene, ray);
	input->geomID = ray.geomID;
	input->length = ray.tfar;
	input->normal = Vec3f(ray.Ng[0], ray.Ng[1], ray.Ng[2]);
	input->primID = ray.primID;
	input->u = ray.v;
	input->v = ray.u;
}

void RayIntersect8(Ray* rays) {
	RTCRay8 ray8;
	__aligned(64) int valid8[8] = { -1,-1,-1,-1,-1,-1,-1,-1 };
	for (int i = 0; i < 8; i++) {
		Ray& input = rays[i];
		ray8.orgx[i] = input.pos.x; ray8.orgy[i] = input.pos.y; ray8.orgz[i] = input.pos.z;
		ray8.dirx[i] = input.dir.x; ray8.diry[i] = input.dir.y; ray8.dirz[i] = input.dir.z;
		ray8.tnear[i] = 0;
		ray8.tfar[i] = input.length;
		ray8.geomID[i] = input.geomID;
		ray8.primID[i] = RTC_INVALID_GEOMETRY_ID;
		ray8.instID[i] = RTC_INVALID_GEOMETRY_ID;
		ray8.mask[i] = input.mask;
		ray8.time[i] = 0.f;
	}
	rtcIntersect8(valid8, scene, ray8);
	for (int i = 0; i < 8; i++) {
		Ray& input = rays[i];
		input.geomID = ray8.geomID[i];
		input.length = ray8.tfar[i];
		input.normal = Vec3f(ray8.Ngx[i], ray8.Ngy[i], ray8.Ngz[i]);
		input.primID = ray8.primID[i];
		input.u = ray8.v[i];
		input.v = ray8.u[i];
	}
}

void RayIntersect4(Ray* rays) {
	RTCRay4 ray4;
	__aligned(64) int valid4[4] = { -1,-1,-1,-1 };
	for (int i = 0; i < 4; i++) {
		const Ray& input = rays[i];
		ray4.orgx[i] = input.pos.x; ray4.orgy[i] = input.pos.y; ray4.orgz[i] = input.pos.z;
		ray4.dirx[i] = input.dir.x; ray4.diry[i] = input.dir.y; ray4.dirz[i] = input.dir.z;
		ray4.tnear[i] = 0;
		ray4.tfar[i] = input.length;
		ray4.geomID[i] = input.geomID;
		ray4.primID[i] = RTC_INVALID_GEOMETRY_ID;
		ray4.instID[i] = RTC_INVALID_GEOMETRY_ID;
		ray4.mask[i] = input.mask;
		ray4.time[i] = 0.f;
	}
	rtcIntersect4(valid4, scene, ray4);
	for (int i = 0; i < 4; i++) {
		Ray& input = rays[i];
		input.length = ray4.tfar[i];
		input.geomID = ray4.geomID[i];
		input.normal = Vec3f(ray4.Ngx[i], ray4.Ngy[i], ray4.Ngz[i]);
		input.primID = ray4.primID[i];
		input.u = ray4.v[i];
		input.v = ray4.u[i];
	}
}


DLLEXPORT void BatchIntersect(int rayCount, Ray* rays)
{
	int batchCount = rayCount / 8;
	int otherCount = rayCount % 8;
	for (int i = 0; i < batchCount; i++) {
		RayIntersect8(&rays[i * 8]);
	}
	for (int i = 0; i < otherCount; i++) {
		Intersect(&rays[batchCount * 8 + i]);
	}
}
