using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeted : MonoBehaviour
{

    private Transform target;

    public Transform Target
    {
        get
        {
            return target;
        }
        set
        {
            target = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Target != null)
        {
            Vector3 distance = Target.position - transform.position;
            transform.Translate(distance.normalized * Time.deltaTime, Space.World);
        }
    }
}
