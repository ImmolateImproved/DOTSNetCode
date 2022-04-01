using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct PlayerSpawnPositions : IBufferElementData
{
    public float3 position;
    public float3 direction;
}