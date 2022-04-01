using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[GenerateAuthoringComponent]
public struct ObstacleSpawner : IComponentData
{
    public Entity prefab;

    public float spawnDelay;
    public float timer;
}
