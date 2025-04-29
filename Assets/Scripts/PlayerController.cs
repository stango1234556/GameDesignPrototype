using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;

    [SerializeField] private float _speed;
    [SerializeField] private float _jumpSpeed;
    [SerializeField] private float _turnSpeed;

    private int _jumpCount;
    [SerializeField] private int _maxJumpCount = 2;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundCheckDistance = 0.1f;

    private Rigidbody _rigidbody;
    private Vector3 _input;
    private bool _jumpTriggered;
    private PlayerInputController _playerInputController;

    private void Awake(){
        _playerInputController = GetComponent<PlayerInputController>();
        _rigidbody = GetComponent<Rigidbody>();

        _playerInputController.OnJumpButtonPressed += JumpButtonPressed;
    }

    void Update(){
        // GatherInput();
        // Look();
    }

    void FixedUpdate(){
        GatherInput();
        Move();
        Look();
        

        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, _groundCheckDistance, _groundLayer);

        if (isGrounded){
            _jumpCount = 0;
        }

        // Vector3 velocity = new Vector3(_playerInputController.MovementInputVector.x, 0, _playerInputController.MovementInputVector.y) * _speed;
        // velocity.y = _rigidbody.velocity.y;

        // if(_jumpTriggered){
        //     velocity.y = _jumpSpeed;
        //     _jumpTriggered = false;
        // }

        // _rigidbody.velocity = velocity; 
    }

    void GatherInput(){
        _input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }

    void Look(){

        if(_input != Vector3.zero){

            var relative = (transform.position + _input.ToIso()) - transform.position;
            var rot = Quaternion.LookRotation(relative, Vector3.up);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, _turnSpeed * Time.deltaTime);
        }
    }

    void Move(){
        _rb.MovePosition(transform.position + (transform.forward * _input.magnitude) * _speed * Time.deltaTime);
    }

    private void JumpButtonPressed(){
        if(_jumpCount < _maxJumpCount){
            _jumpTriggered = true;
            _jumpCount++;
        }
    }
}
