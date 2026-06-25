using UnityEngine;
using UnityEngine.UI;

public class BetSelector : MonoBehaviour
{
    [SerializeField] private GameController _gameController;

    [Header("Buttons")]
    [SerializeField] private Button _lowBetButton;
    [SerializeField] private Button _highBetButton;

    [Header("Selected Visual")]
    [SerializeField] private Color _selectedColor = Color.yellow;
    [SerializeField] private Color _unselectedColor = Color.white;

    void Start()
    {
        _lowBetButton.onClick.AddListener(() => SelectBet(BetTier.Low));
        _highBetButton.onClick.AddListener(() => SelectBet(BetTier.High));

        // Default to Low on startup, matching GameController's default.
        SelectBet(BetTier.Low);
    }

    private void SelectBet(BetTier tier)
    {
        _gameController.SelectBet(tier);

        _lowBetButton.image.color = tier == BetTier.Low ? _selectedColor : _unselectedColor;
        _highBetButton.image.color = tier == BetTier.High ? _selectedColor : _unselectedColor;
    }

    void OnDestroy()
    {
        _lowBetButton.onClick.RemoveAllListeners();
        _highBetButton.onClick.RemoveAllListeners();
    }
}