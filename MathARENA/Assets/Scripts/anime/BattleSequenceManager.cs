using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BattleSequenceManager : MonoBehaviour
{
    [Header("UI Icons")]
    [SerializeField]
    private GameObject iconO;

    [SerializeField]
    private GameObject iconX;

    [Header("UI Image References")]
    [SerializeField]
    private Image heroDisplay;

    [SerializeField]
    private Image enemyDisplay;

    [Header("Individual Scale Settings")]
    [SerializeField]
    private float heroAttackScale = 1.2f;

    [SerializeField]
    private float enemyAttackScale = 1.5f;

    // --- 피격 스케일 설정을 새로 추가했습니다. (기본값 1.0) ---
    [SerializeField]
    private float heroHitScale = 1.0f;

    [SerializeField]
    private float enemyHitScale = 1.0f;

    [Header("Hero Sprites")]
    [SerializeField]
    private Sprite heroDefault;

    [SerializeField]
    private Sprite heroAttack;

    [SerializeField]
    private Sprite heroHit;

    [Header("Enemy Sprites")]
    [SerializeField]
    private Sprite enemyDefault;

    [SerializeField]
    private Sprite enemyAttack;

    [SerializeField]
    private Sprite enemyHit;

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip correctSFX;

    [SerializeField]
    private AudioClip wrongSFX;

    [SerializeField]
    private AudioClip hitSFX;

    public System.Action OnSequenceComplete;

    public void PlaySequence(bool isCorrect)
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(SequenceRoutine(isCorrect));
        }
    }

    private IEnumerator SequenceRoutine(bool isCorrect)
    {
        // 1. 아이콘 연출
        GameObject activeIcon = isCorrect ? iconO : iconX;
        if (activeIcon != null)
        {
            activeIcon.SetActive(true);
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(isCorrect ? correctSFX : wrongSFX);
        }

        yield return new WaitForSeconds(1.0f);
        if (activeIcon != null)
            activeIcon.SetActive(false);

        // 2. 타격/피격 연출 (스케일 적용 부분 수정)
        if (isCorrect)
        {
            // [정답] 용사 공격 + 적 피격

            // --- 용사 공격 이미지 및 스케일 적용 ---
            heroDisplay.sprite = heroAttack;
            heroDisplay.transform.localScale = Vector3.one * heroAttackScale;

            // --- 적 피격 이미지 및 스케일 적용 ---
            enemyDisplay.sprite = enemyHit;
            enemyDisplay.transform.localScale = Vector3.one * enemyHitScale; // 피격 스케일 적용

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(hitSFX);
            yield return new WaitForSeconds(0.4f);

            // --- 원래 상태로 복구 ---
            heroDisplay.sprite = heroDefault;
            heroDisplay.transform.localScale = Vector3.one;

            enemyDisplay.sprite = enemyDefault;
            enemyDisplay.transform.localScale = Vector3.one; // 복구
        }
        else
        {
            // [오답] 적 공격 + 용사 피격

            // --- 적 공격 이미지 및 스케일 적용 ---
            enemyDisplay.sprite = enemyAttack;
            enemyDisplay.transform.localScale = Vector3.one * enemyAttackScale;

            // --- 용사 피격 이미지 및 스케일 적용 ---
            heroDisplay.sprite = heroHit;
            heroDisplay.transform.localScale = Vector3.one * heroHitScale; // 피격 스케일 적용

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(hitSFX);
            yield return new WaitForSeconds(0.4f);

            // --- 원래 상태로 복구 ---
            enemyDisplay.sprite = enemyDefault;
            enemyDisplay.transform.localScale = Vector3.one;

            heroDisplay.sprite = heroDefault;
            heroDisplay.transform.localScale = Vector3.one; // 복구
        }

        yield return new WaitForSeconds(0.2f);
        OnSequenceComplete?.Invoke();
    }
}
