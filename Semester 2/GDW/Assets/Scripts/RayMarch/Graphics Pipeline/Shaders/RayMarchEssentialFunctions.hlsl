struct rmPixel
{
    float dist;
    float4 colour;
    float4 reflInfo;
    float2 refractInfo;
    int texID;
    float totalDist;
};


// ######### Ray March Variables #########
int _maxSteps = 600.0;
float _maxDrawDist = 600.0;
// ######### Ray March Variables #########
float3 _CameraPos;


float cheapMap(float3 p);
float map(float3 p);
rmPixel mapMat();


// Union
// .x: distance
float opU(float d1, float d2)
{
    return min(d1, d2);
}

float opUAbs(float d1, float d2)
{
    //if (abs(d1) > abs(d2))
    //    d1 = d2;

    //return d1;

    return abs(d1) > abs(d2) ? d2 : d1;

    //return min(abs(d1), abs(d2));
}

// Subtraction
float opS(float d1, float d2)
{
    //return max(-d1, d2);
    
    //d1.dist = max(-d1.dist, d2.dist);

    return max(-d1, d2);
}

// Intersection
float opI(float d1, float d2)
{
    //return max(d1, d2);

    //d1.dist = max(d1.dist, d2.dist);

    return max(d1, d2);
}

// Smooth Union
float opSmoothUnion(float d1, float d2, float k)
{
    float h = clamp(0.5 + (0.5 * (d2 - d1) / k), 0.0, 1.0);

    //d1.dist = lerp(d2.dist, d1.dist, h) - (k * h * (1.0 - h));

    return lerp(d2, d1, h) - (k * h * (1.0 - h));;
}

// Smooth Subtraction
float opSmoothSub(float d1, float d2, float k)
{
    float h = clamp(0.5 - (0.5 * (d2 + d1) / k), 0.0, 1.0);

    //d1 = lerp(d2, -d1, h) + (k * h * (1.0 - h));

    return lerp(d2, -d1, h) + (k * h * (1.0 - h));;
}

// Smooth Intersection
float opSmoothInt(float d1, float d2, float k)
{
    float h = clamp(0.5 - (0.5 * (d2 - d1) / k), 0.0, 1.0);

    //d1.dist = lerp(d2.dist, d1.dist, h) + (k * h * (1.0 - h));

    return lerp(d2, d1, h) + (k * h * (1.0 - h));;
}

//float opRep(float3 p, float3 c)
//{
//    float3 q = fmod(p, c) - 0.5 * c;

//    //q = p;
//    //q.xy = fmod(p.xy, 2.0) - float2(0.5, 0.5) * 2.0;

//    return sdSphere(q, 0.5);
//}


// Union for materials
// .x: distance
rmPixel opUMat(rmPixel d1, rmPixel d2)
{
    //d1.dist = min(d1.dist, d2.dist);

    //float d1Weight = when_le_float(d1.dist, d2.dist);
    //float d2Weight = 1.0 - d1Weight;

    //d1.dist = (d1.dist * d1Weight) + (d2.dist * (1.0 - d2Weight));
    //d1.colour = (d1.colour * d1Weight) + (d2.colour * (1.0 - d2Weight));
    
    d1.colour = d1.dist < d2.dist ? d1.colour : d2.colour;
    d1.reflInfo = d1.dist < d2.dist ? d1.reflInfo : d2.reflInfo;
    d1.refractInfo = d1.dist < d2.dist ? d1.refractInfo : d2.refractInfo;
    d1.dist = d1.dist < d2.dist ? d1.dist : d2.dist;

    //if (d1.dist > d2.dist)
    //    d1 = d2;

    return d1;
}

rmPixel opUAbsMat(rmPixel d1, rmPixel d2)
{
    //if (abs(d1) > abs(d2))
    //    d1 = d2;

    d1.colour = abs(d1.dist) > abs(d2.dist) ? d2.colour : d1.colour;
    d1.reflInfo = abs(d1.dist) > abs(d2.dist) ? d2.reflInfo : d1.reflInfo;
    d1.refractInfo = abs(d1.dist) > abs(d2.dist) ? d2.refractInfo : d1.refractInfo;
    d1.dist = abs(d1.dist) > abs(d2.dist) ? d2.dist : d1.dist;

    return d1;
}

// Subtraction for materials
rmPixel opSMat(rmPixel d1, rmPixel d2)
{
    //return max(-d1, d2);
    
    //d1.dist = max(-d1.dist, d2.dist);;

    float d1Weight = when_gt_float(-d1.dist, d2.dist);
    float d2Weight = 1.0 - d1Weight;

    d1.dist = (d1Weight * d1.dist) + (d2Weight * d2.dist);
    d1.colour = (d1Weight * d1.colour) + (d2Weight * d2.colour);
    d1.reflInfo = (d1Weight * d1.reflInfo) + (d2Weight * d2.reflInfo);
    d1.refractInfo = (d1Weight * d1.refractInfo) + (d2Weight * d2.refractInfo);

    //if (-d1.dist < d2.dist)
    //{
    //    d1 = d2;
    //}

    return d1;
}

