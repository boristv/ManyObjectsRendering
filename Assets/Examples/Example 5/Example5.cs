using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class Example5 : MonoBehaviour
{
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;

    private ObjectPositionJob _job;
    private NativeArray<float> _nativeCubeYOffsets;
    private NativeArray<Matrix4x4> _nativeMatrices;
    private NativeArray<float3> _nativePositions;
    private RenderParams _rp;

    private void Start()
    {
        var count = MovingController.GetCount;
        _nativePositions = new NativeArray<float3>(count, Allocator.Persistent);
        _nativeMatrices = new NativeArray<Matrix4x4>(count, Allocator.Persistent);
        _nativeCubeYOffsets = new NativeArray<float>(count, Allocator.Persistent);

        MovingController.SetPositions((i, p) =>
        {
            _nativeCubeYOffsets[i] = p.y;
            _nativePositions[i] = p;
        });

        _job = new ObjectPositionJob
        {
            Positions = _nativePositions,
            YOffsets = _nativeCubeYOffsets
        };

        _rp = new RenderParams(_material);
    }

    private void Update()
    {
        _job.Matrices = _nativeMatrices;
        _job.Time = Time.time;
        _job.Schedule(_nativeMatrices.Length, 64).Complete();

        Graphics.RenderMeshInstanced(_rp, _mesh, 0, _nativeMatrices);
    }

    private void OnDestroy()
    {
        _nativePositions.Dispose();
        _nativeMatrices.Dispose();
        _nativeCubeYOffsets.Dispose();
    }
}

[BurstCompile]
internal struct ObjectPositionJob : IJobParallelFor
{
    public NativeArray<float3> Positions;
    [ReadOnly] public NativeArray<float> YOffsets;
    public NativeArray<Matrix4x4> Matrices;
    public float Time;

    public void Execute(int index)
    {
        var (pos, rot) = Positions[index].CalculatePosBurst(YOffsets[index], Time);

        Positions[index] = pos;
        Matrices[index] = Matrix4x4.TRS(pos, rot, MovingController.ObjectSize);
    }
}