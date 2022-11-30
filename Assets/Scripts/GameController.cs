using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.Concurrent;
using UnityEngine.UI;
using System.Threading;
using System.Threading.Tasks;
using NativeWebSocket;
using System.Text;

namespace AutoCraft
{

    public class GameController : MonoBehaviour
    {
        [Serializable]
        public class AuthRequest
        {
            public string playerName;
        }

        [Serializable]
        public class AuthRecipe
        {
            public bool authenticated;
            public UnityEngine.Vector3 spawnPosition;
        }

        [Serializable]
        public class UpdateMessage
        {
            public string type;
            public int id;
            public UnityEngine.Vector3 vector;
        }

        public enum State
        {
            None,
            Connecting,
            Authenticating,
            Starting,
            Playing,
        }

        public GameObject PlayerPrefab;

        public static Dictionary<int, GameObject> objects = new Dictionary<int, GameObject>();

        public static Text statusText;
        public static string statusStr;
        public static Vector3 spawnPosition;

        public static State state = State.None;

        private static WebSocket webSocket;

        public static bool Started()
        {
            return state >= State.Playing;
        }

        void Start()
        {
            if (PlayerPrefab == null)
            {
                Debug.LogError("GameController: player prefab not set");
                Application.Quit();
                return;
            }

            GameObject statusGO = GameObject.FindGameObjectWithTag("Status");
            statusText = statusGO.GetComponent<Text>();

            webSocket = new WebSocket("ws://localhost:12000");

            statusText.text = "Connecting...";
            webSocket.OnOpen += () =>
            {
                Debug.Log("OPENED");
                AuthRequest authRequest = new AuthRequest();
                authRequest.playerName = "Player123";
                var json = JsonUtility.ToJson(authRequest);
                Debug.Log(Encoding.UTF8.GetBytes(json));
                webSocket.SendText(json);
                statusText.text = "Authenticating...";
                state = State.Authenticating;
            };

            webSocket.OnError += (e) =>
            {
                Debug.Log("Error! " + e);
            };

            webSocket.OnClose += (e) =>
            {
                Debug.Log("Connection closed!");
            };

            webSocket.OnMessage += (bytes) =>
            {
                var message = System.Text.Encoding.UTF8.GetString(bytes);
                Debug.Log("OnMessage! " + message);
                if (state == State.Authenticating)
                {
                    AuthRecipe authRecipe = JsonUtility.FromJson<AuthRecipe>(message);
                    if (!authRecipe.authenticated)
                    {
                        statusText.text = "Server refused authentication";
                        return;
                    }
                    spawnPosition = authRecipe.spawnPosition;
                    statusText.text = "";
                    state = State.Starting;
                    Instantiate(PlayerPrefab, spawnPosition, Quaternion.identity);
                }
            };


            state = State.Connecting;
            Debug.Log("Connecting");
            _ = webSocket.Connect();
        }

        void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            webSocket.DispatchMessageQueue();
#endif
        }

        void OnDestroy()
        {
            _ = webSocket.Close();
        }
    }
}