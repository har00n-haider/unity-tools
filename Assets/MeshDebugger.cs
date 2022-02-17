using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MeshDebugger : MonoBehaviour
{
    /// <summary>
    /// Visualises debugging of vertex/face normals.
    /// Originally taken from here: https://www.reddit.com/r/Unity3D/comments/5m2uuw/draw_normals/
    /// </summary>
    [System.Serializable]
    public class MeshDebuggers 
    {
        private GameObject _selGo;
        [SerializeField]
        private MeshFilter _selMeshFilter = null;
        [SerializeField]
        private NormalsDrawData _faceNormals = new NormalsDrawData(new Color32(34, 221, 221, 155), true);
        [SerializeField]
        private NormalsDrawData _vertexNormals = new NormalsDrawData(new Color32(200, 255, 195, 127), false);

        [System.Serializable]
        private class NormalsDrawData
        {

            [SerializeField]
            protected DrawType _draw = DrawType.Selected;
            protected enum DrawType { Never, Selected, Always }
            [SerializeField]
            protected float _length = 0.3f;
            [SerializeField]
            protected Color _normalColor;
            private Color _baseColor = new Color32(255, 133, 0, 255);
            private const float _baseSize = 0.0125f;


            public NormalsDrawData(Color normalColor, bool draw)
            {
                _normalColor = normalColor;
                _draw = draw ? DrawType.Selected : DrawType.Never;
            }

            public bool CanDraw(bool isSelected)
            {
                return (_draw == DrawType.Always) || (_draw == DrawType.Selected && isSelected);
            }

            public void Draw(Vector3 from, Vector3 direction)
            {
                if (Camera.current.transform.InverseTransformDirection(direction).z < 0f)
                {
                    //Gizmos.color = _baseColor;
                    //Gizmos.DrawWireSphere(from, _baseSize);

                    Gizmos.color = _normalColor;
                    Gizmos.DrawRay(from, direction * _length);
                }
            }
        }

        private GameObject Debugger;

        public void Init(GameObject Debugger) 
        {
            this.Debugger = Debugger;
        }

        public void Update()
        {
            var selection = Selection.gameObjects;
            if (selection.Length > 0)
            {
                _selGo = Selection.activeGameObject;

                if (_selGo != null)
                {
                    foreach (MeshFilter mf in _selGo.GetComponentsInChildren<MeshFilter>())
                    {
                        _selGo = mf.gameObject;
                        _selMeshFilter = mf;
                        OnDrawNormals(false);
                    }
                }
            }
        }

        private void OnDrawNormals(bool isSelected)
        {
            if (_selMeshFilter == null) return;

            Mesh mesh = _selMeshFilter.sharedMesh;

            //Draw Face Normals
            if (_faceNormals.CanDraw(isSelected))
            {
                int[] triangles = mesh.triangles;
                Vector3[] vertices = mesh.vertices;

                for (int i = 0; i < triangles.Length; i += 3)
                {
                    Vector3 v0 = _selGo.transform.TransformPoint(vertices[triangles[i]]);
                    Vector3 v1 = _selGo.transform.TransformPoint(vertices[triangles[i + 1]]);
                    Vector3 v2 = _selGo.transform.TransformPoint(vertices[triangles[i + 2]]);
                    Vector3 center = (v0 + v1 + v2) / 3;

                    Vector3 dir = Vector3.Cross(v1 - v0, v2 - v0);
                    dir /= dir.magnitude;

                    _faceNormals.Draw(center, dir);
                }
            }

            //Draw Vertex Normals
            if (_vertexNormals.CanDraw(isSelected))
            {
                Vector3[] vertices = mesh.vertices;
                Vector3[] normals = mesh.normals;
                for (int i = 0; i < vertices.Length; i++)
                {
                    _vertexNormals.Draw(
                        _selGo.transform.TransformPoint(vertices[i]),
                        _selGo.transform.TransformVector(normals[i])
                    );
                }
            }
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
