using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    [SerializeField]
    List<TestPortal> _portals = new List<TestPortal>();

    
    public void activatePortals()
    {
        GameObject obj;

        foreach (TestPortal portal in _portals)
        {
            obj = portal.transform.Find("Portal Render").gameObject;
            obj.SetActive(true);
            obj.GetComponent<Animator>().SetTrigger("PortalActivate");
        }
    }
}
