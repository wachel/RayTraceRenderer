// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#include "targetver.h"

#include <stdio.h>
#include <tchar.h>
#include <string>


#include "rtcore.h"
#include "rtcore_ray.h"
#include "math/vec3.h"
#include "math/quaternion.h"
#include "image/image.h"

struct Vertex { float x, y, z, a; };
struct Triangle { int v0, v1, v2; };

using namespace embree;
// TODO: reference additional headers your program requires here
