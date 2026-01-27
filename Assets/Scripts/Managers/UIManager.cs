using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Behavior;
using UnityEngine;

public class UIManager
{
    int _order = 10;

    Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();
    UI_Scene _sceneUI = null;
    public GameObject Root
    {
        get
        {
			GameObject root = GameObject.Find("@UI_Root");
			if (root == null)
				root = new GameObject { name = "@UI_Root" };
            return root;
		}
    }

    public void SetCanvas(GameObject go, bool sort = true)
    {
        Canvas canvas = go.GetOrAddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;

        if (sort)
        {
            canvas.sortingOrder = _order;
            _order++;
        }
        else
        {
            canvas.sortingOrder = 0;
        }
    }

	public T MakeSubItem<T>(string addrres, Transform parent = null) where T : Component
	{
		GameObject go = Managers.Resource.Instantiate(addrres);
		if (parent != null)
			go.transform.SetParent(parent);
		else
			go.transform.SetParent(_sceneUI.transform);
		
		return go.GetOrAddComponent<T>();
	}

	public T ShowSceneUI<T>(string address=null) where T : UI_Scene
	{
		if (string.IsNullOrEmpty(address))
			address = typeof(T).Name;

		GameObject go = Managers.Resource.Instantiate($"Assets/Prefabs/UI/Scene/{address}.prefab");
		T sceneUI = go.GetOrAddComponent<T>();
        _sceneUI = sceneUI;

		go.transform.SetParent(Root.transform);

		return sceneUI;
	}

	public T LoadScene<T>(string address = null) where T : UI_Scene
	{
		// 1. 만약 이미 로드된 씬 UI가 있다면 반환
		if (_sceneUI != null && _sceneUI is T sceneUI)
			return sceneUI;

		// 2. 만약 로드된 게 없거나 타입이 다르다면 null
		return null;
	}

	public T ShowPopupUI<T>(string address = null) where T : UI_Popup
    {
        if (string.IsNullOrEmpty(address))
	        address = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"Assets/Prefabs/UI/Popup/{address}.prefab");
        T popup = go.GetOrAddComponent<T>();
        _popupStack.Push(popup);

        go.transform.SetParent(_sceneUI.transform);
        RectTransform rect = go.GetComponent<RectTransform>();
        if (rect != null)
        {
	        // 1. 앵커를 중앙으로 설정 (부모의 중앙 기준)
	        rect.anchorMin = new Vector2(0.5f, 0.5f);
	        rect.anchorMax = new Vector2(0.5f, 0.5f);
	        rect.pivot = new Vector2(0.5f, 0.5f);

	        // 2. 위치 좌표를 0,0으로 (중앙 정렬)
	        rect.anchoredPosition = Vector2.zero;
        
	        // 3. Scale이나 Z값도 필요하다면 초기화
	        rect.localScale = Vector3.zero; // 이후 DOTween으로 1.0f까지 커짐
        }
        
        go.transform.localScale = Vector3.zero;
        go.transform.DOScale(1.0f, 0.3f).SetEase(Ease.OutBack);

		return popup;
    }
	
	public void ShowFloatingText(Vector3 worldPos, string message, Color color,bool isCritical)
	{
		
		// 1. 월드 좌표를 화면 좌표로 변환 (Screen Space Overlay 기준)
		Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
		// 2. ResourceManager를 통해 생성
		var floatingText = MakeSubItem<UI_FloatingText>(Address.UI_FloatingText);
		floatingText.GetComponent<UI_FloatingText>().Init(screenPos,message, color,isCritical);
	}

    public void ClosePopupUI(UI_Popup popup)
    {
		if (_popupStack.Count == 0)
			return;

        if (_popupStack.Peek() != popup)
        {
            Debug.Log("Close Popup Failed!");
            return;
        }

        ClosePopupUI();
    }

    public void ClosePopupUI()
    {
	    if (_popupStack.Count == 0)
		    return;
	    
	    UI_Popup popup = _popupStack.Pop();
	    _order--;
	    
	    popup.transform.DOScale(0.0f, 0.2f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
	    {
		    Managers.Resource.Destroy(popup.gameObject);
		    popup = null;

	    });
    }

    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > 0)
            ClosePopupUI();
    }

    public void Clear()
    {
        CloseAllPopupUI();
        Managers.Resource.Destroy(_sceneUI.gameObject);
        _sceneUI = null;
    }
}
