using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
partial class MovementSystem : SystemBase
{
    private GhostPredictionSystemGroup ghostPredictionSystemGroup;

    protected override void OnCreate()
    {
        ghostPredictionSystemGroup = World.GetExistingSystem<GhostPredictionSystemGroup>();
    }

    protected override void OnUpdate()
    {
        var dt = Time.DeltaTime;
        var tick = ghostPredictionSystemGroup.PredictingTick;

        Entities.ForEach((ref PhysicsVelocity physicsVelocity, ref Translation translation, in MovementData movementData, in DynamicBuffer<PlayerInput> inputBuffer, in PredictedGhostComponent prediction) =>
        {
            if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
                return;

            inputBuffer.GetDataAtTick(tick, out var input);

            var velocity = math.normalizesafe(input.direction) * movementData.moveSpeed;// * dt;

            //translation.Value += velocity * dt;

            translation.Value.y = 1;

            physicsVelocity.Linear = velocity;

        }).Run();
    }
}
