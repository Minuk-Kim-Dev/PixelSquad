using UnityEngine;

public class LobbyScene : BaseScene
{
    private void Start()
    {
        Managers.Room.UpdateRoomList();
    }

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Lobby;

        Application.runInBackground = true;
    }

    public override void Clear()
    {

    }
}
