using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameInput : MonoBehaviour
{
    [Header("UI")] 
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button continueButton;
    
    public static string DisplayName { get; private set; }
    private const string PlayerPrefsNameKey = "PlayerName";
    
    
    // Start is called before the first frame update
    void Start()
    {
        SetupInputField();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetupInputField()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefsNameKey))
            return;

        string defaultName = PlayerPrefs.GetString(PlayerPrefsNameKey);

        nameInputField.text = defaultName;

        SetPlayerName(defaultName);
    }

    public void SetPlayerName(string name)
    {
        continueButton.interactable = true;
    }

    public void SavePlayerName()
    {
        DisplayName = nameInputField.text;
        PlayerPrefs.SetString(PlayerPrefsNameKey, DisplayName);
    }
}
