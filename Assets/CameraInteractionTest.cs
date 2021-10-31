using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraInteractionTest : MonoBehaviour
{
    // config
    [Range(7, 25)]
    public float distFromCamPlane;

    // unity refs
    public RectTransform unityScreenImgRect;
    public RectTransform ourScreenImgRect;
    private new Camera camera;

    // point visualisation
    Vector3 unityWorldPoint;
    Vector3 ourWorldPoint;

    Vector3 w2sViewportPoint;
    Vector3 w2sNdcPoint;
    Vector3 w2sCameraPoint;

    Vector3 s2wViewportPoint;
    Vector3 s2wNdcPoint;
    Vector3 s2wCameraPoint;

    Vector3 ourScreenPoint;
    Vector3 unityScreenPoint;

    void Awake()
    {
        camera = GetComponent<Camera>();
    }

    void Update()
    {
        // setup unity positions
        unityScreenImgRect.position = unityScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distFromCamPlane);
        unityWorldPoint = camera.ScreenToWorldPoint(unityScreenPoint);
        
        // run our conversions
        WorldToScreenTest();
        ScreenToWorldTest();
    }

    void WorldToScreenTest() 
    {
        ourScreenImgRect.position = ourScreenPoint = CameraStatics.WorldToScreenPoint(
            unityWorldPoint,
            camera.projectionMatrix,
            camera.worldToCameraMatrix,
            camera.pixelHeight,
            camera.pixelWidth,
            out Vector3 viewportPoint,
            out Vector3 ndcPoint,
            out Vector3 oglCameraPoint);
        w2sViewportPoint = viewportPoint;
        w2sNdcPoint = ndcPoint;
        w2sCameraPoint = oglCameraPoint;
    }

    void ScreenToWorldTest() 
    {
        ourWorldPoint = CameraStatics.ScreenToWorldPoint(
            unityScreenPoint,
            camera.projectionMatrix,
            camera.worldToCameraMatrix,
            camera.pixelHeight,
            camera.pixelWidth,
            out Vector3 viewportPoint,
            out Vector3 ndcPoint,
            out Vector3 cameraPoint);
        s2wViewportPoint = viewportPoint;
        s2wNdcPoint = ndcPoint;
        s2wCameraPoint = cameraPoint;
    }

    private void OnDrawGizmos()
    {
        if (camera == null) return;

        //    5+----+4    ^ y
        //    /    /|     |  7 z
        //  3+----+6|     | /
        //   | 2  | +7    |/
        //   |    |/      +--------> x
        //  0+----+ 1
        // Draw the canonical volume
        float scale = 1;
        List<Vector3> points = new List<Vector3> {
            new Vector3(-scale,-scale,-scale),//0
            new Vector3( scale,-scale,-scale),//1
            new Vector3(-scale,-scale, scale),//2
            new Vector3(-scale, scale,-scale),//3
            new Vector3( scale, scale, scale),//4
            new Vector3(-scale, scale, scale),//5
            new Vector3( scale, scale,-scale),//6
            new Vector3( scale,-scale, scale),//7
        };
        Gizmos.color = Color.red;
        // top 
        Gizmos.DrawLine(points[3], points[6]);
        Gizmos.DrawLine(points[6], points[4]);
        Gizmos.DrawLine(points[4], points[5]);
        Gizmos.DrawLine(points[5], points[3]);
        // bottom
        Gizmos.DrawLine(points[0], points[1]);
        Gizmos.DrawLine(points[1], points[7]);
        Gizmos.DrawLine(points[7], points[2]);
        Gizmos.DrawLine(points[2], points[0]);
        // columns
        Gizmos.DrawLine(points[3], points[0]);
        Gizmos.DrawLine(points[6], points[1]);
        Gizmos.DrawLine(points[4], points[7]);
        Gizmos.DrawLine(points[5], points[2]);

        float sbigSize = 0.4f;
        float ssmallSize = sbigSize * 0.8f;

        float bbigSize = 0.8f;
        float bsmallSize = bbigSize * 0.8f;

        // Input from unity
        Gizmos.color = Color.red; Gizmos.DrawCube(unityWorldPoint, new Vector3(bsmallSize, bsmallSize, bsmallSize));

        // Output from our functions
        Gizmos.color = Color.blue; Gizmos.DrawWireCube(ourWorldPoint, new Vector3(bbigSize, bbigSize, bbigSize));            

        // world to screen debug - works 
        Gizmos.color = Color.green; Gizmos.DrawSphere(camera.cameraToWorldMatrix.MultiplyPoint(w2sCameraPoint), ssmallSize);
        Gizmos.color = Color.black; Gizmos.DrawSphere(w2sNdcPoint, ssmallSize); 
        Gizmos.color = Color.white; Gizmos.DrawSphere(w2sViewportPoint, ssmallSize);

        // screen to world debug
        Gizmos.color = Color.green; Gizmos.DrawWireSphere(camera.cameraToWorldMatrix.MultiplyPoint(s2wCameraPoint), sbigSize);
        Gizmos.color = Color.black; Gizmos.DrawWireSphere(s2wNdcPoint, sbigSize);
        Gizmos.color = Color.white; Gizmos.DrawWireSphere(s2wViewportPoint, sbigSize);

        // Line through point in the world
        Gizmos.color = Color.white; Gizmos.DrawLine(camera.transform.position, unityWorldPoint);

    }

}

