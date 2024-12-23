using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameEnd : MonoBehaviour
{
    [SerializeField] TMP_Text _rank;
    [SerializeField] Button _exit;
    [SerializeField] Button _monitor;

    public void Monitor()
    {
        StartCoroutine(Managers.UI.CoFadeOut(this.gameObject, Monitoring));
    }

    public void Exit()
    {
        StartCoroutine(Managers.UI.CoFadeOut(this.gameObject, ExitLobby));
    }

    void ExitLobby()
    {
        _rank.gameObject.SetActive(true);
        _exit.gameObject.SetActive(true);

        if (Managers.Game.Rank == 1)
            _rank.text = $"You are {Managers.Game.Rank}st";
        else if (Managers.Game.Rank == 2)
            _rank.text = $"You are {Managers.Game.Rank}nd";
        else if (Managers.Game.Rank == 3)
            _rank.text = $"You are {Managers.Game.Rank}rd";
        else
            _rank.text = $"You are {Managers.Game.Rank}th";
    }

    void Monitoring()
    {
        _rank.gameObject.SetActive(true);
        _monitor.gameObject.SetActive(true);

        if(Managers.Game.Rank == 1)
            _rank.text = $"You are {Managers.Game.Rank}st";
        else if(Managers.Game.Rank == 2)
            _rank.text = $"You are {Managers.Game.Rank}nd";
        else if(Managers.Game.Rank == 3)
            _rank.text = $"You are {Managers.Game.Rank}rd";
        else
            _rank.text = $"You are {Managers.Game.Rank}th";
    }
}
