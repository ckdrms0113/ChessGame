using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    public CanvasGroup cutsceneGroup;
    public Image cutsceneImage;

    // 문양별 컷씬 이펙트 프리팹
    public GameObject spadeEffectPrefab;
    public GameObject heartEffectPrefab;
    public GameObject diaEffectPrefab;
    public GameObject clubEffectPrefab;

    private GameObject currentEffectInstance;
    private Dictionary<string, GameObject> effectPrefabs;

    private void Awake()
    {
        // 문양 프리팹 초기화
        effectPrefabs = new Dictionary<string, GameObject>()
        {
            { "Spade", spadeEffectPrefab },
            { "Heart", heartEffectPrefab },
            { "Dia", diaEffectPrefab },
            { "Club", clubEffectPrefab }
        };
    }

    // ✅ 콜백 지원 PlayCutscene
    public void PlayCutscene(Sprite sprite, string emblem, System.Action onComplete = null)
    {
        Debug.Log($"[컷씬] 재생 시작: {sprite?.name}, 문양: {emblem}");
        StopAllCoroutines();
        StartCoroutine(CutsceneRoutine(sprite, emblem, onComplete));
    }

    // ✅ 콜백을 포함한 컷씬 루틴
    private IEnumerator CutsceneRoutine(Sprite sprite, string emblem, System.Action onComplete = null)
    {
        cutsceneImage.sprite = sprite;
        cutsceneGroup.alpha = 1f;
        cutsceneGroup.blocksRaycasts = true;

        // 문양 이펙트 생성
        if (effectPrefabs.ContainsKey(emblem) && effectPrefabs[emblem] != null)
        {
            currentEffectInstance = Instantiate(effectPrefabs[emblem], cutsceneImage.transform.parent);
            currentEffectInstance.transform.SetAsFirstSibling(); // 이미지 뒤로

            // ✅ 레이어 설정
            SetLayerRecursive(currentEffectInstance, "CutsceneEffect");
        }

        // 스케일 연출
        cutsceneImage.transform.localScale = Vector3.one * 1.2f;
        float scaleTime = 0.15f;
        float t = 0f;
        while (t < scaleTime)
        {
            t += Time.deltaTime;
            float scale = Mathf.Lerp(1.2f, 1.0f, t / scaleTime);
            cutsceneImage.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        yield return new WaitForSeconds(0.6f);

        // 페이드 아웃
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

        if (currentEffectInstance != null)
        {
            Destroy(currentEffectInstance);
        }

        // ✅ 컷씬 종료 후 콜백 실행
        onComplete?.Invoke();
    }

    private void SetLayerRecursive(GameObject obj, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
        {
            Debug.LogWarning($"❗ 레이어 '{layerName}' 가 존재하지 않습니다. Unity Editor에서 먼저 생성하세요.");
            return;
        }

        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layerName);
        }
    }
}
