using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _coinText;
    [SerializeField] private int _startingCoins = 100;
    [SerializeField] private int _costPerRoll = 5;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _coinChangeClip;

    public int CurrentCoins { get; private set; }
    public event Action<int, int> CoinsChanged = delegate { }; // (oldValue, newValue)

    private Coroutine _countRoutine;

    void Awake()
    {
        CurrentCoins = _startingCoins;
        _coinText.text = CurrentCoins.ToString();
    }

    void OnEnable()
    {
        CoinsChanged += OnCoinsChanged;
    }

    void OnDisable()
    {
        CoinsChanged -= OnCoinsChanged;
    }

    public bool TrySpend(int amount)
    {
        if (CurrentCoins < amount) return false;

        int old = CurrentCoins;
        CurrentCoins -= amount;
        CoinsChanged.Invoke(old, CurrentCoins);
        return true;
    }

    public void Award(int amount)
    {
        int old = CurrentCoins;
        CurrentCoins += amount;
        CoinsChanged.Invoke(old, CurrentCoins);
    }

    public int CostPerRoll => _costPerRoll;
    public int MaxAffordableRolls => CurrentCoins / _costPerRoll;

    private void OnCoinsChanged(int oldValue, int newValue)
    {
        if (_audioSource != null && _coinChangeClip != null)
        {
            _audioSource.PlayOneShot(_coinChangeClip);
        }

        if (_countRoutine != null) StopCoroutine(_countRoutine);
        _countRoutine = StartCoroutine(AnimateCoinText(oldValue, newValue));
    }

    private IEnumerator AnimateCoinText(int from, int to)
    {
        if (from == to)
        {
            _coinText.text = to.ToString();
            yield break;
        }

        int step = to > from ? 1 : -1;
        int current = from;
        float delay = Mathf.Clamp(0.4f / Mathf.Abs(to - from), 0.005f, 0.05f);

        while (current != to)
        {
            current += step;
            _coinText.text = current.ToString();
            yield return new WaitForSeconds(delay);
        }
    }
}