float dot2(float2 v)
{
    return dot(v, v);
}

float dot2(float3 v)
{
    return dot(v, v);
}

// Sphere
// s: size/diameter
float sdSphere(float3 p, float s)
{
    return length(p) - s;
}

// Box
// b: size of box in x/y/z
float sdBox(float3 p, float3 b)
{
    float3 d = abs(p) - b;
    
    return min(max(d.x, max(d.y, d.z)), 0.0) + length(max(d, 0.0));
}

float sdRoundBox(float3 pos, float3 geoInfo, float roundness)
{
    float3 d = abs(pos) - geoInfo;
    return length(max(d, 0.0)) - roundness
           + min(max(d.x, max(d.y, d.z)), 0.0);
}

// Torus
// t.x: diameter
// t.y: thickness
float sdTorus(float3 p, float2 t)
{
    float2 q = float2(length(p.xz) - t.x, p.y);
    return length(q) - t.y;
}

float sdCappedTorus(float3 pos, float2 sc, float ra, float rb)
{
    pos.x = abs(pos.x);
    float k = ((sc.y * pos.x) > (sc.x * pos.y)) ? dot(pos.xy, sc) : length(pos.xy);
    return sqrt(dot(pos, pos) + (ra * ra) - (2.0 * ra * k)) - rb;
}

float sdLink(float3 pos, float le, float r1, float r2)
{
    float3 q = float3(pos.x, max(abs(pos.y) - le, 0.0), pos.z);
    return length(float2(length(q.xy) - r1, q.z)) - r2;
}

// Cylinder
// h:
// r:
float sdCylinder(float3 p, float h, float r)
{
    float2 d = abs(float2(length(p.xz), p.y)) - float2(h, r);
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0));
}

float sdCappedCylinder(float3 pos, float h, float r)
{
    float2 d = abs(float2(length(pos.xz), pos.y)) - float2(h, r);
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0));
}

float sdCappedCylinder(float3 pos, float3 a, float3 b, float r)
{
    float3 ba = b - a;
    float3 pa = pos - a;
    float baba = dot(ba, ba);
    float paba = dot(pa, ba);
    float x = length((pa * baba) - (ba * paba)) - (r * baba);
    float y = abs(paba - (baba * 0.5)) - (baba * 0.5);
    float x2 = x * x;
    float y2 = y * y * baba;
    float d = (max(x, y) < 0.0) ? -min(x2, y2) : (((x > 0.0) ? x2 : 0.0) + ((y > 0.0) ? y2 : 0.0));
    return sign(d) * sqrt(abs(d)) / baba;
}

float sdRoundedCylinder(float3 pos, float ra, float rb, float h)
{
    float2 d = float2(length(pos.xz) - (2.0 * ra) + rb, abs(pos.y) - h);
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - rb;
}

float sdCone(float3 pos, float2 c)
{
    // c is the sin/cos of the angle
    float q = length(pos.xy);
    return dot(c, float2(q, pos.z));
}

float sdCappedCone(float3 pos, float h, float r1, float r2)
{
    float2 q = float2(length(pos.xz), pos.y);

    float2 k1 = float2(r2, h);
    float2 k2 = float2(r2 - r1, 2.0 * h);
    float2 ca = float2(q.x - min(q.x, (q.y < 0.0) ? r1 : r2), abs(q.y) - h);
    float2 cb = q - k1 + (k2 * clamp(dot(k1 - q, k2) / dot2(k2), 0.0, 1.0));
    float s = (cb.x < 0.0 && ca.y < 0.0) ? -1.0 : 1.0;
    return s * sqrt(min(dot2(ca), dot2(cb)));
}

float sdRoundCone(float3 pos, float r1, float r2, float h)
{
    float2 q = float2(length(pos.xz), pos.y);

    float b = (r1 - r2) / h;
    float a = sqrt(1.0 - (b * b));
    float k = dot(q, float2(-b, a));

    if (k < 0.0)
        return length(q) - r1;

    if (k > (a * h))
        return length(q - float2(0.0, h)) - r2;

    return dot(q, float2(a, b)) - r1;
}

float sdPlane(float3 pos, float4 n)
{
    // n must be normalized
    return dot(pos, n.xyz);
}

float sdHexagonalPrism(float3 pos, float2 h)
{
    const float3 k = float3(-0.8660254, 0.5, 0.57735);
    pos = abs(pos);
    pos.xy -= 2.0 * min(dot(k.xy, pos.xy), 0.0) * k.xy;
    float2 d = float2(length(pos.xy - float2(clamp(pos.x, -k.z * h.x, k.z * h.x), h.x)) * sign(pos.y - h.x),
                        pos.z - h.y);
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0));
}

float sdTriangularPrism(float3 pos, float2 h)
{
    float3 q = abs(pos);
    return max(q.z - h.y, max((q.x * 0.866025) + (pos.y * 0.5), -pos.y) - h.x * 0.5);
}

float sdCapsule(float3 pos, float3 a, float3 b, float r)
{
    float3 pa = pos - a, ba = b - a;
    float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    return length(pa - (ba * h)) - r;
}

float sdVerticalCapsule(float3 pos, float h, float r)
{
    pos.y -= clamp(pos.y, 0.0, h);
    return length(pos) - r;
}

