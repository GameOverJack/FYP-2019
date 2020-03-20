using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class VrControllerMovement2 : MonoBehaviour
{
    public float Speed = 0.0f;
    public float Deadzone = 0.0f;
    public float JumpHeight = 1.0f;
    public float MaxAcceleration = 2f;
    public float AccelerationResetTimer = 2.0f;
    public float AccelerationDecay = 0.05f;
    public float GlidingDecay;

    public SteamVR_Input_Sources MovementController;
    public SteamVR_Action_Vector2 TouchPadValue = null;
    public SteamVR_Action_Boolean JumpTrigger;
    public GameObject HandAxis;
    public Transform RespawnPoint;

    public PhysicMaterial NoFrictionMaterial;
    public PhysicMaterial FrictionMaterial;

    public LevelChanger LevelChanger;

    private bool _isWalkingFloor = true;
    private bool _isJumping = false;
    private bool _isGliding = false;

    private float _previousAcceleration = 0.0f;
    private float _currentTimer = 0.0f;
    private float _accelerationFactor = 0.0f;

    private Vector3 _moveDirection;
    private Vector3 _surfDirection;
    private Vector3 _velocity = new Vector3(0, 0, 0);
    private Vector3 _collisionNormal = new Vector3(0, 0, 0);
    private Vector3 _angularVelocity = new Vector3(0, 0, 0);
    private Quaternion _previousRotation;

    private int _floorValue;
    private int _surfingValue;

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
        CalculateAngularVelocity();
        CalculateAcceleration();
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
        /*If the player is on a surfing platform, get the direction the player is facing 
        offset it by the crossproduct of the normal of the surfing platform and the direction the player is facing*/
        Debug.Log("is jumping " + _isJumping);
        Debug.Log("_isWalkingFloor " + _isWalkingFloor);
        Debug.Log("_isGliding " + _isWalkingFloor);
        if (_surfingValue > 0)
        {
            _surfDirection = _head.forward * (Speed * _accelerationFactor);
            Vector3 temp = Vector3.Cross(_collisionNormal, _surfDirection);
            _surfDirection = Vector3.Cross(temp, _collisionNormal);
            _rigidBody.AddForce(_surfDirection.x - _rigidBody.velocity.x, _surfDirection.y - _rigidBody.velocity.y, _surfDirection.z - _rigidBody.velocity.z, ForceMode.VelocityChange);
        }
        else if(!_isWalkingFloor && !_isJumping && _isGliding)
        {
            _moveDirection = _head.forward * (Speed * _accelerationFactor);
            _moveDirection.y -= _moveDirection.y + GlidingDecay;
            _rigidBody.AddForce(_moveDirection.x - _rigidBody.velocity.x, _moveDirection.y - _rigidBody.velocity.y, _moveDirection.z - _rigidBody.velocity.z, ForceMode.VelocityChange);
        }
        else if (TouchPadValue.axis.magnitude > Deadzone)
        {
            _collider.material = NoFrictionMaterial;
            _moveDirection = orientation * (Speed * Vector3.forward);
            _velocity = _moveDirection;
            //disable jumping until a solution for gliding is found
            if(JumpTrigger.GetStateDown(MovementController) && _floorValue > 0)
            {
                float jumpSpeed = Mathf.Sqrt(2 * JumpHeight * 9.81f);
                _rigidBody.AddForce(0, jumpSpeed, 0, ForceMode.VelocityChange);
                _isJumping = true;
            }
            
            _rigidBody.AddForce(_velocity.x - _rigidBody.velocity.x, 0, _velocity.z - _rigidBody.velocity.z, ForceMode.VelocityChange);

        }
        else if(_floorValue > 0)
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
    // Calculates the velocity in which the players head is turning
    private void CalculateAngularVelocity()
    {
        Quaternion deltaRotation = CalculateOrientation() * Quaternion.Inverse(_previousRotation);

        _previousRotation = CalculateOrientation();

        deltaRotation.ToAngleAxis(out var angle, out var axis);

        angle *= Mathf.Deg2Rad;

        _angularVelocity = (1.0f / Time.deltaTime) * angle * axis;
        //Debug.Log(angularVelocity);
    }
    //uses the angular velocity from the player head turning into an acceleration multiplier that is applied while moving
    private void CalculateAcceleration()
    {
        _accelerationFactor = Mathf.Abs(_angularVelocity.y);
        _accelerationFactor = (_accelerationFactor / 3) + 1;
        if (_accelerationFactor > MaxAcceleration)
        {
            _accelerationFactor = MaxAcceleration;
        }
        if (_accelerationFactor > _previousAcceleration)
        {
            _currentTimer = 0.0f;
            _previousAcceleration = _accelerationFactor;
        }
        else 
        {
            _accelerationFactor = _previousAcceleration;
        }
        if (_currentTimer >= AccelerationResetTimer)
        {
            _accelerationFactor = _accelerationFactor * (1 - AccelerationDecay);
            if (_accelerationFactor <= 1.0f)
            {
                _accelerationFactor = 1.0f;
            }
            _previousAcceleration = _accelerationFactor;
        }
        _currentTimer += Time.deltaTime;
    }
    //If the character comes into contact with a deathfloor they will respawn at the levels start point.
    private void RespawnCharacter()
    {
        this.transform.position = RespawnPoint.position;
        this.transform.rotation = RespawnPoint.rotation;
    }

    /*Checks to see if the player is on a surface, if they are enable the ability to jump 
     get the normal of the collsion for surfing*/
    private void OnCollisionEnter(Collision collision)
    {
        _isJumping = false;

        if (collision.transform.tag == "Deathfloor")
        {
            _isWalkingFloor = true;
            RespawnCharacter();
        }
        else if (collision.transform.tag == "Teleporter")
        {
            LevelChanger.FadeToNextLevel();
        }
        else if (collision.transform.tag == "Walkingfloor")
        {
            _isWalkingFloor = true;
        }

        if (collision.transform.tag == "SurfingPlatform")
        {
            _isGliding = false;
        }

        _collisionNormal = collision.contacts[0].normal;
        _floorValue++;
    }

    // As they are jumping disable the ability to jump in midair
    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.tag == "Walkingfloor")
        {
            _isWalkingFloor = false;
        }

        if (collision.transform.tag == "SurfingPlatform")
        {
            _isGliding = true;
        }
        _floorValue--;
    }

    //disable the gravity when surfing
    private void OnTriggerEnter(Collider collider)
    {
        _surfingValue++;
    }

    //re-enable the rigidbody when leaving a SurfingPlatform
    private void OnTriggerExit(Collider collider)
    {
        _surfingValue--;
    }
}
