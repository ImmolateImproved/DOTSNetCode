using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class MovementAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float moveSpeed;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new MovementData { moveSpeed = moveSpeed });
    }
}