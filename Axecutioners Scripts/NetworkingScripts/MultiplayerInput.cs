using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class MultiplayerInput : NetworkBehaviour
{
    //from the input system - must be in a network behaviour and doesnt record input if the network is paused
    public void Move(InputAction.CallbackContext context)
    {
        if (gameObject.GetComponent<PlayerScript>().networkPause == false && gameObject.GetComponent<PlayerScript>().IsAttacking() == false) 
        {
            int id = SteamLobby.Instance.GetIndex(gameObject.GetComponent<NetworkIdentity>().assetId);
            if (isLocalPlayer)
                gameObject.GetComponent<NetworkPlayerController>().Move(context.ReadValue<Vector2>(), id);
        }
    }
    public void Attack(InputAction.CallbackContext context)
    {
        if (gameObject.GetComponent<PlayerScript>().networkPause == false && gameObject.GetComponent<PlayerScript>().IsAttacking() == false)
        {
            int id = SteamLobby.Instance.GetIndex(gameObject.GetComponent<NetworkIdentity>().assetId);
            if (isLocalPlayer)
                gameObject.GetComponent<NetworkPlayerController>().Attack(context.ReadValue<float>(), id);
        }
    }
    public void Dash(InputAction.CallbackContext context)
    {
        if (gameObject.GetComponent<PlayerScript>().networkPause == false && gameObject.GetComponent<PlayerScript>().IsAttacking() == false)
        {
            int id = SteamLobby.Instance.GetIndex(gameObject.GetComponent<NetworkIdentity>().assetId);
            if (isLocalPlayer)
                gameObject.GetComponent<NetworkPlayerController>().Dash(context.ReadValue<float>(), id);
        }
    }

    //OBSOLETE
    //public void Block(InputAction.CallbackContext context)
    //{
    //    int id = SteamLobby.Instance.GetIndex(gameObject.GetComponent<NetworkIdentity>().assetId);
    //    if (isLocalPlayer)
    //    {
    //        if (id == 1)
    //            gameObject.GetComponent<NetworkPlayerController>().BlockFromServer(context.ReadValue<float>(), id);
    //        else if (id == 0)
    //            gameObject.GetComponent<NetworkPlayerController>().Block(context.ReadValue<float>(), id);
    //    }
    //}
}
