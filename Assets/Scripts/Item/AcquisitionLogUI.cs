using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class AcquisitionLogUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image itemIcon;
    public TextMeshProUGUI logText;

    [Header("Fade Settings")]
    public CanvasGroup canvasGroup;
    public float displayTime = 2.0f; // 화면에 머무는 시간
    public float fadeTime = 1.0f;    // 서서히 사라지는 시간

    public void Setup(ItemDataBase itemData, int amount)
    {
        // 1. 아이콘과 텍스트 세팅
        if (itemIcon != null && itemData.icon != null)
        {
            itemIcon.sprite = itemData.icon;
        }

        if (logText != null)
        {
            logText.text = $"{itemData.itemName} <color=#FFD700>x{amount}</color>";
        }

        // 2. 투명도를 조절할 캔버스 그룹 초기화
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;

        // 3. 서서히 사라지는 코루틴 실행
        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        // 지정된 시간(displayTime)만큼 대기
        yield return new WaitForSeconds(displayTime);

        float elapsedTime = 0f;

        // fadeTime 동안 투명도(alpha)를 1에서 0으로 깎아냄
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
            }
            yield return null; // 다음 프레임까지 대기
        }

        // 완전히 투명해지면 오브젝트 파괴
        Destroy(gameObject);
    }
}