using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// In charge of determining behaviour when connecting, disconnecting, etc.
/// </summary>
public class EntityNetworking : NetworkedBehaviour
{
    public GameObject EntityCamera;

    public override void NetworkStart()
    {
        if (IsLocalPlayer)
        {
            Debug.Log("Local player has been set. Initializing GUI.");
            PlayerManager.Singleton.LocalPlayer = gameObject;

            InventoryManager.Singleton.InventoryUI.Init();
            InventoryManager.Singleton.HotbarUI.Init();

            PlayerManager.Singleton.PlayerUI.SetActive(true);
        }
        else
        {
            // Disable camera
            EntityCamera.SetActive(false);
            Debug.Log($"Camera for this player {OwnerClientId} has been disabled.");

            // Disable player input
            GetComponent<PlayerInput>().enabled = false;
        }

        // Add this player to local list
        PlayerManager.Singleton.PlayerList.Add(NetworkedObject);
        Debug.Log($"Adding player {NetworkedObject.OwnerClientId} to the player list");
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}