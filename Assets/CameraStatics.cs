using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CameraStatics
{

    #region Coordinate trasformations

    /// <summary>
    /// Converts a screen point to an NDC space point.
    /// </summary>
    /// <returns>The deprojected screen point NDC space point (-1 to 1) in unity coordinate system</returns>
    public static Vector3 ScreenToNdcPoint(
        Vector3 screenPoint,
        Matrix4x4 projectionMatrix,
        float screenPxlHeight,
        float screenPxlWidth)
    {
        ScreenToWorldPoint(
            screenPoint,
            projectionMatrix,
            null,
            screenPxlHeight,
            screenPxlWidth,
            out Vector3 viewportPoint,
            out Vector3 ndcPoint,
            out Vector3 cameraPoint);
        return ndcPoint;
    }

    /// <summary>
    /// Static implementation of Unity's Camera.ScreenToWorldPoint.
    /// </summary>
    public static Vector3 ScreenToWorldPoint(
        Vector3 screenPoint,
        Matrix4x4 projectionMatrix,
        Matrix4x4 worldToCameraMatrix,
        float screenPxlHeight,
        float screenPxlWidth)
    {
        return ScreenToWorldPoint(
            screenPoint,
            projectionMatrix,
            worldToCameraMatrix,
            screenPxlHeight,
            screenPxlWidth,
            out Vector3 viewportPoint,
            out Vector3 ndcPoint,
            out Vector3 cameraPoint);
    }

    /// <summary>
    /// Static implementation of Unity's Camera.ScreenToWorldPoint. Provides intermediate points.
    /// </summary>
    /// <param name="screenPoint"> The z coordinate should be z coordinate of the resulting point in the camera system</param>
    /// <param name="projectionMatrix">Camera projection matrix</param>
    /// <param name="worldToCameraMatrix">Camera view matrix, set to null if you only need coordinates in or before view space</param>
    /// <param name="viewportPoint">Unity style viewport point (0 - 1)</param>
    /// <param name="ndcPoint">NDC space point (-1 to 1) in unity coordinate system</param>
    /// <param name="oglCameraPoint">Camera point in OpenGL coordinate system (reversed z)</param>
    /// <returns>World point</returns>
    public static Vector3 ScreenToWorldPoint(
        Vector3 screenPoint,
        Matrix4x4 projectionMatrix,
        Matrix4x4? worldToCameraMatrix,
        float screenPxlHeight,
        float screenPxlWidth,
        out Vector3 viewportPoint,
        out Vector3 ndcPoint,
        out Vector3 oglCameraPoint)
    {
        // viewport point (i.e. values between 0 - 1 in x/y, unity style)
        viewportPoint = new Vector3(
            screenPoint.x / screenPxlWidth,
            screenPoint.y / screenPxlHeight
        );

        // figure out the NDC z coordinate by projecting the camera z into NDC space
        Vector4 zClip = projectionMatrix * new Vector4(0, 0, -screenPoint.z, 1); // negative z for opengl
        float zNdc = zClip.z / zClip.w; // perspective divide after homogenouse operation

        // Normalised device coordinate point (i.e. values between -1 and 1 in all axes, OpenGl style)
        ndcPoint = new Vector3(
            (viewportPoint.x * 2f) - 1f,
            (viewportPoint.y * 2f) - 1f,
            zNdc
        );

        // homogenous clip space point
        Vector4 clipPoint = new Vector4(
            ndcPoint.x,
            ndcPoint.y,
            ndcPoint.z,
            1
        );

        // Undo View transform & Project matrix
        Vector4 deprojectedPoint = projectionMatrix.inverse * clipPoint;

        //perspective divide
        oglCameraPoint.x = deprojectedPoint.x / deprojectedPoint.w;
        oglCameraPoint.y = deprojectedPoint.y / deprojectedPoint.w;
        oglCameraPoint.z = deprojectedPoint.z / deprojectedPoint.w;

        if (worldToCameraMatrix.HasValue)
        {
            Vector3 worldPoint = worldToCameraMatrix.Value.inverse.MultiplyPoint(oglCameraPoint);
            return worldPoint;
        }
        else
        {
            return Vector3.zero;
        }
    }

    /// <summary>
    /// Static implementation of Unity's Camera.WorldToScreenPoint.
    /// </summary>
    public static Vector3 WorldToScreenPoint(
        Vector3 worldPoint,
        Matrix4x4 projectionMatrix,
        Matrix4x4 worldToCameraMatrix,
        float screenPxlHeight,
        float screenPxlWidth)
    {
        return WorldToScreenPoint(
            worldPoint,
            projectionMatrix,
            worldToCameraMatrix,
            screenPxlHeight,
            screenPxlWidth,
            out Vector3 viewportPoint,
            out Vector3 ndcPoint,
            out Vector3 cameraPoint);
    }

    /// <summary>
    /// Static implementation of Unity's Camera.WorldToScreenPoint. Provides intermediate points.
    /// </summary>
    /// <param name="worldPoint">Input world point</param>
    /// <param name="projectionMatrix">Camera projection matrix</param>
    /// <param name="worldToCameraMatrix">Camera view matrix</param>
    /// <param name="viewportPoint">Unity style viewport point (0 - 1)</param>
    /// <param name="ndcPoint">NDC space point (-1 to 1) in unity coordinate system</param>
    /// <param name="oglCameraPoint">Camera point in OpenGL coordinate system (reversed z)</param>
    /// <returns>Screen point in unity screen space ([0,0] to [Screen.width, Screen.height])</returns>
    public static Vector3 WorldToScreenPoint(
        Vector3 worldPoint,
        Matrix4x4 projectionMatrix,
        Matrix4x4 worldToCameraMatrix,
        float screenPxlHeight,
        float screenPxlWidth,
        out Vector3 viewportPoint,
        out Vector3 ndcPoint,
        out Vector3 oglCameraPoint)
    {
        // Be careful here to treat the world point as a point (i.e. dont just use * operator blindly)
        oglCameraPoint = worldToCameraMatrix.MultiplyPoint(worldPoint);

        // Same here, need to explicitly set the 4th value to 1 to get the correct depth in the canonical volume
        // homogenous clip space point
        Vector4 clipPoint = projectionMatrix * new Vector4(oglCameraPoint.x, oglCameraPoint.y, oglCameraPoint.z, 1);

        // Consideration for points that are don't exist in 3d space (will this ever happen?)
        if (clipPoint.w == 0f)
        {
            ndcPoint = Vector3.zero;
            viewportPoint = Vector3.zero;
            return Vector3.zero;
        }

        // Perspective division (projective space -> 3d space)
        ndcPoint = new Vector3(
            clipPoint.x / clipPoint.w,
            clipPoint.y / clipPoint.w,
            clipPoint.z / clipPoint.w
        );

        // viewport point (i.e. values between 0 - 1 in x/y, unity style)
        viewportPoint = new Vector3(
            (ndcPoint.x + 1f) * .5f,
            (ndcPoint.y + 1f) * .5f
        );

        // Screen point (i.e. values between 0 - pixel width in that axis))
        Vector3 screenPoint = new Vector3(
            viewportPoint.x * screenPxlWidth,
            viewportPoint.y * screenPxlHeight
        );

        return new Vector3(screenPoint.x, screenPoint.y, worldPoint.z);
    }

    #endregion

    /// <summary>
    /// Returns the world position of a camera given its view matrix
    /// </summary>
    /// <param name="worldToCameraMatrix"></param>
    /// <returns></returns>
    public static Vector3 GetWorldPosition(this Matrix4x4 worldToCameraMatrix)
    {
        return worldToCameraMatrix.inverse.MultiplyPoint(Vector3.zero);
    }
}

