using System;
using System.Collections;
using UnityEngine;

public enum PositionAlignMode
{
    TransformPivot,
    VisualCenter,
    BottomCenter
}

[RequireComponent(typeof(Collider))]
public class MorphInteractable : MonoBehaviour, IInteractable
{
    public static event Action<MorphInteractable> MorphCompleted;
    [Header("Dönüşüm")]
    [Tooltip("E'ye basınca spawn edilecek yeni obje prefab'ı")]
    [SerializeField] private GameObject morphTargetPrefab;
    [Tooltip("Prefab yerine sahnede hazır obje kullan (başlangıçta kapalı olmalı)")]
    [SerializeField] private GameObject morphTargetInScene;
    [SerializeField] private bool oneTimeOnly = true;
    [Tooltip("Kapalı: yeni obje prefab'ın kendi boyutunu kullanır. Açık: hatalı objenin scale değerini kopyalar.")]
    [SerializeField] private bool inheritScaleFromSource = false;

    [Header("Konum")]
    [Tooltip("Prefab pivot'u farklıysa görsel merkeze hizala")]
    [SerializeField] private PositionAlignMode positionAlignMode = PositionAlignMode.VisualCenter;
    [SerializeField] private Vector3 positionOffset;

    [Header("Zamanlama")]
    [SerializeField] private float morphDelay = 0.4f;
    [SerializeField] private float smokeScale = 1f;

    private bool _morphed;
    private bool _busy;

    private void Awake()
    {
        EnsureInteractionCollider();
    }

    public bool CanInteract => !_busy && (!oneTimeOnly || !_morphed);
    public bool IsMorphed => _morphed;

    public void Interact(Transform interactor)
    {
        if (!CanInteract) return;
        StartCoroutine(MorphRoutine());
    }

    private IEnumerator MorphRoutine()
    {
        _busy = true;

        Vector3 smokePos = GetSmokePosition();
        SmokeEffect.Play(smokePos, smokeScale);

        yield return new WaitForSeconds(morphDelay);

        ApplyMorph();
        _morphed = true;
        _busy = false;
        MorphCompleted?.Invoke(this);
    }

