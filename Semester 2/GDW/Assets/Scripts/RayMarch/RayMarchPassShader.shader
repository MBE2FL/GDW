Shader "FullScreen/RayMarchPassShader"
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



	float cheapMap(float3 p)
	{
		float scene = _maxDrawDist;

		float4 pos = float4(0.0, 0.0, 0.0, 0.0);
		float4 geoInfo = float4(0.0, 0.0, 0.0, 0.0);
		float radius = 0.0;

		float obj;

		// ######### New Game Object #########
		pos = mul(_invModelMats[0], float4(p, 1.0));
		geoInfo = _boundGeoInfo[0];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[0].y);
		// ######### New Game Object #########

		// ######### New Game Object (1) #########
		pos = mul(_invModelMats[1], float4(p, 1.0));
		geoInfo = _boundGeoInfo[1];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[1].y);
		// ######### New Game Object (1) #########

		// ######### New Game Object (2) #########
		pos = mul(_invModelMats[2], float4(p, 1.0));
		geoInfo = _boundGeoInfo[2];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[2].y);
		// ######### New Game Object (2) #########

		// ######### New Game Object (3) #########
		pos = mul(_invModelMats[3], float4(p, 1.0));
		geoInfo = _boundGeoInfo[3];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[3].y);
		// ######### New Game Object (3) #########

		// ######### New Game Object (4) #########
		pos = mul(_invModelMats[4], float4(p, 1.0));
		geoInfo = _boundGeoInfo[4];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[4].y);
		// ######### New Game Object (4) #########

		// ######### New Game Object (5) #########
		pos = mul(_invModelMats[5], float4(p, 1.0));
		geoInfo = _boundGeoInfo[5];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[5].y);
		// ######### New Game Object (5) #########

		// ######### New Game Object (6) #########
		pos = mul(_invModelMats[6], float4(p, 1.0));
		geoInfo = _boundGeoInfo[6];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[6].y);
		// ######### New Game Object (6) #########

		// ######### New Game Object (7) #########
		pos = mul(_invModelMats[7], float4(p, 1.0));
		geoInfo = _boundGeoInfo[7];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[7].y);
		// ######### New Game Object (7) #########

		// ######### New Game Object (8) #########
		pos = mul(_invModelMats[8], float4(p, 1.0));
		geoInfo = _boundGeoInfo[8];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[8].y);
		// ######### New Game Object (8) #########

		// ######### New Game Object (9) #########
		pos = mul(_invModelMats[9], float4(p, 1.0));
		geoInfo = _boundGeoInfo[9];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[9].y);
		// ######### New Game Object (9) #########

		// ######### New Game Object (10) #########
		pos = mul(_invModelMats[10], float4(p, 1.0));
		geoInfo = _boundGeoInfo[10];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[10].y);
		// ######### New Game Object (10) #########

		// ######### New Game Object (11) #########
		pos = mul(_invModelMats[11], float4(p, 1.0));
		geoInfo = _boundGeoInfo[11];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[11].y);
		// ######### New Game Object (11) #########

		// ######### New Game Object (12) #########
		pos = mul(_invModelMats[12], float4(p, 1.0));
		geoInfo = _boundGeoInfo[12];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[12].y);
		// ######### New Game Object (12) #########

		// ######### New Game Object (13) #########
		pos = mul(_invModelMats[13], float4(p, 1.0));
		geoInfo = _boundGeoInfo[13];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[13].y);
		// ######### New Game Object (13) #########

		// ######### New Game Object (14) #########
		pos = mul(_invModelMats[14], float4(p, 1.0));
		geoInfo = _boundGeoInfo[14];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[14].y);
		// ######### New Game Object (14) #########

		// ######### New Game Object (15) #########
		pos = mul(_invModelMats[15], float4(p, 1.0));
		geoInfo = _boundGeoInfo[15];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[15].y);
		// ######### New Game Object (15) #########

		// ######### New Game Object (16) #########
		pos = mul(_invModelMats[16], float4(p, 1.0));
		geoInfo = _boundGeoInfo[16];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[16].y);
		// ######### New Game Object (16) #########

		// ######### New Game Object (17) #########
		pos = mul(_invModelMats[17], float4(p, 1.0));
		geoInfo = _boundGeoInfo[17];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[17].y);
		// ######### New Game Object (17) #########

		// ######### New Game Object (18) #########
		pos = mul(_invModelMats[18], float4(p, 1.0));
		geoInfo = _boundGeoInfo[18];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[18].y);
		// ######### New Game Object (18) #########

		// ######### New Game Object (19) #########
		pos = mul(_invModelMats[19], float4(p, 1.0));
		geoInfo = _boundGeoInfo[19];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[19].y);
		// ######### New Game Object (19) #########

		// ######### New Game Object (20) #########
		pos = mul(_invModelMats[20], float4(p, 1.0));
		geoInfo = _boundGeoInfo[20];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[20].y);
		// ######### New Game Object (20) #########

		// ######### New Game Object (21) #########
		pos = mul(_invModelMats[21], float4(p, 1.0));
		geoInfo = _boundGeoInfo[21];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[21].y);
		// ######### New Game Object (21) #########

		// ######### New Game Object (22) #########
		pos = mul(_invModelMats[22], float4(p, 1.0));
		geoInfo = _boundGeoInfo[22];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[22].y);
		// ######### New Game Object (22) #########

		// ######### New Game Object (23) #########
		pos = mul(_invModelMats[23], float4(p, 1.0));
		geoInfo = _boundGeoInfo[23];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[23].y);
		// ######### New Game Object (23) #########

		// ######### New Game Object (24) #########
		pos = mul(_invModelMats[24], float4(p, 1.0));
		geoInfo = _boundGeoInfo[24];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[24].y);
		// ######### New Game Object (24) #########

		// ######### New Game Object (25) #########
		pos = mul(_invModelMats[25], float4(p, 1.0));
		geoInfo = _boundGeoInfo[25];
		obj = sdSphere(pos.xyz, geoInfo.x);

		scene = opSmoothUnion(scene, obj, _combineOps[25].y);
		// ######### New Game Object (25) #########

		return scene;
		//<Insert Cheap Map Here>
	}

	/// Distance field function.
	/// The distance field represents the closest distance to the surface of any object
	/// we put in the scene. If the given point (point p) is inside of any object, we return an negative answer.
	/// Return.x: Distance field value.
	/// Return.y: Colour of closest object (0 - 1).
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

		// ######### New Game Object #########
		pos = mul(_invModelMats[0], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[0];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[0] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[0].y);
		// ######### New Game Object #########

		// ######### New Game Object (1) #########
		pos = mul(_invModelMats[1], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[1];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[1] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[1].y);
		// ######### New Game Object (1) #########

		// ######### New Game Object (2) #########
		pos = mul(_invModelMats[2], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[2];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[2] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[2].y);
		// ######### New Game Object (2) #########

		// ######### New Game Object (3) #########
		pos = mul(_invModelMats[3], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[3];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[3] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[3].y);
		// ######### New Game Object (3) #########

		// ######### New Game Object (4) #########
		pos = mul(_invModelMats[4], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[4];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[4] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[4].y);
		// ######### New Game Object (4) #########

		// ######### New Game Object (5) #########
		pos = mul(_invModelMats[5], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[5];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[5] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[5].y);
		// ######### New Game Object (5) #########

		// ######### New Game Object (6) #########
		pos = mul(_invModelMats[6], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[6];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[6] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[6].y);
		// ######### New Game Object (6) #########

		// ######### New Game Object (7) #########
		pos = mul(_invModelMats[7], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[7];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[7] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[7].y);
		// ######### New Game Object (7) #########

		// ######### New Game Object (8) #########
		pos = mul(_invModelMats[8], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[8];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[8] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[8].y);
		// ######### New Game Object (8) #########

		// ######### New Game Object (9) #########
		pos = mul(_invModelMats[9], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[9];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[9] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[9].y);
		// ######### New Game Object (9) #########

		// ######### New Game Object (10) #########
		pos = mul(_invModelMats[10], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[10];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[10] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[10].y);
		// ######### New Game Object (10) #########

		// ######### New Game Object (11) #########
		pos = mul(_invModelMats[11], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[11];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[11] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[11].y);
		// ######### New Game Object (11) #########

		// ######### New Game Object (12) #########
		pos = mul(_invModelMats[12], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[12];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[12] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[12].y);
		// ######### New Game Object (12) #########

		// ######### New Game Object (13) #########
		pos = mul(_invModelMats[13], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[13];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[13] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[13].y);
		// ######### New Game Object (13) #########

		// ######### New Game Object (14) #########
		pos = mul(_invModelMats[14], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[14];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[14] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[14].y);
		// ######### New Game Object (14) #########

		// ######### New Game Object (15) #########
		pos = mul(_invModelMats[15], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[15];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[15] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[15].y);
		// ######### New Game Object (15) #########

		// ######### New Game Object (16) #########
		pos = mul(_invModelMats[16], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[16];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[16] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[16].y);
		// ######### New Game Object (16) #########

		// ######### New Game Object (17) #########
		pos = mul(_invModelMats[17], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[17];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[17] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[17].y);
		// ######### New Game Object (17) #########

		// ######### New Game Object (18) #########
		pos = mul(_invModelMats[18], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[18];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[18] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[18].y);
		// ######### New Game Object (18) #########

		// ######### New Game Object (19) #########
		pos = mul(_invModelMats[19], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[19];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[19] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[19].y);
		// ######### New Game Object (19) #########

		// ######### New Game Object (20) #########
		pos = mul(_invModelMats[20], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[20];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[20] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[20].y);
		// ######### New Game Object (20) #########

		// ######### New Game Object (21) #########
		pos = mul(_invModelMats[21], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[21];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[21] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[21].y);
		// ######### New Game Object (21) #########

		// ######### New Game Object (22) #########
		pos = mul(_invModelMats[22], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[22];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[22] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[22].y);
		// ######### New Game Object (22) #########

		// ######### New Game Object (23) #########
		pos = mul(_invModelMats[23], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[23];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[23] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[23].y);
		// ######### New Game Object (23) #########

		// ######### New Game Object (24) #########
		pos = mul(_invModelMats[24], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[24];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[24] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[24].y);
		// ######### New Game Object (24) #########

		// ######### New Game Object (25) #########
		pos = mul(_invModelMats[25], float4(p, 1.0));
		geoInfo = _primitiveGeoInfo[25];
		obj = sdSphere(pos.xyz, geoInfo.x);
		distBuffer[25] = obj;

		scene = opSmoothUnion(scene, obj, _combineOps[25].y);
		// ######### New Game Object (25) #########

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
		// ######### New Game Object #########
		obj.dist = distBuffer[0];
		obj.colour = _rm_colours[0];
		obj.reflInfo = _reflInfo[0];
		obj.refractInfo = _refractInfo[0];
		scene = opSmoothUnionMat(scene, obj, _combineOps[0].y);
		// ######### New Game Object #########

		// ######### New Game Object (1) #########
		obj.dist = distBuffer[1];
		obj.colour = _rm_colours[1];
		obj.reflInfo = _reflInfo[1];
		obj.refractInfo = _refractInfo[1];
		scene = opSmoothUnionMat(scene, obj, _combineOps[1].y);
		// ######### New Game Object (1) #########

		// ######### New Game Object (2) #########
		obj.dist = distBuffer[2];
		obj.colour = _rm_colours[2];
		obj.reflInfo = _reflInfo[2];
		obj.refractInfo = _refractInfo[2];
		scene = opSmoothUnionMat(scene, obj, _combineOps[2].y);
		// ######### New Game Object (2) #########

		// ######### New Game Object (3) #########
		obj.dist = distBuffer[3];
		obj.colour = _rm_colours[3];
		obj.reflInfo = _reflInfo[3];
		obj.refractInfo = _refractInfo[3];
		scene = opSmoothUnionMat(scene, obj, _combineOps[3].y);
		// ######### New Game Object (3) #########

		// ######### New Game Object (4) #########
		obj.dist = distBuffer[4];
		obj.colour = _rm_colours[4];
		obj.reflInfo = _reflInfo[4];
		obj.refractInfo = _refractInfo[4];
		scene = opSmoothUnionMat(scene, obj, _combineOps[4].y);
		// ######### New Game Object (4) #########

		// ######### New Game Object (5) #########
		obj.dist = distBuffer[5];
		obj.colour = _rm_colours[5];
		obj.reflInfo = _reflInfo[5];
		obj.refractInfo = _refractInfo[5];
		scene = opSmoothUnionMat(scene, obj, _combineOps[5].y);
		// ######### New Game Object (5) #########

		// ######### New Game Object (6) #########
		obj.dist = distBuffer[6];
		obj.colour = _rm_colours[6];
		obj.reflInfo = _reflInfo[6];
		obj.refractInfo = _refractInfo[6];
		scene = opSmoothUnionMat(scene, obj, _combineOps[6].y);
		// ######### New Game Object (6) #########

		// ######### New Game Object (7) #########
		obj.dist = distBuffer[7];
		obj.colour = _rm_colours[7];
		obj.reflInfo = _reflInfo[7];
		obj.refractInfo = _refractInfo[7];
		scene = opSmoothUnionMat(scene, obj, _combineOps[7].y);
		// ######### New Game Object (7) #########

		// ######### New Game Object (8) #########
		obj.dist = distBuffer[8];
		obj.colour = _rm_colours[8];
		obj.reflInfo = _reflInfo[8];
		obj.refractInfo = _refractInfo[8];
		scene = opSmoothUnionMat(scene, obj, _combineOps[8].y);
		// ######### New Game Object (8) #########

		// ######### New Game Object (9) #########
		obj.dist = distBuffer[9];
		obj.colour = _rm_colours[9];
		obj.reflInfo = _reflInfo[9];
		obj.refractInfo = _refractInfo[9];
		scene = opSmoothUnionMat(scene, obj, _combineOps[9].y);
		// ######### New Game Object (9) #########

		// ######### New Game Object (10) #########
		obj.dist = distBuffer[10];
		obj.colour = _rm_colours[10];
		obj.reflInfo = _reflInfo[10];
		obj.refractInfo = _refractInfo[10];
		scene = opSmoothUnionMat(scene, obj, _combineOps[10].y);
		// ######### New Game Object (10) #########

		// ######### New Game Object (11) #########
		obj.dist = distBuffer[11];
		obj.colour = _rm_colours[11];
		obj.reflInfo = _reflInfo[11];
		obj.refractInfo = _refractInfo[11];
		scene = opSmoothUnionMat(scene, obj, _combineOps[11].y);
		// ######### New Game Object (11) #########

		// ######### New Game Object (12) #########
		obj.dist = distBuffer[12];
		obj.colour = _rm_colours[12];
		obj.reflInfo = _reflInfo[12];
		obj.refractInfo = _refractInfo[12];
		scene = opSmoothUnionMat(scene, obj, _combineOps[12].y);
		// ######### New Game Object (12) #########

		// ######### New Game Object (13) #########
		obj.dist = distBuffer[13];
		obj.colour = _rm_colours[13];
		obj.reflInfo = _reflInfo[13];
		obj.refractInfo = _refractInfo[13];
		scene = opSmoothUnionMat(scene, obj, _combineOps[13].y);
		// ######### New Game Object (13) #########

		// ######### New Game Object (14) #########
		obj.dist = distBuffer[14];
		obj.colour = _rm_colours[14];
		obj.reflInfo = _reflInfo[14];
		obj.refractInfo = _refractInfo[14];
		scene = opSmoothUnionMat(scene, obj, _combineOps[14].y);
		// ######### New Game Object (14) #########

		// ######### New Game Object (15) #########
		obj.dist = distBuffer[15];
		obj.colour = _rm_colours[15];
		obj.reflInfo = _reflInfo[15];
		obj.refractInfo = _refractInfo[15];
		scene = opSmoothUnionMat(scene, obj, _combineOps[15].y);
		// ######### New Game Object (15) #########

		// ######### New Game Object (16) #########
		obj.dist = distBuffer[16];
		obj.colour = _rm_colours[16];
		obj.reflInfo = _reflInfo[16];
		obj.refractInfo = _refractInfo[16];
		scene = opSmoothUnionMat(scene, obj, _combineOps[16].y);
		// ######### New Game Object (16) #########

		// ######### New Game Object (17) #########
		obj.dist = distBuffer[17];
		obj.colour = _rm_colours[17];
		obj.reflInfo = _reflInfo[17];
		obj.refractInfo = _refractInfo[17];
		scene = opSmoothUnionMat(scene, obj, _combineOps[17].y);
		// ######### New Game Object (17) #########

		// ######### New Game Object (18) #########
		obj.dist = distBuffer[18];
		obj.colour = _rm_colours[18];
		obj.reflInfo = _reflInfo[18];
		obj.refractInfo = _refractInfo[18];
		scene = opSmoothUnionMat(scene, obj, _combineOps[18].y);
		// ######### New Game Object (18) #########

		// ######### New Game Object (19) #########
		obj.dist = distBuffer[19];
		obj.colour = _rm_colours[19];
		obj.reflInfo = _reflInfo[19];
		obj.refractInfo = _refractInfo[19];
		scene = opSmoothUnionMat(scene, obj, _combineOps[19].y);
		// ######### New Game Object (19) #########

		// ######### New Game Object (20) #########
		obj.dist = distBuffer[20];
		obj.colour = _rm_colours[20];
		obj.reflInfo = _reflInfo[20];
		obj.refractInfo = _refractInfo[20];
		scene = opSmoothUnionMat(scene, obj, _combineOps[20].y);
		// ######### New Game Object (20) #########

		// ######### New Game Object (21) #########
		obj.dist = distBuffer[21];
		obj.colour = _rm_colours[21];
		obj.reflInfo = _reflInfo[21];
		obj.refractInfo = _refractInfo[21];
		scene = opSmoothUnionMat(scene, obj, _combineOps[21].y);
		// ######### New Game Object (21) #########

		// ######### New Game Object (22) #########
		obj.dist = distBuffer[22];
		obj.colour = _rm_colours[22];
		obj.reflInfo = _reflInfo[22];
		obj.refractInfo = _refractInfo[22];
		scene = opSmoothUnionMat(scene, obj, _combineOps[22].y);
		// ######### New Game Object (22) #########

		// ######### New Game Object (23) #########
		obj.dist = distBuffer[23];
		obj.colour = _rm_colours[23];
		obj.reflInfo = _reflInfo[23];
		obj.refractInfo = _refractInfo[23];
		scene = opSmoothUnionMat(scene, obj, _combineOps[23].y);
		// ######### New Game Object (23) #########

		// ######### New Game Object (24) #########
		obj.dist = distBuffer[24];
		obj.colour = _rm_colours[24];
		obj.reflInfo = _reflInfo[24];
		obj.refractInfo = _refractInfo[24];
		scene = opSmoothUnionMat(scene, obj, _combineOps[24].y);
		// ######### New Game Object (24) #########

		// ######### New Game Object (25) #########
		obj.dist = distBuffer[25];
		obj.colour = _rm_colours[25];
		obj.reflInfo = _reflInfo[25];
		obj.refractInfo = _refractInfo[25];
		scene = opSmoothUnionMat(scene, obj, _combineOps[25].y);
		// ######### New Game Object (25) #########

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
        //int rayHit = cheapRaymarch(rayOrigin, rayDir, linearEyeDepth, _maxSteps, _maxDrawDist, p, distField);
        int rayHit = raymarch(rayOrigin, rayDir, linearEyeDepth, _maxSteps, _maxDrawDist, p, distField, 1);

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
        rayDir = normalize(input.ray);
        rayOrigin = _CameraPos.xyz;
        p = 0.0;
        distField.totalDist = 0.0;

        rayHit = cheapRaymarch(rayOrigin, rayDir, depth, _maxSteps, _maxDrawDist, p, distField);
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
