using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class UI_TutorialFinger : UI_Base
{
    enum Images { Image_Hand }
    protected override void Awake()
    {
        base.Awake();
        //BindImages(typeof(Images));
        PlayTap();
    }
    // 1. 톡톡 두드리는 애니메이션 (클릭 유도)
    public void PlayTap(Vector3 position)
    {
        transform.position = position;
        gameObject.SetActive(true);
        transform.localScale = Vector3.one;
        transform.DOScale(0.8f, 0.4f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutQuad);
    }

    // 1. 톡톡 두드리는 애니메이션 (클릭 유도)
    public void PlayTap()
    {
        gameObject.SetActive(true);
        //transform.localScale = Vector3.one;
        transform.DOScale(0.8f * transform.localScale.x, 0.4f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutQuad);
    }
    // 2. 특정 위치로 이동 (드래그 유도)
    public void PlayMove(Vector3 start, Vector3 end)
    {
        transform.position = start;
        gameObject.SetActive(true);
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMove(end, 1.0f).SetEase(Ease.InOutQuad));
        seq.AppendInterval(0.2f);
        seq.Append(transform.DOMove(start, 0.1f).SetEase(Ease.InExpo)); // 순식간에 복귀
        seq.SetLoops(-1);
    }
    public void Stop()
    {
        transform.DOKill();
        gameObject.SetActive(false);
    }
}