using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For some reason, when running on Unity, have to start at Game page then switch to Scene then back to get the console to update properly
public class Player : MonoBehaviour, IKitchenObjectParent
{

    // Static means it belongs to player class itself and not an instance of player
    // Instance is a property, using public getter and private setter
    public static Player Instance { get; private set; }

    public event EventHandler OnPickedSomething;

    // <> C# generics
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs {
        public BaseCounter selectedCounter;
    }

    // SerializeField allows direct testing and editing of speed on Unity
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;

    private bool isWalking;
    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("There is more than one Player instance.");
        }
        Instance = this;
    }

    private void Start() {
        gameInput.OnInteractAction += GameInput_OnInteractAction;
        gameInput.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
    }

    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e) {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;
        
        if (selectedCounter != null) {
            selectedCounter.InteractAlternate(this);
        }
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e) {
        // If game is not playing, player cannot interact with anything
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        if (selectedCounter != null) {
            selectedCounter.Interact(this);
        }
    }

    private void Update() {
        HandleMovement();
        HandleInteractions();
    }

    public bool IsWalking() {
        return isWalking;
    }

    private void HandleInteractions() {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        // Add on to the Player's transform in Unity but need to cast to 3D to match transform (x,y,z)
        // Use x and z, instead of y, so the player doesn't move up into the sky
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        // If moving in some direction, store it
        // If player is facing the counter but not moving, it'll still show interaction
        if (moveDir != Vector3.zero) {
            lastInteractDir = moveDir;
        }

        float interactDistance = 2f;

        // Interact with counters with output that gives information
        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, countersLayerMask)) {
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter)) {
                // Has baseCounter if not null or False
                if (baseCounter != selectedCounter) {
                    SetSelectedCounter(baseCounter);
                }
            }
            else {
                SetSelectedCounter(null);
            }
        }
        else {
            SetSelectedCounter(null);  
        }
    }

    private void HandleMovement() {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        // Add on to the Player's transform in Unity but need to cast to 3D to match transform (x,y,z)
        // Use x and z, instead of y, so the player doesn't move up into the sky
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = .7f;
        float playerHeight = 2f;

        // Detect collision
        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);

        if (!canMove) {
            // Can't move towards moveDir so split movement direction instead of being idle

            // Attempt only X movement
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = (moveDir.x < -.5f || moveDir.x > +.5f) && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);

            if (canMove) {
                // Can move only on the X
                moveDir = moveDirX;
            }
            else {
                // Can't move only on the X

                // Attempt only Z movement
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = (moveDir.z < -.5f || moveDir.z > +.5f) && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);
            
                if (canMove) {
                    // Can move only on the Z
                    moveDir = moveDirZ;
                }
                else {
                    // Can't move in any direction -- blank means do nothing
                }
            }
        }

        if (canMove) {
            // Time.deltaTime ensures movement is independent of framerate so computer difference shouldn't matter
            transform.position += moveDir * moveDistance;
        }

        // If movement direction is different from (0, 0, 0) then player is moving
        isWalking = moveDir != Vector3.zero;

        // Have player face the moving direction in a smooth rotation
        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
    }

    private void SetSelectedCounter(BaseCounter selectedCounter) {
        this.selectedCounter = selectedCounter;

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs {
            // The first one is the field inside the event
            selectedCounter = selectedCounter
        });
    }

    // Only function signature needs to be the same for interface so can add animation, etc.
        public Transform GetKitchenObjectFollowTransform() {
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject) {
        // Player picks up something
        this.kitchenObject = kitchenObject;

        if (kitchenObject != null) {
            OnPickedSomething?.Invoke(this, EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject() {
        return kitchenObject;
    }

    public void ClearKitchenObject() {
        kitchenObject = null;
    }

    public bool HasKitchenObject() {
        return kitchenObject != null;
    }

}
