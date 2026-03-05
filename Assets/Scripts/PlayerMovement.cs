using UnityEngine;
using Zenject;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private PlayerInput input;
    private CharacterController controller;
    private PlayerConfig config;
    private Vector2 moveInput;

    [Inject]
    public void Construct(PlayerConfig playerConfig)
    {
        config = playerConfig;
    }
    
    private void Awake()
    {
        input = new PlayerInput();
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        input.Player.Enable();
    }

    private void OnDisable()
    {
        input.Player.Disable();
    }

    private void Update()
    {
        moveInput = input.Player.Move.ReadValue<Vector2>();
        
        PlayerMove();
    }

    private void PlayerMove()
    {
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        controller.Move(move * (config.MoveSpeed * Time.deltaTime));

        if (!(move.sqrMagnitude > 0.01f)) return;
        
        Quaternion lookRotation = Quaternion.LookRotation(move);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * config.MoveSpeed);
    }
}