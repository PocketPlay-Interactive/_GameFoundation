using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DWAnimation : MonoBehaviour
{
    public enum DWAnimtionType
    {
        Scale,
        LoopScale,
        LoopHorizontal,
        LoopVertical,
        MoveVerticalToCenter
    }

    private Vector3 _originalScale;
    private Vector3 _toScale;

    public List<DWAnimtionType> dWAnimtionTypes;

    private void Start()
    {
        PlayAnimationSequence();
    }

    private void PlayAnimationSequence()
    {
        Sequence sequence = DOTween.Sequence();

        for (int i = 0; i < dWAnimtionTypes.Count; i++)
        {
            var type = dWAnimtionTypes[i];
            sequence.AppendCallback(() =>
            {
                PlayAnimation(type, null); // callback không cần nữa, vì sequence sẽ tự nối tiếp
            });

            // Nếu cần delay giữa các animation
            sequence.AppendInterval(0.1f);
        }
    }

    private void PlayAnimation(DWAnimtionType dWAnimtionType, System.Action callBack)
    {
        switch(dWAnimtionType)
        {
            case DWAnimtionType.Scale:
                OnScale(callBack);
                break;
            case DWAnimtionType.LoopScale:
                OnLoopScale();
                break;
            case DWAnimtionType.LoopHorizontal:
                OnLoopHorizontal();
                break;
            case DWAnimtionType.LoopVertical:
                OnLoopVertical();
                break;
            case DWAnimtionType.MoveVerticalToCenter:
                OnVerticalMoveToCenter(callBack);
                break;
            default:
                callBack?.Invoke();
                break;
        }
    }

    private void OnScale(System.Action callBack)
    {
        _originalScale = transform.localScale;
        _toScale = transform.localScale * 1.2f;
        this.transform.localScale = Vector3.zero;

        this.transform.DOKill();
        this.transform.DOScale(_toScale, 0.5f)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                transform.DOScale(_originalScale, 0.3f)
                    .SetEase(Ease.OutBounce)
                    .OnComplete(() =>
                    {
                        callBack?.Invoke();
                    });
            });
    }

    private void OnLoopScale()
    {
        this.transform.DOScale(1.1f, 0.5f)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                transform.DOScale(0.9f, 0.5f)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(() =>
                    {
                        OnLoopScale();
                    });
            });
    }

    private float maxY, minY;
    private void OnLoopHorizontal(bool initialized = true)
    {
        if(initialized)
        {
            maxY = this.transform.position.y + 0.5f;
            minY = this.transform.position.y - 0.5f;
        }

        this.transform.DOMoveY(maxY, 1f)
        .SetEase(Ease.InOutSine)
        .OnComplete(() =>
        {
            transform.DOMoveY(minY, 1f)
                .SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    OnLoopHorizontal(false);
                });
        });
    }

    private float maxX, minX;
    private void OnLoopVertical(bool initialized = true)
    {
        if (initialized)
        {
            maxX = this.transform.position.x + 0.5f;
            minX = this.transform.position.x - 0.5f;
        }

        this.transform.DOMoveX(maxX, 1f)
        .SetEase(Ease.InOutSine)
        .OnComplete(() =>
        {
            transform.DOMoveX(minX, 1f)
                .SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    OnLoopVertical(false);
                });
        });
    }

    private void OnVerticalMoveToCenter(System.Action callBack)
    {
        this.transform.DOLocalMoveX(0, 2f).OnComplete(() => callBack?.Invoke());
    }
}
