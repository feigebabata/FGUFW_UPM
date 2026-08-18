using UnityEngine;

namespace FGUFW
{

    [RequireComponent(typeof(MeshFilter))]
    public class DynamicCube : MonoBehaviour
    {
        public static readonly Vector3[] Def_Vertices = new Vector3[]{new Vector3(1f,0f,1f),new Vector3(0f,0f,1f),new Vector3(1f,1f,1f),new Vector3(0f,1f,1f),new Vector3(1f,1f,0f),new Vector3(0f,1f,0f),new Vector3(1f,0f,0f),new Vector3(0f,0f,0f),new Vector3(1f,1f,1f),new Vector3(0f,1f,1f),new Vector3(1f,1f,0f),new Vector3(0f,1f,0f),new Vector3(1f,0f,0f),new Vector3(1f,0f,1f),new Vector3(0f,0f,1f),new Vector3(0f,0f,0f),new Vector3(0f,0f,1f),new Vector3(0f,1f,1f),new Vector3(0f,1f,0f),new Vector3(0f,0f,0f),new Vector3(1f,0f,0f),new Vector3(1f,1f,0f),new Vector3(1f,1f,1f),new Vector3(1f,0f,1f),};
        public static readonly Vector2[] Def_UV = new Vector2[]{new Vector2(0f,0f),new Vector2(1f,0f),new Vector2(0f,1f),new Vector2(1f,1f),new Vector2(0f,1f),new Vector2(1f,1f),new Vector2(0f,1f),new Vector2(1f,1f),new Vector2(0f,0f),new Vector2(1f,0f),new Vector2(0f,0f),new Vector2(1f,0f),new Vector2(0f,0f),new Vector2(0f,1f),new Vector2(1f,1f),new Vector2(1f,0f),new Vector2(0f,0f),new Vector2(0f,1f),new Vector2(1f,1f),new Vector2(1f,0f),new Vector2(0f,0f),new Vector2(0f,1f),new Vector2(1f,1f),new Vector2(1f,0f),};
        public static readonly int[] Def_Triangles = new int[]{0,2,3,0,3,1,8,4,5,8,5,9,10,6,7,10,7,11,12,13,14,12,14,15,16,17,18,16,18,19,20,21,22,20,22,23,};
        public static readonly Vector3[] Def_Normals = new Vector3[]{new Vector3(0f,0f,1f),new Vector3(0f,0f,1f),new Vector3(0f,0f,1f),new Vector3(0f,0f,1f),new Vector3(0f,1f,0f),new Vector3(0f,1f,0f),new Vector3(0f,0f,-1f),new Vector3(0f,0f,-1f),new Vector3(0f,1f,0f),new Vector3(0f,1f,0f),new Vector3(0f,0f,-1f),new Vector3(0f,0f,-1f),new Vector3(0f,-1f,0f),new Vector3(0f,-1f,0f),new Vector3(0f,-1f,0f),new Vector3(0f,-1f,0f),new Vector3(-1f,0f,0f),new Vector3(-1f,0f,0f),new Vector3(-1f,0f,0f),new Vector3(-1f,0f,0f),new Vector3(1f,0f,0f),new Vector3(1f,0f,0f),new Vector3(1f,0f,0f),new Vector3(1f,0f,0f),};
        public static readonly Vector4[] Def_Tangents = new Vector4[]{new Vector4(-1f,0f,0f,-1f),new Vector4(-1f,0f,0f,-1f),new Vector4(-1f,0f,0f,-1f),new Vector4(-1f,0f,0f,-1f),new Vector4(-1f,0f,0f,-1f),new Vector4(-1f,0f,0f,-1f),new Vector4(-1f,0f,0f,-1f),new Vector4(-1f,0f,0f,-1f),new Vector4(-1f,0f,0f,-1f),new Vector4(-1f,0f,0f,-1f),new Vector4(-1f,0f,0f,-1f),new Vector4(-1f,0f,0f,-1f),new Vector4(-1f,0f,0f,-1f),new Vector4(-1f,0f,0f,-1f),new Vector4(-1f,0f,0f,-1f),new Vector4(-1f,0f,0f,-1f),new Vector4(0f,0f,-1f,-1f),new Vector4(0f,0f,-1f,-1f),new Vector4(0f,0f,-1f,-1f),new Vector4(0f,0f,-1f,-1f),new Vector4(0f,0f,1f,-1f),new Vector4(0f,0f,1f,-1f),new Vector4(0f,0f,1f,-1f),new Vector4(0f,0f,1f,-1f),};


        private Mesh _mesh;

        [SerializeField]Vector3 _pivot = new Vector3(0.5f,0.5f,0.5f);
        public Vector3 Pivot
        {
            get=>_pivot;
            set
            {
                _pivot = value;   
                ResetMesh();
            }
        }

        [SerializeField]Vector3 _size = new Vector3(1,1,1);
        public Vector3 Size
        {
            get=>_size;
            set
            {
                _size = value;   
                ResetMesh();
            }
        }

        void OnValidate()
        {
            ResetMesh();
        }

        void Start()
        {
            ResetMesh();
        }

        public void ResetMesh()
        {
            if(_mesh.IsNull())
            {
                _mesh = new Mesh();
                _mesh.name = "DynamicCube";

                GetComponent<MeshFilter>().mesh = _mesh;
            }
            _mesh.Clear();

            var vertices = new Vector3[Def_Vertices.Length];
            var v_offset = VectorUtility.Multiply(-1*_pivot,_size);
            for (int i = 0; i < Def_Vertices.Length; i++)
            {
                var def_v = Def_Vertices[i];
                var v = VectorUtility.Multiply(def_v,_size);
                v += v_offset;
                vertices[i] = v;
            }

            _mesh.vertices = vertices;
            _mesh.uv = Def_UV;
            _mesh.triangles = Def_Triangles;
            _mesh.normals = Def_Normals;
            _mesh.tangents = Def_Tangents;

            _mesh.RecalculateBounds();

            var boxCollider = GetComponent<BoxCollider>();
            if(!boxCollider.IsNull())
            {
                boxCollider.size = _size;
                boxCollider.center = _size*0.5f+v_offset;
            }
        }


    }

}
