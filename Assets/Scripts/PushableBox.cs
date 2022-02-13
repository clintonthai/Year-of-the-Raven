using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableBox : PhysicsObject
{
    public float pushSpeed;
    private float peckedTimer;
    public RideRegion rideRegion;

    protected override void Awake() {
        base.Awake();
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
        
        peckedTimer = Mathf.Max(peckedTimer - Time.deltaTime, 0f);
        if (peckedTimer == 0f) {
            rigidbody.mass = 1000f;
        }
    }


    protected override Vector3 HandleHorizontalMovement(Vector3 hVelocity) {
        if (peckedTimer > 0f) {
            // while the box has been pecked, ignore any rides and apply friction
            hVelocity = Vector3.MoveTowards(hVelocity, Vector3.zero, friction);
            return hVelocity;
        }

        return base.HandleHorizontalMovement(hVelocity);
    }


    public override Vector3 GetRidePoint() {
        return rideRegion.transform.position;
    }


    protected void OnTriggerEnter(Collider other) {
        // base.OnTriggerEnter(other);

        if (peckedTimer == 0f && other.CompareTag("Peck") && (groundNormal != Vector3.zero || currentTornado != null)) {
            Vector3 pushDirection = other.transform.forward;
            Debug.DrawRay(collider.bounds.center, other.transform.forward * 1f, Color.yellow, 0.2f);
            
            // move
            rigidbody.velocity = (pushDirection * pushSpeed).WithY(rigidbody.velocity.y);
            peckedTimer = 0.18f;
            rigidbody.mass = 10f;
        }
    }


    
}
