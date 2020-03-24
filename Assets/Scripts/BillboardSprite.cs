using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    public bool lookOnYAxis;
    public Transform target;
    public bool findPlayer = true;

    void Start(){
        if(findPlayer)
            target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void LateUpdate(){
        transform.LookAt(new Vector3(target.position.x, (lookOnYAxis ? target.position.y : transform.position.y), target.position.z));
    }
}
