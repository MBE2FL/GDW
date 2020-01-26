//float4x4 unity_ObjectToWorld;
//float4x4 unity_MatrixVP;
//float4x4 UNITY_MATRIX_MVP;

// Floats
float _smoothUnionFactor = 0.0;
float _totalTime;

// Matrices
float4x4 _FrustumCornersES;
float4x4 _TorusMat_InvModel;
float4x4 _CameraInvMatrix;

// Vectors
float4 _MainTex_TexelSize;
float4 _CameraPos;

// Textures
sampler2D _MainTex;
sampler2D _CameraDepthTexture;
sampler2D _colourRamp;
sampler2D _performanceRamp;


// ######### Light Variables #########
// Floats
float _specularExp;
float _attenuationConstant;
float _attenuationLinear;
float _attenuationQuadratic;

// Vectors
float3 _LightDir;
float3 _lightPos;
float3 _ambientColour;
float3 _diffuseColour;
float3 _specularColour;
float3 _lightConstants; // .x: ambient, .y: diffuse, .z: specular
float3 _rimLightColour;

// Textures

// ######### Light Variables #########


// ######### Shadow Variables #########
float _penumbraFactor;
float _shadowMinDist;
float _shadowIntensity;
// ######### Shadow Variables #########


// ######### Ray March Variables #########
int _maxSteps;
float _maxDrawDist;
// ######### Ray March Variables #########

// ######### Reflection Variables #########
int _reflectionCount;
float _reflectionIntensity;
float _envReflIntensity;
samplerCUBE _skybox;
// ######### Reflection Variables #########

// ######### Refraction Variables #########
float2 _refractInfo[32];
// ######### Refraction Variables #########

// ######### Ambient Occlusion Variables #########
int _aoMaxSteps;
float _aoStepSize;
float _aoIntensity;
// ######### Ambient Occlusion Variables #########

/// ######### RM OBJS Information #########
static const uint MAX_RM_OBJS = 32;
static const uint MAX_CSG_CHILDREN = 16;
float4x4 _invModelMats[MAX_RM_OBJS];
//int _primIndices;
float4 _rm_colours[MAX_RM_OBJS];
int _primitiveTypes[MAX_RM_OBJS];
float2 _combineOps[MAX_RM_OBJS];
float4 _primitiveGeoInfo[MAX_RM_OBJS];
float4 _reflInfo[MAX_RM_OBJS];

//int _csgNodesPerRoot[MAX_CSG_CHILDREN];
float4 _bufferedCSGs[MAX_CSG_CHILDREN];
float4 _combineOpsCSGs[MAX_CSG_CHILDREN];
//int _totalRootCSGs;
/// ######### RM OBJS Information #########

sampler2D _wood;
sampler2D _brick;

float _refractIndex;
float _vignetteIntensity;

struct VertexInput
{
    float4 pos : POSITION;
    float2 uv : TEXCOORD0;
};

struct VertexOutput
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 ray : TEXCOORD1;
};

struct rmPixel
{
    float dist;
    float4 colour;
    float4 reflInfo;
    float2 refractInfo;
    int texID;
};

struct reflectInfo
{
    float3 pos;
    float3 normal;
    float3 dir;
    rmPixel distField;
};


/// ######### Conditional Functions #########
float4 when_eq_float4(float4 x, float4 y)
{
    return 1.0 - abs(sign(x - y));
}

int when_eq_int(int x, int y)
{
    return 1 - abs(sign(x - y));
}

int when_gt_int(int x, int y)
{
    return max(sign(x - y), 0);
}

float when_gt_float(float x, float y)
{
    return max(sign(x - y), 0.0);
}

float when_lt_float(float x, float y)
{
    return max(sign(y - x), 0.0);
}

float when_ge_float(float x, float y)
{
    return 1.0 - when_lt_float(x, y);
}
/// ######### Conditional Functions #########


/// ######### Signed Distance Functions #########
// Torus
// t.x: diameter
// t.y: thickness
float sdTorus(float3 p, float2 t)
{
    float2 q = float2(length(p.xz) - t.x, p.y);
    return length(q) - t.y;
}

// Box
// b: size of box in x/y/z
float sdBox(float3 p, float3 b)
{
    float3 d = abs(p) - b;
    
    return min(max(d.x, max(d.y, d.z)), 0.0) + length(max(d, 0.0));
}

// Sphere
// s: size/diameter
float sdSphere(float3 p, float s)
{
    return length(p) - s;
}

// Cylinder
// h:
// r:
float sdCylinder(float3 p, float h, float r)
{
    float2 d = abs(float2(length(p.xz), p.y)) - float2(h, r);
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0));
}

// Tetrahedron
float sdTetra(float3 p)
{
    float3 a1 = float3(1, 1, 1) * 2;
    float3 a2 = float3(-1, -1, 1) * 2;
    float3 a3 = float3(1, -1, -1) * 2;
    float3 a4 = float3(-1, 1, -1) * 2;

    float3 c;
    int n = 0;
    float dist, d;
    float scale = 2.0;

    while (n < 15)
    {
        c = a1;
        dist = length(p - a1);

        d = length(p - a2);
        if (d < dist)
        {
            c = a2;
            dist = d;
        }

        d = length(p - a3);
        if (d < dist)
        {
            c = a3;
            dist = d;
        }

        d = length(p - a4);
        if (d < dist)
        {
            c = a4;
            dist = d;
        }

        p = (scale * p) - (c * (scale - 1.0));
        n++;
    }

    return length(p) * pow(scale, float(-n));
}

// Union
// .x: distance
rmPixel opU(rmPixel d1, rmPixel d2)
{
    //return min(d1, d2);
    //return (d1.dist < d2.dist) ? d1 : d2;

    //if (when_lt_float(d1.dist, d2.dist))
    //    return d1;

    //return d2;


    //float d1Weight = when_lt_float(d1.dist, d2.dist);
    //float d2Weight = 1.0 - d1Weight;

    //d1.dist = (d1Weight * d1.dist) + (d2Weight * d2.dist);
    //d1.colour = (d1Weight * d1.colour) + (d2Weight * d2.colour);
    //d1.reflInfo = (d1Weight * d1.reflInfo) + (d2Weight * d2.reflInfo);
    //d1.refractInfo = (d1Weight * d1.refractInfo) + (d2Weight * d2.refractInfo);

    //d1.texID = (d1Weight * d1.texID) + (d2Weight * d2.texID);

    //return d1;




    if (d1.dist > d2.dist)
        d1 = d2;

    return d1;
}