// Intersection for materials
rmPixel opIMat(rmPixel d1, rmPixel d2)
{
    //return max(d1, d2);


    float d1Weight = when_gt_float(d1.dist, d2.dist);
    float d2Weight = 1.0 - d1Weight;

    d1.dist = (d1Weight * d1.dist) + (d2Weight * d2.dist);
    d1.colour = (d1Weight * d1.colour) + (d2Weight * d2.colour);
    d1.reflInfo = (d1Weight * d1.reflInfo) + (d2Weight * d2.reflInfo);
    d1.refractInfo = (d1Weight * d1.refractInfo) + (d2Weight * d2.refractInfo);

    //d1.dist = max(d1.dist, d2.dist);


    //if (d1.dist <= d2.dist)
    //{
    //    d1 = d2;
    //}

    return d1;
}

// Smooth Union for materials
rmPixel opSmoothUnionMat(rmPixel d1, rmPixel d2, float k)
{
    float h = clamp(0.5 + (0.5 * (d2.dist - d1.dist) / k), 0.0, 1.0);

    d1.dist = lerp(d2.dist, d1.dist, h) - (k * h * (1.0 - h));
    d1.colour = lerp(d2.colour, d1.colour, h);

    //float d1Weight = when_lt_float(d1.dist, d2.dist);
    //float d2Weight = 1.0 - d1Weight;
    //d1.texID = (d1Weight * d1.texID) + (d2Weight * d2.texID);
    //if (d1.dist > d2.dist) // TO-DO Check with textured obj
    //    d1.texID = d2.texID;

    float reflWeight = step(h, 0.5);
    d1.reflInfo = (1.0 - reflWeight) * d1.reflInfo + reflWeight * d2.reflInfo;
    d1.refractInfo = (1.0 - reflWeight) * d1.refractInfo + reflWeight * d2.refractInfo;

    return d1;
}

// Smooth Subtraction for materials
rmPixel opSmoothSubMat(rmPixel d1, rmPixel d2, float k)
{
    float h = clamp(0.5 - (0.5 * (d2.dist + d1.dist) / k), 0.0, 1.0);

    d1.dist = lerp(d2.dist, -d1.dist, h) + (k * h * (1.0 - h));
    d1.colour = lerp(d2.colour, d1.colour, h);

    //float d1Weight = when_gt_float(d1.dist, d2.dist);
    //float d2Weight = 1.0 - d1Weight;
    //d1.texID = (d1Weight * d1.texID) + (d2Weight * d2.texID);

    //if (d1.dist < d2.dist) // TO-DO Check with textured obj
    //    d1.texID = d2.texID;

    float reflWeight = step(h, 0.5);
    d1.reflInfo = (1.0 - reflWeight) * d1.reflInfo + reflWeight * d2.reflInfo;
    d1.refractInfo = (1.0 - reflWeight) * d1.refractInfo + reflWeight * d2.refractInfo;

    return d1;
}

// Smooth Intersection for materials
rmPixel opSmoothIntMat(rmPixel d1, rmPixel d2, float k)
{
    float h = clamp(0.5 - (0.5 * (d2.dist - d1.dist) / k), 0.0, 1.0);

    //d1.dist = lerp(d2.dist, d1.dist, h) + (k * h * (1.0 - h));

    d1.dist = lerp(d2.dist, d1.dist, h) + (k * h * (1.0 - h));
    d1.colour = lerp(d2.colour, d1.colour, h);

    float reflWeight = step(h, 0.5);
    d1.reflInfo = (1.0 - reflWeight) * d1.reflInfo + reflWeight * d2.reflInfo;
    d1.refractInfo = (1.0 - reflWeight) * d1.refractInfo + reflWeight * d2.refractInfo;

    return d1;
}





