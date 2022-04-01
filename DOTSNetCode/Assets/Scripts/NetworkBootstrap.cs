using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine.SceneManagement;

public class NetworkBootstrap : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName)
    {
        var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
        GenerateSystemLists(systems);

        var world = new World(defaultWorldName);
        World.DefaultGameObjectInjectionWorld = world;

        DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, ExplicitDefaultWorldSystems);
        ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(world);

        if (SceneManager.GetActiveScene().name == "Game")
        {
            StartClientServer(Connection.networkPort, false);
        }

        return true;
    }

    public static void StartClientServer(ushort networkPort, bool predictedPhysics = false)
    {
        StartServer(networkPort, predictedPhysics);
        ConnectToServer("127.0.0.1", networkPort, predictedPhysics);
    }

    public static void StartServer(ushort networkPort, bool predictedPhysics)
    {
        var server = CreateServerWorld(World.DefaultGameObjectInjectionWorld, "ServerWorld");

        var ep = NetworkEndPoint.AnyIpv4.WithPort(networkPort);
        server.GetExistingSystem<NetworkStreamReceiveSystem>().Listen(ep);

        if (predictedPhysics)
        {
            CreatePredictedPhysicsConfig(server);
        }
    }

    public static void ConnectToServer(string address, ushort networkPort, bool predictedPhysics = false)
    {
        var client = CreateClientWorld(World.DefaultGameObjectInjectionWorld, "ClientWorld");

        var ep = NetworkEndPoint.Parse(address, networkPort);
        client.GetExistingSystem<NetworkStreamReceiveSystem>().Connect(ep);

        if (predictedPhysics)
        {
            CreatePredictedPhysicsConfig(client);
        }
    }

    public static void CreatePredictedPhysicsConfig(World world)
    {
        var em = world.EntityManager;

        var config = em.CreateEntity(typeof(PredictedPhysicsConfig));
        //em.SetComponentData(config, new PredictedPhysicsConfig { PhysicsTicksPerSimTick = 60 });
        em.SetName(config, "CONFIG");
    }
}