rmPixel opUAbs(rmPixel d1, rmPixel d2)
{
    //return min(d1, d2);
    //return (d1.dist < d2.dist) ? d1 : d2;

    //if (when_lt_float(d1.dist, d2.dist))
    //    return d1;

    //return d2;


    //float d1Weight = when_lt_float(abs(d1.dist), abs(d2.dist));
    //float d2Weight = 1.0 - d1Weight;

    //d1.dist = (d1Weight * d1.dist) + (d2Weight * d2.dist);
    //d1.colour = (d1Weight * d1.colour) + (d2Weight * d2.colour);
    //d1.reflInfo = (d1Weight * d1.reflInfo) + (d2Weight * d2.reflInfo);
    //d1.refractInfo = (d1Weight * d1.refractInfo) + (d2Weight * d2.refractInfo);

    //d1.texID = (d1Weight * d1.texID) + (d2Weight * d2.texID);

    //return d1;




    if (abs(d1.dist) > abs(d2.dist))
        d1 = d2;

    return d1;
}

// Subtraction
rmPixel opS(rmPixel d1, rmPixel d2)
{
    //return max(-d1, d2);


    //float d1Weight = when_gt_float(-d1.dist, d2.dist);
    //float d2Weight = 1.0 - d1Weight;
    

    d1.dist = max(-d1.dist, d2.dist);
    //d1.colour = (d1Weight * d1.colour) + (d2Weight * d2.colour);
    //d1.reflInfo = (d1Weight * d1.reflInfo) + (d2Weight * d2.reflInfo);

    //d1.texID = (d1Weight * d1.texID) + (d2Weight * d2.texID);

    if (-d1.dist < d2.dist)
    {
        d1.colour = d2.colour;
        d1.reflInfo = d2.reflInfo;
        d1.texID = d2.texID;
    }

    return d1;
}

// Intersection
rmPixel opI(rmPixel d1, rmPixel d2)
{
    //return max(d1, d2);


    float d1Weight = when_gt_float(d1.dist, d2.dist);
    float d2Weight = 1.0 - d1Weight;

    d1.dist = (d1Weight * d1.dist) + (d2Weight * d2.dist);
    //d1.colour = (d1Weight * d1.colour) + (d2Weight * d2.colour);
    //d1.reflInfo = (d1Weight * d1.reflInfo) + (d2Weight * d2.reflInfo);

    //d1.texID = (d1Weight * d1.texID) + (d2Weight * d2.texID);

    if (d1.dist <= d2.dist)
    {
        d1.colour = d2.colour;
        d1.reflInfo = d2.reflInfo;
        d1.texID = d2.texID;
    }

    return d1;
}

// Smooth Union
rmPixel opSmoothUnion(rmPixel d1, rmPixel d2, float k)
{
    float h = clamp(0.5 + (0.5 * (d2.dist - d1.dist) / k), 0.0, 1.0);

    d1.dist = lerp(d2.dist, d1.dist, h) - (k * h * (1.0 - h));
    d1.colour = lerp(d2.colour, d1.colour, h);

    //float d1Weight = when_lt_float(d1.dist, d2.dist);
    //float d2Weight = 1.0 - d1Weight;
    //d1.texID = (d1Weight * d1.texID) + (d2Weight * d2.texID);
    if (d1.dist > d2.dist) // TO-DO Check with textured obj
        d1.texID = d2.texID;

    float reflWeight = step(h, 0.5);
    d1.reflInfo = (1.0 - reflWeight) * d1.reflInfo + reflWeight * d2.reflInfo;

    return d1;
}

// Smooth Subtraction
rmPixel opSmoothSub(rmPixel d1, rmPixel d2, float k)
{
    float h = clamp(0.5 - (0.5 * (d2.dist + d1.dist) / k), 0.0, 1.0);

    d1.dist = lerp(d2.dist, -d1.dist, h) + (k * h * (1.0 - h));
    d1.colour = lerp(d2.colour, d1.colour, h);

    //float d1Weight = when_gt_float(d1.dist, d2.dist);
    //float d2Weight = 1.0 - d1Weight;
    //d1.texID = (d1Weight * d1.texID) + (d2Weight * d2.texID);
    if (d1.dist < d2.dist) // TO-DO Check with textured obj
        d1.texID = d2.texID;

    float reflWeight = step(h, 0.5);
    d1.reflInfo = (1.0 - reflWeight) * d1.reflInfo + reflWeight * d2.reflInfo;

    return d1;
}

// Smooth Intersection
rmPixel opSmoothInt(rmPixel d1, rmPixel d2, float k)
{
    float h = clamp(0.5 - (0.5 * (d2.dist - d1.dist) / k), 0.0, 1.0);

    d1.dist = lerp(d2.dist, d1.dist, h) + (k * h * (1.0 - h));
    d1.colour = lerp(d2.colour, d1.colour, h);

    float d1Weight = when_gt_float(d1.dist, d2.dist);
    float d2Weight = 1.0 - d1Weight;
    d1.texID = (d1Weight * d1.texID) + (d2Weight * d2.texID);

    float reflWeight = step(h, 0.5);
    d1.reflInfo = (1.0 - reflWeight) * d1.reflInfo + reflWeight * d2.reflInfo;

    return d1;
}

float opRep(float3 p, float3 c)
{
    float3 q = fmod(p, c) - 0.5 * c;

    //q = p;
    //q.xy = fmod(p.xy, 2.0) - float2(0.5, 0.5) * 2.0;

    return sdSphere(q, 0.5);
}


