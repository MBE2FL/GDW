using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NVIDIA.Flex;
using System;

public class MetaBallFluid : MonoBehaviour
{

    Flex.Library _library;
    Flex.Solver _solver;

    Flex.Buffer _particleBuffer;
    Flex.Buffer _velocityBuffer;
    Flex.Buffer _phaseBuffer;
    Flex.Buffer _activeBuffer;

    // Map buffers for reading / writing.
    IntPtr particles;
    IntPtr velocities;
    IntPtr phases;

    RMMemoryManager _memoryManager;


    private void Awake()
    {
        _library = Flex.Init();
        Flex.SolverDesc _solverDesc = new Flex.SolverDesc();
        Flex.SetSolverDescDefaults(ref _solverDesc);
        _solver = Flex.CreateSolver(_library, ref _solverDesc);

        _particleBuffer = Flex.AllocBuffer(_library, 28, sizeof(float) * 4, Flex.BufferType.Host);
        _velocityBuffer = Flex.AllocBuffer(_library, 28, sizeof(float) * 4, Flex.BufferType.Host);
        _phaseBuffer = Flex.AllocBuffer(_library, 28, sizeof(int), Flex.BufferType.Host);
        _activeBuffer = Flex.AllocBuffer(_library, 28, sizeof(int), Flex.BufferType.Host);

        unsafe
        {
            int* activeIndices = (int*)Flex.Map(_activeBuffer);

            for (int i = 0; i < 28; ++i)
            {
                activeIndices[i] = i;
            }

            Flex.Unmap(_activeBuffer);
            Flex.SetActive(_solver, _activeBuffer);
            Flex.SetActiveCount(_solver, 28);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _memoryManager = Camera.main.GetComponent<RMMemoryManager>();

        Flex.Params param = new Flex.Params();
        param.radius = 10.0f;
        param.fluidRestDistance = 1.0f;
        param.gravity = new Vector3(0.0f, -9.81f, 0.0f);
        //param.viscosity = 10.0f;

        Flex.SetParams(_solver, ref param);


        // Map buffers for reading / writing.
        particles = Flex.Map(_particleBuffer);
        velocities = Flex.Map(_velocityBuffer);
        phases = Flex.Map(_phaseBuffer);

        // Spawn particles.
        spawnParticles(particles, velocities, phases);
    }

    private void OnDestroy()
    {
        Flex.FreeBuffer(_particleBuffer);
        Flex.FreeBuffer(_velocityBuffer);
        Flex.FreeBuffer(_phaseBuffer);

        Flex.DestroySolver(_solver);
        Flex.Shutdown(_library);
    }

    // Update is called once per frame
    void Update()
    {
        // Map buffers for reading / writing.
        particles = Flex.Map(_particleBuffer);
        velocities = Flex.Map(_velocityBuffer);
        phases = Flex.Map(_phaseBuffer);

        // Spawn particles.
        //spawnParticles(particles, velocities, phases);

        // Render particles.
        RenderParticles(particles, velocities, phases);

        // Unmap buffers.
        Flex.Unmap(_particleBuffer);
        Flex.Unmap(_velocityBuffer);
        Flex.Unmap(_phaseBuffer);

        // Write to device (async).
        Flex.SetParticles(_solver, _particleBuffer);
        Flex.SetVelocities(_solver, _velocityBuffer);
        Flex.SetPhases(_solver, _phaseBuffer);

        // Tick.
        Flex.UpdateSolver(_solver, Time.deltaTime, 1);

        // Read back (async).
        Flex.GetParticles(_solver, _particleBuffer);
        Flex.GetVelocities(_solver, _velocityBuffer);
        Flex.GetPhases(_solver, _phaseBuffer);
    }

    void spawnParticles(IntPtr particles, IntPtr velocities, IntPtr phases)
    {
        List<RMPrimitive> prims = _memoryManager.RM_Prims;
        Vector4 particle;
        Vector3 velocity;
        int phase = Flex.MakePhase(0, Flex.Phase.SelfCollide | Flex.Phase.Fluid);

        unsafe
        {

            Vector4* particlesPtr = (Vector4*)particles;
            Vector3* velocitiesPtr = (Vector3*)velocities;
            int* phasesPtr = (int*)phases;

            for (int i = 0; i < 28; ++i)
            {
                particlesPtr[i] = new Vector4(UnityEngine.Random.Range(-10.0f, 10.0f), -3.5f, 13.0f, 1.0f);
                velocitiesPtr[i] = new Vector3(0.0f, 0.0f, 0.0f);
                phasesPtr[i] = phase;
            }
        }
    }

    void RenderParticles(IntPtr particles, IntPtr velocities, IntPtr phases)
    {
        List<RMPrimitive> prims = _memoryManager.RM_Prims;
        Vector4 particle;
        Vector3 velocity;

        unsafe
        {

            Vector4* particlesPtr = (Vector4*)particles;
            Vector3* velocitiesPtr = (Vector3*)velocities;

            for (int i = 0; i < 28; ++i)
            {
                particle = particlesPtr[i];
                velocity = velocitiesPtr[i];

                //velocity += new Vector3(0.0f, -9.81f, 0.0f) * Time.deltaTime;

                //prims[i].transform.localPosition += velocity;// * Time.deltaTime;
                prims[i].transform.position = new Vector3(particle.x, particle.y, particle.z);
                prims[i].transform.position += velocity;
                Debug.Log("Position: " + particle.ToString());
                Debug.Log("Velocity: " + velocity.ToString());
            }
        }

    }
}
