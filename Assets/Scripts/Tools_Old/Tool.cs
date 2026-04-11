using UnityEngine;

public abstract class Tool : MonoBehaviour
{
    protected void TryApply<T>(Collider other) where T : class
    {
        T target = other.GetComponent<T>();
        if (target != null)
        {
            OnAction(target);
        }
    }

    protected abstract void OnAction<T>(T target) where T : class;
}
