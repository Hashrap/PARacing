﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public GameObject debrisPrefab;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(15,30,45) * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            GameObject debris = (GameObject)Instantiate(debrisPrefab, transform.position, other.transform.rotation);
            ParticleSystem ps = debris.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.startSpeed = rb.velocity.magnitude;

            Destroy(this.gameObject);
        }
    }
}
