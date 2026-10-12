using System;
using UnityEngine;
using System.Collections;

public class Tester : MonoBehaviour
{
    [Header("PRESS SPACE TO SPAWN MORE BOIDS")]
    [SerializeField] BoidController controller;

    void Start()
    {
        controller = FindObjectOfType<BoidController>();
    }
    
    
    private void Update(){
        if (Input.GetKeyDown(KeyCode.Space)){
            controller.Spawn(controller.transform.position - controller.transform.forward * 8);
        }
    }
}
