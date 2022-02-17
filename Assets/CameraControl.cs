using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    bool isMoving = false;
    new Camera camera;

    System.IO.StreamWriter outputFile;

    // panning variables
    Vector3 initPointWorld;
    float initZDist;

    void Awake()
    {           
        camera = GetComponent<Camera>();
        outputFile = new System.IO.StreamWriter("logUT.csv", false);
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

        if (isMoving) PanGeometric(Input.mousePosition);
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

            initZDist = (camera.farClipPlane - camera.nearClipPlane) * 0.5f;
            initPointWorld = camera.ScreenToWorldPoint(new Vector3(
                Input.mousePosition.x, 
                Input.mousePosition.y,
                initZDist));
        }
    }


    /// <summary>
    /// Pans the camera given an initial world click location and depth.
    /// </summary>
    private void PanGeometric(Vector3 cursorPosition)
    {

        // The plane is at the distance of the target focal point, but uses the camera's orientation, 
        // meaning it's parallel to the camera's panning (XY) plane.
        var cam = camera;
        var ray = cam.ScreenPointToRay(cursorPosition);
        var plane = new Plane(cam.transform.forward, initPointWorld);

        // Intersecting the cursor ray with the focal plane turns the 2D cursor point into a 3D point
        // that is already constrained to lie in the camera's motion plane.
        // The required offset to move the focal point underneath the cursor ray is therefore
        // just the offset between the two points in the plane.
        plane.Raycast(ray, out float enter);
        var d = initPointWorld - ray.GetPoint(enter);

        // As these points are calculated in world-space, the offset can be directly applied to the camera.
        camera.transform.position += d;

        outputFile.WriteLine($"{Time.realtimeSinceStartup:F5},{cursorPosition.magnitude:F5},{d.magnitude:F5}");

    }



    /// <summary>
    /// Pans the camera given an initial world click location and depth.
    /// 
    /// Calculates a new camera view matrix that is required to keep the 
    /// deprojected current screen point equal to the initial click point. 
    /// This forms the equation Pc = Mv*Pi. Where:
    /// Pc - the current screen point deprojected into camera space at
    ///      the same z distance as the initial world point
    /// Mv - view matrix of the camera
    /// Pi - the inital world point that was clicked
    /// </summary>
    private void PanNdc()
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

