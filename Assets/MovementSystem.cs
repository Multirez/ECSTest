using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public class MovementSystem : ComponentSystem
{
    [BurstCompile]
    private struct MoveJobForEach : IJobForEach<Translation, VerticalSpeed>
    {
        public float deltaTime;

        public void Execute(ref Translation trans, ref VerticalSpeed speed)
        {
            float3 pos = trans.Value;
            pos.y += speed.Value * deltaTime;
            trans.Value = pos;

            if (pos.y > 1) speed.Value = -math.abs(speed.Value);
            if (pos.y < -1) speed.Value = math.abs(speed.Value);
        }
    }

    protected override void OnUpdate()
    {
        EntityQuery _group = GetEntityQuery(typeof(Translation), typeof(VerticalSpeed));
        var job = new MoveJobForEach { deltaTime = Time.deltaTime };
        var jobHandler = job.Schedule(_group);
        jobHandler.Complete();
    }
}

[BurstCompile]
public struct VerticalSpeed : IComponentData
{
    public float Value;
}


