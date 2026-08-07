using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public enum CameraAngle { TopDown, Corner1, Corner2, Corner3, Corner4 };

public class CameraManager : MonoBehaviour {
    public static CameraManager Instance;
    private Camera cam;
    public float tileSize = 1.0f;
    public float targetX; // Target zoomed out X
    public float targetY; // Target zoomed out Y
    public float orthoSize;
    private float mapWidth;
    private float mapHeight;
    private Vector3 defaultZoomedOutPos;

    [Header("Zoom Settings")]
    public float zoomSpeed = .5f;
    public float minZoom = 1f;
    public float maxZoom = 20f;
    public float zoomDuration = 1.25f;

    // Tracks the currently running zoom-in coroutine so we can cancel it
    // instead of letting a new one run alongside it.
    private Coroutine zoomCoroutine;

    [Header("Drag Settings")]
    public bool invertDrag = false; 
    private bool isDragging = false;
    private Vector3 dragOriginWorld;

    void Awake() {
        Instance = this;
        cam = GetComponent<Camera>();
    }

    public void SetupCamera(int mapWidth, int mapHeight) {
        float centerX = (mapWidth - 1) * tileSize / 2f;
        float centerY = (mapHeight - 1) * tileSize / 2f;

        defaultZoomedOutPos = new Vector3(centerX, centerY, -10);
        transform.position = defaultZoomedOutPos;
        targetX = centerX;
        targetY = centerY;
        this.mapHeight = mapHeight;
        this.mapWidth = mapWidth;
        float maxDimension = Mathf.Max(mapWidth, mapHeight);
        Debug.Log("Max Dimension: " + maxDimension);
        if (cam == null) {
            cam = GetComponent<Camera>();
        }
        orthoSize = (maxDimension * tileSize) / 2f + 1f;
        maxZoom = orthoSize;
        minZoom = .75f;
        cam.orthographicSize = orthoSize;
    }

    void Update() {
        Vector2 scrollInput = InputManager.Controls.Player.Zoom.ReadValue<Vector2>();
        float scroll = scrollInput.y;
        if (scroll != 0) {
            ApplyZoom(scroll);
        }
        HandleDrag();
    }

    private void ApplyZoom(float scroll) {
        // Zoom in
        Vector3 mousePosBefore = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        float currentX = cam.transform.position.x;
        float currentY = cam.transform.position.y;
        float xMove = 0f;
        float yMove = 0f;
        float scrollsUntilAtTarget = 0f;
        if (scroll > 0) {
            if (cam.orthographicSize > minZoom) {
                scrollsUntilAtTarget = (cam.orthographicSize - minZoom) / zoomSpeed + 1;
                xMove = mousePosBefore.x;
                yMove = mousePosBefore.y;

                if (xMove < 0f) {
                    xMove = 0f;
                }
                else if (xMove > mapWidth) {
                    xMove = mapWidth;
                }

                if (yMove > mapHeight) {
                    yMove = mapHeight;
                }
                else if (yMove < 0f) {
                    yMove = 0f;
                }

                if (zoomCoroutine != null) {
                    StopCoroutine(zoomCoroutine);
                }
                zoomCoroutine = StartCoroutine(ZoomInRoutine(minZoom, new Vector3(xMove, yMove, -10)));
            }
        }
        // Zoom out
        else {
            if (cam.orthographicSize < maxZoom) {
                if (zoomCoroutine != null) {
                    StopCoroutine(zoomCoroutine);
                    zoomCoroutine = null;
                }

                // Based on where X and Y are when you zoom out, plus how far the player is from fully zooming out, shift the 
                // X and Y to slowly move back to the original top position
                scrollsUntilAtTarget = Mathf.Abs(orthoSize - cam.orthographicSize) / zoomSpeed;
                if (currentX != targetX) {
                    xMove = Mathf.Abs(currentX - targetX) / scrollsUntilAtTarget;
                    xMove = (currentX >= targetX) ? xMove *= -1 : xMove;
                }

                if (currentY != targetY) {
                    yMove = Mathf.Abs(currentY - targetY) / scrollsUntilAtTarget;
                    yMove = (currentY >= targetY) ? yMove *= -1 : yMove;
                }
                cam.transform.position += new Vector3(xMove, yMove, 0);
                cam.orthographicSize += zoomSpeed;
            }
        }
        mousePosBefore = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

    private IEnumerator ZoomInRoutine(float targetSize, Vector3 xyTarget) {
        float startSize = cam.orthographicSize;
        float elapsedTime = 0f;
        Vector3 startPosition = cam.transform.position;

        while (elapsedTime < zoomDuration) {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / zoomDuration);

            t = Mathf.SmoothStep(0, 1, t);
            transform.position = Vector3.Lerp(startPosition, xyTarget, t);
            cam.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            yield return null;
        }
        cam.transform.position = xyTarget;
        cam.orthographicSize = targetSize;
        zoomCoroutine = null;
    }

    private void HandleDrag() {
        if (Mouse.current == null) return;

        if (InputManager.Controls.Player.DragCamera.inProgress) {
            if (!isDragging) {
                if (zoomCoroutine != null) {
                    StopCoroutine(zoomCoroutine);
                    zoomCoroutine = null;
                }
                dragOriginWorld = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            }
            isDragging = true;
        }
        else {
            isDragging = false;
        }

        Collider2D hit = Physics2D.OverlapPoint(dragOriginWorld);
        if (hit != null) {
            if (hit.tag != null) {
                if (hit.CompareTag("Piece")) {
                    isDragging = false;
                }
            }
        }

        if (!isDragging) return;

        Vector3 currentMouseWorld = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector3 delta = dragOriginWorld - currentMouseWorld;
        if (invertDrag) delta = -delta;

        Vector3 newPos = cam.transform.position + delta;
        newPos.z = cam.transform.position.z;
        //newPos = ClampCameraPosition(newPos);
        cam.transform.position = newPos;
        dragOriginWorld = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

    public void ResetToDefaultView() {
        transform.position = defaultZoomedOutPos;
        cam.orthographicSize = orthoSize;
    }

    private Vector3 ClampCameraPosition(Vector3 pos) {
        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * cam.aspect;

        // Map world bounds, matching the centering math used in SetupCamera:
        // tiles run from 0 to (mapWidth-1)/(mapHeight-1), padded by half a tile
        // on each side.
        float mapMinX = -tileSize / 2f;
        float mapMaxX = (mapWidth - 1) * tileSize + tileSize / 2f;
        float mapMinY = -tileSize / 2f;
        float mapMaxY = (mapHeight - 1) * tileSize + tileSize / 2f;

        float minX = mapMinX + horzExtent;
        float maxX = mapMaxX - horzExtent;
        float minY = mapMinY + vertExtent;
        float maxY = mapMaxY - vertExtent;

        float clampedX = (minX <= maxX) ? Mathf.Clamp(pos.x, minX, maxX) : targetX;
        float clampedY = (minY <= maxY) ? Mathf.Clamp(pos.y, minY, maxY) : targetY;

        return new Vector3(clampedX, clampedY, pos.z);
    }
}