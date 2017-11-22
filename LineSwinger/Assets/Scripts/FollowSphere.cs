using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowSphere : MonoBehaviour { 
    public Transform sphere;
    private Vector3 offset;
    private void Start()
    {
        // get relative position of camera from player/sphere
        offset = sphere.position + transform.position;
    }

    // Use lateupdate to ensure positioning of sphere is complete
    void LateUpdate () {
        // Move to new poition
        transform.position = sphere.position + offset;
	}
}
