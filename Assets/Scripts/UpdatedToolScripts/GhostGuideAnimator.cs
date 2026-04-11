using UnityEngine;

public class GhostGuideAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    public Transform startPositionGuide;
    public Transform endPositionGuide;
    public Transform originPoint; 
    public float moveDuration = 1f;
    public float endPauseDuration = 0.5f;
    public LeanTweenType easeType = LeanTweenType.easeOutCubic;

    private GameObject currentGhost;
    private Quaternion originalRotation;

    private Vector3 GetOffsetPosition(Vector3 targetWorldPos)
    {
        if (originPoint == null) return targetWorldPos;
        // How far the tip is from the ghost's pivot in world space
        Vector3 tipOffset = originPoint.position - currentGhost.transform.position;
        return targetWorldPos - tipOffset;
    }

    public void PlayAnimation(GameObject ghost)
    {
        if (ghost == null) return;
        currentGhost = ghost;
        originalRotation = ghost.transform.rotation;

        LeanTween.cancel(currentGhost);

        ghost.transform.position = GetOffsetPosition(startPositionGuide.position);
        ghost.transform.rotation = originalRotation;

        AnimateStep();
    }

    private void AnimateStep()
    {
        LeanTween.move(currentGhost, GetOffsetPosition(endPositionGuide.position), moveDuration)
                 .setEase(easeType)
                 .setOnComplete(() =>
                 {
                     LeanTween.delayedCall(endPauseDuration, () =>
                     {
                         if (currentGhost == null) return;

                         currentGhost.transform.position = GetOffsetPosition(startPositionGuide.position);
                         currentGhost.transform.rotation = originalRotation;
                         AnimateStep();
                     });
                 });
    }
}