using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
partial class CameraInitializationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var playerSpawnPosEntity = GetSingletonEntity<PlayerSpawnPositions>();
        var playerSpawnPosBuffer = GetBuffer<PlayerSpawnPositions>(playerSpawnPosEntity);

        Entities.WithAll<PredictedGhostComponent, MovementData>()
            .ForEach((Entity ent) =>
            {
                var cameraRoot = GameObject.Find("CameraRoot").transform;

                EntityManager.AddComponentObject(ent, cameraRoot);
                EntityManager.AddComponentData(ent, new CopyTransformToGameObject());

                Enabled = false;

            }).WithStructuralChanges().Run();
    }
}