/// Distance field function.
/// The distance field represents the closest distance to the surface of any object
/// we put in the scene. If the given point (point p) is inside of any object, we return an negative answer.
/// Return.x: Distance field value.
/// Return.y: Colour of closest object (0 - 1).
rmPixel map(float3 p)
{
	rmPixel scene;
	scene.dist = _maxDrawDist;
	scene.colour = float4(0.0, 0.0, 0.0, 0.0);
	scene.reflInfo = float4(0.0, 0.0, 0.0, 0.0);
	scene.refractInfo = float2(0.0, 1.0);
	scene.texID = 0;

	float4 pos = float4(0.0, 0.0, 0.0, 0.0);
	rmPixel obj;
	obj.reflInfo = float4(0.0, 0.0, 0.0, 0.0);
	obj.refractInfo = float2(0.0, 1.0);
	obj.texID = 0;
	float4 geoInfo = float4(0.0, 0.0, 0.0, 0.0);

	rmPixel obj2;
	obj2.reflInfo = float4(0.0, 0.0, 0.0, 0.0);
	obj2.refractInfo = float2(0.0, 1.0);
	obj2.texID = 0;
	rmPixel csg;
	csg.reflInfo = float4(0.0, 0.0, 0.0, 0.0);
	csg.refractInfo = float2(0.0, 1.0);
	csg.texID = 0;
	rmPixel storedCSGs[MAX_CSG_CHILDREN];
	float reflWeight;

	// ######### rmSphere #########
	pos = mul(_invModelMats[0], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[0];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[0];
	obj.reflInfo = _reflInfo[0];
	obj.refractInfo = _refractInfo[0];
	scene = opSmoothUnion(scene, obj, _combineOps[0].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[1], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[1];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[1];
	obj.reflInfo = _reflInfo[1];
	obj.refractInfo = _refractInfo[1];
	scene = opSmoothUnion(scene, obj, _combineOps[1].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[2], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[2];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[2];
	obj.reflInfo = _reflInfo[2];
	obj.refractInfo = _refractInfo[2];
	scene = opSmoothUnion(scene, obj, _combineOps[2].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[3], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[3];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[3];
	obj.reflInfo = _reflInfo[3];
	obj.refractInfo = _refractInfo[3];
	scene = opSmoothUnion(scene, obj, _combineOps[3].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[4], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[4];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[4];
	obj.reflInfo = _reflInfo[4];
	obj.refractInfo = _refractInfo[4];
	scene = opSmoothUnion(scene, obj, _combineOps[4].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[5], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[5];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[5];
	obj.reflInfo = _reflInfo[5];
	obj.refractInfo = _refractInfo[5];
	scene = opSmoothUnion(scene, obj, _combineOps[5].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[6], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[6];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[6];
	obj.reflInfo = _reflInfo[6];
	obj.refractInfo = _refractInfo[6];
	scene = opSmoothUnion(scene, obj, _combineOps[6].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[7], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[7];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[7];
	obj.reflInfo = _reflInfo[7];
	obj.refractInfo = _refractInfo[7];
	scene = opSmoothUnion(scene, obj, _combineOps[7].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[8], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[8];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[8];
	obj.reflInfo = _reflInfo[8];
	obj.refractInfo = _refractInfo[8];
	scene = opSmoothUnion(scene, obj, _combineOps[8].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[9], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[9];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[9];
	obj.reflInfo = _reflInfo[9];
	obj.refractInfo = _refractInfo[9];
	scene = opSmoothUnion(scene, obj, _combineOps[9].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[10], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[10];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[10];
	obj.reflInfo = _reflInfo[10];
	obj.refractInfo = _refractInfo[10];
	scene = opSmoothUnion(scene, obj, _combineOps[10].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[11], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[11];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[11];
	obj.reflInfo = _reflInfo[11];
	obj.refractInfo = _refractInfo[11];
	scene = opSmoothUnion(scene, obj, _combineOps[11].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[12], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[12];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[12];
	obj.reflInfo = _reflInfo[12];
	obj.refractInfo = _refractInfo[12];
	scene = opSmoothUnion(scene, obj, _combineOps[12].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[13], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[13];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[13];
	obj.reflInfo = _reflInfo[13];
	obj.refractInfo = _refractInfo[13];
	scene = opSmoothUnion(scene, obj, _combineOps[13].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[14], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[14];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[14];
	obj.reflInfo = _reflInfo[14];
	obj.refractInfo = _refractInfo[14];
	scene = opSmoothUnion(scene, obj, _combineOps[14].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[15], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[15];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[15];
	obj.reflInfo = _reflInfo[15];
	obj.refractInfo = _refractInfo[15];
	scene = opSmoothUnion(scene, obj, _combineOps[15].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[16], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[16];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[16];
	obj.reflInfo = _reflInfo[16];
	obj.refractInfo = _refractInfo[16];
	scene = opSmoothUnion(scene, obj, _combineOps[16].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[17], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[17];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[17];
	obj.reflInfo = _reflInfo[17];
	obj.refractInfo = _refractInfo[17];
	scene = opSmoothUnion(scene, obj, _combineOps[17].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[18], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[18];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[18];
	obj.reflInfo = _reflInfo[18];
	obj.refractInfo = _refractInfo[18];
	scene = opSmoothUnion(scene, obj, _combineOps[18].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[19], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[19];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[19];
	obj.reflInfo = _reflInfo[19];
	obj.refractInfo = _refractInfo[19];
	scene = opSmoothUnion(scene, obj, _combineOps[19].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[20], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[20];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[20];
	obj.reflInfo = _reflInfo[20];
	obj.refractInfo = _refractInfo[20];
	scene = opSmoothUnion(scene, obj, _combineOps[20].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[21], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[21];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[21];
	obj.reflInfo = _reflInfo[21];
	obj.refractInfo = _refractInfo[21];
	scene = opSmoothUnion(scene, obj, _combineOps[21].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[22], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[22];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[22];
	obj.reflInfo = _reflInfo[22];
	obj.refractInfo = _refractInfo[22];
	scene = opSmoothUnion(scene, obj, _combineOps[22].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[23], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[23];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[23];
	obj.reflInfo = _reflInfo[23];
	obj.refractInfo = _refractInfo[23];
	scene = opSmoothUnion(scene, obj, _combineOps[23].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[24], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[24];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[24];
	obj.reflInfo = _reflInfo[24];
	obj.refractInfo = _refractInfo[24];
	scene = opSmoothUnion(scene, obj, _combineOps[24].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[25], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[25];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[25];
	obj.reflInfo = _reflInfo[25];
	obj.refractInfo = _refractInfo[25];
	scene = opSmoothUnion(scene, obj, _combineOps[25].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[26], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[26];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[26];
	obj.reflInfo = _reflInfo[26];
	obj.refractInfo = _refractInfo[26];
	scene = opSmoothUnion(scene, obj, _combineOps[26].y);
	// ######### rmSphere #########

	// ######### rmSphere #########
	pos = mul(_invModelMats[27], float4(p, 1.0));
	geoInfo = _primitiveGeoInfo[27];
	obj.dist = sdSphere(pos.xyz, geoInfo.x);
	obj.colour = _rm_colours[27];
	obj.reflInfo = _reflInfo[27];
	obj.refractInfo = _refractInfo[27];
	scene = opSmoothUnion(scene, obj, _combineOps[27].y);
	// ######### rmSphere #########

	return scene;
}
/// ######### Signed Distance Functions #########

float ambientOcclusion(float3 p, float3 normal)
{
    float step = _aoStepSize;
    float ao = 0.0;
    float dist;

    for (int i = 1; i <= _aoMaxSteps; ++i)
    {
        dist = step * i;
        ao += max((dist - map(p + (normal * dist)).dist) / dist, 0.0);
    }

    return (1.0 - (ao * _aoIntensity));
}

float shadow(float3 rayOrigin, float3 rayDir, float mint, float maxt, float _penumbraFactor)
{
    float shadowFactor = 1.0;

    for (float t = mint; t < maxt;)
    {
        float h = map(rayOrigin + (rayDir * t)).dist;

        if (h < 0.001)
            return 0.0;

        shadowFactor = min(shadowFactor, _penumbraFactor * h / t);

        t += h;
    }

    return shadowFactor;
}

float3 calcNormal(in float3 pos)
{
    // Epsilon - used to approximate dx when taking the derivative.
    const float2 EPS = float2(0.001, 0.0);

    // Find the "gradient" of the distance field at pos.
    // Remember, the distance field is not boolean - even if you are inside an object
    // the number is negative, so this calculation still works.
    // Essentially you are approximating the derivative of the distance field at this point.
    float3 normal = float3(
                            map(pos + EPS.xyy).dist - map(pos - EPS.xyy).dist,
                            map(pos + EPS.yxy).dist - map(pos - EPS.yxy).dist,
                            map(pos + EPS.yyx).dist - map(pos - EPS.yyx).dist);

    return normalize(normal);
}

float3 triplanarMap(float3 p, float3 normal, sampler2D tex)
{
    float3x3 triSamples = float3x3(
                            tex2D(tex, p.yz).rgb,
                            tex2D(tex, p.xz).rgb,
                            tex2D(tex, p.xy).rgb
                            );

    //return triSamples * abs(normal);
    return mul(abs(normal), triSamples);
}

float4 calcLighting(float3 p, float3 normal, rmPixel distField, float useShadow = 1.0, float useAO = 1.0)
{
    float4 colour = 0.0;

    if (when_gt_int(distField.texID, 0))
    {
        //tex = _textures[0];
        //tex = distField.tex;
        float4 firstTex = float4(triplanarMap(p, normal, _wood), 1.0) * when_eq_int(distField.texID, 1);
        firstTex += float4(triplanarMap(p, normal, _brick), 1.0) * when_eq_int(distField.texID, 2);
        colour += firstTex;
    }

    // Ambient contribution
    colour.rgb += _lightConstants.x * _ambientColour;

    //colour += float4(dot(-_LightDir, normal).rrr, 1.0);
    float NdotL = saturate(dot(-_LightDir, normal));

    float shadowFactor = 1.0;

    // Light contributes diffuse and specular lighting.
    if (NdotL > 0.0)
    {
        float dist = length(_lightPos - p);

        // Calculate light's attenuation
        float attenuation = 1.0 / (_attenuationConstant + (_attenuationLinear * dist) + (_attenuationQuadratic * dist * dist));

        // Apply shadow
        //float3 lightToPoint = p - _lightPos;
        //float lightToPointDist = length(lightToPoint);
        shadowFactor = shadow(p, -_LightDir, _shadowMinDist, 64.0 * useShadow, _penumbraFactor) * 0.5 + 0.5; // * 3;
        shadowFactor = max(0.0, pow(shadowFactor, _shadowIntensity)); // Note: In shaders 0^0 is undefined i.e. 0.
        //colour = float4(shadowFactor.rrr, 1.0);

        // Diffuse Contribution
        //colour.rgb += _lightConstants.y * _diffuseColour * NdotL * attenuation;
        colour.rgb += _diffuseColour * (_lightConstants.y * NdotL * attenuation);

        // Calculate the Half-Angle vector
        float3 H = normalize(-_LightDir + normalize(-p));
        float NdotH = saturate(dot(normal, H));
            
        // Specular Contribution
        //colour.rgb += _lightConstants.z * _specularColour * pow(NdotH, _specularExp) * attenuation;
        colour.rgb += _specularColour * (_lightConstants.z * pow(NdotH, _specularExp) * attenuation);
    }

    // Use y value given by map() to choose a colour from our Colour Ramp.
    colour.rgb += distField.colour.rgb * max(NdotL, 0.2);
    colour.a = distField.colour.a;

    // Apply rim lighting
    float rimFactor = 1.0 - saturate(dot(normal, normalize(_CameraPos.xyz - p)));
    rimFactor = smoothstep(0.5, 1.0, rimFactor);
    colour.rgb += _rimLightColour * rimFactor;

    // Apply shadow
    colour.rgb *= shadowFactor;

    // Apply ambient occlusion.
    float ao = ambientOcclusion(p, normal);
    colour.rgb *= ao;


    //colour.rgb = ao;
    //colour.a = 1.0;
    //colour = float4(shadowFactor.rrr, 1.0);
    return colour;
}

float4 cheapLighting(float3 p, float3 normal, rmPixel distField)
{
    float4 colour = 0.0;

    if (when_gt_int(distField.texID, 0))
    {
        //tex = _textures[0];
        //tex = distField.tex;
        float4 firstTex = float4(triplanarMap(p, normal, _wood), 1.0) * when_eq_int(distField.texID, 1);
        firstTex += float4(triplanarMap(p, normal, _brick), 1.0) * when_eq_int(distField.texID, 2);
        colour += firstTex;
    }

    // Ambient contribution
    colour.rgb += _lightConstants.x * _ambientColour;

    //colour += float4(dot(-_LightDir, normal).rrr, 1.0);
    float NdotL = saturate(dot(-_LightDir, normal));

    // Light contributes diffuse and specular lighting.
    if (NdotL > 0.0)
    {
        float dist = length(_lightPos - p);

        // Calculate light's attenuation
        float attenuation = 1.0 / (_attenuationConstant + (_attenuationLinear * dist) + (_attenuationQuadratic * dist * dist));

        // Diffuse Contribution
        colour.rgb += _diffuseColour * (_lightConstants.y * NdotL * attenuation);

        // Calculate the Half-Angle vector
        float3 H = normalize(-_LightDir + normalize(-p));
        float NdotH = saturate(dot(normal, H));
            
        // Specular Contribution
        colour.rgb += _specularColour * (_lightConstants.z * pow(NdotH, _specularExp) * attenuation);
    }

    // Use y value given by map() to choose a colour from our Colour Ramp.
    colour.rgb += distField.colour.rgb * max(NdotL, 0.2);
    colour.a = distField.colour.a;


    return colour;
}

// Raymarch along the ray
bool raymarch(float3 rayOrigin, float3 rayDir, float depth, int maxSteps, float maxDrawDist, inout float3 p, inout rmPixel distField)
{
    bool rayHit = false;

    //const int MAX_STEP = 100;
    //const float DRAW_DIST = 64.0;
    float t = 0; // Current distance travaled along the ray.

    /// ### Performance Test ###
    //float performance = 0.95;
    /// ### Performance Test ###

    for (int i = 0; i < maxSteps; ++i)
    {
        p = rayOrigin + (rayDir * t); // World space position of sample.
        distField = map(p); // Sample of distance field. d.x: Distance field ouput, d.y: Material data.

        // If we run past the depth buffer, stop and return nothing (transparent pixel).
        // This way raymarched objects and traditional meshes can co-exist.
        if ((t >= depth) || (t > maxDrawDist))
        {
            rayHit = false;

            /// ### Performance Test ###
            //performance = (float) i / MAX_STEP;
            /// ### Performance Test ###

            break;
        }
        // If the sample <= 0, we have hit an object.
        else if (distField.dist < 0.001)
        {
            // Perform colour/lighting
            rayHit = true;

            /// ### Performance Test ###
            //performance = (float) i / MAX_STEP;
            /// ### Performance Test ###

            break;
        }

        // If the sample > 0, we haven't hit anything yet so we should march forward.
        // We step forward by distance d, because d is the minimum distance possible to intersect an object.
        t += distField.dist;
    }

    return rayHit;

    /// ### Performance Test ###
    //colour = float4(tex2D(_performanceRamp, float2(performance, 0.0)).xyz, 1.0);
    /// ### Performance Test ###

    // Perform colour/lighting
    //if (useColourLighting)
    //{
        /// ### Performance Test ###
        //colour = float4(tex2D(_performanceRamp, float2(performance, 0.0)).xyz, 1.0);
        /// ### Performance Test ###
    //}
}

// Raymarch along the ray
bool unsignedRaymarch(float3 rayOrigin, float3 rayDir, float depth, int maxSteps, float maxDrawDist, inout float3 p, inout rmPixel distField)
{
    bool rayHit = false;

    float t = 0; // Current distance travaled along the ray.

    for (int i = 0; i < maxSteps; ++i)
    {
        p = rayOrigin + (rayDir * t); // World space position of sample.
        distField = map(p); // Sample of distance field. d.x: Distance field ouput, d.y: Material data.
        distField.dist = abs(distField.dist);

        // If we run past the depth buffer, stop and return nothing (transparent pixel).
        // This way raymarched objects and traditional meshes can co-exist.
        if ((t >= depth) || (t > maxDrawDist))
        {
            rayHit = false;
            break;
        }
        // If the sample <= 0, we have hit an object.
        else if (distField.dist < 0.001)
        {
            // Perform colour/lighting
            rayHit = true;
            break;
        }

        // If the sample > 0, we haven't hit anything yet so we should march forward.
        // We step forward by distance d, because d is the minimum distance possible to intersect an object.
        t += distField.dist;
    }

    return rayHit;
}

// ior: Index of refraction.
// eta (Greek letter n): Represents a refraction index.
//  eta_i: Index of refraction for the medium we are currently in. (Incident medium)
//  eta_t: Index of refraction for the medium we want to transmit through. (Transmission medium)
float2 fresnel(float ior, float3 incidenceRay, float3 normal)
{
    float2 ratio = 0.0;

    float cos_i = clamp(dot(incidenceRay, normal), -1.0, 1.0);



    // If incidence ray is inside of the medium with the greater refraction index, then swap the indices around.
    //float cos_i_gt_zero_true = when_gt_float(cos_i, 0.0);
    //float cos_i_gt_zero_false = 1.0 - cos_i_gt_zero_true;

    //float eta_i = (cos_i_gt_zero_false * 1.0) + (cos_i_gt_zero_true * ior);
    //float eta_t = (cos_i_gt_zero_true * 1.0) + (cos_i_gt_zero_false * ior);
    float eta_i = 1.0;
    float eta_t = ior;

    if (cos_i > 0)
    {
        eta_i = ior;
        eta_t = 1.0;
    }


    // In the case of total internal reflection, just return 1 for reflection and 0 for refraction.
    float sin_t = (eta_i / eta_t) * sqrt(max(0.0, 1.0 - (cos_i * cos_i)));

    //float sin_t_ge_zero_true = when_ge_float(sin_t, 1.0);
    //float sin_t_ge_zero_false = 1.0 - sin_t_ge_zero_true;

    
    if (sin_t >= 1.0)
        ratio.x = 1.0; //Total internal reflection.
        //float c2SqrtTerm = 1 - pow((eta_i / eta_t), 2.0) * (1.0 - pow(cos_i, 2.0));
        //float c2SqrtTerm_lt_zero = when_lt_float(c2SqrtTerm, 0.0);
    else
    {
        // Fresnel equations
        float cos_t = sqrt(max(0.0, 1.0 - (sin_t * sin_t)));
        cos_i = abs(cos_i);
    
        float parallel = pow(((eta_t * cos_i) - (eta_i * cos_t)) / ((eta_t * cos_i) + (eta_i * cos_t)), 2.0);
        float perpen = pow(((eta_i * cos_t) - (eta_t * cos_i)) / ((eta_i * cos_t) + (eta_t * cos_i)), 2.0);
        ratio.x = saturate((parallel + perpen) * 0.5);
    }

    // Fresnel equations
    //float cos_t = sqrt(max(0.0, 1.0 - (sin_t * sin_t)));
    //cos_i = abs(cos_i);
    
    //float parallel = pow(((eta_t * cos_i) - (eta_i * cos_t)) / ((eta_t * cos_i) + (eta_i * cos_t)), 2.0);
    //float perpen = pow(((eta_i * cos_t) - (eta_t * cos_i)) / ((eta_i * cos_t) + (eta_t * cos_i)), 2.0);
    //ratio.x = saturate((parallel + perpen) * 0.5);

    //ratio.x = (sin_t_ge_zero_true * 1.0) + (sin_t_ge_zero_false * ratio.x);
    ratio.y = 1.0 - ratio.x;

    return ratio;
}

float3 calcRefractRay(float3 i, float3 n, float etat)
{
    float cos_i = clamp(dot(i, n), -1.0, 1.0);
    float etai = 1.0;
  

    //if (cos_i < 0.0)
    //{
    //    cos_i = -cos_i;
    //}
    //else
    //{
    //    etai = etat;
    //    n = -n;
    //}


    float cos_i_lt_zero_true = when_lt_float(cos_i, 0);
    float cos_i_lt_zero_false = 1.0 - cos_i_lt_zero_true;

    // cos_i < 0
    cos_i = (cos_i_lt_zero_true * -cos_i) + (cos_i_lt_zero_false * cos_i);
    // cos_i >= 0
    etai = (cos_i_lt_zero_false * etat) + (cos_i_lt_zero_true * etai);
    n = (cos_i_lt_zero_false * -n) + (cos_i_lt_zero_true * n);


    float eta = etai / etat;
    float k = 1.0 - (eta * eta) * (1.0 - (cos_i * cos_i));
    return k < 0.0 ? 0.0 : (eta * i) + ((eta * cos_i - sqrt(k)) * n);
}

void performReflection(inout float4 add, float3 rayOrigin, float3 rayDir, float3 pos, float3 normal, rmPixel distField, float2 ratio, inout reflectInfo info)
{
    // Distance field reflection.
    bool rayHit = false;
    uint quality;
    float4 refl = distField.reflInfo;
    float prevRefl = 0;

    quality = 2;
    rayDir = normalize(reflect(rayDir, normal));
    rayOrigin = pos + (rayDir * 0.01);
    rayHit = unsignedRaymarch(rayOrigin, rayDir, _maxDrawDist, (_maxSteps * refl.x) / quality, _maxDrawDist / quality, pos, distField);

    if (rayHit)
    {
        normal = calcNormal(pos);
        add += float4(calcLighting(pos, normal, distField).rgb, 0.0) * refl.w * ratio.x;
    }


    info.pos = pos;
    info.normal = normal;
    info.dir = rayDir;
    info.distField = distField;
}

// Will skip the case of if another reflective or refractive is inside another refractive object.
void performRefraction(inout float4 add, float3 rayOrigin, float3 rayDir, float3 pos, float3 normal, rmPixel distField, float2 ratio)
{
    reflectInfo info;

    bool rayHit = false;
    float3 refractRayDir = normalize(calcRefractRay(rayDir, normal, distField.refractInfo.y));
    float3 refractRayOrigin = pos + (refractRayDir * 0.05);


    // March along refraction ray THROUGH the current object (medium).
    rayHit = unsignedRaymarch(refractRayOrigin, refractRayDir, _maxDrawDist, _maxSteps * (int) distField.refractInfo.x, _maxDrawDist, pos, distField);

    // See inside of current object.
    if (rayHit)
    {
        // Add colour of any object hit inside of the current object, or the current object itself.
        normal = calcNormal(pos);
        add += float4(calcLighting(pos, normal, distField, 1.0, 0.0).rgb, 0.0) * ratio.y;

        ratio = fresnel(distField.refractInfo.y, refractRayDir, normal);
        performReflection(add, refractRayOrigin, refractRayDir, pos, -normal, distField, ratio, info);
        //add = float4(ratio.xxx, 1.0);
        //add = float4(ratio.yyy, 1.0);
        //add += float4(1.0, 0.0, 0.0, 1.0);

        // March along refraction ray EXITING the current object (medium).
        refractRayDir = normalize(calcRefractRay(refractRayDir, normal, 1.5));
        refractRayOrigin = pos + (refractRayDir * 0.05);
        rayHit = raymarch(refractRayOrigin, refractRayDir, _maxDrawDist, _maxSteps * (int) distField.refractInfo.x, _maxDrawDist, pos, distField);

        // See behind the current object.
        if (rayHit)
        {
            // Add the colour of what is behind the current object.
            normal = calcNormal(pos);
            add += float4(calcLighting(pos, normal, distField, 1.0, 1.0).rgb, 0.0) * ratio.y;
            //add = float4(ratio.xxx, 1.0);

            ratio = fresnel(distField.refractInfo.y, refractRayDir, normal);
            //ratio.x = 1.0;
            ratio.x = (distField.refractInfo.x > 0.0) ? ratio.x : 1.0;
            performReflection(add, refractRayOrigin, refractRayDir, pos, normal, distField, ratio, info); // TO-DO check if inside object still (Total internal reflection)

            //add += float4(0.0, 1.0, 0.0, 0.0);
            //add = float4(ratio.xxx, 1.0);
            //add = float4(ratio.yyy, 1.0);

            refractRayDir = normalize(calcRefractRay(refractRayDir, normal, 1.5));
            refractRayOrigin = pos + (refractRayDir * 0.05);
            rayHit = raymarch(refractRayOrigin, refractRayDir, _maxDrawDist, _maxSteps * (int) distField.refractInfo.x, _maxDrawDist, pos, distField);

            if (rayHit)
            {
                normal = calcNormal(pos);
                add += float4(calcLighting(pos, normal, distField, 1.0, 1.0).rgb, 0.0) * ratio.y;
            }
        }
    }
}

void ReflectAndRefract(inout float4 add, float3 rayOrigin, float3 rayDir, float3 pos, float3 normal, rmPixel distField, float2 ratio)
{
    reflectInfo info;

    // Calculate refraction.
    bool rayHit = false;
    float3 refractRayDir = normalize(calcRefractRay(rayDir, normal, distField.refractInfo.y));
    refractRayDir = normalize(refract(rayDir, normal, 1.0 / distField.refractInfo.y));
    float3 refractRayOrigin = pos + (refractRayDir * 0.05);

    // March along refraction ray THROUGH the current object (medium).
    rayHit = unsignedRaymarch(refractRayOrigin, refractRayDir, _maxDrawDist, _maxSteps * (int) distField.refractInfo.x, _maxDrawDist, pos, distField);


    // See inside of current object.
    if (rayHit)
    {
        // Add colour of any object hit inside of the current object, or the current object itself.
        normal = calcNormal(pos);
        add += float4(calcLighting(pos, normal, distField, 1.0, 0.0).rgb, 0.0) * ratio.y;

        // Calculate frensel ratio.
        ratio = fresnel(distField.refractInfo.y, refractRayDir, normal);

        // Calculate reflection.
        performReflection(add, refractRayOrigin, refractRayDir, pos, -normal, distField, ratio, info);

        // Calculate refraction for reflection.
        info.dir = normalize(calcRefractRay(info.dir, info.normal, distField.refractInfo.y));
        //info.dir = normalize(refract(info.dir, -info.normal, distField.refractInfo.y / 1.0));
        info.pos = info.pos + (info.dir * 0.05);

        if (dot(info.dir, info.normal) > 0.0)
        {
            rayHit = raymarch(refractRayOrigin, refractRayDir, _maxDrawDist, _maxSteps * (int) info.distField.refractInfo.x, _maxDrawDist, info.pos, info.distField);
            if (rayHit)
            {
                float2 reflectRatio = fresnel(info.distField.refractInfo.y, info.dir, info.normal);
                //normal = calcNormal(reflectPos);
                add += float4(calcLighting(info.pos, info.normal, info.distField, 1.0, 1.0).rgb, 0.0) * reflectRatio.y * ratio.x;
                //add = float4(reflectRatio.yyy, 1.0);
            }
        }


        // Calculate refraction.
        // March along refraction ray EXITING the current object (medium).
        //refractRayDir = normalize(calcRefractRay(refractRayDir, normal, distField.refractInfo.y));
        refractRayDir = normalize(refract(refractRayDir, -normal, distField.refractInfo.y / 1.0));
        refractRayOrigin = pos + (refractRayDir * 0.05);
        //float refractive = when
        rayHit = raymarch(refractRayOrigin, refractRayDir, _maxDrawDist, _maxSteps * (int) distField.refractInfo.x, _maxDrawDist, pos, distField);


        // See behind the current object.
        if (rayHit)
        {
            // Add the colour of what is behind the current object.
            normal = calcNormal(pos);
            add += float4(calcLighting(pos, normal, distField, 1.0, 1.0).rgb, 0.0) * ratio.y * 0.6;
            //add = float4(pos, 1.0);

            // Calculate frensel ratio.
            ratio = fresnel(distField.refractInfo.y, refractRayDir, normal) * ratio.x;
            ratio.x = (distField.refractInfo.x > 0.0) ? ratio.x : 1.0;

            // Calculate reflection.
            //if (dot(refractRayDir, normal) < 0.0)
            //{
            //    performReflection(add, refractRayOrigin, refractRayDir, pos, normal, distField, ratio, reflectPos, reflectNormal, reflectDir, reflectDistField); // TO-DO check if inside object still (Total internal reflection)


            //    // Calculate refraction.
            //    refractRayDir = normalize(calcRefractRay(refractRayDir, normal, distField.refractInfo.y));
            //    refractRayOrigin = pos + (refractRayDir * 0.05);
            //    rayHit = raymarch(refractRayOrigin, refractRayDir, _maxDrawDist, _maxSteps * (int) distField.refractInfo.x, _maxDrawDist, pos, distField);


            //    if (rayHit)
            //    {
            //        normal = calcNormal(pos);
            //        add += float4(calcLighting(pos, normal, distField, 1.0, 1.0).rgb, 0.0) * ratio.y;
            //    }
            //}
        }
    }
}

void cheapRefract(inout float4 add, float3 rayOrigin, float3 rayDir, float3 pos, float3 normal, rmPixel distField, float2 ratio)
{
    reflectInfo info;

    // Calculate refraction.
    bool rayHit = false;
    float3 refractRayDir = normalize(calcRefractRay(rayDir, normal, distField.refractInfo.y));
    refractRayDir = normalize(refract(rayDir, normal, 1.0 / distField.refractInfo.y));
    float3 refractRayOrigin = pos + (refractRayDir * 0.02);

    // March along refraction ray THROUGH the current object (medium).
    rayHit = unsignedRaymarch(refractRayOrigin, refractRayDir, _maxDrawDist, _maxSteps * (int) distField.refractInfo.x, _maxDrawDist, pos, distField);


    // See inside of current object.
    if (rayHit)
    {
        //ratio.y = 
        // Add colour of any object hit inside of the current object, or the current object itself.
        normal = calcNormal(pos);
        //add += float4(calcLighting(pos, normal, distField, 1.0, 0.0).rgb, 0.0) * ratio.y;
        add += float4(cheapLighting(pos, normal, distField).rgb, 0.0) * ratio.y;

        // Calculate frensel ratio.
        ratio = fresnel(distField.refractInfo.y, refractRayDir, normal);
        //ratio.y = 1.0;
        // Calculate reflection.
        //performReflection(add, refractRayOrigin, refractRayDir, pos, -normal, distField, ratio, info);

        //if (dot(info.dir, info.normal) > 0.0)
        //{
        //    float3 dir = normalize(refract(info.dir, -info.normal, distField.refractInfo.y));
        //    dir = normalize(calcRefractRay(info.dir, info.normal, distField.refractInfo.y));
        //    float origin = info.pos + (dir * 0.05);

        //    rayHit = raymarch(origin, dir, _maxDrawDist, _maxSteps * (int) distField.refractInfo.x, _maxDrawDist, info.pos, info.distField);

        //    if (rayHit)
        //    {
        //        float3 norm = calcNormal(info.pos);
        //        add += float4(calcLighting(info.pos, norm, info.distField, 1.0, 1.0).rgb, 0.0) * 0.2;
        //        //add = float4(0.0, 0.0, 0.0, 1.0);
        //    }
        //}

        // Calculate refraction.
        // March along refraction ray EXITING the current object (medium).
        //refractRayDir = normalize(calcRefractRay(refractRayDir, normal, distField.refractInfo.y));
        refractRayDir = normalize(refract(refractRayDir, -normal, distField.refractInfo.y)); //distField.refractInfo.y / 1.0
        refractRayOrigin = pos + (refractRayDir * 0.02);

        rayHit = raymarch(refractRayOrigin, refractRayDir, _maxDrawDist, _maxSteps * (int) distField.refractInfo.x, _maxDrawDist, pos, distField);

        // See behind the current object.
        if (rayHit)
        {
            // Add the colour of what is behind the current object.
            normal = calcNormal(pos);
            add += float4(calcLighting(pos, normal, distField, 1.0, 1.0).rgb, 0.0) * (ratio.y * 0.6);
        }
    }
}


// Vertex program
VertexOutput vert(VertexInput input)
{
    VertexOutput output;

    float index = input.pos.z;
    input.pos.z = 0.0;

    float4 worldPos = mul(unity_ObjectToWorld, float4(input.pos.xyz, 1.0));
    output.pos = mul(unity_MatrixVP, worldPos);
    //output.pos = mul(UNITY_MATRIX_MVP, float4(input.pos.xyz, 1.0));
    output.uv = input.uv;


    #if UNITY_UV_STARTS_AT_TOP
    if (_MainTex_TexelSize.y < 0)
        output.uv.y = 1 - output.uv.y;
    #endif

    // Get the eyespace view ray (normalized)
    output.ray = _FrustumCornersES[(int) index].xyz;

    // Dividing by z "normalizes" it in the z axis.
    // Therefore multiplying the ray by some number i gives the viewspace position
    // of the point on the ray with [viewspace z] = i.
    output.ray /= abs(output.ray.z);

    // Transform the ray from eyespace to worldspace
    output.ray = mul(_CameraInvMatrix, float4(output.ray.xyz, 0.0)).xyz;

    return output;
}

// Fragment program
float4 frag(VertexOutput input) : SV_Target
{
    // Ray direction
    float3 rayDir = normalize(input.ray);
    // Ray origin
    float3 rayOrigin = _CameraPos.xyz;


    float2 uv = input.uv;

    #if UNITY_UV_STARTS_AT_TOP
    if (_MainTex_TexelSize.y < 0)
        uv.y = 1 - uv.y;
    #endif

    // Convert from depth buffer (eye space) to true distance from camera.
    // This is done by multiplying the eyespace depth by the length of the "z-normalized" ray.
    // Think of similar triangles:
    // The view-space z-distance between a point and the camera is proportional to the absolute distance.
    float depth = LinearEyeDepth(tex2D(_CameraDepthTexture, uv).r);
    depth *= length(input.ray);

    // Colour of the scene before this shader was run
    float4 col = tex2D(_MainTex, input.uv);

    // March a ray for each pixel, and check whether anything was hit.
    float3 p = 0.0;
    rmPixel distField;
    float4 add = float4(0.0, 0.0, 0.0, 0.0);
    float3 normal = 0.0;
    bool rayHit = raymarch(rayOrigin, rayDir, depth, _maxSteps, _maxDrawDist, p, distField);

    // Perform shading/lighting.
    if (rayHit)
    {
        normal = calcNormal(p);
        add = calcLighting(p, normal, distField);

        float2 ratio = fresnel(distField.refractInfo.y, rayDir, normal);
        ratio.x = (distField.refractInfo.x > 0.0) ? ratio.x : 1.0;

        //performRefraction(add, rayOrigin, rayDir, p, normal, distField, ratio);
        //ReflectAndRefract(add, rayOrigin, rayDir, p, normal, distField, ratio);
        cheapRefract(add, rayOrigin, rayDir, p, normal, distField, ratio);

		// Distance field reflection.
		float quality;
		float4 refl = distField.reflInfo;
		float prevRefl = 0;

		quality = 0.5;
		rayDir = normalize(reflect(rayDir, normal));
		rayOrigin = p + (rayDir * 0.01);
		rayHit = raymarch(rayOrigin, rayDir, _maxDrawDist, (_maxSteps * refl.x) * quality, _maxDrawDist * quality, p, distField);

		if (rayHit)
		{
			normal = calcNormal(p);
			add += float4(calcLighting(p, normal, distField).rgb, 0.0) * refl.w * ratio.x;//_reflectionIntensity;
		}
		// Skybox reflection.
		//add += float4(texCUBE(_skybox, ogNormal).rgb * _envReflIntensity * _reflectionIntensity, 0.0) * (1.0 - rayHit) * refl.x * prevRefl;
    }

    //add = float4(tex2D(_performanceRamp, float2(distField.dist, 0.0)).xyz, 1.0);

    // Returns final colour using alpha blending.
    //col = float4((col.xyz * (1.0 - add.w)) + (add.xyz * (add.w)), 1.0);
    col = float4(lerp(col.rgb, add.rgb, add.a), 1.0);



    // Gamma correction
    //add.rgb = pow(add.rgb, float3(0.4545, 0.4545, 0.4545));

    // Contrast
    //add.rgb = smoothstep(0.0, 1.0, add.rgb);

    float2 test = abs(input.uv - float2(0.5, 0.5));
    //if (test.x >= 0.4 || test.y >= 0.4)
    //    add.rgb *= 0.2;

    //col.rgb += (smoothstep(0.4, 0.5, test.x) * _vignetteIntensity) * float3(1.0, 0.0, 0.0);
    //col.rgb += (smoothstep(0.4, 0.5, test.y) * _vignetteIntensity) * float3(1.0, 0.0, 0.0);

    col.rgb += (smoothstep(0.4, 0.5, test.x) * _vignetteIntensity) * float3(1.0, 0.0, 0.0);
    col.rgb += (smoothstep(0.4, 0.5, test.y) * _vignetteIntensity) * float3(1.0, 0.0, 0.0);


    return col;
}
