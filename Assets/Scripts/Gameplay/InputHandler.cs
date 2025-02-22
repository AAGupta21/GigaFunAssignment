using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image effectImage;
    
    private Action<bool, int> _dropAction;
    private Action _pickAction;
    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private bool _isDragging;
    private int _key;
    private const float Offset = 100f;
    private const float TimeToMoveBack = 0.45f;
    private const float TimeForGlow = 0.25f;
    private Image _img;

    public void DoSetUp(Vector3 sPosition, Vector3 tPosition, int k, Action<bool, int> onDropAction, Action onPickAction)
    {
        _img = GetComponent<Image>();
        _img.raycastTarget = true;
        _startPosition = sPosition;
        _targetPosition = tPosition;
        _dropAction = onDropAction;
        _pickAction = onPickAction;
        _key = k;
        effectImage.sprite = _img.sprite;
        effectImage.rectTransform.sizeDelta = _img.rectTransform.sizeDelta;
    }

    public void DoCompletedSetup()
    {
        _img = GetComponent<Image>();
        _img.raycastTarget = false;
        effectImage.sprite = _img.sprite;
        effectImage.rectTransform.sizeDelta = _img.rectTransform.sizeDelta;
    }

    public void PlayEffectAfterDelay()
    {
        StartCoroutine(PlayGlowAnimation());
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        _isDragging = true;
        _pickAction?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isDragging = false;
        CheckForCorrection();
    }

    private void Update()
    {
        if (_isDragging)
        {
            transform.position = Input.mousePosition;
        }
    }

    private void CheckForCorrection()
    {
        if (Vector3.Distance(transform.position, _targetPosition) > Offset)
        {
            _dropAction?.Invoke(false, _key);
            StartCoroutine(MoveTo(_startPosition));
        }
        else
        {
            _dropAction?.Invoke(true, _key);
            StartCoroutine(MoveTo(_targetPosition));
            GetComponent<Image>().raycastTarget = false;
        }
    }

    private IEnumerator MoveTo(Vector3 tPos)
    {
        var iniPos = transform.position;
        var time = 0f;
        while (time < TimeToMoveBack)
        {
            transform.position = Vector3.Lerp(iniPos, tPos, time / TimeToMoveBack);
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        
        transform.position = tPos;
    }

    private IEnumerator PlayGlowAnimation()
    {
        yield return new WaitForSeconds(TimeToMoveBack);
        
        var time = 0f;
        var col = new Color(1f, 1f, 0f, 0f);
        _img.rectTransform.sizeDelta += new Vector2(20f, 20f);
        _img.color = col;
        var halfTime = TimeForGlow / 2f;

        while (time < halfTime)
        {
            col.a = Mathf.Lerp(0f, 1f, time / halfTime);
            _img.color = col;
            time += 0.05f;
            yield return new WaitForSeconds(0.05f);
        }
        
        time = 0f;
        while (time < halfTime)
        {
            col.a = Mathf.Lerp(1f, 0f, time / halfTime);
            _img.color = col;
            time += 0.05f;
            yield return new WaitForSeconds(0.05f);
        }
        
        _img.rectTransform.sizeDelta -= new Vector2(20f, 20f);
        _img.color = Color.white;
    }
}
