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

    private int floorValue;

    private Transform _head = null;
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

        Vector3 orientationEuler = new Vector3(0, _head.eulerAngles.y, 0);
        Vector3 movement = Vector3.zero;
        Quaternion orientation = Quaternion.Euler(orientationEuler);

        _moveDirection = Quaternion.AngleAxis(GetTouchpadAngle() + HandAxis.transform.localRotation.eulerAngles.y, Vector3.up) * Vector3.forward;

        if(TouchPadValue.axis.magnitude > Deadzone)
        {
            _collider.material = NoFrictionMaterial;
            velocity = _moveDirection;

            if(JumpTrigger.GetStateDown(MovementController) && floorValue > 0)
            {
                float jumpSpeed = Mathf.Sqrt(2 * JumpHeight * 9.81f);
                _rigidBody.AddForce(0, jumpSpeed, 0, ForceMode.VelocityChange);
            }
            _rigidBody.AddForce(velocity.x *Speed - _rigidBody.velocity.x, 0, velocity.z *Speed - _rigidBody.velocity.z, ForceMode.VelocityChange);

            Debug.Log("Velocity" + velocity);
            Debug.Log("Movement Direction:" + _moveDirection);
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

    private void OnCollisionEnter(Collision collision)
    {
        floorValue++;
    }
    private void OnCollisionExit(Collision collision)
    {
        floorValue--;
    }

    private void PlayerJump() 
    {
        
    }
    
}
