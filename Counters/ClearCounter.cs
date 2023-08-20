using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// In order to extend IKitchenObjectParent, an interface, need to define the functions used in this .cs
public class ClearCounter : BaseCounter
{

    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    // Override overrides the base interact from BaseCounter.cs
    public override void Interact(Player player) {
        // Check if counter is empty
        if (!HasKitchenObject()) {
            // There is no KitchenObject here
            if (player.HasKitchenObject()) {
                // Player is carrying a KitchenObject, drop it on counter
                player.GetKitchenObject().SetKitchenObjectParent(this);
            }
            else {
                // Player not carrying anything so do nothing
            }
        }
        else {
            // There is a KitchenObject here
            if (player.HasKitchenObject()) {
                // Player is carrying something (try to get the plate)
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)) {
                    // If player is holding a plate then add ingredient to the plate
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())) {
                        GetKitchenObject().DestroySelf();
                    }
                }
                else {
                    // Player is not carrying plate but something else so check if counter has plate
                    if (GetKitchenObject().TryGetPlate(out plateKitchenObject)) {
                        // If counter is holding a plate then add ingredient to the plate
                        if (plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO())) {
                            player.GetKitchenObject().DestroySelf();
                        }
                    }
                }
            }
            else {
                // Player is not carrying anything
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }

}
