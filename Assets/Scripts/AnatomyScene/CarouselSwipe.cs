using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class CarouselSwipe : MonoBehaviour
{
    [Header("Carousel Settings")]
    public Transform container;
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor handInteractor;
    public InputActionProperty grabAction;
    public float swipeSpeed = 2f;
    public float minScroll = -5f;
    public float maxScroll = 5f;

    public enum SwipeAxis { X, Y, Z }
    [Header("Swipe Settings")]
    public SwipeAxis swipeAxis = SwipeAxis.X;

    [Header("Slots & Items")]
    public Transform[] slots;
    public Transform[] items;

    [Header("Boundary Fader")]
    public bool useBoundaryFader = true;
    public BoundaryFader fadeBoundary;

    [Header("Tool Mode")]
    public bool isToolCarousel = false;
    public Vector3 toolDisplayScale = new Vector3(0.1f, 0.1f, 0.1f);

    [Header("Static Mode")]
    public bool staticMode = false;

    [Header("Auto Slots")]
    public bool autoGenerateSlots = false;
    public int slotCount = 5;
    public float slotSpacing = 0.3f;

    private Vector3 lastHandPos;
    private int[] itemSlotIndex;
    private Vector3[] originalScales;

    private Vector3 Axis => swipeAxis switch
    {
        SwipeAxis.Y => Vector3.up,
        SwipeAxis.Z => Vector3.forward,
        _ => Vector3.right
    };

    void Start()
    {
        if (autoGenerateSlots)
            GenerateSlots();

        if (!staticMode)
            lastHandPos = handInteractor != null ? handInteractor.transform.position : Vector3.zero;

        itemSlotIndex = new int[items.Length];
        originalScales = new Vector3[items.Length];

        foreach (Transform slot in slots)
            slot.SetParent(container, true);

        for (int i = 0; i < items.Length && i < slots.Length; i++)
        {
            items[i].SetParent(container, true);
            items[i].position = slots[i].position;
            items[i].rotation = slots[i].rotation;
            itemSlotIndex[i] = i;

            if (isToolCarousel)
            {
                items[i].localScale = toolDisplayScale;
                originalScales[i] = toolDisplayScale;
                SetPhysics(items[i], kinematic: true);
            }
            else
            {
                originalScales[i] = items[i].localScale;
            }

            UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab = items[i].GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grab != null)
            {
                grab.selectEntered.AddListener(OnGrabbed);
                grab.selectExited.AddListener(OnReleased);
            }
        }
    }

    void Update()
    {
        if (staticMode) return;
        if (handInteractor == null) return;

        if (handInteractor.firstInteractableSelected != null)
        {
            lastHandPos = handInteractor.transform.position;
            return;
        }

        if (grabAction.action.ReadValue<float>() > 0.5f)
        {
            Vector3 delta = handInteractor.transform.position - lastHandPos;
            float scroll = Vector3.Dot(delta, Axis) * swipeSpeed;
            float current = Vector3.Dot(container.position, Axis);
            float clamped = Mathf.Clamp(current + scroll, minScroll, maxScroll);
            container.position += Axis * (clamped - current);
        }

        lastHandPos = handInteractor.transform.position;
    }

    void GenerateSlots()
    {
        slots = new Transform[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slot = new GameObject($"Slot_{i}");
            slot.transform.SetParent(container);
            slot.transform.localRotation = Quaternion.identity;
            slot.transform.localScale = Vector3.one;

            float offset = (i - (slotCount - 1) / 2f) * slotSpacing;
            slot.transform.localPosition = Axis * offset;

            slots[i] = slot.transform;
        }
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        Transform grabbedItem = args.interactableObject.transform;

        if (isToolCarousel)
            SetPhysics(grabbedItem, kinematic: false);

        grabbedItem.SetParent(null);

        if (!staticMode && useBoundaryFader && fadeBoundary != null)
            fadeBoundary.RemoveFromFade(grabbedItem);
    }

    void OnReleased(SelectExitEventArgs args)
    {
        SnapToOriginalSlot(args.interactableObject.transform);
    }

    void SnapToOriginalSlot(Transform item)
    {
        int itemIndex = System.Array.IndexOf(items, item);
        if (itemIndex == -1) return;

        int originalSlot = itemSlotIndex[itemIndex];
        float distanceToSlot = Vector3.Distance(item.position, slots[originalSlot].position);

        if (distanceToSlot <= 0.5f)
        {
            item.SetParent(container);
            item.position = slots[originalSlot].position;
            item.rotation = slots[originalSlot].rotation;

            if (isToolCarousel)
            {
                item.localScale = toolDisplayScale;
                SetPhysics(item, kinematic: true);
            }
            else
            {
                item.localScale = originalScales[originalSlot];
            }

            if (!staticMode && useBoundaryFader && fadeBoundary != null)
                fadeBoundary.AddToFade(item);
        }
        else
        {
            item.SetParent(null);

            if (!staticMode && useBoundaryFader && fadeBoundary != null)
                fadeBoundary.RemoveFromFade(item);
        }
    }

    void SetPhysics(Transform t, bool kinematic)
    {
        var rb = t.GetComponent<Rigidbody>();
        if (rb == null) return;
        rb.isKinematic = kinematic;
        rb.useGravity = !kinematic;
    }
}