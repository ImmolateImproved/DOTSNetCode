using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Networking.Transport.Utilities;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
partial class ShootingSystem : SystemBase
{
    private GhostPredictionSystemGroup ghostPredictionSystemGroup;

    private const int k_CoolDownTicksCount = 5;

    protected override void OnCreate()
    {
        ghostPredictionSystemGroup = World.GetExistingSystem<GhostPredictionSystemGroup>();
    }

    protected override void OnUpdate()
    {
        var dt = Time.DeltaTime;
        var tick = ghostPredictionSystemGroup.PredictingTick;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        Entities.ForEach((ref ShootingData shootingData) =>
        {
            if (shootingData.predictedPrefab == Entity.Null)
            {
                shootingData.predictedPrefab = GhostCollectionSystem.CreatePredictedSpawnPrefab(EntityManager, shootingData.prefab);
            }

        }).WithStructuralChanges().Run();

        Entities.ForEach((ref ShootingData shootingData, in LocalToWorld ltw, in DynamicBuffer<PlayerInput> inputBuffer, in GhostOwnerComponent ghostOwner, in PredictedGhostComponent prediction) =>
        {
            if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
                return;

            if (!inputBuffer.GetDataAtTick(tick, out var input))
            {
                input.shoot = false;
            }

            var canShoot = shootingData.weaponCooldown == 0 || SequenceHelpers.IsNewer(tick, shootingData.weaponCooldown);

            if (input.shoot && canShoot)
            {
                var projectile = ecb.Instantiate(shootingData.predictedPrefab);
                ecb.SetComponent(projectile, new Translation { Value = ltw.Position + shootingData.direction });
                ecb.SetComponent(projectile, new PhysicsVelocity { Linear = shootingData.direction * 15 });

                ecb.SetComponent(projectile, new GhostOwnerComponent { NetworkId = ghostOwner.NetworkId });

                shootingData.weaponCooldown = tick + k_CoolDownTicksCount;
            }

        }).Run();

        ecb.Playback(EntityManager);
    }
}
