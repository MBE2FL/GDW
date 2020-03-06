using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctreeCPU : MonoBehaviour
{
    LeafNode[] _leafNodes;
    InternalNode[] _internalNodes;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }



    // Expands a 10-bit integer into 30 bits
    // by inserting 2 zeros after each bit.
    uint expandBits(uint v)
    {
        v = (v * 0x00010001u) & 0xFF0000FFu;
        v = (v * 0x00000101u) & 0x0F00F00Fu;
        v = (v * 0x00000011u) & 0xC30C30C3u;
        v = (v * 0x00000005u) & 0x49249249u;
        return v;
    }

    // Calculates a 30-bit Morton code for the
    // given 3D point located within the unit cube [0,1].
    uint morton3D(float x, float y, float z)
    {
        x = Mathf.Min(Mathf.Max(x * 1024.0f, 0.0f), 1023.0f);
        y = Mathf.Min(Mathf.Max(y * 1024.0f, 0.0f), 1023.0f);
        z = Mathf.Min(Mathf.Max(z * 1024.0f, 0.0f), 1023.0f);
        uint xx = expandBits((uint)x);
        uint yy = expandBits((uint)y);
        uint zz = expandBits((uint)z);
        return xx * 4 + yy * 2 + zz;
    }


    Node generateHierarchy(uint[] sortedMortonCodes,
                         int[] sortedObjectIDs,
                         int numObjects)
    {
        LeafNode[] leafNodes = new LeafNode[numObjects];
        InternalNode[] internalNodes = new InternalNode[numObjects - 1];

        // Construct leaf nodes.
        // Note: This step can be avoided by storing
        // the tree in a slightly different way.
        for (int idx = 0; idx < numObjects; idx++) // in parallel
            leafNodes[idx].objectID = sortedObjectIDs[idx];


        // Construct internal nodes.
        for (int idx = 0; idx < numObjects - 1; idx++) // in parallel
        {
            // Find out which range of objects the node corresponds to.
            // (This is where the magic happens!)

            // Run compute shader.
        }

        // Node 0 is the root.
        return internalNodes[0];
    }
}


public class Node
{

}

public class LeafNode : Node
{

}

public class InternalNode : Node
{

}
