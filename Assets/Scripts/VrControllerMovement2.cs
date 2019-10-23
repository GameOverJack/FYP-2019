using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class VrControllerMovement2 : MonoBehaviour
{
    public float Speed = 0.0f;
    public float Deadzone = 0.0f;

    public SteamVR_Action_Vector2 TouchPadValue = null;
    public GameObject HandAxis;

    private  Vector3 _moveDirection;

 
    private Transform _head = null;

    private void Start()
    {
        _head = SteamVR_Render.Top().head;
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
        GetComponent<CapsuleCollider>().height = headHeight;
        //Cut in half
        Vector3 newCenter = Vector3.zero;
        newCenter.y = GetComponent<CapsuleCollider>().height / 2;


        //Move player capsule in local space
        newCenter.x = _head.localPosition.x;
        newCenter.z = _head.localPosition.z;

        //Rotate
        newCenter = Quaternion.Euler(0, -transform.eulerAngles.y, 0) * newCenter;


        //Apply
        GetComponent<CapsuleCollider>().center = newCenter;
    }

    private void CalculateMovement()
    {
        //Calculate movement orientation

        Vector3 orientationEuler = new Vector3(0, _head.eulerAngles.y, 0);
        Vector3 movement = Vector3.zero;
        Quaternion orientation = Quaternion.Euler(orientationEuler);

        _moveDirection = Quaternion.AngleAxis(GetTouchpadAngle() + HandAxis.transform.localRotation.eulerAngles.y, Vector3.up) * Vector3.forward;

        if(GetComponent<Rigidbody>().velocity.magnitude < Speed && TouchPadValue.axis.magnitude > Deadzone)
        {
            GetComponent<Rigidbody>().AddForce(_moveDirection * 30);
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

    private void PlayerJump() 
    {
        
    }
    
}
