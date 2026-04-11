using UnityEngine;

public class FragmentPlacementZone : MonoBehaviour
{
    public ToolActionController toolController;
    public GameObject[] fragments;
    private int placedCount;

    private void OnTriggerEnter(Collider other)
    {
        for (int i = 0; i < fragments.Length; i++)
        {
            if (fragments[i] == other.gameObject)
            {
                placedCount++;
                if (placedCount >= fragments.Length)
                    toolController.MarkActionCompleted();
                return;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        for (int i = 0; i < fragments.Length; i++)
        {
            if (fragments[i] == other.gameObject)
            {
                placedCount--;
                return;
            }
        }
    }
}