float sdSolidAngle(float3 pos, float2 c, float ra)
{
    // c is the sin/cos of the angle
    float2 q = float2(length(pos.xz), pos.y);
    float l = length(q) - ra;
    float m = length(q - (c * clamp(dot(q, c), 0.0, ra)));
    return max(l, m * sign((c.y * q.x) - (c.x * q.y)));
}

float sdEllipsoid(float3 pos, float3 r)
{
    float k0 = length(pos / r);
    float k1 = length(pos / (r * r));
    
    return k0 * (k0 - 1.0) / k1;
}

float sdOctahedron(float3 pos, float s)
{
    pos = abs(pos);
    float m = pos.x + pos.y + pos.z - s;
    float3 q;

    if ((3.0 * pos.x) < m)
        q = pos.xyz;
    else if ((3.0 * pos.y) < m)
        q = pos.yzx;
    else if ((3.0 * pos.z) < m)
        q = pos.zxy;

    float k = clamp(0.5 * (q.z - q.y + s), 0.0, s);
    
    return length(float3(q.x, q.y - s + k, q.z - k));
}

float sdOctahedronBound(float3 pos, float s)
{
    pos = abs(pos);
    
    return (pos.x + pos.y + pos.z - s) * 0.57735027;
}

float sdTriangle(float3 pos, float3 a, float3 b, float3 c)
{
    float3 ba = b - a;
    float3 pa = pos - a;
    float3 cb = c - b;
    float3 pb = pos - b;
    float3 ac = a - c;
    float3 pc = pos - c;
    float3 nor = cross(ba, ac);

    return sqrt(
                (sign(dot(cross(ba, nor), pa)) +
                sign(dot(cross(cb, nor), pb)) +
                sign(dot(cross(ac, nor), pc)) < 2.0)
                ?
                min(min(
                dot2(ba * clamp(dot(ba, pa) / dot2(ba), 0.0, 1.0) - pa),
                dot2(cb * clamp(dot(cb, pb) / dot2(cb), 0.0, 1.0) - pb)),
                dot2(ac * clamp(dot(ac, pc) / dot2(ac), 0.0, 1.0) - pc))
                :
                dot(nor, pa) * dot(nor, pa) / dot2(nor));
}

float sdQuad(float3 pos, float3 a, float3 b, float3 c, float3 d)
{
    float3 ba = b - a;
    float3 pa = pos - a;
    float3 cb = c - b;
    float3 pb = pos - b;
    float3 dc = d - c;
    float3 pc = pos - c;
    float3 ad = a - d;
    float3 pd = pos - d;
    float3 nor = cross(ba, ad);

    return sqrt(
                (sign(dot(cross(ba, nor), pa)) +
                sign(dot(cross(cb, nor), pb)) +
                sign(dot(cross(dc, nor), pc)) +
                sign(dot(cross(ad, nor), pd)) < 3.0)
                ?
                min(min(min(
                dot2(ba * clamp(dot(ba, pa) / dot2(ba), 0.0, 1.0) - pa),
                dot2(cb * clamp(dot(cb, pb) / dot2(cb), 0.0, 1.0) - pb)),
                dot2(dc * clamp(dot(dc, pc) / dot2(dc), 0.0, 1.0) - pc)),
                dot2(ad * clamp(dot(ad, pd) / dot2(ad), 0.0, 1.0) - pd))
                :
                dot(nor, pa) * dot(nor, pa) / dot2(nor));
}




void opElongate1D(inout float3 pos, float3 h)
{
    pos -= clamp(pos, -h, h);
}

float4 opElongate(float3 pos, float3 h)
{
    float3 q = abs(pos) - h;

    return float4(max(q, 0.0), min(max(q.x, max(q.y, q.z)), 0.0));
}

void opRound(inout float dist, float rad)
{
    // return primitive(pos) - rad;
    //return float4(pos, -rad);
    dist -= rad;
}

void opOnion(inout float dist, float thickness)
{
    // return abs(sdf) - thickness;
    //return float4(abs(pos), -thickness);
    dist = abs(dist) - thickness;
}

void opSymX(inout float3 pos)
{
    pos.x = abs(pos.x);
}

void opSymXZ(inout float3 pos)
{
    pos.xz = abs(pos.xz);
}

void opRep(inout float3 pos, float3 c)
{
    pos.x = modf(pos.x, c.x) - (c.x * 0.5);
    //pos = modf(pos, c) - (0.5 * c);
}

void opRepLim(inout float3 pos, float3 c, float3 l)
{
    pos = pos - (c * clamp(round(pos / c), -l, l));
}

void opDisplace(float3 pos, inout float dist, float3 c)
{
    //float disp = displacement(pos);
    float disp = sin(c.x * pos.x) * sin(c.y * pos.y) * sin(c.z * pos.z);

    dist += disp;
}

void opTwist(inout float3 pos, float k)
{
    //const float k = 2.0; // or some other amount
    float c = cos(k * pos.y);
    float s = sin(k * pos.y);
    float2x2 m = float2x2(c, -s, s, c);
    pos = float3(mul(pos.xz, m), pos.y);
}

void opCheapBend(inout float3 pos, float k)
{
    //const float k = 0.2; // or some other amount
    float c = cos(k * pos.x);
    float s = sin(k * pos.x);
    float2x2 m = float2x2(c, -s, s, c);
    //pos = float3(m * pos.xy, pos.z);
    pos = float3(mul(pos.xy, m), pos.z);
}