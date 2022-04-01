using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Connection : MonoBehaviour
{
    public TMP_InputField addressInputField;
    public const ushort networkPort = 7979;

    public bool predictedPhysics;

    public void StartClientServer()
    {
        NetworkBootstrap.StartClientServer(networkPort, predictedPhysics);
        SceneManager.LoadSceneAsync(1);
    }

    public void StartServer()
    {
        NetworkBootstrap.StartServer(networkPort, predictedPhysics);
        SceneManager.LoadSceneAsync(1);
    }

    public void ConnectToServer()
    {
        NetworkBootstrap.ConnectToServer(addressInputField.text, networkPort, predictedPhysics);
        SceneManager.LoadSceneAsync(1);
    }
}