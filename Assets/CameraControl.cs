using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    bool isMoving = false;
    new Camera camera;

    // panning variables
    Vector3 initPointWorld;
    float initZDist;

    void Awake()
    {           
        camera = GetComponent<Camera>();
    }

    void Update()
    {
        // on click down
        if (Input.GetMouseButtonDown(0))
        {
            // storing the initial values required
            isMoving = true;
            SetInitialValuesForPan();
        }

        // on release
        if (Input.GetMouseButtonUp(0))
        {
            isMoving = false;
        }

        if (isMoving) Pan();
    }


    private void SetInitialValuesForPan() 
    {
        if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out RaycastHit raycastHit))
        {
            initPointWorld = raycastHit.point;
            initZDist = camera.transform.InverseTransformPoint(initPointWorld).z;
        }
        else
        {
            initPointWorld = camera.ScreenToWorldPoint(new Vector3(
                Input.mousePosition.x, 
                Input.mousePosition.y, 
                camera.farClipPlane));
            initZDist = camera.farClipPlane;
        }
    }


    /// <summary>
    /// Pans the camera given an initial world click locations and depth.
    /// 
    /// Calculates a new camera view matrix that is required to keep the 
    /// deprojected current screen point equal to the initial click point. 
    /// This forms the equation Pc = Mv*Pi. Where:
    /// Pc - the current screen point deprojected into camera space at
    ///      the same z distance as the initial world point
    /// Mv - view matrix of the camera
    /// Pi - the inital world point that was clicked
    /// </summary>
    private void Pan()
    {
        // Get the current screet point in camera space using the
        // inital click depth  (Pc)
        Vector3 currNdcPoint = CameraStatics.ScreenToNdcPoint(
            new Vector3(
                Input.mousePosition.x,
                Input.mousePosition.y,
                initZDist
            ),
            camera.projectionMatrix,
            camera.pixelHeight,
            camera.pixelWidth);
        Vector4 currCamPoint = camera.projectionMatrix.inverse * 
            new Vector4(
                currNdcPoint.x,
                currNdcPoint.y,
                currNdcPoint.z,
                1
            );
        // homogenous > 3d point
        currCamPoint.x /= currCamPoint.w;
        currCamPoint.y /= currCamPoint.w;
        currCamPoint.z /= currCamPoint.w;
        currCamPoint.w /= currCamPoint.w;

        // Calculate the translation required for the pan using 
        // the initial world click point (Pi)
        float tx = currCamPoint.x -
            camera.worldToCameraMatrix.m00 * initPointWorld.x -
            camera.worldToCameraMatrix.m01 * initPointWorld.y -
            camera.worldToCameraMatrix.m02 * initPointWorld.z;
        float ty = currCamPoint.y -
            camera.worldToCameraMatrix.m10 * initPointWorld.x -
            camera.worldToCameraMatrix.m11 * initPointWorld.y -
            camera.worldToCameraMatrix.m12 * initPointWorld.z;
        float tz = currCamPoint.z -
            camera.worldToCameraMatrix.m20 * initPointWorld.x -
            camera.worldToCameraMatrix.m21 * initPointWorld.y -
            camera.worldToCameraMatrix.m22 * initPointWorld.z;
        Vector4 cameraTranslation = new Vector4(tx, ty, tz, 1);

        // Use the original view matrix (to preserve scale/rotation) and the
        // new translation to calculate a new view matrix (Mv)
        Matrix4x4 newViewMatrix = camera.worldToCameraMatrix;
        newViewMatrix.SetColumn(3, cameraTranslation);

        // Extract new camera position from the new view matrix
        camera.transform.position = newViewMatrix.GetWorldPosition();
    }




}

