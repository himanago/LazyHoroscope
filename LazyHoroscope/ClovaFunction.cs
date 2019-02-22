using CEK.CSharp;
using CEK.CSharp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace LazyHoroscope
{
    public static class ClovaFunction
    {
        [FunctionName("ClovaFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            CEKResponse response = null;;

            if (req.Headers.TryGetValue("SignatureCEK", out var signature))
            {
                var client = new ClovaClient();
                var request = await client.GetRequest(signature, req.Body);

                // IntentRequestのみここにくる
                if (request.Request.Type == RequestType.IntentRequest)
                {
                    // メイン処理
                    var result = IntentHandler.Handle(request.Request.Intent.Name);
                    response = result.GetClovaResponse();
                }
                else
                {
                    // エラー
                    response = new CEKResponse();
                    response.AddText("よくわかりませんでしたが、たぶんラッキーな1日になると思いますよ。");
                    response.ShouldEndSession = true;
                }
            }
            else
            {
                // エラー
                response = new CEKResponse();
                response.AddText("よくわかりませんでしたが、きっと大丈夫ですよ。");
                response.ShouldEndSession = true;
            }
            return new OkObjectResult(response);
        }
    }
}
