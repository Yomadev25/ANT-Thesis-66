using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SaveHudManager : MonoBehaviour
{
    public enum State
    {
        NONE,
        SAVE,
        LOAD
    }

    [SerializeField]
    private State _currentState;
    [SerializeField]
    private Transform _saveRoot;

    [Header("Save Slot")]
    [SerializeField]
    private GameObject _saveSlot;
    [SerializeField]
    private TMP_Text _saveNameText;
    [SerializeField]
    private TMP_Text _playTimeText;
    [SerializeField]
    private TMP_Text _saveDateText;
    [SerializeField]
    private TMP_Text _saveTimeText;

    private void Start()
    {
        FetchSaveList();
    }

    private void FetchSaveList()
    {
        var saveList = SaveManager.Instance.GetSaveFiles();

        foreach (Transform saveslot in _saveRoot)
        {
            Destroy(saveslot.gameObject);
        }

        for (int i = 0; i < 4; i++)
        {
            if (i <= saveList.Count - 1)
            {
                Player player = SaveManager.Instance.Load(int.Parse(saveList[i]));
                TimeSpan duration = player.lastDate - player.startDate;

                _saveNameText.text = i == 0 ? "AUTOSAVE" : "SLOT " + player.id;
                _playTimeText.text = $"{duration.TotalHours.ToString("00")}.{duration.TotalMinutes.ToString("00")}.{duration.TotalSeconds.ToString("00")}";
                _saveDateText.text = $"{player.lastDate.ToString("dd / MM / yyyy")}";
                _saveTimeText.text = $"{player.lastDate.ToString("hh:mm tt")}";

                GameObject GO = Instantiate(_saveSlot, _saveRoot);
                Button button = GO.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                int id = i;
                button.onClick.AddListener(() =>
                {
                    switch (_currentState)
                    {
                        case State.NONE:
                            break;
                        case State.SAVE:
                            Save(PlayerData.Instance.GetPlayerData());
                            break;
                        case State.LOAD:
                            Load(player, id);
                            break;
                        default:
                            break;
                    }
                });
                GO.SetActive(true);
            }
            else
            {
                _saveNameText.text = i == 0 ? "AUTOSAVE" : "SLOT " + i;
                _playTimeText.text = "--.--.--";
                _saveDateText.text = "-- / -- / --";
                _saveTimeText.text = "--:--";

                GameObject GO = Instantiate(_saveSlot, _saveRoot);
                GO.SetActive(true);
            }
        }
    }

    private void Save(Player player)
    {
        player.lastDate = DateTime.Now;

        SaveManager.Instance.Save(player);
        FetchSaveList();
    }

    private void Load(Player player, int id)
    {
        player.id = id;
        PlayerData.Instance.PlayerSetup(player);
        TransitionManager.Instance.SceneFadeIn(1, () =>
            SceneManager.LoadScene("Hikari"));
    }

    public void SetState(int state)
    {
        if (_currentState == (State)state) return;
        _currentState = (State)state;
        FetchSaveList();
    }
}