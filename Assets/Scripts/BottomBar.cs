using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BottomBar : MonoBehaviour
{
    [SerializeField] private Canvas _clickerScreen;
    [SerializeField] private Canvas _weatherScreen;
    [SerializeField] private Canvas _breedFactsScreen;

    [SerializeField] private Button _clickerButton;
    [SerializeField] private Button _weatherButton;
    [SerializeField] private Button _breedFactsButton;

    [SerializeField] private Image _clickerButtonImage;
    [SerializeField] private Image _weatherButtonImage;
    [SerializeField] private Image _breedFactsButtonImage;

    [SerializeField] private TextMeshProUGUI _clickerButtonText;
    [SerializeField] private TextMeshProUGUI _weatherButtonText;
    [SerializeField] private TextMeshProUGUI _breedFactsButtonText;

    [SerializeField] private Color _normalColor;
    [SerializeField] private Color _selectedColor;

    [SerializeField] private Color _normalTextColor;
    [SerializeField] private Color _selectedTextColor;

    private Button _currentActiveButton;

    private void Start()
    {
        _clickerButton.onClick.AddListener(() => SwitchToScreen(_clickerScreen, _clickerButton));
        _weatherButton.onClick.AddListener(() => SwitchToScreen(_weatherScreen, _weatherButton));
        _breedFactsButton.onClick.AddListener(() => SwitchToScreen(_breedFactsScreen, _breedFactsButton));

        SwitchToScreen(_clickerScreen, _clickerButton);
    }

    private void SwitchToScreen(Canvas screenToShow, Button selectedButton)
    {
        if (screenToShow == null) return;
        if (_currentActiveButton == selectedButton) return;

        DisableAllScreens();

        screenToShow.gameObject.SetActive(true);

        UpdateButtonColors(selectedButton);

        _currentActiveButton = selectedButton;
    }

    private void DisableAllScreens()
    {
        if (_clickerScreen != null) _clickerScreen.gameObject.SetActive(false);
        if (_weatherScreen != null) _weatherScreen.gameObject.SetActive(false);
        if (_breedFactsScreen != null) _breedFactsScreen.gameObject.SetActive(false);
    }

    private void UpdateButtonColors(Button selectedButton)
    {
        UpdateButtonColor(_clickerButton, _clickerButtonImage, _clickerButtonText, selectedButton == _clickerButton);

        UpdateButtonColor(_weatherButton, _weatherButtonImage, _weatherButtonText, selectedButton == _weatherButton);

        UpdateButtonColor(_breedFactsButton, _breedFactsButtonImage, _breedFactsButtonText,
            selectedButton == _breedFactsButton);
    }

    private void UpdateButtonColor(Button button, Image buttonImage, TextMeshProUGUI buttonText, bool isSelected)
    {
        if (button == null) return;

        Color targetColor = isSelected ? _selectedColor : _normalColor;

        buttonImage.color = targetColor;
        buttonText.color = isSelected ? _selectedTextColor : _normalTextColor;
    }

    public void ShowClickerScreen()
    {
        SwitchToScreen(_clickerScreen, _clickerButton);
    }

    public void ShowWeatherScreen()
    {
        SwitchToScreen(_weatherScreen, _weatherButton);
    }

    public void ShowBreedFactsScreen()
    {
        SwitchToScreen(_breedFactsScreen, _breedFactsButton);
    }

    private void OnDestroy()
    {
        _clickerButton.onClick.RemoveAllListeners();
        _weatherButton.onClick.RemoveAllListeners();
        _breedFactsButton.onClick.RemoveAllListeners();
    }
}