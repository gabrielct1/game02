using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Player : MonoBehaviour, IKitchenObjectParent {

    public static Player Instance { get; private set; }

    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs {
        public BaseCounter selectedCounter;
    }

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;


    private BaseCounter selectedCounter;
    private bool isWalking;
    private Vector3 lastInteractDir;
    private KitchenObject kitchenObject;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Debug.Log("There is already an instance");
        }
        Instance = this;
    }

    private void Start() {
        inputManager.OnInteractAction += InputManager_OnInteractAction;
        inputManager.OnInteractAlternateAction += InputManager_OnInteractAlternateAction;
    }

    private void InputManager_OnInteractAlternateAction(object sender, EventArgs e) {
        if (selectedCounter != null) {
            selectedCounter.InteractAlternate(this);
        }
    }

    private void InputManager_OnInteractAction(object sender, System.EventArgs e) {
        if (selectedCounter != null) {
            selectedCounter.Interact(this);
        }
    }

    private void Update() {
        HandleMovement();
        HandleInteraction();
    }
    public bool IsWalking() {
        return isWalking;
    }

    private void HandleMovement() {

        Vector2 inputVector = inputManager.GetMovementVectorNormalized();
        Vector3 movDir = new Vector3(inputVector.x, 0f, inputVector.y);


        float moveDistance = moveSpeed * Time.deltaTime;
        float playerHeight = 2f;
        float playerRadius = 0.6f;

        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, movDir, moveDistance);

        if (!canMove) {
            Vector3 movDirX = new Vector3(movDir.x, 0, 0).normalized;
            canMove = movDir.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, movDirX, moveDistance);

            //Checa para ver se é possível se mover apenas no eixo X
            if (canMove) {
                movDir = movDirX;
            } else {
                Vector3 movDirZ = new Vector3(0, 0, movDir.z).normalized;
                canMove = movDir.z != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, movDirZ, moveDistance);

                //Checa para ver se é possível se mover apenas no eixo Y
                if (canMove) {
                    movDir = movDirZ;
                } else {
                    //Não é possível se mover em nenhuma direção
                }
            }
        }
        //Checa para ver se é possível mover na direção
        if (canMove) {
            transform.position += movDir * moveDistance;
        }

        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, movDir, Time.deltaTime * rotateSpeed);
        isWalking = movDir != Vector3.zero;
    }
    private void HandleInteraction() {
        Vector2 inputVector = inputManager.GetMovementVectorNormalized();
        Vector3 movDir = new Vector3(inputVector.x, 0f, inputVector.y);

        if (movDir != Vector3.zero) {
            lastInteractDir = movDir;
        }

        float interactDistance = 2f;
        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, countersLayerMask)) {
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter)) {
                // Has ClearCounter
                if (baseCounter != selectedCounter) {
                    SetSelectedCounter(baseCounter);
                }
            } else {
                SetSelectedCounter(null);
            }
        } else {
            SetSelectedCounter(null);
        }
    }
    private void SetSelectedCounter(BaseCounter selectedCounter) {
        this.selectedCounter = selectedCounter;

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs {
            selectedCounter = selectedCounter
        });
    }

    public Transform GetKitchenObjectFollowTransform() {
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject) {
        this.kitchenObject = kitchenObject;
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