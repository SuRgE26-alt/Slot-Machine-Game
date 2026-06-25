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
    public int Payout; // base payout, multiplied by the chosen bet tier
}

public enum BetTier
{
    Low,
    High
}

public class GameController : MonoBehaviour
{
    public static event Action HandlePulled = delegate { };

    [SerializeField] private TextMeshProUGUI _prizeText;
    [SerializeField] private Row[] _rows;
    [SerializeField] private Animator _slotMachineAnimator;
    [SerializeField] private Camera _mainCamera;

    [SerializeField] private PayoutRule[] _payoutRules;

    [SerializeField] private CoinManager _coinManager;

    [Header("Bet Tiers")]
    [SerializeField] private int _lowBetCost = 5;
    [SerializeField] private int _highBetCost = 15;
    [SerializeField] private int _lowBetMultiplier = 1;
    [SerializeField] private int _highBetMultiplier = 3;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _leverPullClip;

    private BetTier _selectedBet = BetTier.Low;
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

    public void SelectBet(BetTier tier)
    {
        _selectedBet = tier;
    }

    private int CurrentBetCost => _selectedBet == BetTier.Low ? _lowBetCost : _highBetCost;
    private int CurrentBetMultiplier => _selectedBet == BetTier.Low ? _lowBetMultiplier : _highBetMultiplier;

    private IEnumerator PullHandle()
    {
        if (Mouse.current == null)
            yield return new WaitForSeconds(0.01f);

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Ray ray = _mainCamera.ScreenPointToRay(mouseScreenPos);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null && hit.collider.CompareTag("Handle"))
        {
            if (!_coinManager.TrySpend(CurrentBetCost))
            {
                yield break;
            }

            if (_audioSource != null && _leverPullClip != null)
            {
                _audioSource.PlayOneShot(_leverPullClip);
            }

            _spinInProgress = true;
            _stoppedRowCount = 0;
            _prizeText.enabled = false;

            HandlePulled.Invoke();
            _slotMachineAnimator.SetBool("machineUsed", true);

            yield return new WaitForSeconds(0.5f);

            _slotMachineAnimator.SetBool("machineUsed", false);
        }
    }

    private void OnRowStopped(Row row)
    {
        _stoppedRowCount++;

        if (_stoppedRowCount >= _rows.Length)
        {
            _spinInProgress = false;
            int prize = CalculatePrize() * CurrentBetMultiplier;

            if (prize > 0)
            {
                _coinManager.Award(prize);
            }

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