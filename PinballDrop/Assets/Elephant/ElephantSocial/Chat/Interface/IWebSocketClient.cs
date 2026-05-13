using System;
using System.Collections.Generic;
using ElephantUniTask.Threading.Tasks;

namespace ElephantSocial.Chat.Interface
{
    public enum WebSocketState
    {
        Connecting,
        Open,
        Closing,
        Closed
    }
    
    public interface IWebSocketClient
    {
        event Action OnOpen;
        event Action<string> OnClose;
        event Action<string> OnError;
        event Action<byte[]> OnMessage;
        WebSocketState State { get; }
        UniTask ConnectAsync(string url, Dictionary<string, string> headers = null);
        UniTask CloseAsync();
        UniTask SendTextAsync(string data);
    }
}