// Raymarch along the ray
int raymarch(float3 rayOrigin, float3 rayDir, float depth, int maxSteps, float maxDrawDist, inout float3 p, inout rmPixel distField, int rayHit)
{
    //int rayHit = 0;

    //const int MAX_STEP = 100;
    //const float DRAW_DIST = 64.0;
    float totalDist = distField.totalDist; // Current distance travaled along the ray.

    /// ### Performance Test ###
    //float performance = 0.95;
    /// ### Performance Test ###

    int2 bothCond;
    int2 breakLoop;

//#if BOUNDING_SPHERE_DEBUG
//    int test = rayHit;
//#endif


    // Only march through actual map, iff the ray hit an object in the cheap map.
    for (int i = 0 + ((1 - rayHit) * maxSteps); i < maxSteps; ++i)
    {
        p = rayOrigin + (rayDir * totalDist); // World space position of sample.
        distField.dist = map(p); // Sample of distance field. d.x: Distance field ouput, d.y: Material data.

        // If we run past the depth buffer, stop and return nothing (transparent pixel).
        // This way raymarched objects and traditional meshes can co-exist.
        bothCond = when_ge_float(float4(totalDist, totalDist, 0.0, 0.0), float4(depth, maxDrawDist, 0.0, 0.0)).xy;
        breakLoop.x = saturate(bothCond.x + bothCond.y);
        i += maxSteps * breakLoop.x;
        //if ((totalDist >= depth) || (totalDist > maxDrawDist))
        //{
        //    rayHit = false;

        //    /// ### Performance Test ###
        //    //performance = (float) i / MAX_STEP;
        //    /// ### Performance Test ###

        //    break;
        //}
        // If the sample <= 0, we have hit an object.
        breakLoop.y = when_lt_float(float4(distField.dist, 0.0, 0.0, 0.0), float4(0.001, 0.0, 0.0, 0.0)).x;
        i += maxSteps * breakLoop.y;
        //else if (distField.dist < 0.001)
        //{
        //    // Perform colour/lighting
        //    rayHit = true;

        //    /// ### Performance Test ###
        //    //performance = (float) i / MAX_STEP;
        //    /// ### Performance Test ###

        //    break;
        //}

        rayHit = saturate((1 - breakLoop.x) + breakLoop.y);

        // If the sample > 0, we haven'totalDist hit anything yet so we should march forward.
        // We step forward by distance d, because d is the minimum distance possible to intersect an object.
        totalDist += distField.dist;
    }

    //determineMaterial(distField);
    distField = mapMat();
    distField.totalDist = totalDist;



//#if BOUNDING_SPHERE_DEBUG
//    if (test)
//    {
//        distField.colour.rgb += float3(0.6, 0.0, 0.0);
//        distField.colour.a = 1.0;
//        rayHit = test;
//    }
//#endif

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
int unsignedRaymarch(float3 rayOrigin, float3 rayDir, float depth, int maxSteps, float maxDrawDist, inout float3 p, inout rmPixel distField)
{
    int rayHit = 0;

    //const int MAX_STEP = 100;
    //const float DRAW_DIST = 64.0;
    float t = 0; // Current distance travaled along the ray.

    /// ### Performance Test ###
    //float performance = 0.95;
    /// ### Performance Test ###

    int2 bothCond;
    int2 breakLoop;

    for (int i = 0; i < maxSteps; ++i)
    {
        p = rayOrigin + (rayDir * t); // World space position of sample.
        distField.dist = map(p); // Sample of distance field. d.x: Distance field ouput, d.y: Material data.
        distField.dist = abs(distField.dist);

        // If we run past the depth buffer, stop and return nothing (transparent pixel).
        // This way raymarched objects and traditional meshes can co-exist.
        bothCond = when_ge_float(float4(t, t, 0.0, 0.0), float4(depth, maxDrawDist, 0.0, 0.0)).xy;
        breakLoop.x = saturate(bothCond.x + bothCond.y);
        i += maxSteps * breakLoop.x;
        //if ((t >= depth) || (t > maxDrawDist))
        //{
        //    rayHit = false;

        //    /// ### Performance Test ###
        //    //performance = (float) i / MAX_STEP;
        //    /// ### Performance Test ###

        //    break;
        //}
        // If the sample <= 0, we have hit an object.
        breakLoop.y = when_lt_float(float4(distField.dist, 0.0, 0.0, 0.0), float4(0.001, 0.0, 0.0, 0.0)).x;
        i += maxSteps * breakLoop.y;
        //else if (distField.dist < 0.001)
        //{
        //    // Perform colour/lighting
        //    rayHit = true;

        //    /// ### Performance Test ###
        //    //performance = (float) i / MAX_STEP;
        //    /// ### Performance Test ###

        //    break;
        //}

        rayHit = saturate((1 - breakLoop.x) + breakLoop.y);

        // If the sample > 0, we haven't hit anything yet so we should march forward.
        // We step forward by distance d, because d is the minimum distance possible to intersect an object.
        t += distField.dist;
    }

    distField = mapMat();
    distField.totalDist = t;

    return rayHit;
}

int cheapRaymarch(float3 rayOrigin, float3 rayDir, float depth, int maxSteps, float maxDrawDist, inout float3 p, inout rmPixel distField)
{
    int rayHit = 0;
    float totalDist = distField.totalDist; // Current distance travaled along the ray.
    int2 bothCond;
    int2 breakLoop;



    // March through the cheap map.
    for (int i = 0; i < maxSteps; ++i)
    {
        p = rayOrigin + (rayDir * totalDist); // World space position of sample.
        distField.dist = cheapMap(p); // Sample cheap distance field.

        // If we run past the depth buffer, stop and return nothing (transparent pixel).
        // This way raymarched objects and traditional meshes can co-exist.
        bothCond = when_ge_float(float4(totalDist, totalDist, 0.0, 0.0), float4(depth, maxDrawDist, 0.0, 0.0)).xy;
        breakLoop.x = saturate(bothCond.x + bothCond.y);
        i += maxSteps * breakLoop.x;

        // If the sample <= 0, we have hit an object.
        breakLoop.y = when_lt_float(float4(distField.dist, 0.0, 0.0, 0.0), float4(0.001, 0.0, 0.0, 0.0)).x;
        i += maxSteps * breakLoop.y;

        rayHit = saturate((1 - breakLoop.x) + breakLoop.y);

        // If the sample > 0, we haven'totalDist hit anything yet so we should march forward.
        // We step forward by distance d, because d is the minimum distance possible to intersect an object.
        totalDist += distField.dist;
    }

    distField.colour = float4(0.0, 0.0, 0.0, 0.0);
    distField.reflInfo = float4(0.0, 0.0, 0.0, 0.0);
    distField.refractInfo = float2(0.0, 1.0);
    distField.texID = 0;
    distField.totalDist = totalDist;

    return rayHit;
}

//int invertCheapRaymarch(float3 rayOrigin, float3 rayDir, float depth, int maxSteps, float maxDrawDist, inout float3 p, inout rmPixel distField)
//{
//    int rayHit = 0;
//    float totalDist = distField.totalDist; // Current distance travaled along the ray.
//    int2 bothCond;
//    int2 breakLoop;



//    // March through the cheap map.
//    for (int i = 0; i < maxSteps; ++i)
//    {
//        p = rayOrigin + (rayDir * totalDist); // World space position of sample.
//        distField.dist = cheapMap(p); // Sample cheap distance field.
//        distField.dist *= -1.0;

//        // If we run past the depth buffer, stop and return nothing (transparent pixel).
//        // This way raymarched objects and traditional meshes can co-exist.
//        bothCond = when_ge_float(float4(totalDist, totalDist, 0.0, 0.0), float4(depth, maxDrawDist, 0.0, 0.0)).xy;
//        breakLoop.x = saturate(bothCond.x + bothCond.y);
//        i += maxSteps * breakLoop.x;

//        // If the sample <= 0, we have hit an object.
//        breakLoop.y = when_lt_float(float4(distField.dist, 0.0, 0.0, 0.0), float4(0.001, 0.0, 0.0, 0.0)).x;
//        i += maxSteps * breakLoop.y;

//        rayHit = saturate((1 - breakLoop.x) + breakLoop.y);

//        // If the sample > 0, we haven'totalDist hit anything yet so we should march forward.
//        // We step forward by distance d, because d is the minimum distance possible to intersect an object.
//        totalDist += distField.dist;
//    }


//    return rayHit;
//}

int unsignedCheapRaymarch(float3 rayOrigin, float3 rayDir, float depth, int maxSteps, float maxDrawDist, inout float3 p, inout rmPixel distField)
{
    int rayHit = 0;
    float totalDist = distField.totalDist; // Current distance travaled along the ray.
    int2 bothCond;
    int2 breakLoop;



    // March through the cheap map.
    for (int i = 0; i < maxSteps; ++i)
    {
        p = rayOrigin + (rayDir * totalDist); // World space position of sample.
        distField.dist = cheapMap(p); // Sample cheap distance field.
        distField.dist = abs(distField.dist);

        // If we run past the depth buffer, stop and return nothing (transparent pixel).
        // This way raymarched objects and traditional meshes can co-exist.
        bothCond = when_ge_float(float4(totalDist, totalDist, 0.0, 0.0), float4(depth, maxDrawDist, 0.0, 0.0)).xy;
        breakLoop.x = saturate(bothCond.x + bothCond.y);
        i += maxSteps * breakLoop.x;

        // If the sample <= 0, we have hit an object.
        breakLoop.y = when_lt_float(float4(distField.dist, 0.0, 0.0, 0.0), float4(0.001, 0.0, 0.0, 0.0)).x;
        i += maxSteps * breakLoop.y;

        rayHit = saturate((1 - breakLoop.x) + breakLoop.y);

        // If the sample > 0, we haven'totalDist hit anything yet so we should march forward.
        // We step forward by distance d, because d is the minimum distance possible to intersect an object.
        totalDist += distField.dist;
    }


    return rayHit;
}