    private Vector3 GetSmokePosition()
    {
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null) return rend.bounds.center;
        return transform.position + Vector3.up * 0.5f;
    }

    private void ApplyMorph()
    {
        if (morphTargetPrefab != null)
        {
            Transform parent = transform.parent;
            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;

            GameObject spawned = Instantiate(morphTargetPrefab, pos, rot, parent);

            if (inheritScaleFromSource)
                spawned.transform.localScale = transform.localScale;
            else
                spawned.transform.localScale = morphTargetPrefab.transform.localScale;

            ApplyCollidersFromPrefab(spawned, morphTargetPrefab);
            AlignObjectToSource(spawned, gameObject);
            RemoveMorphInteractableFromSpawned(spawned);

            HideSourceObject();
            return;
        }

        if (morphTargetInScene != null)
        {
            morphTargetInScene.transform.SetPositionAndRotation(transform.position, transform.rotation);

            if (inheritScaleFromSource)
                morphTargetInScene.transform.localScale = transform.localScale;

            morphTargetInScene.SetActive(true);

            if (morphTargetPrefab != null)
                ApplyCollidersFromPrefab(morphTargetInScene, morphTargetPrefab);
            else
                RemoveAutoInteractionColliders(morphTargetInScene);

            AlignObjectToSource(morphTargetInScene, gameObject);

            HideSourceObject();
            return;
        }

        Debug.LogWarning($"MorphInteractable ({name}): morphTargetPrefab veya morphTargetInScene atanmadı!", this);
    }

    /// <summary>
    /// Spawn edilen objede prefab'taki collider boyutlarını kullanır; kaynak objenin dev collider'ını taşımaz.
    /// </summary>
    private static void ApplyCollidersFromPrefab(GameObject instance, GameObject prefabAsset)
    {
        if (instance == null || prefabAsset == null) return;

        Collider[] prefabColliders = prefabAsset.GetComponentsInChildren<Collider>(true);

        RemoveAutoInteractionColliders(instance);

        if (prefabColliders.Length == 0)
        {
            FitBoxColliderFromRenderers(instance);
            return;
        }

        if (prefabAsset.GetComponent<Collider>() == null)
        {
            BoxCollider strayRootBox = instance.GetComponent<BoxCollider>();
            if (strayRootBox != null)
                Destroy(strayRootBox);
        }

        foreach (Collider prefabCol in prefabColliders)
        {
            Transform target = ResolveMatchingTransform(instance.transform, prefabAsset.transform, prefabCol.transform);
            if (target == null) continue;

            Collider instanceCol = GetColliderOfType(target, prefabCol.GetType());
            if (instanceCol == null)
                instanceCol = CloneCollider(prefabCol, target.gameObject);
            else
                CopyColliderValues(prefabCol, instanceCol);

            instanceCol.enabled = true;
        }
    }

    private static void RemoveAutoInteractionColliders(GameObject root)
    {
        foreach (AutoInteractionColliderMarker marker in root.GetComponentsInChildren<AutoInteractionColliderMarker>(true))
        {
            Collider col = marker.GetComponent<Collider>();
            if (col != null) Destroy(col);
            Destroy(marker);
        }
    }

    private static void RemoveMorphInteractableFromSpawned(GameObject spawned)
    {
        MorphInteractable morph = spawned.GetComponent<MorphInteractable>();
        if (morph != null) Destroy(morph);
    }

    private void AlignObjectToSource(GameObject target, GameObject source)
    {
        if (positionAlignMode == PositionAlignMode.TransformPivot)
        {
            target.transform.position = source.transform.position + positionOffset;
            return;
        }

        Vector3 sourcePoint = GetAlignmentPoint(source, positionAlignMode) + positionOffset;
        Vector3 targetPoint = GetAlignmentPoint(target, positionAlignMode);
        target.transform.position += sourcePoint - targetPoint;
    }

    private static Vector3 GetAlignmentPoint(GameObject go, PositionAlignMode mode)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return go.transform.position;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return mode switch
        {
            PositionAlignMode.BottomCenter => new Vector3(bounds.center.x, bounds.min.y, bounds.center.z),
            PositionAlignMode.VisualCenter => bounds.center,
            _ => go.transform.position
        };
    }

    private void HideSourceObject()
    {
        foreach (Collider col in GetComponentsInChildren<Collider>(true))
            col.enabled = false;

        foreach (Renderer rend in GetComponentsInChildren<Renderer>())
            rend.enabled = false;

        gameObject.SetActive(false);
    }

    private void EnsureInteractionCollider()
    {
        if (GetComponentInChildren<Collider>(true) != null)
            return;

        Renderer rend = GetComponentInChildren<Renderer>();
        BoxCollider box = gameObject.AddComponent<BoxCollider>();
        gameObject.AddComponent<AutoInteractionColliderMarker>();

        if (rend == null) return;

        Bounds worldBounds = rend.bounds;
        box.center = transform.InverseTransformPoint(worldBounds.center);
        Vector3 size = transform.InverseTransformVector(worldBounds.size);
        box.size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
    }

    private static void FitBoxColliderFromRenderers(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        BoxCollider box = target.GetComponent<BoxCollider>() ?? target.AddComponent<BoxCollider>();
        Bounds combined = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            combined.Encapsulate(renderers[i].bounds);

        box.center = target.transform.InverseTransformPoint(combined.center);
        Vector3 size = target.transform.InverseTransformVector(combined.size);
        box.size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
    }

    private static Transform ResolveMatchingTransform(Transform instanceRoot, Transform prefabRoot, Transform prefabNode)
    {
        if (prefabNode == prefabRoot)
            return instanceRoot;

        string path = GetHierarchyPath(prefabRoot, prefabNode);
        if (string.IsNullOrEmpty(path))
            return instanceRoot;

        Transform found = instanceRoot.Find(path);
        return found != null ? found : instanceRoot;
    }

    private static string GetHierarchyPath(Transform root, Transform node)
    {
        if (node == root) return string.Empty;

        string path = node.name;
        Transform current = node.parent;
        while (current != null && current != root)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }

    private static Collider GetColliderOfType(Transform target, System.Type type)
    {
        Collider[] colliders = target.GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            if (col.GetType() == type) return col;
        }
        return null;
    }

    private static Collider CloneCollider(Collider source, GameObject target)
    {
        switch (source)
        {
            case BoxCollider srcBox:
                BoxCollider box = target.AddComponent<BoxCollider>();
                CopyColliderValues(srcBox, box);
                return box;
            case MeshCollider srcMesh:
                MeshCollider mesh = target.AddComponent<MeshCollider>();
                CopyColliderValues(srcMesh, mesh);
                return mesh;
            case SphereCollider srcSphere:
                SphereCollider sphere = target.AddComponent<SphereCollider>();
                CopyColliderValues(srcSphere, sphere);
                return sphere;
            case CapsuleCollider srcCapsule:
                CapsuleCollider capsule = target.AddComponent<CapsuleCollider>();
                CopyColliderValues(srcCapsule, capsule);
                return capsule;
            default:
                return null;
        }
    }

    private static void CopyColliderValues(Collider source, Collider target)
    {
        switch (source)
        {
            case BoxCollider srcBox when target is BoxCollider dstBox:
                dstBox.center = srcBox.center;
                dstBox.size = srcBox.size;
                dstBox.isTrigger = srcBox.isTrigger;
                break;
            case MeshCollider srcMesh when target is MeshCollider dstMesh:
                dstMesh.sharedMesh = srcMesh.sharedMesh;
                dstMesh.convex = srcMesh.convex;
                dstMesh.isTrigger = srcMesh.isTrigger;
                break;
            case SphereCollider srcSphere when target is SphereCollider dstSphere:
                dstSphere.center = srcSphere.center;
                dstSphere.radius = srcSphere.radius;
                dstSphere.isTrigger = srcSphere.isTrigger;
                break;
            case CapsuleCollider srcCapsule when target is CapsuleCollider dstCapsule:
                dstCapsule.center = srcCapsule.center;
                dstCapsule.radius = srcCapsule.radius;
                dstCapsule.height = srcCapsule.height;
                dstCapsule.direction = srcCapsule.direction;
                dstCapsule.isTrigger = srcCapsule.isTrigger;
                break;
        }
    }

    private void Reset()
    {
        EnsureInteractionCollider();
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = false;
    }
}
