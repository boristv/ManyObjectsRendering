using Unity.Mathematics;
using UnityEngine;

public class Example8 : MonoBehaviour
{
    [SerializeField] private Mesh _instanceMesh;
    [SerializeField] private Material _instanceMaterial;
    [SerializeField] private ComputeShader _compute;
    
    [SerializeField] private Transform _pusher;
    [SerializeField] private float _pusherSpeed = 20;   
    
    private readonly uint[] _args = { 0, 0, 0, 0, 0 };
    private ComputeBuffer _argsBuffer;

    private int _count;

    private int _kernel;

    private ComputeBuffer _meshPropertiesBuffer;

    private void Start()
    {
        _kernel = _compute.FindKernel("cs_main");
        _count = MovingController.GetCount;

        _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        UpdateBuffers();
    }

    private void Update()
    {
        var dir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        _pusher.Translate(dir * (_pusherSpeed * Time.deltaTime));
        
        _compute.SetVector("pusher_position", _pusher.position);
        _compute.Dispatch(_kernel, Mathf.CeilToInt(_count / 64f), 1, 1);

        Graphics.DrawMeshInstancedIndirect(_instanceMesh, 0, _instanceMaterial, new Bounds(Vector3.zero, Vector3.one * 1000), _argsBuffer);
    }

    private void OnDisable()
    {
        _argsBuffer?.Release();
        _argsBuffer = null;
        
        _meshPropertiesBuffer?.Release();
        _meshPropertiesBuffer = null;
    }

    private void UpdateBuffers()
    {
        var offset = Vector3.zero;
        var data = new MeshData[_count];

        var rot = Quaternion.identity;
        MovingController.SetPositions((i, p) =>
        {
            data[i] = new MeshData
            {
                BasePos = p,
                OffsetY = p.y,
                Mat = Matrix4x4.TRS(p, rot, MovingController.ObjectSize),
                Amount = 0
            };
        });

        _meshPropertiesBuffer = new ComputeBuffer(_count, 84);
        _meshPropertiesBuffer.SetData(data);

        _compute.SetBuffer(_kernel, "data", _meshPropertiesBuffer);
        _instanceMaterial.SetBuffer("data", _meshPropertiesBuffer);

        // Verts
        _args[0] = _instanceMesh.GetIndexCount(0);
        _args[1] = (uint)_count;
        _args[2] = _instanceMesh.GetIndexStart(0);
        _args[3] = _instanceMesh.GetBaseVertex(0);

        _argsBuffer.SetData(_args);
    }

    private struct MeshData
    {
        public float3 BasePos;
        public float OffsetY;
        public Matrix4x4 Mat;
        public float Amount;
    }
}