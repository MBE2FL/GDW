using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    [SerializeField]
    List<TestPortal> _portals = new List<TestPortal>();

    public bool _turnOnPortals = false;

    private void Update()
    {
        if (_turnOnPortals)
        {
            activatePortals();
            _turnOnPortals = false;
        }
    }

    public void activatePortals()
    {
        GameObject obj;

        foreach (TestPortal portal in _portals)
        {
            obj = portal.transform.Find("Portal Render").gameObject;
            obj.SetActive(true);
            obj.GetComponent<Animator>().SetTrigger("PortalActivate");

            obj = portal.transform.Find("portal w_particles").transform.Find("Portal particles").gameObject;
            obj.SetActive(true);
            obj.GetComponent<Animator>().SetTrigger("PortalActivate");
        }
    }
}
