using Google.Protobuf.Protocol;
using System.Collections;
using UnityEngine;
using static Define;
using Object = UnityEngine.Object;

public class UI_TitleScene : UI_Scene
{
	private enum GameObjects
	{
		Object_Start,
	}

	private enum Texts
	{
		StatusText,
		StartText,
	}

	private enum TitleSceneState
	{
		None,
		AssetLoading,
		AssetLoaded,
		ConnectingToServer,
		ConnectedToServer,
		FailedToConnectToServer,
	}

	TitleSceneState _state = TitleSceneState.None;
	TitleSceneState State
	{
		get { return _state; }
		set
		{
			_state = value;
			switch (value)
			{
				case TitleSceneState.None:
					break;
				case TitleSceneState.AssetLoading:
					//GetText((int)Texts.StatusText).text = $"TODO 로딩중";
                    break;
				case TitleSceneState.AssetLoaded:
					//GetText((int)Texts.StatusText).text = "TODO 로딩 완료";
                    break;
				case TitleSceneState.ConnectingToServer:
					//GetText((int)Texts.StatusText).text = "TODO 서버 접속중";
                    break;
				case TitleSceneState.ConnectedToServer:
					//GetText((int)Texts.StatusText).text = "TODO 서버 접속 성공";
                    break;
				case TitleSceneState.FailedToConnectToServer:
					GetText((int)Texts.StatusText).text = "TODO 서버 접속 실패";
                    break;
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();

		BindObjects(typeof(GameObjects));
		BindTexts(typeof(Texts));

		// GetText((int)Texts.StartText).gameObject.BindEvent((evt) =>
		// {
		// 	Debug.Log("OnClick");
		// 	Managers.Scene.LoadScene(EScene.LoadingScene);
		// });

		GetGameObject((int)GameObjects.Object_Start).BindEvent((evt) =>
		{
			Debug.Log("OnClick");
			Managers.Scene.LoadScene(EScene.LoadingScene);
		});

        GetText((int)Texts.StartText).gameObject.SetActive(false);
		GetGameObject((int)GameObjects.Object_Start).SetActive(false);
	}

	protected override void Start()
	{
		base.Start();

		// Load ����
		State = TitleSceneState.AssetLoading;

		Managers.Resource.LoadAllAsync<Object>("Preload", (key, count, totalCount) =>
		{
			//GetText((int)Texts.StatusText).text = $"TODO �ε��� : {key} {count}/{totalCount}";
            //Debug.Log(GetText((int)Texts.StatusText).text);

            if (count == totalCount)
			{
				OnAssetLoaded();
			}
		});
	}

	private void OnAssetLoaded()
	{
		Managers.Data.Init();
		Managers.Game.Init();
		Managers.Player.Init();
		//State = TitleSceneState.AssetLoaded;
		//Managers.Data.Init();

		//Debug.Log("Connecting To Server");
		//State = TitleSceneState.ConnectingToServer;

		//IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
		//IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);
		//Managers.Network.GameServer.Connect(endPoint, OnConnectionSuccess, OnConnectionFailed);

		OnConnectionSuccess();
    }

	private void OnConnectionSuccess()
	{
		Debug.Log("Connected To Server");
		State = TitleSceneState.ConnectedToServer;

		GetText((int)Texts.StartText).gameObject.SetActive(true);
		GetGameObject((int)GameObjects.Object_Start).SetActive(true);

		StartCoroutine(CoSendTestPackets());
	}

	private void OnConnectionFailed()
	{
		Debug.Log("Failed To Connect To Server");
		State = TitleSceneState.FailedToConnectToServer;
	}

	IEnumerator CoSendTestPackets()
	{
		while (true)
		{
			yield return new WaitForSeconds(1);

			C_Test pkt = new C_Test();
			pkt.Temp = 1;
			Managers.Network.Send(pkt);
		}
	}
}
