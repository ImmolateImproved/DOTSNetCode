using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

[GenerateAuthoringComponent]
public struct PlayerInput : ICommandData
{
    public int3 direction;

    public bool shoot;

    public uint Tick { get; set; }
}