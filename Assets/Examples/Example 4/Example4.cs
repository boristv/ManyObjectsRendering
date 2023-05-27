using UnityEngine;

public class Example4 : MonoBehaviour
{
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;

    private float[] _objectYOffsets;
    private Matrix4x4[] _matrices;
    private Vector3[] _positions;
    private RenderParams _rp;

    private void Start()
    {
        var count = MovingController.GetCount;
        _objectYOffsets = new float[count];
        _positions = new Vector3[count];
        _matrices = new Matrix4x4[_positions.Length];

        MovingController.SetPositions((i, p) =>
        {
            _objectYOffsets[i] = p.y;
            _positions[i] = p;
        });

        _rp = new RenderParams(_material);
    }

    private void Update()
    {
        var time = Time.time;
        for (var i = 0; i < _positions.Length; i++)
        {
            var (pos, rot) = _positions[i].CalculatePos(_objectYOffsets[i], time);

            _matrices[i].SetTRS(pos, rot, MovingController.ObjectSize);

            _positions[i].y = pos.y;
        }

        Graphics.RenderMeshInstanced(_rp, _mesh, 0, _matrices);
    }
}