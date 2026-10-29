using UnityEngine;

public class SittingCamera : MonoBehaviour
{
    [Header("Camera Turning")]
    [SerializeField] float lookSensitivityX = 1f;
    [SerializeField] float lookSensitivityY = 1f;
    [SerializeField] float minXLookAngle = -45f;
    [SerializeField] float maxXLookAngle = 45f;
    [SerializeField] float minYLookAngle = -45f;
    [SerializeField] float maxYLookAngle = 45f;
    private float verticalRotation;
    private float horizontalRotation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLook();
    }
    
    void UpdateLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * lookSensitivityX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * lookSensitivityY;

        verticalRotation -= mouseY;
        horizontalRotation += mouseX;
        
        verticalRotation = Mathf.Clamp(verticalRotation, minYLookAngle, maxYLookAngle);
        horizontalRotation = Mathf.Clamp(horizontalRotation, minYLookAngle, maxYLookAngle);

        transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
    }
}
