using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
partial class SpawnerSystem : SystemBase
{
    private Random rng;

    protected override void OnCreate()
    {
        rng = new Random();
        rng.InitState((uint)System.DateTime.Now.Second);
    }

    protected override void OnUpdate()
    {
        var dt = Time.DeltaTime;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var random = rng;

        Entities.ForEach((int entityInQueryIndex, ref ObstacleSpawner spawnerData, in LocalToWorld ltw) =>
        {
            spawnerData.timer += dt;

            if (spawnerData.timer >= spawnerData.spawnDelay)
            {
                spawnerData.timer = 0;

                var cube = ecb.Instantiate(spawnerData.prefab);

                var position = ltw.Position + new float3(random.NextFloat(-10, 10), 0, 0);

                ecb.SetComponent(cube, new Translation { Value = position });
            }

        }).Run();

        rng = random;

        ecb.Playback(EntityManager);
    }
}