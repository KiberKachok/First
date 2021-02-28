using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RealmsNetwork;
using UnityEngine.UI;

public class UINickname : NetworkedMonoBehaviour
{
    public TextMeshProUGUI playersCountText;
    public GameObject changeNicknamePanel;
    public TMP_InputField changeNicknameInputField;
    public Button changeNicknameButton;
    public TextMeshProUGUI nicknameText;
    string nickname;

    public void OnEnable()
    {
        nickname = PlayerPrefs.GetString("nickname");
        if(nickname == string.Empty)
        {
            nickname = FantasyNameGenerator.GetRandomName();
            FindObjectOfType<Client>().nickname = nickname;
            PlayerPrefs.SetString("nickname", nickname);

            nicknameText.text = nickname;
            StartCoroutine(UpdateNicknameBorderBounds(nickname));

            ChangeNickname();
        }
        else
        {
            FindObjectOfType<Client>().nickname = nickname;
            nicknameText.text = nickname;
            StartCoroutine(UpdateNicknameBorderBounds(nickname));
            changeNicknamePanel.SetActive(false);
        }
    }

    public void ChangeNickname()
    {
        changeNicknameInputField.text = nickname;
        RecalculateChangeNicknameButton();
        changeNicknamePanel.SetActive(true);
    }

    public void RecalculateChangeNicknameButton()
    {
        changeNicknameButton.interactable = changeNicknameInputField.text.Length > 0;
    }

    public void TrySetNickname()
    {
        if(changeNicknameInputField.text != string.Empty)
        {
            string nickname = changeNicknameInputField.text;
            PlayerPrefs.SetString("nickname", nickname);
            nicknameText.text = nickname;
            Client.main.SetNickname(nickname);
            StartCoroutine(UpdateNicknameBorderBounds(nickname));
            changeNicknamePanel.SetActive(false);
        }
    }

    IEnumerator UpdateNicknameBorderBounds(string nickname)
    {
        nicknameText.gameObject.SetActive(false);
        yield return null;
        nicknameText.gameObject.SetActive(true);
    }

    void Update()
    {
        
    }

    public override void OnLobbyStatisticsUpdate(int playersInGameCount, int playersInRoomsCount)
    {
        playersCountText.text = $"В лобби: {playersInGameCount - playersInRoomsCount}\nВ игре: {playersInRoomsCount}";
    }

    [ContextMenu("ClearPrefs")]
    public void ClearPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}
