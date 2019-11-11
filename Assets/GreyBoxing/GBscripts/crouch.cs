using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class crouch : MonoBehaviour
{
    public BoxCollider collider1;
    public GameObject brother;

    public Vector3 pos1;

    private bool activate = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(activate && Input.GetKey(KeyCode.E))
        {
            brother.gameObject.transform.position = pos1;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Brother")
            activate = true;
        else
            activate = false;
    }

}
