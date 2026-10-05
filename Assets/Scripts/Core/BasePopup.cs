using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum PopupOpenEffect { None, ScaleUp, SlideDown }
public enum PopupCloseEffect { None, ScaleDown, SlideUp }

[RequireComponent(typeof(CanvasGroup))]
public class BasePopup : MonoBehaviour
{
    [SerializeField] private PopupOpenEffect m_OpenEffect = PopupOpenEffect.ScaleUp;
    [SerializeField] private PopupCloseEffect m_CloseEffect = PopupCloseEffect.ScaleDown;
    [SerializeField] private bool m_UseDimBackground = true;
    [SerializeField] private Color m_DimColor = new Color(0f, 0f, 0f, 0.78f);
    [SerializeField] private float m_Duration = 0.25f;
    [SerializeField] private float m_SlideOffset = 600f;

    public Action OnClose;
    private Vector3 _origScale = Vector3.one;
    private Vector2 _origPos;
    private CanvasGroup _cg;
    private RectTransform _rt;
    private GameObject _dimObj;
    private bool _isClosing;



    private Color _currentDimColor;

    private void _CreateDimBackground()
    {
        if (!m_UseDimBackground || transform.parent == null) return;

        Color dimColor = m_DimColor;
        
        // 1. Tắt Image ở root của popup (nếu có) để nó không bị transform (scale/slide) cùng popup gây lộ khối đen
        var rootImage = GetComponent<Image>();
        if (rootImage != null)
        {
            if (rootImage.color.a > 0f) dimColor = rootImage.color;
            rootImage.enabled = false;
        }

        // 2. Tắt child DimBG thủ công nếu có trong prefab
        Transform childDim = transform.Find("DimBG") ?? transform.Find("Dim") ?? transform.Find("Dim_BG") ?? transform.Find("Background_Dim");
        if (childDim != null)
        {
            var childImg = childDim.GetComponent<Image>();
            if (childImg != null && childImg.color.a > 0f) dimColor = childImg.color;
            childDim.gameObject.SetActive(false);
        }

        _currentDimColor = dimColor;

        if (_dimObj != null) return;

        _dimObj = new GameObject($"{name}_DimBG", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var rt = _dimObj.GetComponent<RectTransform>();
        rt.SetParent(transform.parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        var img = _dimObj.GetComponent<Image>();
        img.color = dimColor;
        img.raycastTarget = true;

        _EnsureDimBehind();
    }

    private void _EnsureDimBehind()
    {
        if (_dimObj == null || transform.parent == null) return;
        int myIndex = transform.GetSiblingIndex();
        int dimIndex = _dimObj.transform.GetSiblingIndex();
        if (dimIndex > myIndex)
        {
            _dimObj.transform.SetSiblingIndex(myIndex);
        }
        else if (dimIndex < myIndex - 1)
        {
            _dimObj.transform.SetSiblingIndex(myIndex - 1);
        }
    }

    protected virtual void OnEnable()
    {
        if (_rt == null) _rt = GetComponent<RectTransform>();
        if (_cg == null)
        {
            _cg = GetComponent<CanvasGroup>();
            if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
        }

        _CreateDimBackground();
        _isClosing = false;
        transform.localScale = _origScale;
        if (_rt != null) _rt.anchoredPosition = _origPos;
        if (_cg != null) _cg.alpha = 1f;

        if (_dimObj != null)
        {
            _dimObj.SetActive(true);
            _EnsureDimBehind();
            if (m_OpenEffect != PopupOpenEffect.None)
            {
                StartCoroutine(AnimFadeDim(0f, _currentDimColor.a));
            }
            else
            {
                var img = _dimObj.GetComponent<Image>();
                if (img != null) img.color = _currentDimColor;
            }
        }

        switch (m_OpenEffect)
        {
            case PopupOpenEffect.ScaleUp:
                StartCoroutine(AnimScale(Vector3.zero, _origScale));
                break;
            case PopupOpenEffect.SlideDown:
                StartCoroutine(AnimSlide(_origPos + Vector2.up * m_SlideOffset, _origPos, 0f, 1f));
                break;
        }
    }

    protected virtual void OnDisable()
    {
        if (_dimObj != null)
            _dimObj.SetActive(false);
    }

    public void Close(Action onFinish = null)
    {
        if (_isClosing) return;
        _isClosing = true;
        OnClose?.Invoke();

        Action finish = onFinish ?? (() => Destroy(gameObject));

        if (_dimObj != null && m_CloseEffect != PopupCloseEffect.None)
        {
            var img = _dimObj.GetComponent<Image>();
            float currentA = img != null ? img.color.a : _currentDimColor.a;
            StartCoroutine(AnimFadeDim(currentA, 0f));
        }

        switch (m_CloseEffect)
        {
            case PopupCloseEffect.ScaleDown:
                StartCoroutine(AnimScale(_origScale, Vector3.zero, finish));
                break;
            case PopupCloseEffect.SlideUp:
                StartCoroutine(AnimSlide(_origPos, _origPos + Vector2.up * m_SlideOffset, 1f, 0f, finish));
                break;
            default:
                finish();
                break;
        }
    }

    private IEnumerator AnimFadeDim(float fromA, float toA)
    {
        if (_dimObj == null) yield break;
        var img = _dimObj.GetComponent<Image>();
        if (img == null) yield break;

        Color c = img.color;
        c.a = fromA;
        img.color = c;

        for (float t = 0; t < m_Duration; t += Time.unscaledDeltaTime)
        {
            c.a = Mathf.Lerp(fromA, toA, t / m_Duration);
            img.color = c;
            yield return null;
        }
        c.a = toA;
        img.color = c;
    }

    private IEnumerator AnimScale(Vector3 from, Vector3 to, Action onFinish = null)
    {
        for (float t = 0; t < m_Duration; t += Time.unscaledDeltaTime)
        {
            transform.localScale = Vector3.Lerp(from, to, t / m_Duration);
            yield return null;
        }
        transform.localScale = to;
        onFinish?.Invoke();
    }

    private IEnumerator AnimSlide(Vector2 fromPos, Vector2 toPos, float fromA, float toA, Action onFinish = null)
    {
        for (float t = 0; t < m_Duration; t += Time.unscaledDeltaTime)
        {
            float p = t / m_Duration;
            if (_rt != null) _rt.anchoredPosition = Vector2.Lerp(fromPos, toPos, p);
            if (_cg != null) _cg.alpha = Mathf.Lerp(fromA, toA, p);
            yield return null;
        }
        if (_rt != null) _rt.anchoredPosition = toPos;
        if (_cg != null) _cg.alpha = toA;
        onFinish?.Invoke();
    }

    
    protected virtual void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _cg = GetComponent<CanvasGroup>();
        if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
        _origScale = transform.localScale;
        _origPos = _rt != null ? _rt.anchoredPosition : Vector2.zero;
        if (_rt != null) 
        {
            Vector3 pos = _rt.localPosition;
            pos.z = 0;
            _rt.localPosition = pos;
        }

        _CreateDimBackground();
    }

    protected virtual void OnDestroy()
    {
        if (_dimObj != null)
            Destroy(_dimObj);

        if (UIManager.INSTANCE != null && UIManager.INSTANCE.m_ScalingSUIC != null)
            UIManager.INSTANCE.m_ScalingSUIC.CheckClearUnusedItems();
    }
}
