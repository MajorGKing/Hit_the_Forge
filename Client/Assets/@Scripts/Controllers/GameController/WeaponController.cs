using DG.Tweening;
using System.Collections;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private GameScene gameScene;

    public void Init(GameScene scene)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameScene = scene;

        spriteRenderer.material.SetFloat("_FillAmount", 0);
    }

    private void OnEnable()
    {
        Managers.Game.OnWeaponHpChanged -= UpdateFill;
        Managers.Game.OnWeaponHpChanged += UpdateFill;
        Managers.Game.OnWeaponEnhancementSucess -= EnhancemenetSucess;
        Managers.Game.OnWeaponEnhancementSucess += EnhancemenetSucess;
        Managers.Game.OnWeaponEnhancementFail -= EnhancementFail;
        Managers.Game.OnWeaponEnhancementFail += EnhancementFail;
        Managers.Game.OnWeaponFinish -= OnWeaponFinish;
        Managers.Game.OnWeaponFinish += OnWeaponFinish;
    }

    private void OnDisable()
    {
        Managers.Game.OnWeaponHpChanged -= UpdateFill;
        Managers.Game.OnWeaponEnhancementSucess -= EnhancemenetSucess;
        Managers.Game.OnWeaponEnhancementFail -= EnhancementFail;
        Managers.Game.OnWeaponFinish -= OnWeaponFinish;
    }

    private void OnWeaponFinish()
    {
        transform.DORotate(new Vector3(0, 0, 360), 0.15f, RotateMode.FastBeyond360)
            .SetEase(Ease.OutBack);
    }

    public void UpdateFill()
    {
        float amount = ((float)Managers.Game.WeaponHp / (float)Managers.Game.WeaponMaxHp);
        spriteRenderer.material.SetFloat("_FillAmount", amount);
    }

    public void EnhancemenetSucess()
    {
        var mat = spriteRenderer.material;

        // TODO ILHAK 레벨에 따른 Flash색 변경 필요
        int level = Managers.Game.GetEnhancementLevel();

        mat.SetFloat("_FlashIntensity", 1f);

        // Flash 색 강제로 흰색으로 초기화
        mat.SetColor("_FlashColor", Color.white);

        // 점점 1 → 0으로 감소시키기
        DOTween.To(
            () => mat.GetFloat("_FlashIntensity"),
            x => mat.SetFloat("_FlashIntensity", x),
            0f,
            0.15f
        );
    }

    public void EnhancementFail()
    {
        Material mat = spriteRenderer.material;

        // 붉은 플래시
        DOTween.To(() => 0f, x =>
        {
            mat.SetColor("_FlashColor", Color.red);
            mat.SetFloat("_FlashIntensity", x);
        }, 1f, 0.1f)
        .OnComplete(() =>
        {
            DOTween.To(() => 1f, x =>
            {
                mat.SetFloat("_FlashIntensity", x);
            }, 0f, 0.2f);
        });

        // 균열 효과
        DOTween.To(() => 0f, x =>
        {
            mat.SetFloat("_CrackIntensity", x);
        }, 1f, 0.25f)
        .OnComplete(() =>
        {
            DOTween.To(() => 1f, x =>
            {
                mat.SetFloat("_CrackIntensity", x);
            }, 0f, 0.5f);
        });

    }
}
