using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

// RPC request from client to server for game to go "in game" and send snapshots / inputs
public struct GoInGameRequest : IRpcCommand
{

}

// When client has a connection with network id, go in game and tell server to also go in game
[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
partial class GoInGameClientSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<PlayerPrefab>();
        RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<NetworkIdComponent>(), ComponentType.Exclude<NetworkStreamInGame>()));
    }

    protected override void OnUpdate()
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

        Entities.WithNone<NetworkStreamInGame>().ForEach((Entity ent, in NetworkIdComponent id) =>
        {
            commandBuffer.AddComponent<NetworkStreamInGame>(ent);
            var req = commandBuffer.CreateEntity();
            commandBuffer.AddComponent<GoInGameRequest>(req);
            commandBuffer.AddComponent(req, new SendRpcCommandRequestComponent { TargetConnection = ent });

        }).Run();

        commandBuffer.Playback(EntityManager);
    }
}

// When server receives go in game request, go in game and delete request
[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
partial class GoInGameServerSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<PlayerPrefab>();
        RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<GoInGameRequest>(), ComponentType.ReadOnly<ReceiveRpcCommandRequestComponent>()));
    }

    protected override void OnUpdate()
    {
        var spawnerEntity = GetSingletonEntity<PlayerPrefab>();
        var playerSpawnPosBuffer = GetBuffer<PlayerSpawnPositions>(spawnerEntity);

        var prefab = GetSingleton<PlayerPrefab>().value;
        var prefabName = new FixedString32Bytes(EntityManager.GetName(prefab));
        var worldName = new FixedString32Bytes(World.Name);

        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

        Entities.ForEach((Entity reqEnt, in GoInGameRequest req, in ReceiveRpcCommandRequestComponent reqSrc) =>
        {
            commandBuffer.AddComponent<NetworkStreamInGame>(reqSrc.SourceConnection);
            var networkIdComponent = GetComponent<NetworkIdComponent>(reqSrc.SourceConnection);

            Debug.Log($"'{worldName}' setting connection '{networkIdComponent.Value}' to in game, spawning a Ghost '{prefabName}' for them!");

            var player = commandBuffer.Instantiate(prefab);
            commandBuffer.SetComponent(player, new GhostOwnerComponent { NetworkId = networkIdComponent.Value });

            // Add the player to the linked entity group so it is destroyed automatically on disconnect
            commandBuffer.AppendToBuffer(reqSrc.SourceConnection, new LinkedEntityGroup { Value = player });

            // Give each NetworkId their own spawn pos:
            {
                //var isEven = (networkIdComponent.Value & 1) == 0;
                //var staggeredXPos = networkIdComponent.Value * math.@select(.55f, -.55f, isEven) + math.@select(-0.25f, 0.25f, isEven);
                //var preventZFighting = -0.01f * networkIdComponent.Value;

                var index = ((networkIdComponent.Value - 1) % 2);

                var position = playerSpawnPosBuffer[index].position;

                var shootingData = GetComponent<ShootingData>(prefab);

                shootingData.direction = playerSpawnPosBuffer[index].direction;

                commandBuffer.SetComponent(player, new Translation { Value = position });
                commandBuffer.SetComponent(player, shootingData);
            }

            commandBuffer.DestroyEntity(reqEnt);

        }).Run();

        commandBuffer.Playback(EntityManager);
    }
}