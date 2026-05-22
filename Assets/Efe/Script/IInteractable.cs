using UnityEngine;

public interface IInteractable
{
    bool CanInteract { get; }
    void Interact(Transform interactor);
}
