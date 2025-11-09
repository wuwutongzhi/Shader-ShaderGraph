using UnityEngine;
using UnityEngine.InputSystem;

namespace BubbleLiquid
{
    public class CameraController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float movementSpeed = 5f;
        public float mouseSensitivity = 4f;
        public float smoothTime = 0.5f;

        [Header("Input Actions")]
        public PlayerInput playerInput;
        public InputActionReference moveAction;
        public InputActionReference elevateAction;
        public InputActionReference lookAction;
        public InputActionReference rotateCameraAction;
        public InputActionReference scrollAction;

        private float yaw;
        private float pitch;
        private Vector3 currentSpeed;
        private Vector3 refVelocity;
        private Vector2 lookInput;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            yaw = transform.eulerAngles.y;
            pitch = transform.eulerAngles.x;
            currentSpeed = Vector3.zero;
            refVelocity = Vector3.zero;

            // 启用所有输入动作
            moveAction.action.Enable();
            elevateAction.action.Enable();
            lookAction.action.Enable();
            rotateCameraAction.action.Enable();
            scrollAction.action.Enable();
        }

        private void OnEnable()
        {
            // 订阅 look 输入事件
            lookAction.action.performed += OnLookPerformed;
            lookAction.action.canceled += OnLookCanceled;
        }

        private void OnDisable()
        {
            // 取消订阅
            lookAction.action.performed -= OnLookPerformed;
            lookAction.action.canceled -= OnLookCanceled;
        }

        private void OnLookPerformed(InputAction.CallbackContext context)
        {
            lookInput = context.ReadValue<Vector2>();
        }

        private void OnLookCanceled(InputAction.CallbackContext context)
        {
            lookInput = Vector2.zero;
        }

        private void Update()
        {
            HandleScrollInput();
            HandleMovement();
            HandleRotation();
            ApplyMovement();
        }

        private void HandleScrollInput()
        {
            float scroll = scrollAction.action.ReadValue<float>();
            movementSpeed = Mathf.Clamp(movementSpeed + scroll * 2, 0.1f, 15);
        }

        private void HandleMovement()
        {
            // 读取移动输入
            Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
            float elevateInput = elevateAction.action.ReadValue<float>();

            Vector3 x = moveInput.y * transform.forward;  // 前后移动 (W/S)
            Vector3 z = moveInput.x * transform.right;    // 左右移动 (A/D)
            Vector3 y = elevateInput * transform.up;      // 升降移动 (Q/E)

            Vector3 targetSpeed = (x + y + z) * movementSpeed;
            currentSpeed = Vector3.SmoothDamp(currentSpeed, targetSpeed, ref refVelocity, smoothTime);
        }

        private void HandleRotation()
        {
            // 检查是否按下右键进行旋转
            bool isRotating = rotateCameraAction.action.ReadValue<float>() > 0.5f;

            if (isRotating && lookInput != Vector2.zero)
            {
                yaw += mouseSensitivity * lookInput.x * 0.1f;   // 调整灵敏度
                pitch -= mouseSensitivity * lookInput.y * 0.1f; // 调整灵敏度
                
                // 限制俯仰角
                pitch = Mathf.Clamp(pitch, -90f, 90f);
                
                transform.eulerAngles = new Vector3(pitch, yaw, 0);
            }
        }

        private void ApplyMovement()
        {
            transform.position += currentSpeed * Time.deltaTime;
        }
    }
}