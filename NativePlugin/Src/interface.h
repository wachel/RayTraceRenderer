#pragma once

#define STDCALL __stdcall
#define DLLEXPORT __declspec(dllexport)


//typedef void(STDCALL *RayTrace_GetChunkFun)(int chx, int chy, int chz, int** outBlocks, int* length);
//extern "C" DLLEXPORT int RayTrace_createManager(int chunkNumX, int chunkNumY, int chunkNumZ, int chunkSize, float blockLength);
//extern "C" DLLEXPORT void RayTrace_deleteManager(int handle);
//extern "C" DLLEXPORT void RayTrace_moveTo(int handle, int startChunkX, int startChunkY, int startChunkZ, RayTrace_GetChunkFun getChunk);
//extern "C" DLLEXPORT bool RayTrace_rayCast(int handle, float startX, float startY, float startZ, float dirX, float dirY, float dirZ, float length,
//	float* outHitLength, int* outHitFaceIndex);
//extern "C" DLLEXPORT void RayTrace_batchRayCast(int handle, int num, float* posArray, float* dirArray, float* lineLength, int* outBHit, float* outHitLength, int* outHitFaceIndex);

struct RST {
	float rot[4];
	Vec3f scale;
	Vec3f pos;
};

struct Ray {
	Vec3f pos;  
	int mask;

	Vec3f dir;
	float length;

	unsigned int geomID;
	unsigned int primID;
	float u;
	float v;

	Vec3f normal;
	float align1;
};

extern "C" DLLEXPORT void Init();
extern "C" DLLEXPORT void Release();
extern "C" DLLEXPORT unsigned int AddMesh(RST* rst, int vertexCount, int indexCount, float* vertices, int* triangles, int mask);
extern "C" DLLEXPORT void Commit();
extern "C" DLLEXPORT void Render(int width, int height, float* pos, float* dir, float* up, float fov, float length);
extern "C" DLLEXPORT void Intersect(Ray* ray);
extern "C" DLLEXPORT void BatchIntersect(int rayCount, Ray* rays);
