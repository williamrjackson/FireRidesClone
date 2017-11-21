using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowSphere : MonoBehaviour { 
    public Transform sphere;
    private Vector3 offset;
    private void Start()
    {
        offset = sphere.position + transform.position;
    }

    // Update is called once per frame
    void LateUpdate () {
        transform.position = sphere.position + offset;
	}
}
