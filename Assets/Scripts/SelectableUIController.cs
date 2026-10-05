using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SelectableUIController : MonoBehaviour
{
    private enum EFFECT { SCALING, }
    [SerializeField] private EFFECT m_EffectType = EFFECT.SCALING;
    [SerializeField] private List<Selectable> m_DataSs = new();
    [SerializeField] private AudioSource m_SoundAS;
    [SerializeField] private AudioClip m_ClickAC, m_HoverAC;
    [Min(0)][SerializeField] private float m_SelectedScale = 1.1f, m_ScaleDuration = .25f;
    [SerializeField] private bool m_IsClickSound = true, m_IsHoverSound = true, m_IsClickEffect = true, m_IsHoverEffect = true;
    private Dictionary<Selectable, Vector3> _OriginalScalesD = new();
    private Selectable _ClickedS;
    private TweenCallback _OnClickScaleDownCb;
    private Tween _ScaleUpT, _ScaleDownT;

    public void AddASelectable(Selectable _aS)
    {
        _AddSelectionListener(_aS);
        _OriginalScalesD.Add(_aS, _aS.transform.localScale);
    }
    public void CheckClearUnusedItems()
    {
        Dictionary<Selectable, Vector3> tempD = new();
        foreach (KeyValuePair<Selectable, Vector3> aKVP in _OriginalScalesD)
        {
            if (aKVP.Key == null) continue;
            tempD[aKVP.Key] = aKVP.Value;
        }
        _OriginalScalesD = tempD;
    }

    private void _TryClickSound() { if (m_IsClickSound) m_SoundAS.PlayOneShot(m_ClickAC); }
    private void _TryHoverSound() { if (m_IsHoverSound) m_SoundAS.PlayOneShot(m_HoverAC); }
    private void _ScaleUp(Selectable _aS, TweenCallback _onCompleteCb = null)
    {
        switch (m_EffectType)
        {
            case EFFECT.SCALING:
                {
                    Vector3 maxScaleUpV3 = m_SelectedScale * m_SelectedScale * _OriginalScalesD[_aS];
                    _ScaleUpT = _aS.transform.DOScale(Vector3.Min(m_SelectedScale * _aS.transform.localScale, maxScaleUpV3), m_ScaleDuration);
                    if (_onCompleteCb != null) _ScaleUpT.OnComplete(_onCompleteCb);
                    break;
                }
        }
    }
    private void _ScaleDown(Selectable _aS)
    {
        switch (m_EffectType)
        {
            case EFFECT.SCALING:
                {
                    if (_ScaleDownT.IsActive() && _ScaleDownT.IsPlaying()) return;
                    _ScaleDownT = _aS.transform.DOScale(_OriginalScalesD[_aS], m_ScaleDuration);
                    break;
                }
        }
    }
    private void _OnPointerClick(BaseEventData _aBED, Selectable _aS)
    {
        switch (m_EffectType)
        {
            case EFFECT.SCALING:
                {
                    _TryClickSound();
                    if (m_IsClickEffect) { _ClickedS = _aS; _ScaleUp(_aS, _OnClickScaleDownCb); }
                    break;
                }
        }
    }
    private void _OnPointerEnter(BaseEventData _aBED, Selectable _aS)
    {
        switch (m_EffectType)
        {
            case EFFECT.SCALING:
                {
                    _TryHoverSound();
                    if (m_IsHoverEffect) _ScaleUp(_aS);
                    break;
                }
        }
    }
    private void _OnPointerExit(BaseEventData _aBED, Selectable _aS)
    {
        switch (m_EffectType)
        {
            case EFFECT.SCALING:
                {
                    if (!m_IsHoverEffect || (_aS is TMP_Dropdown aTMPD && aTMPD.IsExpanded)) return;
                    _ScaleDown(_aS);
                    break;
                }
        }
    }
    private void _OnSelect(BaseEventData _aBED, Selectable _aS)
    {
        switch (m_EffectType)
        {
            case EFFECT.SCALING:
                {
                    if (_aS is not TMP_Dropdown aTMPD) return;
                    if (aTMPD.IsExpanded) _TryClickSound();
                    _ScaleDown(_aS);
                    break;
                }
        }
    }

    private void _AddSelectionListener(Selectable _aS)
    {
        if (!_aS.gameObject.TryGetComponent<EventTrigger>(out var aET)) aET = _aS.gameObject.AddComponent<EventTrigger>();

        // addEvent(EventTriggerType.Submit, x => _OnPointerClick(x, _aS)); // keyboard/gamepad confirmation (Enter/A button) is also triggered 
        addEvent(EventTriggerType.PointerClick, x => _OnPointerClick(x, _aS));
        addEvent(EventTriggerType.PointerEnter, x => _OnPointerEnter(x, _aS));
        addEvent(EventTriggerType.PointerExit, x => _OnPointerExit(x, _aS));
        addEvent(EventTriggerType.Select, x => _OnSelect(x, _aS));

        void addEvent(EventTriggerType _aETT, UnityAction<BaseEventData> _aCb)
        {
            EventTrigger.Entry aE = new() { eventID = _aETT };
            aE.callback.AddListener(x => _aCb(x));
            aET.triggers.Add(aE);
        }
    }

    private void OnDisable()
    {
        _ScaleUpT.Kill(true);
        _ScaleDownT.Kill(true);
    }
    private void OnEnable() { foreach (Selectable aS in m_DataSs) aS.transform.localScale = _OriginalScalesD[aS]; }
    private void Awake()
    {
        _OnClickScaleDownCb = () => { if (_ClickedS is not TMP_Dropdown) _ScaleDown(_ClickedS); };
        foreach (Selectable aS in m_DataSs)
        {
            _AddSelectionListener(aS);
            _OriginalScalesD.Add(aS, aS.transform.localScale);
        }
        DontDestroyOnLoad(gameObject);
    }
}
