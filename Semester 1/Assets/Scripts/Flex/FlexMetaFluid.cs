using NVIDIA.Flex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FlexMetaFluid : FlexFluidRenderer
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public override void OnFlexUpdate(FlexContainer.ParticleData _particleData)
    {
        //base.OnFlexUpdate(_particleData);

        if (m_actor && m_actor.container)
        {
            m_actor.container.AddFluidIndices(m_actor.indices, m_actor.indexCount);
        }

        if (Application.IsPlaying(this))
        {
            Vector4[] particles = new Vector4[10000];
            //_particleData.GetParticles(0, 9999, particles);
            _particleData.GetRestParticles(0, 9999, particles);

            Array.Sort<Vector4>(particles, new Comparison<Vector4>(
                                                                    (i1, i2) => Vector4.SqrMagnitude(i2).CompareTo(Vector4.SqrMagnitude(i1))));

            //RMMemoryManager rmm = Camera.main.GetComponent<RMMemoryManager>();
            //List<RMPrimitive> prims = rmm.RM_Prims;
            RayMarcher rayMarcher = Camera.main.GetComponent<RayMarcher>();
            List<RMObj> renderList = rayMarcher.RenderList;
            Vector4 force;

            for (int i = 0; i < 28; ++i)
            {
                force = particles[i];
                force += new Vector4(0.0f, -9.81f, 0.0f, 0.0f);
                renderList[i].transform.position += new Vector3(force.x, force.y, force.z) * Time.deltaTime;
            }
        }
    }
}
