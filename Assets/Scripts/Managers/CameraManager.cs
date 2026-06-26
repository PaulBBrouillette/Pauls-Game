using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public enum CameraAngle { TopDown, Corner1, Corner2, Corner3, Corner4 };

public class CameraManager : MonoBehaviour {
    public static CameraManager Instance;
    private Camera cam;
    public float tileSize = 1.0f;
    public float topDownTargetX; // Target zoomed out X
    public float topDownTargetZ; // Target zoomed out Z
    public float orthoSize;
    private float mapWidth;
    private float mapHeight;

    [Header("Zoom Settings")]
    public float zoomSpeed = .5f;
    public float minZoom = 3f;
    public float maxZoom = 20f;
    public float zoomDuration = .5f;
    public float quickZoomDuration = .1f;

    void Awake() {
        Instance = this;
        cam = GetComponent<Camera>();
    }

    public void SetupCamera(int mapWidth, int mapHeight) {
        float centerX = (mapWidth - 1) * tileSize / 2f;
        float centerZ = (mapHeight - 1) * tileSize / 2f;
        topDownTargetX = centerX;
        topDownTargetZ = centerZ;
        this.mapHeight = mapHeight;
        this.mapWidth = mapWidth;
        transform.position = new Vector3(centerX, 45, centerZ);
        float maxDimension = Mathf.Max(mapWidth, mapHeight);
        Debug.Log("Max Dimension: " + maxDimension);
        if (cam == null) {
            cam = GetComponent<Camera>();
        }
        orthoSize = (maxDimension * tileSize) / 2f + 1f;
        maxZoom = orthoSize;
        cam.orthographicSize = orthoSize;
    }

    void Update() {
        Vector2 scrollInput = InputManager.Controls.Player.Zoom.ReadValue<Vector2>();
        float scroll = scrollInput.y;
        if (scroll != 0) {
            ApplyZoom(scroll);
        }
    }

    private void ApplyZoom(float scroll) {
        // Zoom in
        Vector3 mousePosBefore = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        float currentX = cam.transform.position.x;
        float currentZ = cam.transform.position.z;
        float xMove = 0f;
        float zMove = 0f;
        float scrollsUntilAtTarget = 0f;
        if (scroll > 0) {
            if (cam.orthographicSize > minZoom) {
                scrollsUntilAtTarget = (cam.orthographicSize - minZoom) / zoomSpeed + 1;
                xMove = mousePosBefore.x;
                zMove = mousePosBefore.z;

                if (xMove < 0f) {
                    xMove = 0f;
                }
                else if (xMove > mapWidth) {
                    xMove = mapWidth;
                }

                if (zMove > mapHeight) {
                    zMove = mapHeight;
                }
                else if (zMove < 0f) {
                    zMove = 0f;
                }

                // For an incremental zoom with mouse position, comment out coroutine and use
                // cam.transform.position = new Vector3(xMove, 45, zMove);
                // cam.orthographicSize -= zoomSpeed;

                StartCoroutine(ZoomInRoutine(minZoom, new Vector3(xMove, 45, zMove)));
                
            }
        }
        // Zoom out
        else {
            if (cam.orthographicSize < maxZoom) {
                // Based on where X and Z are when you zoom out, plus how far the player is from fully zooming out, shift the 
                // X and Z to slowly move back to the original top position
                scrollsUntilAtTarget = Mathf.Abs(orthoSize - cam.orthographicSize) / zoomSpeed;
                if (currentX != topDownTargetX) {
                    xMove = Mathf.Abs(currentX - topDownTargetX) / scrollsUntilAtTarget;
                    xMove = (currentX >= topDownTargetX) ? xMove *= -1 : xMove;
                }

                if (currentZ != topDownTargetZ) {
                    zMove = Mathf.Abs(currentZ - topDownTargetZ) / scrollsUntilAtTarget;
                    zMove = (currentZ >= topDownTargetZ) ? zMove *= -1 : zMove;
                }
                cam.transform.position += new Vector3(xMove, 0, zMove);
                cam.orthographicSize += zoomSpeed;
                //StartCoroutine(ZoomOutRoutine(cam.orthographicSize + zoomSpeed, new Vector3(xMove, 45, zMove)));
            }
        }
        mousePosBefore = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

    private IEnumerator ZoomInRoutine(float targetSize, Vector3 xzTarget) {
        float startSize = cam.orthographicSize;
        float elapsedTime = 0f;
        Vector3 startPosition = cam.transform.position;

        while (elapsedTime < zoomDuration) {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / zoomDuration);

            // Apply a smooth step if you want to ease in and out
            t = Mathf.SmoothStep(0, 1, t);
            transform.position = Vector3.Lerp(startPosition, xzTarget, t);
            cam.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            //cam.transform.position = Mathf.Lerp
            yield return null;
        }
        cam.transform.position = xzTarget;
        cam.orthographicSize = targetSize; // Ensure it lands exactly on the target
    }

    private IEnumerator ZoomOutRoutine(float targetSize, Vector3 xzTarget) {
        float startSize = cam.orthographicSize;
        float elapsedTime = 0f;
        Vector3 startPosition = cam.transform.position;

        while (elapsedTime < quickZoomDuration) {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / quickZoomDuration);

            // Apply a smooth step if you want to ease in and out
            t = Mathf.SmoothStep(0, 1, t);
            transform.position = Vector3.Lerp(startPosition, xzTarget, t);
            cam.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            //cam.transform.position = Mathf.Lerp
            yield return null;
        }
        cam.transform.position = xzTarget;
        cam.orthographicSize = targetSize; // Ensure it lands exactly on the target
    }

    public void SetAngle(CameraAngle angle) {
        Debug.Log("Switching camera angle to " + angle);
        switch (angle) {
            case CameraAngle.TopDown:
                transform.rotation = Quaternion.Euler(90, 0, 0);
                cam.transform.localPosition = new Vector3(topDownTargetX, 10, topDownTargetZ);
                break;
            case CameraAngle.Corner1:
                transform.rotation = Quaternion.Euler(45, 45, 0);
                cam.transform.localPosition = new Vector3(0, orthoSize, 0);
                break;
        }
    }
}
