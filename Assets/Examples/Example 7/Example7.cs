using UnityEngine;

public class Example7 : MonoBehaviour
{
    [SerializeField] private Mesh _instanceMesh;
    [SerializeField] private Material _instanceMaterial;

    private readonly uint[] _args = { 0, 0, 0, 0, 0 };
    private ComputeBuffer _argsBuffer;
    private int _count;
    private float[] _objectYOffsets;
    private Vector3[] _positions;

    private ComputeBuffer _positionBuffer, _offsetBuffer;

    private void Start()
    {
        _count = MovingController.GetCount;
    
        _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        UpdateBuffers();
    }

    private void Update()
    {
        Graphics.DrawMeshInstancedIndirect(_instanceMesh, 0, _instanceMaterial, new Bounds(Vector3.zero, Vector3.one * 1000), _argsBuffer);
    }

    private void OnDisable()
    {
        _positionBuffer?.Release();
        _positionBuffer = null;

        _offsetBuffer?.Release();
        _offsetBuffer = null;

        _argsBuffer?.Release();
        _argsBuffer = null;
    }

    private void UpdateBuffers()
    {
        _positionBuffer?.Release();
        _offsetBuffer?.Release();
        _positionBuffer = new ComputeBuffer(_count, 12);
        _offsetBuffer = new ComputeBuffer(_count, 4);

        _positions = new Vector3[_count];
        _objectYOffsets = new float[_count];

        MovingController.SetPositions((i, p) =>
        {
            _positions[i] = p;
            _objectYOffsets[i] = p.y;
        });

        _positionBuffer.SetData(_positions);
        _offsetBuffer.SetData(_objectYOffsets);
        _instanceMaterial.SetBuffer("position_buffer_1", _positionBuffer);
        _instanceMaterial.SetBuffer("position_buffer_2", _offsetBuffer);

        // Verts
        _args[0] = _instanceMesh.GetIndexCount(0);
        _args[1] = (uint)_count;
        _args[2] = _instanceMesh.GetIndexStart(0);
        _args[3] = _instanceMesh.GetBaseVertex(0);

        _argsBuffer.SetData(_args);
    }
}