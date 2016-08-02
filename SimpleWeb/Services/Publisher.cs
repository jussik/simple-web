using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace SimpleWeb.Services
{
    public class Publisher
    {
        private readonly IHttpContextAccessor context;
        private readonly IDictionary<string, WebSocket> sockets;

        public Publisher(IHttpContextAccessor context) {
            this.context = context;
            sockets = new ConcurrentDictionary<string, WebSocket>();
        }

        private const string ClientIdKey = "PublisherClientId";
        public string GetClientId() {
            var context = this.context.HttpContext;
            string id = context.Request.Cookies[ClientIdKey];
            if(id == null) {
                id = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                context.Response.Cookies.Append(ClientIdKey, id, new CookieOptions {
                    Path = "/",
                    HttpOnly = false
                });
            }
            return id;
        }

        public async Task Connect(HttpContext context)
        {
            string id = GetClientId();
            var socket = await context.WebSockets.AcceptWebSocketAsync();
            sockets.Add(id, socket);
            var token = CancellationToken.None;

            StringBuilder sb = new StringBuilder();
            var buffer = new ArraySegment<byte>(new byte[0x4000]); // 16k
            while(socket.State == WebSocketState.Open) {
                var message = await socket.ReceiveAsync(buffer, token);
                if(message.MessageType == WebSocketMessageType.Text) {
                    var payload = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, message.Count);
                    sb.Append(payload);
                    if(message.EndOfMessage) {
                        await PublishRawAsync(sb.ToString(), id);
                        sb.Clear();
                    }
                }
            }

            sockets.Remove(id);
        }

        public async Task PublishAsync<T>(T data) {
            await PublishRawAsync(JsonConvert.SerializeObject(data), GetClientId());
        }

        private async Task PublishRawAsync(string message, string exceptId) {
            var token = CancellationToken.None;
            var buffer = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(message));
            await Task.WhenAll(sockets.ToList()
                .Where(p => p.Key != exceptId && p.Value.State == WebSocketState.Open)
                .Select(p => p.Value.SendAsync(buffer, WebSocketMessageType.Text, true, token))
                .ToArray());
        }
    }
}