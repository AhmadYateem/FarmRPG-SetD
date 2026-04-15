using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple WASD / arrow key movement for the player character.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;

    private SpriteRenderer _sr;

    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current[Key.W].isPressed || Keyboard.current[Key.UpArrow].isPressed)
            input.y += 1f;
        if (Keyboard.current[Key.S].isPressed || Keyboard.current[Key.DownArrow].isPressed)
            input.y -= 1f;
        if (Keyboard.current[Key.A].isPressed || Keyboard.current[Key.LeftArrow].isPressed)
            input.x -= 1f;
        if (Keyboard.current[Key.D].isPressed || Keyboard.current[Key.RightArrow].isPressed)
            input.x += 1f;

        input = input.normalized;
        transform.position += (Vector3)input * _moveSpeed * Time.deltaTime;

        // flip sprite based on horizontal input
        if (input.x != 0 && _sr != null)
        {
            _sr.flipX = input.x < 0;
        }
    }
}
