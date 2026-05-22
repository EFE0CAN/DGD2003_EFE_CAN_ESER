using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Kameradan E ile etkileşim. MorphInteractable ve diğer IInteractable bileşenlerini çağırır.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private float interactionRange = 6f;
    [SerializeField] private LayerMask interactionMask = ~0;

    [Header("Hariç")]
    [SerializeField] private string[] ignoreTags = { "Player", "MainCamera" };

    [Header("Debug")]
    [SerializeField] private bool logWhenInteractFails = true;

    private IInteractable _focused;

    private void Update()
    {
        UpdateFocus();

        if (!WasInteractPressedThisFrame())
            return;

        TryInteract();
    }

    private static bool WasInteractPressedThisFrame()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            return true;

        return Input.GetKeyDown(KeyCode.E);
    }

    private void UpdateFocus()
    {
        _focused = null;

        Ray ray = new Ray(transform.position, transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionMask))
            return;

        if (ShouldIgnore(hit.collider.gameObject))
            return;

        _focused = hit.collider.GetComponentInParent<IInteractable>();
        if (_focused != null && !_focused.CanInteract)
            _focused = null;
    }

    private void TryInteract()
    {
        if (TryGetInteractableFromRay(out IInteractable target))
        {
            target.Interact(transform);
            return;
        }

        if (logWhenInteractFails)
            Debug.Log("E: Etkileşim yok — objeye bak ve Collider + MorphInteractable olduğundan emin ol.", this);
    }

    private bool TryGetInteractableFromRay(out IInteractable interactable)
    {
        interactable = _focused;

        Ray ray = new Ray(transform.position, transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionMask))
            return false;

        if (ShouldIgnore(hit.collider.gameObject))
            return false;

        interactable = hit.collider.GetComponentInParent<IInteractable>();
        return interactable != null && interactable.CanInteract;
    }

    private bool ShouldIgnore(GameObject obj)
    {
        foreach (string tag in ignoreTags)
        {
            if (obj.CompareTag(tag)) return true;
        }
        return false;
    }
}
