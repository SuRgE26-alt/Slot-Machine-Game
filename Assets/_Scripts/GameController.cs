using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public struct PayoutRule
{
    public SlotSymbol Symbol;
    public int MatchCount;
    public int Payout;
}

public class GameController : MonoBehaviour
{
    public static event Action HandlePulled = delegate { };

    [SerializeField] private TextMeshProUGUI _prizeText;
    [SerializeField] private Row[] _rows;
    [SerializeField] private Animator _slotMachineAnimator;
    [SerializeField] private Camera _mainCamera;

    [SerializeField] private PayoutRule[] _payoutRules;

    private int _stoppedRowCount;
    private bool _spinInProgress;

    void OnEnable()
    {
        foreach (var row in _rows)
        {
            row.RowStopped += OnRowStopped;
        }
    }

    void OnDisable()
    {
        foreach (var row in _rows)
        {
            row.RowStopped -= OnRowStopped;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !_spinInProgress)
        {
            StartCoroutine(PullHandle());
        }
    }

    private IEnumerator PullHandle()
    {
        if (Mouse.current == null)
            yield return new WaitForSeconds(0.01f);

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Ray ray = _mainCamera.ScreenPointToRay(mouseScreenPos);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Handle"))
            {
                _spinInProgress = true;
                _stoppedRowCount = 0;
                _prizeText.enabled = false;

                HandlePulled.Invoke();
                _slotMachineAnimator.SetBool("machineUsed", true);

                yield return new WaitForSeconds(0.5f);

                _slotMachineAnimator.SetBool("machineUsed", false);
            }
        }
    }

    private void OnRowStopped(Row row)
    {
        _stoppedRowCount++;

        if (_stoppedRowCount >= _rows.Length)
        {
            _spinInProgress = false;
            int prize = CalculatePrize();

            _prizeText.enabled = true;
            _prizeText.text = $"You won {prize} coins!";
        }
    }

    private int CalculatePrize()
    {
        var counts = new Dictionary<SlotSymbol, int>();
        foreach (var row in _rows)
        {
            SlotSymbol s = row._stoppedSlot;
            counts[s] = counts.TryGetValue(s, out int c) ? c + 1 : 1;
        }

        foreach (var rule in _payoutRules)
        {
            if (counts.TryGetValue(rule.Symbol, out int count) && count >= rule.MatchCount)
            {
                return rule.Payout;
            }
        }

        return 0;
    }
}