#include "RayMarchEssentialFunctions.hlsl"

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


// ######### Ambient Occlusion Variables #########
int _aoMaxSteps;
float _aoStepSize;
float _aoIntensity;
// ######### Ambient Occlusion Variables #########


// ######### Refraction Variables #########
float2 _refractInfo[32];
// ######### Refraction Variables #########

sampler2D _wood;
sampler2D _brick;


float ambientOcclusion(float3 p, float3 normal)
{
    float step = _aoStepSize;
    float ao = 0.0;
    float dist;

    for (int i = 1; i <= _aoMaxSteps; ++i)
    {
        dist = step * i;
        ao += max((dist - map(p + (normal * dist))) / dist, 0.0);
    }

    return (1.0 - (ao * _aoIntensity));
}

float shadow(float3 rayOrigin, float3 rayDir, float mint, float maxt, float _penumbraFactor)
{
    float shadowFactor = 1.0;

    for (float t = mint; t < maxt;)
    {
        float h = map(rayOrigin + (rayDir * t));

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
                            map(pos + EPS.xyy) - map(pos - EPS.xyy),
                            map(pos + EPS.yxy) - map(pos - EPS.yxy),
                            map(pos + EPS.yyx) - map(pos - EPS.yyx));

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

    int hasTex = when_gt_int(distField.texID, 0);
    //if (when_gt_int(distField.texID, 0))
    //{
        //tex = _textures[0];
        //tex = distField.tex;
    float4 firstTex = float4(triplanarMap(p, normal, _wood), 1.0) * (when_eq_int(distField.texID, 1) * hasTex);
    firstTex += float4(triplanarMap(p, normal, _brick), 1.0) * (when_eq_int(distField.texID, 2) * hasTex);
    colour += firstTex;
    //}

    // Ambient contribution
    colour.rgb += _lightConstants.x * _ambientColour;

    //colour += float4(dot(-_LightDir, normal).rrr, 1.0);
    float NdotL = saturate(dot(-_LightDir, normal));

    float shadowFactor = 1.0;


    // Light contributes diffuse and specular lighting.
    float NdotL_gt_0 = when_gt_float(NdotL, 0.0);
    //if (NdotL > 0.0)
    //{
    float dist = length(_lightPos - p);

    // Calculate light's attenuation
    float attenuation = 1.0 / (_attenuationConstant + (_attenuationLinear * dist) + (_attenuationQuadratic * dist * dist));

    // Apply shadow
    shadowFactor = shadow(p, -_LightDir, _shadowMinDist, 64.0 * useShadow * NdotL_gt_0, _penumbraFactor) * 0.5 + 0.5; // * 3;
    shadowFactor = max(0.0, pow(shadowFactor, _shadowIntensity)); // Note: In shaders 0^0 is undefined i.e. 0.

    // Diffuse Contribution
    colour.rgb += _diffuseColour * (_lightConstants.y * NdotL * attenuation * NdotL_gt_0);

    // Calculate the Half-Angle vector
    float3 H = normalize(-_LightDir + normalize(-p));
    float NdotH = saturate(dot(normal, H));
            
    // Specular Contribution
    colour.rgb += _specularColour * (_lightConstants.z * pow(NdotH, _specularExp) * attenuation * NdotL_gt_0);
    //}



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

    int hasTex = when_gt_int(distField.texID, 0);
    //if (when_gt_int(distField.texID, 0))
    //{
        //tex = _textures[0];
        //tex = distField.tex;
    float4 firstTex = float4(triplanarMap(p, normal, _wood), 1.0) * (when_eq_int(distField.texID, 1) * hasTex);
    firstTex += float4(triplanarMap(p, normal, _brick), 1.0) * (when_eq_int(distField.texID, 2) * hasTex);
    colour += firstTex;
    //}

    // Ambient contribution
    colour.rgb += _lightConstants.x * _ambientColour;

    //colour += float4(dot(-_LightDir, normal).rrr, 1.0);
    float NdotL = saturate(dot(-_LightDir, normal));

    // Light contributes diffuse and specular lighting.
    float NdotL_gt_0 = when_gt_float(NdotL, 0.0);
    //if (NdotL > 0.0)
    //{
    float dist = length(_lightPos - p);

        // Calculate light's attenuation
    float attenuation = 1.0 / (_attenuationConstant + (_attenuationLinear * dist) + (_attenuationQuadratic * dist * dist));

        // Diffuse Contribution
    colour.rgb += _diffuseColour * (_lightConstants.y * NdotL * attenuation * NdotL_gt_0);

        // Calculate the Half-Angle vector
    float3 H = normalize(-_LightDir + normalize(-p));
    float NdotH = saturate(dot(normal, H));
            
        // Specular Contribution
    colour.rgb += _specularColour * (_lightConstants.z * pow(NdotH, _specularExp) * attenuation * NdotL_gt_0);
    //}

    // Use y value given by map() to choose a colour from our Colour Ramp.
    colour.rgb += distField.colour.rgb * max(NdotL, 0.2);
    colour.a = distField.colour.a;


    return colour;
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

//float3 calcRefractRay(float3 i, float3 n, float etat)
//{
//    float cos_i = clamp(dot(i, n), -1.0, 1.0);
//    float etai = 1.0;
  

//    //if (cos_i < 0.0)
//    //{
//    //    cos_i = -cos_i;
//    //}
//    //else
//    //{
//    //    etai = etat;
//    //    n = -n;
//    //}


//    float cos_i_lt_zero_true = when_lt_float(cos_i, 0);
//    float cos_i_lt_zero_false = 1.0 - cos_i_lt_zero_true;

//    // cos_i < 0
//    cos_i = (cos_i_lt_zero_true * -cos_i) + (cos_i_lt_zero_false * cos_i);
//    // cos_i >= 0
//    etai = (cos_i_lt_zero_false * etat) + (cos_i_lt_zero_true * etai);
//    n = (cos_i_lt_zero_false * -n) + (cos_i_lt_zero_true * n);


//    float eta = etai / etat;
//    float k = 1.0 - (eta * eta) * (1.0 - (cos_i * cos_i));
//    return k < 0.0 ? 0.0 : (eta * i) + ((eta * cos_i - sqrt(k)) * n);
//}

//void performReflection(inout float4 add, float3 rayOrigin, float3 rayDir, float3 pos, float3 normal, rmPixel distField, float2 ratio, inout reflectInfo info)
//{
//    // Distance field reflection.
//    bool rayHit = false;
//    uint quality;
//    float4 refl = distField.reflInfo;
//    float prevRefl = 0;

//    quality = 2;
//    rayDir = normalize(reflect(rayDir, normal));
//    rayOrigin = pos + (rayDir * 0.01);
//    rayHit = unsignedRaymarch(rayOrigin, rayDir, _maxDrawDist, (_maxSteps * refl.x) / quality, _maxDrawDist / quality, pos, distField);

//    if (rayHit)
//    {
//        normal = calcNormal(pos);
//        add += float4(calcLighting(pos, normal, distField).rgb, 0.0) * refl.w * ratio.x;
//    }


//    info.pos = pos;
//    info.normal = normal;
//    info.dir = rayDir;
//    info.distField = distField;
//}

//void cheapRefract(inout float4 add, float3 rayOrigin, float3 rayDir, float3 pos, float3 normal, rmPixel distField, float2 ratio)
//{
//    reflectInfo info;

//    // Calculate refraction.
//    int rayHit = 0;
//    //float3 refractRayDir = normalize(calcRefractRay(rayDir, normal, distField.refractInfo.y));
//    float3 refractRayDir = normalize(refract(rayDir, normal, 1.0 / distField.refractInfo.y));
//    float3 refractRayOrigin = pos + (refractRayDir * 0.02);

//    // March along refraction ray THROUGH the current object (medium).
//    rayHit = unsignedRaymarch(refractRayOrigin, refractRayDir, _maxDrawDist, _maxSteps * (int) distField.refractInfo.x, _maxDrawDist, pos, distField);


//    // See inside of current object.
//    if (rayHit)
//    {
//        //ratio.y = 
//        // Add colour of any object hit inside of the current object, or the current object itself.
//        normal = calcNormal(pos);
//        //add += float4(calcLighting(pos, normal, distField, 1.0, 0.0).rgb, 0.0) * ratio.y;
//        add += float4(cheapLighting(pos, normal, distField).rgb, 0.0) * ratio.y;

//        // Calculate frensel ratio.
//        ratio = fresnel(distField.refractInfo.y, refractRayDir, normal);
//        //ratio.y = 1.0; // Ignoring total internal reflection. Just making it refract instead.
//        // Calculate reflection.
//        //performReflection(add, refractRayOrigin, refractRayDir, pos, -normal, distField, ratio, info);

//        //if (dot(info.dir, info.normal) > 0.0)
//        //{
//        //    float3 dir = normalize(refract(info.dir, -info.normal, distField.refractInfo.y));
//        //    dir = normalize(calcRefractRay(info.dir, info.normal, distField.refractInfo.y));
//        //    float origin = info.pos + (dir * 0.05);

//        //    rayHit = raymarch(origin, dir, _maxDrawDist, _maxSteps * (int) distField.refractInfo.x, _maxDrawDist, info.pos, info.distField);

//        //    if (rayHit)
//        //    {
//        //        float3 norm = calcNormal(info.pos);
//        //        add += float4(calcLighting(info.pos, norm, info.distField, 1.0, 1.0).rgb, 0.0) * 0.2;
//        //        //add = float4(0.0, 0.0, 0.0, 1.0);
//        //    }
//        //}

//        // Calculate refraction.
//        // March along refraction ray EXITING the current object (medium).
//        refractRayDir = normalize(calcRefractRay(refractRayDir, normal, distField.refractInfo.y));
//        //refractRayDir = normalize(refract(refractRayDir, -normal, distField.refractInfo.y)); //distField.refractInfo.y / 1.0
//        refractRayOrigin = pos + (refractRayDir * 0.02);

//        rayHit = raymarch(refractRayOrigin, refractRayDir, _maxDrawDist, _maxSteps * (int) distField.refractInfo.x, _maxDrawDist, pos, distField);

//        // See behind the current object.
//        if (rayHit)
//        {
//            // Add the colour of what is behind the current object.
//            normal = calcNormal(pos);
//            add += float4(calcLighting(pos, normal, distField, 1.0, 1.0).rgb, 0.0) * (ratio.y * 0.6);
//            //add = float4(ratio.yyy, 1.0);
//        }
//    }
//}

void reflection(inout float4 add, float3 rayOrigin, float3 rayDir, float3 pos, float3 normal, rmPixel distField, float2 ratio)
{
	// Distance field reflection.
    float quality;
    float4 refl = distField.reflInfo;
    float prevRefl = 0;
    int rayHit = 0;
    int maxSteps;


    // First reflection bounce.
    quality = 0.5;
    rayDir = normalize(reflect(rayDir, normal));
    rayOrigin = pos + (rayDir * 0.01);
    maxSteps = (_maxSteps * refl.x) * quality;
    distField.totalDist = 0.0;

    //rayHit = unsignedCheapRaymarch(rayOrigin, rayDir, _maxDrawDist, maxSteps, _maxDrawDist * quality, pos, distField);
    //distField.totalDist += 0.2;
    //rayHit = cheapRaymarch(rayOrigin, rayDir, _maxDrawDist, maxSteps, _maxDrawDist * quality, pos, distField);
    rayHit = 1;
    rayHit = raymarch(rayOrigin, rayDir, _maxDrawDist, maxSteps, _maxDrawDist * quality, pos, distField, rayHit);

    if (rayHit)
    {
        normal = calcNormal(pos);
        add += float4(calcLighting(pos, normal, distField).rgb, 0.0) * refl.w * ratio.x; //_reflectionIntensity;
    }




    //Skybox reflection.
    //add += float4(texCUBE(_skybox, ogNormal).rgb * _envReflIntensity * _reflectionIntensity, 0.0) * (1.0 - rayHit) * refl.x * prevRefl;
}