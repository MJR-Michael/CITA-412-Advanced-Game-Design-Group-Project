using UnityEngine;

public class ModelViewer : MonoBehaviour
{
    public Transform model;
    public Camera modelCamera; // ← Assign in Inspector
    public float rotationSpeed = 100f;
    public float zoomSpeed = 5f;
    public float minZoom = 2f;
    public float maxZoom = 10f;

    void Update()
    {
        HandleRotation();
        HandleZoom();
    }

    void HandleRotation()
    {
        if (model == null) return;

        if (Input.GetMouseButton(0))
        {
            float rotX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float rotY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            model.Rotate(Vector3.up, -rotX, Space.World);
            model.Rotate(Vector3.right, rotY, Space.World);
        }
    }

    void HandleZoom()
    {
        if (modelCamera == null) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            modelCamera.transform.position += modelCamera.transform.forward * scroll * zoomSpeed;
            float distance = Vector3.Distance(modelCamera.transform.position, model.position);
            distance = Mathf.Clamp(distance, minZoom, maxZoom);
            modelCamera.transform.position = model.position - modelCamera.transform.forward * distance;
        }
    }
}
