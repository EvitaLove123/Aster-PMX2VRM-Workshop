using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Aster.PMX2VRM
{
    /// <summary>
    /// Browser bridge for the Aster PMX2VRM Workshop.
    /// The browser sends JSON with SendMessage("AsterRuntimeBridge", "ReceiveJson", json).
    /// This bridge intentionally does not pretend to export VRM until the UniVRM
    /// import, humanoid validation and exporter pipeline has completed.
    /// </summary>
    public sealed class AsterRuntimeBridge : MonoBehaviour
    {
        public static AsterRuntimeBridge Instance { get; private set; }

        public event Action<string> MessageReceived;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void AsterPostMessage(string json);
#endif

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            gameObject.name = "AsterRuntimeBridge";
            DontDestroyOnLoad(gameObject);
        }

        public void ReceiveJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                PostStatus("error", "Empty message received.");
                return;
            }

            MessageReceived?.Invoke(json);
            PostStatus("received", "Request received by Unity runtime. Import/export pipeline is not enabled yet.");
        }

        public void PostStatus(string state, string message)
        {
            var payload = JsonUtility.ToJson(new BridgeStatus
            {
                source = "aster-univrm",
                state = state,
                message = message,
                univrmVersion = "0.131.4"
            });

#if UNITY_WEBGL && !UNITY_EDITOR
            AsterPostMessage(payload);
#else
            Debug.Log(payload);
#endif
        }

        [Serializable]
        private sealed class BridgeStatus
        {
            public string source;
            public string state;
            public string message;
            public string univrmVersion;
        }
    }
}
