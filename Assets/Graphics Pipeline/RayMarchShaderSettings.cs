using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShaderKeywords
{
    BOUND_DEBUG,
    USE_DEPTH_TEX,
    USE_DIST_TEX
}

[CreateAssetMenu(fileName = "New Ray March Shader", menuName = "Ray March Shader")]
public class RayMarchShaderSettings : ScriptableObject
{
    [SerializeField]
    private List<ShaderKeywords> _keywords = new List<ShaderKeywords>();// The material keywords to disable for this shader.
    [SerializeField]
    private Shader _effectShader = null;
    [SerializeField]
    private string _shaderName;



    [SerializeField]
    private Texture2D _colourRamp = null;
    [SerializeField]
    private Texture2D _performanceRamp = null;
    [SerializeField]
    private Texture2D _wood = null;
    [SerializeField]
    private Texture2D _brick = null;

    [Header("Ray March")]
    [SerializeField]
    [Range(0.0f, 600.0f)]
    private int _maxSteps = 100;
    [SerializeField]
    [Range(0.0f, 600.0f)]
    private float _maxDrawDist = 64.0f;//12

    // ######### Light Variables #########
    [Header("Light")]
    // Floats
    [SerializeField]
    private float _specularExp = 160.0f;
    [SerializeField]
    private float _attenuationConstant = 1.5f;
    [SerializeField]
    private float _attenuationLinear = 0.01f;
    [SerializeField]
    private float _attenuationQuadratic = 0.001f;

    // Vectors
    [SerializeField]
    private Color _ambientColour = new Color(137.0f, 137.0f, 137.0f, 0.0f);
    [SerializeField]
    private Color _diffuseColour = new Color(130.0f, 130.0f, 130.0f, 0.0f);
    [SerializeField]
    private Color _specularColour = new Color(255.0f, 255.0f, 255.0f, 0.0f);
    [SerializeField]
    private Vector3 _lightConstants = new Vector3(0.2f, 0.2f, 2.0f);
    [SerializeField]
    private Color _rimLightColour = new Color(255.0f, 0.0f, 219.0f, 0.0f);
    [SerializeField]
    private Transform _sunLight;
    // ######### Light Variables #########


    [Header("Shadow")]
    // ######### Shadow Variables #########
    [SerializeField]
    [Range(1.0f, 140.0f)]
    private float _penumbraFactor = 15.0f;
    [SerializeField]
    [Range(0.01f, 5.0f)]
    private float _shadowMinDist = 0.01f;
    [SerializeField]
    [Range(0.0f, 10.0f)]
    private float _shadowIntensity = 1.0f;
    // ######### Shadow Variables #########


    // ######### Reflection Variables #########
    [Header("Reflection")]
    [SerializeField]
    [Range(0.0f, 3.0f)]
    private int _reflectionCount = 0;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float _reflectionIntensity = 0.0f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float _envReflIntensity = 0.0f;
    [SerializeField]
    private Texture _skybox = null;
    // ######### Reflection Variables #########

    // ######### Ambient Occlusion Variables #########
    [Header("Ambient Occlusion")]
    [SerializeField]
    [Range(0.0f, 5.0f)]
    int _aoMaxSteps = 3;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    float _aoStepSize = 0.2f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    float _aoIntensity = 0.3f;
    // ######### Ambient Occlusion Variables #########

    [Header("Vignette")]
    [SerializeField]
    [Range(0.0f, 2.0f)]
    public float _vignetteIntensity = 0.0f;

    [Header("Fog")]
    [SerializeField]
    [Range(0.0f, 0.04f)]
    private float _fogExtinction = 0.0f;
    [SerializeField]
    [Range(0.0f, 0.04f)]
    private float _fogInscattering = 0.0f;
    [SerializeField]
    private Color _fogColour = Color.grey;

    // TO-DO delete unsued hidden shaders

    #region Getters and Setters
    public List<ShaderKeywords> Keywords
    {
        get
        {
            return _keywords;
        }
    }

    public Shader EffectShader
    {
        get
        {
            return _effectShader;
        }
    }

    public string ShaderName
    {
        get
        {
            return _shaderName;
        }
        set
        {
            _shaderName = value;
        }
    }

