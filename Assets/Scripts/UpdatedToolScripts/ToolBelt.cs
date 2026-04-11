using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;
using System.Collections.Generic;

public class ToolBelt : MonoBehaviour
{
    [System.Serializable]
    public class ToolSlot
    {
        public Transform anchor;
        public Transform attachPoint;
        public Transform assignedTool;
        public GameObject ghostPrefab;
        [HideInInspector] public Transform ownedTool;
        [HideInInspector] public Vector3 storedLocalScale;
        [HideInInspector] public Vector3 storedLocalPosition;
        [HideInInspector] public GameObject ghostInstance;
    }

    [Header("Slots")]
    public ToolSlot[] slots;

    [Header("Settings")]
    public float snapDistance = 0.2f;

    [Header("Auto Setup")]
    public bool autoSetupSlots = false;
    public float slotSpacing = 0.3f;
    public GameObject defaultGhostPrefab;

    public enum SlotAxis { X, Y, Z }
    public SlotAxis slotAxis = SlotAxis.X;

    private Vector3 Axis => slotAxis switch
    {
        SlotAxis.Y => Vector3.up,
        SlotAxis.Z => Vector3.forward,
        _ => Vector3.right
    };

    void Start()
    {
        if (autoSetupSlots)
            AutoSetupSlots();

        foreach (var slot in slots)
        {
            if (slot.assignedTool != null)
            {
                slot.ownedTool = slot.assignedTool;
                DockTool(slot, slot.assignedTool);
            }
        }
    }

    void Update()
    {
        foreach (var slot in slots)
        {
            if (slot.assignedTool != null) continue;

            foreach (var tool in GetUndockedTools())
            {
                if (tool != slot.ownedTool) continue;

                if (Vector3.Distance(tool.position, slot.anchor.position) <= snapDistance)
                {
                    DockTool(slot, tool);
                    break;
                }
            }
        }
    }

    public void RespawnTool(Transform tool)
    {
        foreach (var slot in slots)
        {
            if (slot.assignedTool != null) continue;

            var rb = tool.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            DockTool(slot, tool);
            return;
        }
    }

    void AutoSetupSlots()
    {
        var beltTools = new List<ToolActionController>();
        foreach (var t in FindObjectsByType<ToolActionController>(FindObjectsSortMode.None))
            if (t.useToolBelt) beltTools.Add(t);

        slots = new ToolSlot[beltTools.Count];

        for (int i = 0; i < beltTools.Count; i++)
        {
            GameObject anchor = new GameObject($"Slot_{beltTools[i].name}");
            anchor.transform.SetParent(transform);
            anchor.transform.localRotation = Quaternion.identity;
            anchor.transform.localScale = Vector3.one;

            float offset = (i - (beltTools.Count - 1) / 2f) * slotSpacing;
            anchor.transform.localPosition = Axis * offset;

            slots[i] = new ToolSlot
            {
                anchor = anchor.transform,
                assignedTool = beltTools[i].transform,
                ownedTool = beltTools[i].transform,
                ghostPrefab = defaultGhostPrefab
            };
        }
    }

    List<Transform> GetUndockedTools()
    {
        var tools = new List<Transform>();
        foreach (var controller in FindObjectsByType<ToolActionController>(FindObjectsSortMode.None))
        {
            if (!controller.useToolBelt || controller.IsDocked) continue;

            var grab = controller.GetComponent<XRGrabInteractable>();
            if (grab != null && grab.isSelected) continue;

            tools.Add(controller.transform);
        }
        return tools;
    }

    void DockTool(ToolSlot slot, Transform tool)
    {
        DestroyGhost(slot);

        slot.storedLocalScale = tool.localScale;
        slot.assignedTool = tool;

        tool.SetParent(slot.anchor);
        tool.localRotation = Quaternion.identity;
        tool.localScale = slot.storedLocalScale;

        if (slot.attachPoint != null)
        {
            Vector3 offset = tool.position - slot.attachPoint.position;
            tool.position = slot.anchor.position + offset;
        }
        else
            tool.localPosition = Vector3.zero;

        slot.storedLocalPosition = tool.localPosition;

        var controller = tool.GetComponent<ToolActionController>();
        if (controller != null) controller.IsDocked = true;

        var grab = tool.GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            grab.selectEntered.RemoveListener(OnToolGrabbed);
            grab.selectEntered.AddListener(OnToolGrabbed);
            grab.selectExited.RemoveListener(OnToolReleased);
            grab.selectExited.AddListener(OnToolReleased);
        }

        StartCoroutine(FreezePhysics(tool));
    }

    IEnumerator FreezePhysics(Transform tool)
    {
        yield return new WaitForEndOfFrame();
        var rb = tool.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void UndockTool(ToolSlot slot)
    {
        if (slot.assignedTool == null) return;

        var tool = slot.assignedTool;

        tool.SetParent(null);
        tool.localScale = slot.storedLocalScale;

        var controller = tool.GetComponent<ToolActionController>();
        if (controller != null) controller.IsDocked = false;

        slot.assignedTool = null;

        SpawnGhost(slot);
    }

    void SpawnGhost(ToolSlot slot)
    {
        if (slot.ghostPrefab == null) return;

        Vector3 prefabWorldScale = slot.ghostPrefab.transform.lossyScale;

        slot.ghostInstance = Instantiate(slot.ghostPrefab, slot.anchor);
        slot.ghostInstance.transform.localRotation = Quaternion.identity;

        Vector3 anchorLossyScale = slot.anchor.lossyScale;
        slot.ghostInstance.transform.localScale = new Vector3(
            prefabWorldScale.x / anchorLossyScale.x,
            prefabWorldScale.y / anchorLossyScale.y,
            prefabWorldScale.z / anchorLossyScale.z
        );

        Transform ghostAttach = slot.ghostInstance.transform.Find("GhostAttachPoint");
        if (ghostAttach != null)
        {
            Vector3 attachOffset = slot.ghostInstance.transform.position - ghostAttach.position;
            slot.ghostInstance.transform.position = slot.anchor.position + attachOffset;
        }
        else
        {
            slot.ghostInstance.transform.localPosition = slot.storedLocalPosition;
        }
    }

    void DestroyGhost(ToolSlot slot)
    {
        if (slot.ghostInstance != null)
        {
            Destroy(slot.ghostInstance);
            slot.ghostInstance = null;
        }
    }

    void OnToolGrabbed(SelectEnterEventArgs args)
    {
        Transform grabbed = args.interactableObject.transform;
        foreach (var slot in slots)
        {
            if (slot.assignedTool == grabbed)
            {
                UndockTool(slot);
                return;
            }
        }
    }

    void OnToolReleased(SelectExitEventArgs args)
    {
        Transform released = args.interactableObject.transform;
        StartCoroutine(RestorePhysics(released));
    }

    IEnumerator RestorePhysics(Transform tool)
    {
        yield return new WaitForEndOfFrame();
        var rb = tool.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }
}