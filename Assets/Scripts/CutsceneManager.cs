using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    public CanvasGroup cutsceneGroup;
    public Image cutsceneImage;

    public void PlayCutscene(Sprite sprite)
    {
        StopAllCoroutines(); // 이전 컷씬 중단
        StartCoroutine(CutsceneRoutine(sprite));
    }

    private IEnumerator CutsceneRoutine(Sprite sprite)
    {
        cutsceneImage.sprite = sprite;
        cutsceneGroup.alpha = 1f;          // 바로 나타남
        cutsceneGroup.blocksRaycasts = true;

        // 스케일 효과 (임팩트용)
        cutsceneImage.transform.localScale = Vector3.one * 1.2f; // 살짝 커졌다가 줄어듦
        float scaleTime = 0.15f;
        float t = 0f;
        while (t < scaleTime)
        {
            t += Time.deltaTime;
            float scale = Mathf.Lerp(1.2f, 1.0f, t / scaleTime);
            cutsceneImage.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        yield return new WaitForSeconds(0.6f); // 유지 시간 (짧게)

        // 빠르게 사라짐
        float fadeOutDuration = 0.15f;
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            cutsceneGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeOutDuration);
            yield return null;
        }

        cutsceneGroup.alpha = 0f;
        cutsceneGroup.blocksRaycasts = false;
    }
}
