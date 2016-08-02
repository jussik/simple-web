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
using Newtonsoft.Json.Serialization;

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

        public async Task Connect(HttpContext context)
        {
            string id = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=');
            var socket = await context.WebSockets.AcceptWebSocketAsync();

            var initMsg = GetMessage(new { id });
            await socket.SendAsync(initMsg, WebSocketMessageType.Text, true, default(CancellationToken));

            try
            {
                sockets.Add(id, socket);

                var sb = new StringBuilder();
                var buffer = new ArraySegment<byte>(new byte[0x4000]); // 16k
                while(socket.State == WebSocketState.Open) {
                    var message = await socket.ReceiveAsync(buffer, default(CancellationToken));
                    if(message.MessageType == WebSocketMessageType.Text) {
                        var payload = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, message.Count);
                        sb.Append(payload);
                        if(message.EndOfMessage) {
                            await PublishAsync(new { type="message", payload=sb.ToString()}, id);
                            sb.Clear();
                        }
                    }
                }
            } finally {
                sockets.Remove(id);
            }
        }

        public async Task PublishAsync<T>(T data) {
            string id = (string)context.HttpContext.Request.Headers["X-Publisher-Client"];
            await PublishAsync(data, id);
        }
        private async Task PublishAsync<T>(T data, string exceptId) {
            var buffer = GetMessage(data);
            await Task.WhenAll(sockets.ToList()
                .Where(p => p.Key != exceptId && p.Value.State == WebSocketState.Open)
                .Select(p => p.Value.SendAsync(buffer, WebSocketMessageType.Text, true, default(CancellationToken)))
                .ToArray());
        }
        private ArraySegment<byte> GetMessage<T>(T data) {
            string json = JsonConvert.SerializeObject(data, Formatting.None, new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            return new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
        }
    }
}