    public Texture2D ColourRamp
    {
        get
        {
            return _colourRamp;
        }
    }

    public Texture2D PerformanceRamp
    {
        get
        {
            return _performanceRamp;
        }
    }

    public Texture2D Wood
    {
        get
        {
            return _wood;
        }
    }

    public Texture2D Brick
    {
        get
        {
            return _brick;
        }
    }

    public int MaxSteps
    {
        get
        {
            return _maxSteps;
        }
        set
        {
            _maxSteps = value;
        }
    }

    public float MaxDrawDist
    {
        get
        {
            return _maxDrawDist;
        }
        set
        {
            _maxDrawDist = value;
        }
    }

    public float SpecularExp
    {
        get
        {
            return _specularExp;
        }
        set
        {
            _specularExp = value;
        }
    }

    public float AttenuationConstant
    {
        get
        {
            return _attenuationConstant;
        }
        set
        {
            _attenuationConstant = value;
        }
    }

    public float AttenuationLinear
    {
        get
        {
            return _attenuationLinear;
        }
        set
        {
            _attenuationLinear = value;
        }
    }

    public float AttenuationQuadratic
    {
        get
        {
            return _attenuationQuadratic;
        }
        set
        {
            _attenuationQuadratic = value;
        }
    }

    public Color AmbientColour
    {
        get
        {
            return _ambientColour;
        }
        set
        {
            _ambientColour = value;
        }
    }

    public Color DiffuseColour
    {
        get
        {
            return _diffuseColour;
        }
        set
        {
            _diffuseColour = value;
        }
    }

    public Color SpecualarColour
    {
        get
        {
            return _specularColour;
        }
        set
        {
            _specularColour = value;
        }
    }

    public Vector3 LightConstants
    {
        get
        {
            return _lightConstants;
        }
        set
        {
            _lightConstants = value;
        }
    }

    public Color RimLightColour
    {
        get
        {
            return _rimLightColour;
        }
        set
        {
            _rimLightColour = value;
        }
    }

    public Transform SunLight
    {
        get
        {
            return _sunLight;
        }
        set
        {
            _sunLight = value;
        }
    }

    public float PenumbraFactor
    {
        get
        {
            return _penumbraFactor;
        }
        set
        {
            _penumbraFactor = value;
        }
    }

    public float ShadowmMinDist
    {
        get
        {
            return _shadowMinDist;
        }
        set
        {
            _shadowMinDist = value;
        }
    }

    public float ShadowIntensity
    {
        get
        {
            return _shadowIntensity;
        }
        set
        {
            _shadowIntensity = value;
        }
    }

    public int ReflectionCount
    {
        get
        {
            return _reflectionCount;
        }
        set
        {
            _reflectionCount = value;
        }
    }

    public float ReflectionIntensity
    {
        get
        {
            return _reflectionIntensity;
        }
        set
        {
            _reflectionIntensity = value;
        }
    }

    public float EnvReflIntensity
    {
        get
        {
            return _envReflIntensity;
        }
        set
        {
            _envReflIntensity = value;
        }
    }

    public Texture SkyBox
    {
        get
        {
            return _skybox;
        }
        set
        {
            _skybox = value;
        }
    }

    public int AOMaxSteps
    {
        get
        {
            return _aoMaxSteps;
        }
        set
        {
            _aoMaxSteps = value;
        }
    }

    public float AOStepSize
    {
        get
        {
            return _aoStepSize;
        }
        set
        {
            _aoStepSize = value;
        }
    }

    public float AOItensity
    {
        get
        {
            return _aoIntensity;
        }
        set
        {
            _aoIntensity = value;
        }
    }

    public float VignetteIntesnity
    {
        get
        {
            return _vignetteIntensity;
        }
        set
        {
            _vignetteIntensity = value;
        }
    }

    public float FogExtinction
    {
        get
        {
            return _fogExtinction;
        }
        set
        {
            _fogExtinction = value;
        }
    }

    public float FogInscattering
    {
        get
        {
            return _fogInscattering;
        }
        set
        {
            _fogInscattering = value;
        }
    }

    public Color FogColour
    {
        get
        {
            return _fogColour;
        }
        set
        {
            _fogColour = value;
        }
    }
    #endregion Getters and Setters



    // Start is called before the first frame update
    
}
