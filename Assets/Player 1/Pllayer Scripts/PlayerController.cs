using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed;
    private Vector2 move, MouseLook, JoystickLook;
    private Vector3 rotationTarget;
    public bool IsPc;

    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    public void OnMouseLook(InputAction.CallbackContext context)
    {
        MouseLook = context.ReadValue<Vector2>();
    }

    public void OnJoystickLookLook(InputAction.CallbackContext context)
    {
        JoystickLook = context.ReadValue<Vector2>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsPc)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(MouseLook);

            if(Physics.Raycast(ray, out hit))
            {
                rotationTarget = hit.point;
            }
            movePlayerWithAim();
        }
        else
        {
         if(JoystickLook.x == 0 && JoystickLook.y == 0)
            {
                movePlayer();
            }
            else
            {
                movePlayerWithAim();
            }
        }
    }

    public void movePlayer()
    {
        Vector3 movement = new Vector3(move.x, 0f, move.y).normalized;
        var matrix = Matrix4x4.Rotate(Quaternion.Euler(0f, 45f, 0f));

        if (movement != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(movement);
        }

        movement = matrix.MultiplyPoint3x4(movement);
        transform.Translate(movement * speed * Time.deltaTime, Space.World);
    }

    public void movePlayerWithAim()
    {
        if (IsPc)
        {
            var lookPos = rotationTarget - transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            Vector3 aimDirection = new Vector3(rotationTarget.x, 0f, rotationTarget.z);
            if(aimDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.15f);
            }
        }
        else
        {
            Vector3 aimDirection = new Vector3(JoystickLook.x, 0f, JoystickLook.y);
            if(aimDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(aimDirection), 0.15f);
            }
        }
        Vector3 movement = new Vector3(move.x, 0f, move.y);
        var matrix = Matrix4x4.Rotate(Quaternion.Euler(0f, 45f, 0f));
        movement = matrix.MultiplyPoint3x4(movement);
        transform.Translate(movement * speed * Time.deltaTime, Space.World);
    }

}
