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
    [Tooltip("Symbols in strip order, top to bottom. Index 0 sits at _topY.")]
    [SerializeField] private SlotSymbol[] _symbols;
    [SerializeField] private float _slotHeight = 2.0f;
    [SerializeField] private float _topY = 6.8f;
    [SerializeField] private float _bottomY = -7.9f;

    [Header("Spin Tuning")]
    [SerializeField] private int _minStepsBeyondFullSpin = 60;
    [SerializeField] private int _maxStepsBeyondFullSpin = 100;
    [SerializeField] private AnimationCurve _spinEase = AnimationCurve.EaseInOut(0, 0.025f, 1, 0.2f);

    public bool _rowStopped { get; private set; } = true;
    public SlotSymbol _stoppedSlot { get; private set; }

    public event Action<Row> RowStopped = delegate { };

    private int _currentIndex;
    private Coroutine _spinRoutine;

    void Start()
    {
        // Snap to a valid starting index/position based on current transform.
        _currentIndex = PositionToNearestIndex(transform.position.y);
        SnapToIndex(_currentIndex);
        _stoppedSlot = _symbols[_currentIndex];

        GameController.HandlePulled += StartRotating;
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

        int fullSpinSteps = _symbols.Length; // one full loop of the strip
        int extraSteps = UnityEngine.Random.Range(_minStepsBeyondFullSpin, _maxStepsBeyondFullSpin + 1);
        int totalSteps = fullSpinSteps + extraSteps;

        for (int i = 0; i < totalSteps; i++)
        {
            StepIndex();

            float t = (float)i / Mathf.Max(1, totalSteps - 1);
            float waitTime = _spinEase.Evaluate(t);

            yield return new WaitForSeconds(waitTime);
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
}