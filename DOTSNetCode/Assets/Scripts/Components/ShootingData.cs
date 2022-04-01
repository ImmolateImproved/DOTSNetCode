using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct ShootingData : IComponentData
{
    public Entity prefab;
    public Entity predictedPrefab;

    public float3 direction;

    public uint weaponCooldown;
}