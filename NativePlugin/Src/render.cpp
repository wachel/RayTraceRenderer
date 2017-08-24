#include "stdafx.h"

#include "Render.h"

using namespace embree;

void RenderToImage(RTCScene scene, int width, int height, Vec3f pos, Vec3f dir, Vec3f up, float fov, float length)
{
	Ref<Image> image = new Image4f(width, height, "test");
	FileName filename("test.tga");

	Vec3f left = -cross(dir, up);
	up = cross(left, dir);
	float lengthX = tan(fov / 2) * 2;
	float lengthY = lengthX * width / height;

	for (int x = -width / 2; x < width / 2;x++) {
		for (int y = -height / 2; y < height / 2; y++) {
			Vec3f temp = pos + dir + left * (lengthX * x / width) + up * (lengthY * y / height);
			Vec3f tempDir = normalize(temp - pos);

			RTCRay ray;
			ray.org[0] = pos.x; ray.org[1] = pos.y; ray.org[2] = pos.z;
			ray.dir[0] = tempDir.x; ray.dir[1] = tempDir.y; ray.dir[2] = tempDir.z;
			ray.tnear = 0;
			ray.tfar = length;
			ray.geomID = RTC_INVALID_GEOMETRY_ID;
			ray.primID = RTC_INVALID_GEOMETRY_ID;
			ray.instID = RTC_INVALID_GEOMETRY_ID;
			ray.mask = 0xFFFFFFFF;
			ray.time = 0.f;
			rtcIntersect(scene, ray);

			float bright = ray.tfar / length;
			image->set(x + width / 2, y + height / 2, Color4(bright, bright, bright, 1));
		}
	}

	storeTga(image, filename);
}

