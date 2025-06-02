using FishNet;
using UnityEngine;

public class ServerManager : MonoBehaviour {
    private void Update() {
        if (!InstanceFinder.IsServerStarted && Input.GetKeyDown(KeyCode.Z)) {
            InstanceFinder.NetworkManager.ServerManager.StartConnection();
            InstanceFinder.NetworkManager.ClientManager.StartConnection();
        }

        if (Input.GetKeyDown(KeyCode.X)) {
            InstanceFinder.NetworkManager.ClientManager.StartConnection();
        }
    }
}