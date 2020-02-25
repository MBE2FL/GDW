Shader "RayMarch/SkeletonTest"
{
    HLSLINCLUDE

    #pragma vertex Vert

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

    #pragma multi_compile_local __ BOUND_DEBUG USE_DEPTH_TEX USE_DIST_TEX

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"
    #include "Graphics Pipeline/Shaders/PrimitiveFunctions.hlsl"
    #include "Graphics Pipeline/Shaders/ConditionalFunctions.hlsl"
    #include "Graphics Pipeline/Shaders/RayMarchCommonFunctions.hlsl"

    // The PositionInputs struct allow you to retrieve a lot of useful information for your fullScreenShader:
    //  struct PositionInputs
    //  {
    //      float3 positionWS;  // World space position (could be camera-relative)
    //      float2 positionNDC; // Normalized screen coordinates within the viewport    : [0, 1) (with the half-pixel offset)
    //      uint2  positionSS;  // Screen space pixel coordinates                       : [0, NumPixels)
    //      uint2  tileCoord;   // Screen tile coordinates                              : [0, NumTiles)
    //      float  deviceDepth; // Depth from the depth buffer                          : [0, 1] (typically reversed)
    //      float  linearDepth; // View space Z coordinate                              : [Near, Far]
    //  };

    // To sample custom buffers, you have access to these functions:
    // But be careful, on most platforms you can't sample to the bound color buffer. It means that you
    // can't use the SampleCustomColor when the pass color buffer is set to custom (and same for camera the buffer).
    // float4 SampleCustomColor(float2 uv);
    // float4 LoadCustomColor(uint2 pixelCoords);
    // float LoadCustomDepth(uint2 pixelCoords);
    // float SampleCustomDepth(float2 uv);

    // There are also a lot of utility function you can use inside Common.hlsl and Color.hlsl,
    // you can check them out in the source code of the core SRP package.

    struct PixelOutput
    {
        float4 sceneCol : SV_Target0;
        float distMap : SV_Target1;
    };

    sampler2D _performanceRamp;


    
    /// ######### RM OBJS Information #########
    static const uint MAX_RM_OBJS = 32;
    static const uint MAX_CSG_CHILDREN = 16;
    float4x4 _invModelMats[MAX_RM_OBJS];
    float4 _rm_colours[MAX_RM_OBJS];
    int _primitiveTypes[MAX_RM_OBJS];
    float2 _combineOps[MAX_RM_OBJS];
    float4 _primitiveGeoInfo[MAX_RM_OBJS];
    float4 _reflInfo[MAX_RM_OBJS];
    float4 _altInfo[MAX_RM_OBJS];

    float4 _bufferedCSGs[MAX_CSG_CHILDREN];
    float4 _combineOpsCSGs[MAX_CSG_CHILDREN];

    float4 _boundGeoInfo[MAX_RM_OBJS];
    /// ######### RM OBJS Information #########
    float distBuffer[MAX_RM_OBJS];

    float4x4 _cameraInvMatrix;
    float4x4 _cameraMatrix;

	// float4x4 inverse(float4x4 m) 
	// {
	// 	float n11 = m[0][0], n12 = m[1][0], n13 = m[2][0], n14 = m[3][0];
	// 	float n21 = m[0][1], n22 = m[1][1], n23 = m[2][1], n24 = m[3][1];
	// 	float n31 = m[0][2], n32 = m[1][2], n33 = m[2][2], n34 = m[3][2];
	// 	float n41 = m[0][3], n42 = m[1][3], n43 = m[2][3], n44 = m[3][3];

	// 	float t11 = n23 * n34 * n42 - n24 * n33 * n42 + n24 * n32 * n43 - n22 * n34 * n43 - n23 * n32 * n44 + n22 * n33 * n44;
	// 	float t12 = n14 * n33 * n42 - n13 * n34 * n42 - n14 * n32 * n43 + n12 * n34 * n43 + n13 * n32 * n44 - n12 * n33 * n44;
	// 	float t13 = n13 * n24 * n42 - n14 * n23 * n42 + n14 * n22 * n43 - n12 * n24 * n43 - n13 * n22 * n44 + n12 * n23 * n44;
	// 	float t14 = n14 * n23 * n32 - n13 * n24 * n32 - n14 * n22 * n33 + n12 * n24 * n33 + n13 * n22 * n34 - n12 * n23 * n34;

	// 	float det = n11 * t11 + n21 * t12 + n31 * t13 + n41 * t14;
	// 	float idet = 1.0f / det;

	// 	float4x4 ret;

	// 	ret[0][0] = t11 * idet;
	// 	ret[0][1] = (n24 * n33 * n41 - n23 * n34 * n41 - n24 * n31 * n43 + n21 * n34 * n43 + n23 * n31 * n44 - n21 * n33 * n44) * idet;
	// 	ret[0][2] = (n22 * n34 * n41 - n24 * n32 * n41 + n24 * n31 * n42 - n21 * n34 * n42 - n22 * n31 * n44 + n21 * n32 * n44) * idet;
	// 	ret[0][3] = (n23 * n32 * n41 - n22 * n33 * n41 - n23 * n31 * n42 + n21 * n33 * n42 + n22 * n31 * n43 - n21 * n32 * n43) * idet;

	// 	ret[1][0] = t12 * idet;
	// 	ret[1][1] = (n13 * n34 * n41 - n14 * n33 * n41 + n14 * n31 * n43 - n11 * n34 * n43 - n13 * n31 * n44 + n11 * n33 * n44) * idet;
	// 	ret[1][2] = (n14 * n32 * n41 - n12 * n34 * n41 - n14 * n31 * n42 + n11 * n34 * n42 + n12 * n31 * n44 - n11 * n32 * n44) * idet;
	// 	ret[1][3] = (n12 * n33 * n41 - n13 * n32 * n41 + n13 * n31 * n42 - n11 * n33 * n42 - n12 * n31 * n43 + n11 * n32 * n43) * idet;

	// 	ret[2][0] = t13 * idet;
	// 	ret[2][1] = (n14 * n23 * n41 - n13 * n24 * n41 - n14 * n21 * n43 + n11 * n24 * n43 + n13 * n21 * n44 - n11 * n23 * n44) * idet;
	// 	ret[2][2] = (n12 * n24 * n41 - n14 * n22 * n41 + n14 * n21 * n42 - n11 * n24 * n42 - n12 * n21 * n44 + n11 * n22 * n44) * idet;
	// 	ret[2][3] = (n13 * n22 * n41 - n12 * n23 * n41 - n13 * n21 * n42 + n11 * n23 * n42 + n12 * n21 * n43 - n11 * n22 * n43) * idet;

	// 	ret[3][0] = t14 * idet;
	// 	ret[3][1] = (n13 * n24 * n31 - n14 * n23 * n31 + n14 * n21 * n33 - n11 * n24 * n33 - n13 * n21 * n34 + n11 * n23 * n34) * idet;
	// 	ret[3][2] = (n14 * n22 * n31 - n12 * n24 * n31 - n14 * n21 * n32 + n11 * n24 * n32 + n12 * n21 * n34 - n11 * n22 * n34) * idet;
	// 	ret[3][3] = (n12 * n23 * n31 - n13 * n22 * n31 + n13 * n21 * n32 - n11 * n23 * n32 - n12 * n21 * n33 + n11 * n22 * n33) * idet;

	// 	return ret;
	// }

	float cheapMap(float3 p)
	{
		float scene = _maxDrawDist;

		float4 pos = float4(0.0, 0.0, 0.0, 0.0);
		float4 geoInfo = float4(0.0, 0.0, 0.0, 0.0);
		float radius = 0.0;

		float obj;
		float obj2;

		float csg;
		float storedCSGs[MAX_CSG_CHILDREN];

		// ######### Ground #########
		pos = mul(_invModelMats[0], float4(p, 1.0));
		geoInfo = _boundGeoInfo[0];
		obj = sdBox(pos.xyz, geoInfo.xyz);

		scene = opU(scene, obj);
		// ######### Ground #########

		// ######### one #########
		pos = mul(_invModelMats[1], float4(p, 1.0));
		geoInfo = _boundGeoInfo[1];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opU(scene, obj);
		// ######### one #########

		// ######### Fractal CSG #########
		pos = mul(_invModelMats[2], float4(p, 1.0));
		geoInfo = _boundGeoInfo[2];
		obj = sdSphere(pos.xyz, geoInfo.x);


		pos = mul(_invModelMats[3], float4(p, 1.0));
		geoInfo = _boundGeoInfo[3];
		obj2 = sdSphere(pos.xyz, geoInfo.x);


		storedCSGs[0] = opU(obj, obj2);

		scene = opSmoothUnion(scene, storedCSGs[0], _combineOpsCSGs[0].w);
		// ######### Fractal CSG #########

		// ######### two #########
		pos = mul(_invModelMats[4], float4(p, 1.0));
		geoInfo = _boundGeoInfo[4];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opU(scene, obj);
		// ######### two #########

		// ######### three #########
		pos = mul(_invModelMats[5], float4(p, 1.0));
		geoInfo = _boundGeoInfo[5];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opU(scene, obj);
		// ######### three #########

		// ######### CSG ONE #########
		pos = mul(_invModelMats[6], float4(p, 1.0));
		geoInfo = _boundGeoInfo[6];
		obj = sdBox(pos.xyz, geoInfo.xyz);


		pos = mul(_invModelMats[7], float4(p, 1.0));
		geoInfo = _boundGeoInfo[7];
		obj2 = sdSphere(pos.xyz, geoInfo.x);


		storedCSGs[1] = opU(obj, obj2);

		obj = storedCSGs[1];

		pos = mul(_invModelMats[8], float4(p, 1.0));
		geoInfo = _boundGeoInfo[8];
		obj2 = sdSphere(pos.xyz, geoInfo.x);


		storedCSGs[2] = opSmoothUnion(obj, obj2, _combineOpsCSGs[2].y);

		scene = opSmoothUnion(scene, storedCSGs[2], _combineOpsCSGs[2].w);
		// ######### CSG ONE #########

		return scene;
	}

	float map(float3 p)
	{
		float scene = _maxDrawDist;

		float4 pos = float4(0.0, 0.0, 0.0, 0.0);
		float4 geoInfo = float4(0.0, 0.0, 0.0, 0.0);

		float obj;
		float obj2;

		float csg;
		float storedCSGs[MAX_CSG_CHILDREN];

		float3 cell = float3(0.0, 0.0, 0.0);

		// ######### Ground #########
		//float4x4 testMat = inverse(_invModelMats[0]);
		pos = mul(_invModelMats[0], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[0];
		//obj = sdRoundBox(pos.xyz, geoInfo.xyz, geoInfo.w) * max(_invModelMats[0][0][0], max(_invModelMats[0][1][1], _invModelMats[0][2][2]));
		obj = sdRoundBox(pos.xyz / float3(1.0, 0.35, 1.0), geoInfo.xyz, geoInfo.w) * min(1.0, min(0.35, 1.0));
		//obj = sdRoundBox(pos.xyz, geoInfo.xyz, geoInfo.w) * min(testMat[0][0], min(testMat[1][1], testMat[2][2]));
		distBuffer[0] = obj;

		scene = opU(scene, obj);
		// ######### Ground #########

		// ######### one #########
		pos = mul(_invModelMats[1], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[1];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[1] = obj;

		scene = opU(scene, obj);
		// ######### one #########

		// ######### Fractal CSG #########
		pos = mul(_invModelMats[2], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[2];
		obj = sdMandelbulb(pos.xyz, geoInfo.xy);
		distBuffer[2] = obj;


		pos = mul(_invModelMats[3], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[3];
		obj2 = sdTetra(pos.xyz);
		distBuffer[3] = obj2;


		storedCSGs[0] = opU(obj, obj2);

		scene = opSmoothUnion(scene, storedCSGs[0], _combineOpsCSGs[0].w);
		// ######### Fractal CSG #########

		// ######### two #########
		pos = mul(_invModelMats[4], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[4];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[4] = obj;

		scene = opU(scene, obj);
		// ######### two #########

		// ######### three #########
		pos = mul(_invModelMats[5], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[5];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[5] = obj;

		scene = opU(scene, obj);
		// ######### three #########

		// ######### CSG ONE #########
		pos = mul(_invModelMats[6], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[6];
		obj = sdBox(pos.xyz, geoInfo.xyz);
		distBuffer[6] = obj;


		pos = mul(_invModelMats[7], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[7];
		obj2 = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[7] = obj2;


		storedCSGs[1] = opI(obj, obj2);

		obj = storedCSGs[1];

		pos = mul(_invModelMats[8], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[8];
		obj2 = sdCylinder(pos.xyz, geoInfo.x, geoInfo.y);
		distBuffer[8] = obj2;


		storedCSGs[2] = opSmoothInt(obj, obj2, _combineOpsCSGs[2].y);

		scene = opSmoothSub(storedCSGs[2], scene, _combineOpsCSGs[2].w);
		// ######### CSG ONE #########

		return scene;
	}

	rmPixel mapMat()
	{
		rmPixel scene;
		scene.dist = _maxDrawDist;
		scene.colour = float4(0.0, 0.0, 0.0, 0.0);
		scene.reflInfo = float4(0.0, 0.0, 0.0, 0.0);
		scene.refractInfo = float2(0.0, 1.0);
		scene.texID = 0;

		rmPixel obj;
		obj.colour = float4(0.0, 0.0, 0.0, 0.0);
		obj.reflInfo = float4(0.0, 0.0, 0.0, 0.0);
		obj.refractInfo = float2(0.0, 1.0);
		obj.texID = 0;

		rmPixel obj2;
		obj2.colour = float4(0.0, 0.0, 0.0, 0.0);
		obj2.reflInfo = float4(0.0, 0.0, 0.0, 0.0);
		obj2.refractInfo = float2(0.0, 1.0);
		obj2.texID = 0;

		rmPixel csg;
		csg.colour = float4(0.0, 0.0, 0.0, 0.0);
		csg.reflInfo = float4(0.0, 0.0, 0.0, 0.0);
		csg.refractInfo = float2(0.0, 1.0);
		csg.texID = 0;
		rmPixel storedCSGs[MAX_CSG_CHILDREN];

		float reflWeight;
		// ######### Ground #########
		obj.dist = distBuffer[0];
		obj.colour = _rm_colours[0];
		obj.reflInfo = _reflInfo[0];
		obj.refractInfo = _refractInfo[0];
		scene = opUMat(scene, obj);
		// ######### Ground #########

		// ######### one #########
		obj.dist = distBuffer[1];
		obj.colour = _rm_colours[1];
		obj.reflInfo = _reflInfo[1];
		obj.refractInfo = _refractInfo[1];
		scene = opUMat(scene, obj);
		// ######### one #########

		// ######### Fractal CSG #########
		obj.dist = distBuffer[2];
		obj.colour = _rm_colours[2];
		obj.reflInfo = _reflInfo[2];
		obj.refractInfo = _refractInfo[2];

		obj2.dist = distBuffer[3];
		obj2.colour = _rm_colours[3];
		obj2.reflInfo = _reflInfo[3];
		obj2.refractInfo = _refractInfo[3];

		storedCSGs[0] = opUMat(obj, obj2);

		scene = opSmoothUnionMat(scene, storedCSGs[0], _combineOpsCSGs[0].w);
		// ######### Fractal CSG #########

		// ######### two #########
		obj.dist = distBuffer[4];
		obj.colour = _rm_colours[4];
		obj.reflInfo = _reflInfo[4];
		obj.refractInfo = _refractInfo[4];
		scene = opUMat(scene, obj);
		// ######### two #########

		// ######### three #########
		obj.dist = distBuffer[5];
		obj.colour = _rm_colours[5];
		obj.reflInfo = _reflInfo[5];
		obj.refractInfo = _refractInfo[5];
		scene = opUMat(scene, obj);
		// ######### three #########

		// ######### CSG ONE #########
		obj.dist = distBuffer[6];
		obj.colour = _rm_colours[6];
		obj.reflInfo = _reflInfo[6];
		obj.refractInfo = _refractInfo[6];

		obj2.dist = distBuffer[7];
		obj2.colour = _rm_colours[7];
		obj2.reflInfo = _reflInfo[7];
		obj2.refractInfo = _refractInfo[7];

		storedCSGs[1] = opIMat(obj, obj2);

		obj = storedCSGs[1];

		obj2.dist = distBuffer[8];
		obj2.colour = _rm_colours[8];
		obj2.reflInfo = _reflInfo[8];
		obj2.refractInfo = _refractInfo[8];

		storedCSGs[2] = opSmoothIntMat(obj, obj2, _combineOpsCSGs[2].y);

		scene = opSmoothSubMat(storedCSGs[2], scene, _combineOpsCSGs[2].w);
		// ######### CSG ONE #########

		return scene;
	}





    PixelOutput FullScreenPass(Varyings varyings)
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
        float depth = LoadCameraDepth(varyings.positionCS.xy);
        PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
        float3 viewDirection = GetWorldSpaceNormalizeViewDir(posInput.positionWS);
        float4 color = float4(0.0, 0.0, 0.0, 0.0);

        // Load the camera color buffer at the mip 0 if we're not at the before rendering injection point
        if (_CustomPassInjectionPoint != CUSTOMPASSINJECTIONPOINT_BEFORE_RENDERING)
            color = float4(CustomPassSampleCameraColor(posInput.positionNDC.xy, 0), 1);

        // Add your custom pass code here

        // Ray direction
        float3 rayDir = -viewDirection;
        // WHY NEW UNITY (View ray faces towards the camera)

        // Ray origin
        float3 rayOrigin = _CameraPos.xyz;

        // #if UNITY_UV_STARTS_AT_TOP
        // if (_MainTex_TexelSize.y < 0)
        //     uv.y = 1 - uv.y;
        // #endif

        
        // Convert from depth buffer (eye space) to true distance from camera.
        // This is done by multiplying the eyespace depth by the length of the "z-normalized" ray.
        // Think of similar triangles:
        // The view-space z-distance between a point and the camera is proportional to the absolute distance.
        //float linearEyeDepth = LinearEyeDepth(posInput.positionWS, UNITY_MATRIX_V);
        //linearEyeDepth = LinearEyeDepth(depth, _ZBufferParams);
        float linearEyeDepth = posInput.linearDepth;
        // Convert ray direction from world space to eyespace(camera).
        float3 zNormRayDir = mul(_cameraInvMatrix, float4(rayDir, 0.0)).xyz;
        // "Normalize" ray direction in the z-axis.
        // Dividing by z "normalizes" it in the z axis.
        // Therefore multiplying the ray by some number i gives the viewspace position
        // of the point on the ray with [viewspace z] = i.
        zNormRayDir /= abs(zNormRayDir.z);
        // Convert new ray direction from eyespace(camera) to world space.
        zNormRayDir = mul(_cameraMatrix, float4(zNormRayDir, 0.0)).xyz;
        // Multiply the eyespace depth by the length of "z-nomralized" ray direction.
        linearEyeDepth *= length(zNormRayDir);


        // Colour of the scene before this shader was run
        float4 col = color;

        // March a ray for each pixel, and check whether anything was hit.
        float3 p = float3(0.0, 0.0, 0.0);
        rmPixel distField;
        float4 add = float4(0.0, 0.0, 0.0, 0.0);
        float3 normal = float3(0.0, 0.0, 0.0);
        int rayHit = cheapRaymarch(rayOrigin, rayDir, linearEyeDepth, _maxSteps, _maxDrawDist, p, distField);
        rayHit = raymarch(rayOrigin, rayDir, linearEyeDepth, _maxSteps, _maxDrawDist, p, distField, rayHit);

        // Perform shading/lighting.
        if (rayHit)
        {
            normal = calcNormal(p);
            add = calcLighting(p, normal, distField);
            //add = float4(1.0, 0.0, 0.0, 1.0);

            // float2 ratio = fresnel(distField.refractInfo.y, rayDir, normal);
            // ratio.x = (distField.refractInfo.x > 0.0) ? ratio.x : 1.0;

            // // Perform refraction
            // //performRefraction(add, rayOrigin, rayDir, p, normal, distField, ratio);
            // //ReflectAndRefract(add, rayOrigin, rayDir, p, normal, distField, ratio);
            // //cheapRefract(add, rayOrigin, rayDir, p, normal, distField, ratio);

            // // Perform reflection
            // reflection(add, rayOrigin, rayDir, p, normal, distField, ratio);
        }

        //add = float4(tex2D(_performanceRamp, float2(distField.dist, 0.0)).xyz, 1.0);
        //add = float4(tex2D(_performanceRamp, float2(distField.totalDist / _maxDrawDist, 0.0)).xyz, 1.0);

        // Returns final colour using alpha blending.
        col = float4(lerp(col.rgb, add.rgb, add.a), 1.0);


        //col.rgb = rayDir;

#if BOUND_DEBUG
        rayDir = -viewDirection;
        rayOrigin = _CameraPos.xyz;
        p = 0.0;
        distField.totalDist = 0.0;

        rayHit = cheapRaymarch(rayOrigin, rayDir, linearEyeDepth, _maxSteps, _maxDrawDist, p, distField);
        if (rayHit)
        {
            col.rgb += float3(0.3 * sin(p.x * 4) + 0.2, 0.0, 0.3 * sin(p.z * 4) + 0.2);
        }
#endif


        // Gamma correction
        //add.rgb = pow(add.rgb, float3(0.4545, 0.4545, 0.4545));

        // Contrast
        //add.rgb = smoothstep(0.0, 1.0, add.rgb);

        // Vignette
        //float2 test = abs(input.uv - float2(0.5, 0.5));
        //col.rgb += (smoothstep(0.4, 0.5, test.x) * _vignetteIntensity) * float3(1.0, 0.0, 0.0);
        //col.rgb += (smoothstep(0.4, 0.5, test.y) * _vignetteIntensity) * float3(1.0, 0.0, 0.0);


        // Fog
        // Technique One
        //float fogAmount = 1.0 - exp(-distField.totalDist * _fogInscattering);
        //float3 fogColour = float3(0.5, 0.6, 0.7);
        //col = float4(lerp(col.rgb, fogColour.rgb, fogAmount), 1.0);

        // Technique Two
        //col.rgb = (col.rgb * (1.0 - exp(-distField.dist * _fogExtinction))) + (fogColour * (exp(-distField.dist * _fogExtinction)));

        // Technique Three
        //float3 extColour = exp(-distField.totalDist * _fogExtinction.rrr);
        //float3 insColour = exp(-distField.totalDist * _fogInscattering.rrr);
        //float3 extColour = float3(exp(-distField.dist * _fogExtinction.x), exp(-distField.dist * _fogExtinction.y), exp(-distField.dist * _fogExtinction.z));
        //float3 insColour = float3(exp(-distField.dist * _fogInscattering.x), exp(-distField.dist * _fogInscattering.y), exp(-distField.dist * _fogInscattering.z));
        //col.rgb = (col.rgb * (1.0 - extColour)) + (fogColour * insColour);

        // Technique Four
        //float b = _fogInscattering;
        //fogAmount = _fogExtinction * exp(-rayOrigin.y * b) * ((1.0 - exp(-distField.totalDist * rayDir.y * b)) / (b * rayDir.y));
        //fogAmount = clamp(fogAmount, 0.0, 1.0);
        //col.rgb = lerp(col.rgb, _fogColour, fogAmount);
        

        //col.rgb = float3(distField.totalDist / _maxDrawDist, 0.0, 0.0);
        //col.rgb = float3(fogAmount, 0.0, 0.0);



        PixelOutput output;
        output.sceneCol = col;
        output.distMap = distField.totalDist;
        //output.sceneCol = float4(output.distMap, 0.0, 0.0, 1.0);

        return output;

        // Fade value allow you to increase the strength of the effect while the camera gets closer to the custom pass volume
        //float f = 1 - abs(_FadeValue * 2 - 1);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "Custom Pass 0"

            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
                #pragma fragment FullScreenPass
            ENDHLSL
        }
    }
    Fallback Off
}
