using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class MergeAnimationController : MonoBehaviour
{
    [Header("Merge Movement")]
    [SerializeField] private float mergeMoveDuration = 0.22f;
    [SerializeField] private float mergeShrinkScale = 0.05f;
    [SerializeField] private Ease mergeMoveEase = Ease.InBack;
    [SerializeField] private Ease mergeScaleEase = Ease.InBack;
    [SerializeField] private bool rotateDuringMerge = true;
    [SerializeField] private float mergeRotationAmount = 180f;

    [Header("New Object Pop")]
    [SerializeField] private float newObjectPopDuration = 0.16f;
    [SerializeField] private float newObjectSettleDuration = 0.08f;
    [SerializeField] private float popOvershootScale = 1.15f;
    [SerializeField] private Ease popEase = Ease.OutBack;
    [SerializeField] private Ease settleEase = Ease.OutQuad;

    [Header("VFX")]
    [SerializeField] private GameObject mergeVFXPrefab;
    [SerializeField] private float mergeVFXLifetime = 2f;

    [Header("SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip mergeSoundClip;
    [SerializeField] private bool randomizePitch = true;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.08f);

    public Tween PlayGatherAnimation(IReadOnlyList<MergeableObject> mergeObjects, Vector3 mergeCenterWorldPos, Action onComplete)
    {
        if (mergeObjects == null || mergeObjects.Count == 0)
        {
            onComplete?.Invoke();
            return null;
        }

        Sequence sequence = DOTween.Sequence();
        bool hasTween = false;

        for (int i = 0; i < mergeObjects.Count; i++)
        {
            MergeableObject obj = mergeObjects[i];

            if (obj == null)
            {
                continue;
            }

            Transform objTransform = obj.transform;

            objTransform.DOKill();

            Vector3 targetScale = objTransform.localScale * mergeShrinkScale;

            sequence.Join(
                objTransform.DOMove(mergeCenterWorldPos, mergeMoveDuration)
                .SetEase(mergeMoveEase)
            );

            sequence.Join(
                objTransform.DOScale(targetScale, mergeMoveDuration)
                .SetEase(mergeScaleEase)
            );

            if (rotateDuringMerge)
            {
                sequence.Join(
                    objTransform
                        .DORotate(
                            new Vector3(0f, mergeRotationAmount, 0f),
                            mergeMoveDuration,
                            RotateMode.LocalAxisAdd)
                        .SetEase(Ease.InOutSine)
                );
            }

            hasTween = true;
        }

        if (!hasTween)
        {
            sequence.Kill();
            onComplete?.Invoke();
            return null;
        }

        bool callbackInvoked = false;

        sequence.OnComplete(() =>
        {
            PlayMergeFeedback(mergeCenterWorldPos);
            InvokeOnce();
        });

        sequence.OnKill(InvokeOnce);

        return sequence;

        void InvokeOnce()
        {
            if (callbackInvoked)
            {
                return;
            }

            callbackInvoked = true;
            onComplete?.Invoke();
        }
    }

    public Tween PlayNewObjectPopAnimation(Transform newObjectTransform, Action onComplete)
    {
        if (newObjectTransform == null)
        {
            onComplete?.Invoke();
            return null;
        }
        Vector3 finalScale = newObjectTransform.localScale;
        Vector3 overshootScale = finalScale * popOvershootScale;

        newObjectTransform.localScale = Vector3.zero;
        Sequence sequence = DOTween.Sequence();

        sequence.Append(
           newObjectTransform
               .DOScale(overshootScale, newObjectPopDuration)
               .SetEase(popEase)
       );

        sequence.Append(
           newObjectTransform
               .DOScale(finalScale, newObjectSettleDuration)
               .SetEase(settleEase)
       );

        bool callbackInvoked = false;

        sequence.OnComplete(InvokeOnce);
        sequence.OnKill(InvokeOnce);

        return sequence;

        void InvokeOnce()
        {
            if (callbackInvoked)
            {
                return;
            }

            callbackInvoked = true;
            onComplete?.Invoke();
        }
    }

    private void PlayMergeFeedback(Vector3 position)
    {
        SpawnMergeVFX(position);
        PlayMergeSFX();
    }

    private void SpawnMergeVFX(Vector3 position)
    {
        if (mergeVFXPrefab == null)
        {
            return;
        }
        GameObject vfx = Instantiate(mergeVFXPrefab, position, Quaternion.identity);
        Destroy(vfx, mergeVFXLifetime);
    }

    private void PlayMergeSFX()
    {
        if (audioSource == null || mergeSoundClip == null)
        {
            return;
        }
        float originalPitch = audioSource.pitch;

        if (randomizePitch)
        {
            audioSource.pitch = UnityEngine.Random.Range(pitchRange.x, pitchRange.y);
        }

        audioSource.PlayOneShot(mergeSoundClip);
        audioSource.pitch = originalPitch;
    }
}
