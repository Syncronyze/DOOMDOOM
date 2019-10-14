using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    public bool lookOnYAxis;
    Transform player;

    void Start(){
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update(){
        transform.LookAt(new Vector3(player.position.x, (lookOnYAxis ? player.position.y : transform.position.y), player.position.z)) ;
    }
}
