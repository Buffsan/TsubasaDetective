using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine;

public class CursorController : MonoBehaviour
{

    public Vector3 MousePos;
    public Vector3 MouseScreenPos;

    public Vector2 InputTarget = Vector2.zero;
    [SerializeField] float ControllerTarget_Range = 2;

    InputAction targetAction;
    Vector2 stickInput;

    PlayerController playerController => PlayerController.Instance;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Awake()
    {
        // InputAction を直接作る or 参照してもOK
        targetAction = new InputAction(
            "Target",
            InputActionType.Value,
            "<Gamepad>/rightStick"
        );
    }

    void OnEnable()
    {
        targetAction.Enable();
    }

    void OnDisable()
    {
        targetAction.Disable();
    }
    void Update()
    {
        var gamepads = Gamepad.all; // すべての接続されているGamepadのリスト
        if (gamepads.Count > 0)
        {
            // ★ 毎フレーム読む（超重要）
            stickInput = targetAction.ReadValue<Vector2>();

            // Deadzone 手動調整（任意）
            if (stickInput.magnitude < 0.15f)
                stickInput = Vector2.zero;

            Vector2 offset = stickInput * ControllerTarget_Range;
            
                
            MousePos =playerController.transform.position + (Vector3)offset;
            transform.position = MousePos;
        }
    }
    private void FixedUpdate()
    {
        var gamepads = Gamepad.all; // すべての接続されているGamepadのリスト
        if (gamepads.Count > 0)
        {/*
            //Debug.Log("コントローラーが接続されています");
            Vector2 offset = InputTarget * ControllerTarget_Range;
            offset = Vector2.ClampMagnitude(offset, ControllerTarget_Range);
            MousePos = playerController.transform.position + (Vector3)offset;*/
        }
        else
        {
            //Debug.Log("コントローラーが接続されていません");
            MouseScreenPos = Input.mousePosition;
            MousePos = Camera.main.ScreenToWorldPoint(MouseScreenPos);
            MousePos.z = 0;
            transform.position = MousePos;
        }
        

        
    }
}
