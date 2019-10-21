﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class VrController : MonoBehaviour
{

    public float Sensitivity = 0.1f;
    public float MaxSpeed = 1.0f;

    public SteamVR_Action_Boolean TouchPadPressed = null;
    public SteamVR_Action_Vector2 TouchPadValue = null;
    
    private float _speed = 0.0f;

    private CharacterController _characterController = null;
    private Transform _cameraRig = null;
    private Transform _head = null;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        _cameraRig = SteamVR_Render.Top().origin;
        _head = SteamVR_Render.Top().head;
    }

    private void Update()
    {
        HandleHead();
        HandleHeight();
        CalculateMovement();
    }

    private void HandleHead()
    {
        //Store current
        Vector3 oldPosition = _cameraRig.position;
        Quaternion oldRotation = _cameraRig.rotation;

        //Rotation
        transform.eulerAngles = new Vector3(0.0f, _head.rotation.eulerAngles.y, 0.0f);

        //Restore position

        _cameraRig.position = oldPosition;
        _cameraRig.rotation = oldRotation;
    }

    private void CalculateMovement()
    {
        //Calculate movement orientation

        Vector3 orientationEuler = new Vector3(0, transform.eulerAngles.y, 0);
        Vector3 movement = Vector3.zero;
        Quaternion orientation = Quaternion.Euler(orientationEuler);

        //If not moving set speed to zero
        if (TouchPadPressed.GetLastStateUp(SteamVR_Input_Sources.Any))
            _speed = 0;

        //If button pressed on either controller
        if (TouchPadPressed.state)
        {
            // Add clamp to speed
            _speed += TouchPadValue.axis.y * Sensitivity;
            _speed = Mathf.Clamp(_speed, -MaxSpeed, MaxSpeed);

            //Orientation
            movement += orientation * (_speed * Vector3.forward * Time.deltaTime);
        }


        //Apply

        _characterController.Move(movement);
    }

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
}
