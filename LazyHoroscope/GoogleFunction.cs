using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace LazyHoroscope
{
    public static class GoogleFunction
    {
        [FunctionName("GoogleFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var parser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
            var webhookRequest = parser.Parse<WebhookRequest>(await req.ReadAsStringAsync());
            WebhookResponse response = null;

            if (webhookRequest.QueryResult.Intent.DisplayName != "input.welcome")
            {
                // メイン処理
                var result = IntentHandler.Handle(webhookRequest.QueryResult.Intent.DisplayName);
                response = result.GetGoogleResponse();
            }
            else
            {
                // エラー
                response = new WebhookResponse();
                response.FulfillmentText = "よくわかりませんでしたが、きっと大丈夫ですよ。";
            }

            return new ProtcolBufJsonResult(response, JsonFormatter.Default);
        }
    }

    /// <summary>
    /// 以下を参考にしました
    /// https://blog.okazuki.jp/entry/2018/11/04/230402
    /// </summary>
    public class ProtcolBufJsonResult : IActionResult
    {
        private readonly object _obj;
        private readonly JsonFormatter _formatter;

        public ProtcolBufJsonResult(object obj, JsonFormatter formatter)
        {
            _obj = obj;
            _formatter = formatter;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.Headers.Add("Content-Type", new Microsoft.Extensions.Primitives.StringValues("application/json; charset=utf-8"));
            var stringWriter = new StringWriter();
            _formatter.WriteValue(stringWriter, _obj);
            await context.HttpContext.Response.WriteAsync(stringWriter.ToString());
        }
    }
}
