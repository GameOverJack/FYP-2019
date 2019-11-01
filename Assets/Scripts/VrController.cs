using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class VrController : MonoBehaviour
{
    public float Gravity = 30.0f;
    public float Sensitivity = 0.1f;
    public float MaxSpeed = 1.0f;

    public SteamVR_Action_Boolean TouchPadPressed = null;
    public SteamVR_Action_Vector2 TouchPadValue = null;
    
    private float _speed = 0.0f;

    private CharacterController _characterController = null;
    private Transform _head = null;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

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
        _characterController.height = headHeight;

        //Cut in half
        Vector3 newCenter = Vector3.zero;
        newCenter.y = _characterController.height / 2;
        newCenter.y += _characterController.skinWidth;


        //Move player capsule in local space
        newCenter.x = _head.localPosition.x;
        newCenter.z = _head.localPosition.z;

        //Rotate
        newCenter = Quaternion.Euler(0, -transform.eulerAngles.y, 0) * newCenter;


        //Apply
        _characterController.center = newCenter;
    }

    private void CalculateMovement()
    {
        //Calculate movement orientation
        Vector3 movement = Vector3.zero;

        //If not moving set speed to zero
        if (TouchPadValue.axis.magnitude == 0)
            _speed = 0;

        // Add clamp to speed
        _speed += TouchPadValue.axis.magnitude * Sensitivity;
        _speed = Mathf.Clamp(_speed, -MaxSpeed, MaxSpeed);

        //Orientation
        movement += CalculateOrientation() * (_speed * Vector3.forward);

        //Gravity
        movement.y -= Gravity * Time.deltaTime;


        //Apply

        _characterController.Move(movement * Time.deltaTime);
    }

    private Quaternion CalculateOrientation() 
    {
        float rotation = Mathf.Atan2(TouchPadValue.axis.x, TouchPadValue.axis.y);
        rotation *= Mathf.Rad2Deg;

        Vector3 orientationEuler = new Vector3(0, _head.eulerAngles.y + rotation, 0);
        return Quaternion.Euler(orientationEuler);
    }

    
}
