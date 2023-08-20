using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerCounter : BaseCounter
{
    
    public event EventHandler OnPlayerGrabbedObject;

    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public override void Interact(Player player) {
        // If player is not carrying anything
        if (!player.HasKitchenObject()) {
            // Player interacts, spawns an object, and give it to the player
            KitchenObject.SpawnKitchenObject(kitchenObjectSO, player);

            // Fire event (animation) when player grabs from the container
            OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
        }
    }

}
