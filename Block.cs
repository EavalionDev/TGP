using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    private Rigidbody rb;
    private GameObject craneGrabScript;
    public bool turnOffVel;
    private bool landed;
    public float velLimit;
    // Start is called before the first frame update
    void Start()
    {
        velLimit = 10f;
        landed = false;
        turnOffVel = false;
        rb = GetComponent<Rigidbody>();
        craneGrabScript = GameObject.Find("NextObjectStartPoint");
    }

    // Update is called once per frame
    void Update()
    {
        if (turnOffVel)
        {
            StartCoroutine(TurnOffVelocity());
        }

        if (craneGrabScript.GetComponent<CraneGrabObject>().boxFalling && craneGrabScript.GetComponent<CraneGrabObject>().blocksDropped == 1)
        {
            rb.mass = 100;
        }
        
    }
    private void FixedUpdate()
    {
        
        if (landed)
        {
            Vector3 vel = rb.velocity;
            if (vel.magnitude > velLimit)
            {
                rb.useGravity = true;
            }
        }
    }

    public IEnumerator TurnOffVelocity()
    {
        yield return new WaitForSeconds(1.5f);
        rb.velocity = Vector3.zero;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Blocks"))
        {
            rb.useGravity = false;
            landed = true;
            //rb.constraints = RigidbodyConstraints.FreezeRotationY |  RigidbodyConstraints.FreezePositionZ;
        }
    }
}
