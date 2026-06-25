using System;
using System.Collections;
using UnityEngine;

public enum SlotSymbol
{
    Bar,
    Bell,
    Cherry,
    Seven
}

public class Row : MonoBehaviour
{
    [Header("Strip Setup")]
    [SerializeField] private SlotSymbol[] _symbols;
    [SerializeField] private float _slotHeight = 2.0f;
    [SerializeField] private float _topY = 6.8f;
    [SerializeField] private float _bottomY = -7.9f;

    [Header("Spin Tuning")]
    [SerializeField] private int _minStepsBeyondFullSpin = 60;
    [SerializeField] private int _maxStepsBeyondFullSpin = 100;
    [SerializeField] private AnimationCurve _spinEase = AnimationCurve.EaseInOut(0, 0.025f, 1, 0.2f);

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _spinLoopClip;

    public bool _rowStopped { get; private set; } = true;
    public SlotSymbol _stoppedSlot { get; private set; }

    public event Action<Row> RowStopped = delegate { };

    private int _currentIndex;
    private Coroutine _spinRoutine;


    void OnValidate()
    {
        // Runs in the editor whenever a value changes in the Inspector,
        // and also right after the script compiles — lets you see spacing
        // without entering Play Mode.
        if (_symbols == null || _symbols.Length == 0) return;

        PositionChildrenEvenly();
    }

    void Start()
    {
        PositionChildrenEvenly();

        _currentIndex = PositionToNearestIndex(transform.position.y);
        _stoppedSlot = _symbols[_currentIndex];

        GameController.HandlePulled += StartRotating;
    }

    private void PositionChildrenEvenly()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            float localY = -i * _slotHeight;
            child.localPosition = new Vector3(0f, localY, child.localPosition.z);
        }
    }

    private void StartRotating()
    {
        if (_spinRoutine != null)
        {
            StopCoroutine(_spinRoutine);
        }

        _spinRoutine = StartCoroutine(Rotate());
    }

    private IEnumerator Rotate()
    {
        _rowStopped = false;

        if (_audioSource != null && _spinLoopClip != null)
        {
            _audioSource.clip = _spinLoopClip;
            _audioSource.loop = true;
            _audioSource.Play();
        }

        int fullSpinSteps = _symbols.Length;
        int extraSteps = UnityEngine.Random.Range(_minStepsBeyondFullSpin, _maxStepsBeyondFullSpin + 1);
        int totalSteps = fullSpinSteps + extraSteps;

        for (int i = 0; i < totalSteps; i++)
        {
            StepIndex();

            float t = (float)i / Mathf.Max(1, totalSteps - 1);
            float waitTime = _spinEase.Evaluate(t);

            yield return new WaitForSeconds(waitTime);
        }

        if (_audioSource != null)
        {
            _audioSource.Stop();
        }

        _stoppedSlot = _symbols[_currentIndex];
        _rowStopped = true;
        _spinRoutine = null;

        RowStopped.Invoke(this);
    }

    private void StepIndex()
    {
        _currentIndex = (_currentIndex + 1) % _symbols.Length;
        SnapToIndex(_currentIndex);
    }

    private void SnapToIndex(int index)
    {
        float y = _topY - (index * _slotHeight);
        transform.position = new Vector2(transform.position.x, y);
    }

    private int PositionToNearestIndex(float y)
    {
        int rawIndex = Mathf.RoundToInt((_topY - y) / _slotHeight);
        return ((rawIndex % _symbols.Length) + _symbols.Length) % _symbols.Length;
    }

    private void OnDisable()
    {
        GameController.HandlePulled -= StartRotating;
    }

    private void OnDrawGizmos()
    {
        float x = transform.position.x;

        // Top point — where index 0 sits.
        Vector3 topPoint = new Vector3(x, _topY, transform.position.z);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(topPoint, 0.15f);
        Gizmos.DrawLine(topPoint + Vector3.left * 0.5f, topPoint + Vector3.right * 0.5f);

        // Bottom point — derived from how many symbols you have, not hand-typed,
        // so it always matches what SnapToIndex would actually produce.
        int count = (_symbols != null && _symbols.Length > 0) ? _symbols.Length : 1;
        float bottomY = _topY - ((count - 1) * _slotHeight);
        Vector3 bottomPoint = new Vector3(x, bottomY, transform.position.z);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(bottomPoint, 0.15f);
        Gizmos.DrawLine(bottomPoint + Vector3.left * 0.5f, bottomPoint + Vector3.right * 0.5f);

        // Connecting line between them for a quick visual span check.
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(topPoint, bottomPoint);

        // Every individual symbol slot in between, so you can see exact spacing.
        Gizmos.color = Color.cyan;
        for (int i = 0; i < count; i++)
        {
            Vector3 p = new Vector3(x, _topY - (i * _slotHeight), transform.position.z);
            Gizmos.DrawWireSphere(p, 0.08f);
        }
    }
}