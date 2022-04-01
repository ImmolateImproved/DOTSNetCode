using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Input = UnityEngine.Input;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
partial class PlayerInputSystem : SystemBase
{
    private ClientSimulationSystemGroup clientSimulationSystemGroup;

    protected override void OnCreate()
    {
        clientSimulationSystemGroup = World.GetExistingSystem<ClientSimulationSystemGroup>();
    }

    protected override void OnUpdate()
    {
        var h = (int)Input.GetAxisRaw("Horizontal");
        var v = (int)Input.GetAxisRaw("Vertical");

        var shoot = Input.GetKeyDown(UnityEngine.KeyCode.Space);

        var tick = clientSimulationSystemGroup.ServerTick;

        Entities.ForEach((ref DynamicBuffer<PlayerInput> inputBuffer) =>
        {
            var input = new PlayerInput
            {
                direction = new int3(h, 0, v),
                shoot = shoot,
                Tick = tick
            };

            inputBuffer.AddCommandData(input);

        }).Run();
    }
}