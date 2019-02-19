using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Kart : MonoBehaviour {
    public Camera mainCam;
    public Vector3 camOffset;

    private Rigidbody physics;
    //TODO: figure out how to do this without plowing into the dirt
    public Vector3 centerOfMass;

    public Vector3[] wheels;
    public Text[] wheelRatios; 
    public float suspensionForce;
    public float suspensionMax;

    public float acceleration;
    public Vector3 accelerationPos;
    public float reversePenaltyMultiplier;
    public float brakeForce;

    public float turnTorque;
    public float tractionMultiplier;
    //TODO: Finetune this value - affects max turn speed
    public float maxAngularVelocity;

    private string item;
    public GameObject ramen;
    private float boost;


	// Use this for initialization
	void Start () {
        boost = 0.0f;
        item = "Ramen";
        physics = GetComponent<Rigidbody>();
        physics.centerOfMass = centerOfMass;
        physics.maxAngularVelocity = maxAngularVelocity;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        //Kart physics
        RaycastHit hit;
        Vector3[] contactPoints = new Vector3[wheels.Length];
        Vector3[] contactNormals = new Vector3[wheels.Length];
        float[] compressionRatios = new float[wheels.Length];
        bool isGrounded = false;

        //Suspension
        //TODO: Finetune
        for (int i = 0; i < wheels.Length; i++)
        {
            //For each wheel location, aim a raycast downwards
            if (Physics.Raycast(transform.TransformPoint(wheels[i]),
                    transform.TransformDirection(Vector3.down),
                    out hit, suspensionMax))
            {   
                isGrounded= true;
                //This info will help us accelerate later.  Store it away for now.
                contactNormals[i] = hit.normal;
                //Calculate how much the suspension for a wheel is compressed from 0 (lax) to 1 (bottomed out)
                compressionRatios[i] = (suspensionMax - hit.distance) / suspensionMax;
                //Get the velocity of the wheel (if kart is rotating, they WILL differ!)
                Vector3 v = physics.GetPointVelocity(transform.TransformPoint(wheels[i]));
                //Project the wheel velocity on the local upwards (y+) axis
                //this value will be used to dampen suspension (avoid infinite oscillations)
                Vector3 suspension = Vector3.Project(v, transform.TransformDirection(Vector3.up));
                //Calculate how much force the suspension should push on the chassis
                Vector3 newSuspension = transform.TransformDirection(Vector3.up) * suspensionForce * compressionRatios[i];
                //Scale the suspension force by the amount of pre-existing upwards momentum (works downwards too!)
                Vector3 deltaSuspension = newSuspension - suspension;
                //Add the force as acceleration (ignores vehicle mass)
                physics.AddForceAtPosition(deltaSuspension, transform.TransformPoint(wheels[i]), ForceMode.Acceleration);
                Debug.DrawRay(transform.TransformPoint(wheels[i]), transform.TransformDirection(Vector3.down) * hit.distance, Color.green);
            }
            else
                Debug.DrawRay(transform.TransformPoint(wheels[i]), transform.TransformDirection(Vector3.down) * hit.distance, Color.red);

            //Debug
            compressionRatios[i] *= 100.0f;
            compressionRatios[i] = (int)compressionRatios[i] / 100.0f;
            wheelRatios[i].text = compressionRatios[i].ToString();

        }

        if(isGrounded)
        {
            //Acceleration/Braking
            //TODO: Disable in midair
            //TODO: add a max speed
            //Maybe: Minimum speed
            float x = 0, y = 0, z = 0;

            //Average the surface normals of all wheel contact points
            for (int i = 0; i < contactNormals.Length; i++)
            {
                x += contactNormals[i].x;
                y += contactNormals[i].y;
                z += contactNormals[i].z;
            }
            x /= contactNormals.Length;
            y /= contactNormals.Length;
            z /= contactNormals.Length;

            //We will use this value to determine the direction force should be applied by the engine
            Vector3 avgNormal = new Vector3(x, y, z);

            //Accelerate/Brake
            if(Input.GetKey(KeyCode.W))
            {
                physics.AddForceAtPosition(
                    //Project the kart's forward (z+) vector onto the average ground plane
                    Vector3.ProjectOnPlane(transform.TransformDirection(Vector3.forward) * acceleration, avgNormal),
                    //Force is applied slightly lower (y-) and moderately foreward (z+) of the center of volume
                    //This tilts the kart back and forth when accelerating and braking
                    transform.TransformPoint(accelerationPos),
                    //Ignore mass
                    ForceMode.Acceleration);
            }
            //Brake/Reverse
            else if (Input.GetKey(KeyCode.S))
            {
                float deceleration = acceleration;
                if(transform.TransformVector(physics.velocity).z <= 0)
                    deceleration *= reversePenaltyMultiplier;
                else
                    deceleration = brakeForce;
                //See above, but along the backwards (z-) axis
                physics.AddForceAtPosition(
                    //We do apply a penalty for being in reverse
                    Vector3.ProjectOnPlane(transform.TransformDirection(Vector3.back) * deceleration, avgNormal),
                    transform.TransformPoint(accelerationPos),
                    ForceMode.Acceleration);
            }

            //Turning
            //TODO: finetune turn penalty at low speed
            //TODO: make turning reversed when kart is going backwards
            //Left
            if (Input.GetKey(KeyCode.A))
            {
                //Apply negative torque to the origin along the y axis (heading).  Scaled by velocity.
                //TODO: Fix: Don't scale by vertical velocity!  Use just x and z, zero out y.
                physics.AddRelativeTorque(0, -turnTorque * Mathf.Clamp01(physics.velocity.magnitude / 4), 0, ForceMode.Force);
            }
            //Right
            else if(Input.GetKey(KeyCode.D))
            {
                //See above, but with positive torque
                physics.AddRelativeTorque(0, turnTorque * Mathf.Clamp01(physics.velocity.magnitude / 4), 0, ForceMode.Force);
            }

            //Traction/skid
            //TODO: finetune - reduce angular momentum further
            //Maybe: Minimum angular velocity
            //Get the local sideways velocity
            float xV = transform.InverseTransformVector(physics.velocity).x;
            //Scale it down by some finetuned value each frame
            physics.AddRelativeForce(-xV * tractionMultiplier, 0, 0, ForceMode.Acceleration);

            if (boost >= 0.0f)
            {

            }

            //Debug impulse
            if(Input.GetKeyDown(KeyCode.Space))
            {
                x = Random.value - 0.5f;
                z = Random.value - 0.5f;
                Vector3 pos = new Vector3(x, 0, z);
                physics.AddForceAtPosition(Vector3.up * 20, transform.TransformPoint(pos), ForceMode.Impulse);
            }
        }
	}

    private void Update()
    {
        //TODO: Camera needs to be better!
        //Maybe: Set camera behind the forward velocity (avoid rolling/pitching)
        Vector3 camPos = transform.position;
        camPos += transform.TransformDirection(Vector3.back) * camOffset.z;
        camPos += transform.TransformDirection(Vector3.up) * camOffset.y;
        mainCam.transform.position = camPos;
        mainCam.transform.LookAt(transform.position);

        if(Input.GetKeyDown(KeyCode.F) && !item.Equals("None"))
        {
            if (item.Equals("Ramen"))
            {
                GameObject tmp = (GameObject)Instantiate(ramen, transform.position, transform.rotation);
                Targeted t = tmp.GetComponent<Targeted>();
                t.Target = this.transform;
            }

            if(item.Equals("Boost"))
            {
                boost = 2.0f;
            }
        }
    }
}
