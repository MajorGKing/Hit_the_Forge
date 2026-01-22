using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 터치/클릭 입력을 중앙에서 관리하는 매니저
/// 특정 시점에 특정 오브젝트만 상호작용 가능하도록 제어
/// </summary>
public class TouchManager
{
    // private Canvas _blockingCanvas;
    // private Image _blockingPanel;
    
    private HashSet<GameObject> _allowedObjects = new HashSet<GameObject>();
    //private Dictionary<Selectable, bool> _originalUIStates = new Dictionary<Selectable, bool>();
    
    private bool _isBlocking = false;

    public void Init()
    {
        //SetupBlockingPanel();
    }

    // private void SetupBlockingPanel()
    // {
    //     // 블로킹용 Canvas 생성
    //     GameObject canvasObj = new GameObject("BlockingCanvas");
    //     _blockingCanvas = canvasObj.AddComponent<Canvas>();
    //     _blockingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
    //     _blockingCanvas.sortingOrder = 9999; // 최상위
        
    //     canvasObj.AddComponent<GraphicRaycaster>();
    //     GameObject.DontDestroyOnLoad(canvasObj);
        
    //     // 투명 패널 생성
    //     GameObject panelObj = new GameObject("BlockingPanel");
    //     panelObj.transform.SetParent(canvasObj.transform, false);
        
    //     _blockingPanel = panelObj.AddComponent<Image>();
    //     _blockingPanel.color = new Color(0, 0, 0, 0.5f); // 반투명 검정
    //     _blockingPanel.raycastTarget = true;
        
    //     RectTransform rt = _blockingPanel.GetComponent<RectTransform>();
    //     rt.anchorMin = Vector2.zero;
    //     rt.anchorMax = Vector2.one;
    //     rt.sizeDelta = Vector2.zero;
        
    //     _blockingCanvas.gameObject.SetActive(false);
    // }

    /// <summary>
    /// 특정 오브젝트들만 활성화 (UI + 게임 오브젝트)
    /// </summary>
    public void AllowOnly(params GameObject[] objects)
    {
        // if (!_isBlocking)
        //     SaveOriginalStates();

        _isBlocking = true;
        
        _allowedObjects.Clear();
        foreach (var obj in objects)
        {
            if (obj != null)
                _allowedObjects.Add(obj);
        }
        
        Selectable[] allSelectables = GameObject.FindObjectsOfType<Selectable>(true);
        foreach (var selectable in allSelectables)
        {
            bool isAllowed = _allowedObjects.Contains(selectable.gameObject);
            selectable.interactable = isAllowed;
        }
    }

    /// <summary>
    /// 모든 제한 해제
    /// </summary>
    public void AllowAll()
    {
        _isBlocking = false;
        //RestoreOriginalStates();
        _allowedObjects.Clear();

        Selectable[] allSelectables = GameObject.FindObjectsOfType<Selectable>(true);
        foreach (var selectable in allSelectables)
        {
            selectable.interactable = true;
        }
    }

    /// <summary>
    /// 모든 것 차단 (아무것도 클릭 불가)
    /// </summary>
    public void BlockAll()
    {
        AllowOnly(); // 빈 배열 = 아무것도 허용 안 함
    }

    /// <summary>
    /// 특정 오브젝트가 현재 허용되었는지 확인
    /// </summary>
    public bool IsObjectAllowed(GameObject obj)
    {
        if (!_isBlocking)
            return true;
        
        return _allowedObjects.Contains(obj);
    }

    // /// <summary>
    // /// 블로킹 패널 투명도 설정 (0~1)
    // /// </summary>
    // public void SetBlockingAlpha(float alpha)
    // {
    //     if (_blockingPanel != null)
    //     {
    //         Color color = _blockingPanel.color;
    //         color.a = Mathf.Clamp01(alpha);
    //         _blockingPanel.color = color;
    //     }
    // }

    // /// <summary>
    // /// 블로킹 패널 색상 설정
    // /// </summary>
    // public void SetBlockingColor(Color color)
    // {
    //     if (_blockingPanel != null)
    //     {
    //         _blockingPanel.color = color;
    //     }
    // }

    /// <summary>
    /// 현재 블로킹 상태인지 확인
    /// </summary>
    public bool IsBlocking => _isBlocking;

    // private void SaveOriginalStates()
    // {
    //     _originalUIStates.Clear();
        
    //     // 모든 UI 요소 상태 저장
    //     Selectable[] allSelectables = GameObject.FindObjectsOfType<Selectable>(true);
    //     foreach (var selectable in allSelectables)
    //     {
    //         _originalUIStates[selectable] = selectable.interactable;
    //     }
    // }

    // private void RestoreOriginalStates()
    // {
    //     // UI 상태 복구
    //     foreach (var kvp in _originalUIStates)
    //     {
    //         if (kvp.Key != null)
    //             kvp.Key.interactable = kvp.Value;
    //     }
        
    //     _originalUIStates.Clear();
    // }

    // private void ApplyBlocking(bool enable)
    // {
    //     if (enable)
    //     {
    //         // 모든 UI 비활성화, 허용된 것만 활성화
    //         Selectable[] allSelectables = GameObject.FindObjectsOfType<Selectable>(true);
    //         foreach (var selectable in allSelectables)
    //         {
    //             bool isAllowed = _allowedObjects.Contains(selectable.gameObject);
    //             selectable.interactable = isAllowed;
    //         }
    //     }
    //     // else는 RestoreOriginalStates()가 처리함
    // }

    public void Clear()
    {
        AllowAll();
        
        // if (_blockingCanvas != null)
        // {
        //     GameObject.Destroy(_blockingCanvas.gameObject);
        //     _blockingCanvas = null;
        //     _blockingPanel = null;
        // }
    }
}
