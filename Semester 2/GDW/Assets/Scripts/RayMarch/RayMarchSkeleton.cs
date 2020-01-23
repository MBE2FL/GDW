using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayMarchSkeleton : MonoBehaviour
{
    [SerializeField]
    List<RMObj> _rmObjs;


    // Update is called once per frame
    void Update()
    {
        int index = 0;

        recurseHierachy(transform, ref index);
    }

    void recurseHierachy(Transform parent, ref int index)
    {
        foreach (Transform child in parent)
        {
            if (index > _rmObjs.Count)
                break;

            _rmObjs[index].transform.position = child.position;

            ++index;

            recurseHierachy(child, ref index);
        }
    }
}
