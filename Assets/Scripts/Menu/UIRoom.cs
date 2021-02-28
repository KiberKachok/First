using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RealmsNetwork;
using System.Linq;
using TMPro;


public class UIRoom : NetworkedMonoBehaviour
{
    public Transform roomPlayersScrollViewTransform;
    public GameObject playerPrefab;
    public TextMeshProUGUI playersCountText;
    public Button startGameButton;
    public Color activeButtonColor;
    public Color inactiveButtonColor;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnRoomPlayersListUpdate(Dictionary<string, Player> players)
    {
        playersCountText.text = $"игроков: {players.Count}/16";

        foreach (Transform child in roomPlayersScrollViewTransform)
            Destroy(child.gameObject);

        for (int i = 0; i < players.Count; i++)
        {
            Player player = players.ElementAt(i).Value;
            GameObject playerObject = Instantiate(playerPrefab, roomPlayersScrollViewTransform);
            playerObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{player.name}";
        }

        if (players.Count > 0 && players.ElementAt(0).Value.hash == Client.main.hash)
        {
            startGameButton.interactable = true;
            startGameButton.GetComponent<Image>().color = activeButtonColor;
            startGameButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "начать";
        }
        else
        {
            startGameButton.interactable = false;
            startGameButton.GetComponent<Image>().color = inactiveButtonColor;
            startGameButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "ожидайте";
        }
    }
}
