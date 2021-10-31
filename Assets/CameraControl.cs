using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    bool isMoving = false;
    new Camera camera;

    float meshToCamPlaneDist;
    Camera initCamComp;

    Vector3 initialCamPlanePoint;
    Vector3 initialCamPos;

    [Range(7,25)]
    public float distFromCamPlane;

    void Awake()
    {
        CreatePanHelperGameObject();
           
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

    private void CreatePanHelperGameObject() 
    {
        // Setting up the dummy game object to hold the camera configuration
        // for the pannig logic. It is refreshed on each click. Should be in
        // the scene root
        GameObject panHelper = new GameObject();
        panHelper.name = "Camera pan helper";
        initCamComp = panHelper.AddComponent<Camera>();
        initCamComp.enabled = false;
    }

    private void SetInitialValuesForPan() 
    {
        if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out RaycastHit raycastHit))
        {
            meshToCamPlaneDist = camera.transform.InverseTransformPoint(raycastHit.point).z;
        }
        else
        {
            meshToCamPlaneDist = camera.nearClipPlane;
        }
        initCamComp.CopyFrom(camera);
        initialCamPlanePoint = GetCameraPlanePointForMousePos();
        initialCamPos = camera.transform.position;
    }

    private void Pan() 
    {
        //Vector3 newCamPlanePoint = GetCameraPlanePointForMousePos();
        //Vector3 camDelta = newCamPlanePoint - initialCamPlanePoint;
        //camera.transform.position = initialCamPos - camDelta;
    }

    /// <summary>
    /// Gets a world space point on the camera x/y plane for the current mouse position
    /// </summary>
    private Vector3 GetCameraPlanePointForMousePos() 
    {
        // 0. Get the screen point, with a z from the initial mesh distance
        Vector3 screenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, meshToCamPlaneDist);
        // 1. Convert The screen point to a world point with with the saved initial camera
        Vector3 worldPoint = initCamComp.ScreenToWorldPoint(screenPoint);
        // 2. Convert the resulting point in camera local space
        Vector3 intermediatePoint = initCamComp.transform.InverseTransformPoint(worldPoint);
        // 3. Edit position to get a point on the camera x/y plane 
        Vector3 newCamPointLocal = new Vector3(
            intermediatePoint.x,
            intermediatePoint.y,
            0);
        // 4. Convert back to world position
        return initCamComp.transform.TransformPoint(newCamPointLocal);
    }


}

