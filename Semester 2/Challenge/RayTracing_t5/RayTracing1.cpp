// RayTracing1.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <float.h>

#include "CImg.h"
#include "vec3.h"
#include "utils.h"
#include "ray.h"
#include "camera.h"

using namespace std;
using namespace cimg_library;

vec3 color(const ray& r, hittable* world) {
	hit_record rec;
	if (world->hit(r, 0.001, FLT_MAX, rec)) {
		//return 0.5f * vec3(rec.normal.x() + 1, rec.normal.y() + 1, rec.normal.z() + 1);
		vec3 target = rec.p + rec.normal + random_in_unit_sphere();
		return 0.5 * color(ray(rec.p, target - rec.p), world);
	}
	else {
		vec3 unit_direction = unit_vector(r.direction());
		float t = 0.5f * (unit_direction.y() + 1.0f);
		return (1.0f - t) * vec3(1.0f, 1.0f, 1.0f) + t * vec3(0.5f, 0.7f, 1.0f);
	}
}
int main()
{
	int nx, ny, countRows, ns;
	nx = 1000;
	ny = 500;
	ns = 1;

	CImg<unsigned char> img(nx, ny, 1, 3);
	CImgDisplay canvas(img, "RayTracing Test", 0);
	
	vec3 lower_left_corner(-2.0f, -1.0f, -1.0f);
	vec3 horizontal(4.0f, 0.0f, 0.0f);
	vec3 vertical(0.0f, 2.0f, 0.0f);
	vec3 origin(0.0f, 0.0f, 0.0f);

	hittable* list[2];
	list[0] = new sphere(vec3(0, 0, -1), 0.5f);
	list[1] = new sphere(vec3(0, -100.5, -1), 100);
	hittable* world = new hittable_list(list, 2);
	camera cam;

	countRows = 0;

	for (int j = ny - 1; j >= 0; j--, countRows++) 
	{
		for (int i = 0; i < nx; i++) 
		{
			vec3 col = vec3(0, 0, 0);

			for (int s = 0; s < ns; ++s)
			{
				float u = float(i) / float(nx);
				float v = float(j) / float(ny);
				//ray r(origin, lower_left_corner + u * horizontal + v * vertical);
				ray r = cam.get_ray(u, v);
				col = color(r, world);
			}
			col /= float(ns);
			col = vec3(sqrt(col[0]), sqrt(col[1]), sqrt(col[2]));


			int ir = int(255.99f * col[0]);
			int ig = int(255.99f * col[1]);
			int ib = int(255.99f * col[2]);

			img(i, j, 0, 0) = ir;
			img(i, j, 0, 1) = ig;
			img(i, j, 0, 2) = ib;
		}

		float percentDone = 100.f * (float(countRows) / float(ny - 1));
		cout << percentDone << "%" << endl;
	}


	canvas.resize(nx, ny);
	img.mirror('y');
	while (!canvas.is_closed() && !canvas.is_keyESC()) 
	{
		img.display(canvas);
		cimg::wait(20);
	}
}


