using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace SimpleWeb.Services
{
    public class PubConsumerAttribute : Attribute {
        public string Name {get;}
        public PubConsumerAttribute(string name) {
            Name = name;
        }
    }

    public class PublisherConfigurator {
        internal readonly Dictionary<string, ConsumerMethodInfo> consumerMap = new Dictionary<string, ConsumerMethodInfo>();

        private readonly IServiceCollection services;

        public PublisherConfigurator(IServiceCollection services) {
            this.services = services;
        }

        public PublisherConfigurator AddConsumers(Assembly asm) {
            foreach(var t in asm.GetExportedTypes()) {
                var consumerName = t.GetTypeInfo().GetCustomAttribute<PubConsumerAttribute>()?.Name;
                if(consumerName != null) {
                    foreach(var m in t.GetMethods(BindingFlags.Public|BindingFlags.Instance)) {
                        var methodName = m.GetCustomAttribute<PubConsumerAttribute>()?.Name;
                        if(methodName != null) {
                            var cmi = new ConsumerMethodInfo(t, consumerName, m, methodName);
                            consumerMap.Add(cmi.ActionName, cmi);
                            services.AddTransient(cmi.ConsumerType);
                        }
                    }
                }
            }
            return this;
        }
    }

    public static class PublisherExt {
        public static IServiceCollection AddPublisher(this IServiceCollection services,
                Action<PublisherConfigurator> configurator = null) {
            var config = new PublisherConfigurator(services);
            if(configurator != null)
                configurator(config);
            return services
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton(config)
                .AddSingleton<Publisher>();
        }
        public static IApplicationBuilder UsePublisher(this IApplicationBuilder app) {
            return app
                .UseWebSockets()
                .Use(async (ctx, next) => {
                    if(ctx.WebSockets.IsWebSocketRequest) {
                        await app.ApplicationServices.GetService<Publisher>().Connect(ctx);
                    } else await next();
                });
        }
    }


    public class PublisherAction {
        public string Type {get;set;}
        public object Payload {get;set;}
    }
    public class ConsumerMethodInfo {
        public string ActionName {get;}
        public bool IsAsync {get;}
        public Type ConsumerType {get;}
        public MethodInfo Method {get;}
        public Type InputType {get;}
        public Type OutputType {get;}
        public PropertyInfo TaskResultGetter {get;}

        public ConsumerMethodInfo(Type consumerType, string consumerName, MethodInfo method, string methodName) {
            ActionName = consumerName + "/" + methodName;
            IsAsync = typeof(Task).IsAssignableFrom(method.ReturnType);
            ConsumerType = consumerType;
            Method = method;
            InputType = method.GetParameters()?[0].ParameterType;
            if(IsAsync) {
                OutputType = method.ReturnType.GetTypeInfo().IsGenericType
                    && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)
                    ? method.ReturnType.GetGenericArguments()[0] : null;
                if(OutputType != null) {
                    TaskResultGetter = method.ReturnType.GetProperty("Result");
                }
            } else {
                OutputType = method.ReturnType == typeof(void) ? null : method.ReturnType;
            }
        }

        public async Task<PublisherAction> Run(object instance, object payload) {
            var pars = InputType != null ? new object[]{payload} : new object[]{};
            object result = null;
            if(IsAsync) {
                var task = (Task)Method.Invoke(instance, pars);
                await task;
                if(TaskResultGetter != null) {
                    result = TaskResultGetter.GetValue(task);
                }
            } else {
                result = Method.Invoke(instance, pars);
            }
            return new PublisherAction {
                Type = ActionName,
                Payload = result
            };
        }
    }

    public class Publisher
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IHttpContextAccessor context;
        private readonly PublisherConfigurator config;
        private readonly IDictionary<string, WebSocket> sockets;

        public Publisher(IServiceProvider serviceProvider, IHttpContextAccessor context, PublisherConfigurator config) {
            this.serviceProvider = serviceProvider;
            this.context = context;
            this.config = config;
            sockets = new ConcurrentDictionary<string, WebSocket>();
        }

        public async Task RunJson(string json) {
            var input = JObject.Parse(json);
            ConsumerMethodInfo method;
            if(config.consumerMap.TryGetValue(input.Value<string>("type"), out method)) {
                var instance = serviceProvider.GetService(method.ConsumerType);
                var payload = new JsonSerializer().Deserialize(new JTokenReader(input["payload"]), method.InputType);
                var output = await method.Run(instance, payload);
                if(output != null)
                    await PublishAsync(output, null);
            }
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
                            await RunJson(sb.ToString());
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