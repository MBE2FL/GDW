#pragma once
#include "vec3.h"
#define M_PI 3.14159265358979323846264338327950288
#include <cstdlib>
#include "ray.h"

inline double random_double() {
	return rand() / (RAND_MAX + 1.0);
}

vec3 random_in_unit_disk() {
	vec3 p;
	do {
		p = 2.0f * vec3(random_double(), random_double(), 0) - vec3(1, 1, 0);
	} while (dot(p, p) >= 1.0);
	return p;
}

vec3 random_in_unit_sphere() {
	vec3 p;
	do {
		p = 2.0f * vec3(random_double(), random_double(), random_double()) - vec3(1, 1, 1);
	} while (dot(p, p) >= 1.0);
	return p;
}

struct hit_record {
	float t;
	vec3 p;
	vec3 normal;
};

// absstract class 
class hittable {
public:
	virtual bool hit(const ray& r, float t_min, float t_max, hit_record& rec) const = 0;
};


// sphere class
class sphere : public hittable {
public:
	sphere() {}
	sphere(vec3 cen, float r) : center(cen), radius(r) {};


	virtual bool hit(const ray& r, float t_min, float t_max, hit_record& rec) const {
		vec3 oc = r.origin() - center;
		float a = dot(r.direction(), r.direction());
		float b = dot(oc, r.direction());
		float c = dot(oc, oc) - radius * radius;
		float discriminant = b * b - a * c;
		if (discriminant > 0) {
			float temp = (-b - sqrt(b * b - a * c)) / a;
			if (temp < t_max && temp > t_min) {
				rec.t = temp;
				rec.p = r.point_at_parameter(rec.t);
				rec.normal = (rec.p - center) / radius;
				return true;
			}
			temp = (-b + sqrt(b * b - a * c)) / a;
			if (temp < t_max && temp > t_min) {
				rec.t = temp;
				rec.p = r.point_at_parameter(rec.t);
				rec.normal = (rec.p - center) / radius;
				return true;
			}
		}
		return false;
	}

	vec3 center;
	float radius;
};


// list of objects
class hittable_list : public hittable {
public:
	hittable_list() {}
	hittable_list(hittable** l, int n) { list = l; list_size = n; }

	virtual bool hit(const ray& r, float t_min, float t_max, hit_record& rec) const {
		hit_record temp_rec;
		bool hit_anything = false;
		double closest_so_far = t_max;
		for (int i = 0; i < list_size; i++) {
			if (list[i]->hit(r, t_min, closest_so_far, temp_rec)) {
				hit_anything = true;
				closest_so_far = temp_rec.t;
				rec = temp_rec;
			}
		}
		return hit_anything;
	}

	hittable** list;
	int list_size;
};