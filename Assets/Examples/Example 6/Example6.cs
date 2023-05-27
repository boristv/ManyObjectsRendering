using UnityEngine;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using System.Runtime.InteropServices;

public class Example6 : MonoBehaviour
{
    private SystemHandle _system;
    private World _world;

    private System.Collections.IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f);

        _world = World.DefaultGameObjectInjectionWorld;
        _system = _world.CreateSystem<SpawnerSystem>();
    }

    private void Update()
    {
        if (_world == null) return;
        _system.Update(_world.Unmanaged);
    }

    private void OnDestroy()
    {
        _world?.DestroySystem(_system);
    }
}

public struct SpawnerPosition : IComponentData
{
    public float3 Value;
}

[BurstCompile]
[DisableAutoCreation]
public partial struct SpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        var prefab = SystemAPI.GetSingleton<Spawner>().Prefab;
        var scale = SystemAPI.GetComponent<LocalTransform>(prefab).Scale;
        var buffer = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        MovingController.SetPositions((_, pos) =>
        {
            var entity = buffer.Instantiate(prefab);
            buffer.AddComponent(entity, new SpawnerPosition { Value = pos.y });
            buffer.SetComponent(entity, LocalTransform.FromPosition(pos).ApplyScale(scale));
        });
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) =>
        new SpawnerJob { Moment = (float)SystemAPI.Time.ElapsedTime }.ScheduleParallel(state.Dependency).Complete();
}

[BurstCompile]
[StructLayout(LayoutKind.Auto)]
public partial struct SpawnerJob : IJobEntity
{
    public float Moment;

    [BurstCompile]
    private void Execute(SpawnerAspect aspect) => aspect.UpdateLocalTransformFromSpawnerPosition(Moment);
}

public readonly partial struct SpawnerAspect : IAspect
{
    private readonly RefRW<LocalTransform> _localTransform;
    private readonly RefRO<SpawnerPosition> _targetPosition;

    public void UpdateLocalTransformFromSpawnerPosition(float moment) =>
        (_localTransform.ValueRW.Position, _localTransform.ValueRW.Rotation) =
            _localTransform.ValueRW.Position.CalculatePosBurst(_targetPosition.ValueRO.Value.y, moment);
}
