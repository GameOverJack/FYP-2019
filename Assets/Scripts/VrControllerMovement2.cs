using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class VrControllerMovement2 : MonoBehaviour
{
    public float Speed = 0.0f;
    public float Deadzone = 0.0f;
    public float JumpHeight = 1.0f;

    public SteamVR_Input_Sources MovementController;
    public SteamVR_Action_Vector2 TouchPadValue = null;
    public SteamVR_Action_Boolean JumpTrigger;
    public GameObject HandAxis;

    public PhysicMaterial NoFrictionMaterial;
    public PhysicMaterial FrictionMaterial;

    private  Vector3 _moveDirection;
    private Vector3 velocity = new Vector3(0, 0, 0);
    private Vector3 _collisionNormal = new Vector3(0, 0, 0);

    private int floorValue;
    private int surfingValue;

    private Transform _head = null;
    private Transform _surfingPlatform;
    private CapsuleCollider _collider;
    private Rigidbody _rigidBody;

    private void Start()
    {
        _head = SteamVR_Render.Top().head;
        _collider = GetComponent<CapsuleCollider>();
        _rigidBody = GetComponent<Rigidbody>();
    }
    
    private void Update()
    {
        HandleHeight();
        CalculateMovement();
    }

    //Changes CharacterController height to be the cameras height
    private void HandleHeight()
    {
        //Get head of player in local space
        float headHeight = Mathf.Clamp(_head.localPosition.y, 1, 2);
        _collider.height = headHeight;
        //Cut in half
        Vector3 newCenter = Vector3.zero;
        newCenter.y = _collider.height / 2;


        //Move player capsule in local space
        newCenter.x = _head.localPosition.x;
        newCenter.z = _head.localPosition.z;

        //Rotate
        newCenter = Quaternion.Euler(0, -transform.eulerAngles.y, 0) * newCenter;


        //Apply
        _collider.center = newCenter;
    }

    private void CalculateMovement()
    {
        //Calculate movement orientation

        Quaternion orientation = CalculateOrientation();
        Vector3 movement = Vector3.zero;
        if(surfingValue > 0)
        {
            _moveDirection = _head.forward;
            Vector3 temp = Vector3.Cross(_collisionNormal, _moveDirection);
            _moveDirection = Vector3.Cross(temp, _collisionNormal);
            _rigidBody.AddForce(_moveDirection.x - _rigidBody.velocity.x, _moveDirection.y - _rigidBody.velocity.y, _moveDirection.z - _rigidBody.velocity.z, ForceMode.VelocityChange);
        }
        else if (TouchPadValue.axis.magnitude > Deadzone)
        {
            _collider.material = NoFrictionMaterial;
            _moveDirection = orientation * (Speed * Vector3.forward);
            velocity = _moveDirection;

            if(JumpTrigger.GetStateDown(MovementController) && floorValue > 0)
            {
                float jumpSpeed = Mathf.Sqrt(2 * JumpHeight * 9.81f);
                _rigidBody.AddForce(0, jumpSpeed, 0, ForceMode.VelocityChange);
            }
            _rigidBody.AddForce(_moveDirection.x - _rigidBody.velocity.x, 0, _moveDirection.z - _rigidBody.velocity.z, ForceMode.VelocityChange);

        }
        else if(floorValue > 0)
        {
            _collider.material = FrictionMaterial;
        }
    }

    private float GetTouchpadAngle()
    {
        if(TouchPadValue.axis.x < 0) 
        {
            return 360 - (Mathf.Atan2(TouchPadValue.axis.x, TouchPadValue.axis.y) * Mathf.Rad2Deg * -1);
        }

        else
        {
            return Mathf.Atan2(TouchPadValue.axis.x, TouchPadValue.axis.y) * Mathf.Rad2Deg;
        }
    }

    private Quaternion CalculateOrientation()
    {
        float rotation = Mathf.Atan2(TouchPadValue.axis.x, TouchPadValue.axis.y);
        rotation *= Mathf.Rad2Deg;

        Vector3 orientationEuler = new Vector3(0, _head.eulerAngles.y + rotation, 0);
        return Quaternion.Euler(orientationEuler);
    }

    /*Checks to see if the player is on a surface, if they are enable the ability to jump 
     get the normal of the collsion for surfing*/
    private void OnCollisionEnter(Collision collision)
    {
        _collisionNormal = collision.contacts[0].normal;
        floorValue++;
    }

    // As they are jumping disable the ability to jump in midair
    private void OnCollisionExit(Collision collision)
    {
            floorValue--;
    }

    //disable the gravity when surfing
    private void OnTriggerEnter(Collider collider)
    {
        surfingValue++;
        
        if (collider.tag == "SurfingPlatform" && surfingValue == 1)
        {
            _surfingPlatform = collider.transform;
            
            _collider.material = FrictionMaterial;
            _rigidBody.useGravity = false;
            _rigidBody.velocity = Vector3.zero;
            _rigidBody.Sleep();
            Debug.Log("Trigger enter");
        }

    }

    //re-enable the rigidbody when leaving a SurfingPlatform
    private void OnTriggerExit(Collider collider)
    {
        surfingValue--;
        if (collider.tag == "SurfingPlatform" && surfingValue == 0)
        {
            _rigidBody.WakeUp();
            _rigidBody.useGravity = true;
            Debug.Log("TriggerExit");
        }

    }
}
