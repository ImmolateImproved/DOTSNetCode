using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
partial class LifetimeSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var dt = Time.DeltaTime;

        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        Entities.ForEach((Entity e, ref LifetimeData lifeTime) =>
        {
            lifeTime.value -= dt;

            if (lifeTime.value <= 0)
                ecb.DestroyEntity(e);

        }).Run();

        ecb.Playback(EntityManager);
    }
}
