using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace LazyHoroscope
{
    public static class AlexaFunction
    {
        [FunctionName("AlexaFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req,
            ILogger log)
        {
            var json = await req.ReadAsStringAsync();
            var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(json);
            SkillResponse response = null;

            if (await ValidateRequest(req, log, skillRequest))
            {
                // IntentRequestのみここにくる
                if (skillRequest.Request is IntentRequest intentRequest)
                {
                    // メイン処理
                    var result = IntentHandler.Handle(intentRequest.Intent.Name);
                    response = result.GetAlexaResponse();
                }
                else
                {
                    // エラー
                    response = new SkillResponse()
                    {
                        Version = "1.0",
                        Response = new ResponseBody()
                        {
                            OutputSpeech = new PlainTextOutputSpeech { Text = "よくわかりませんでしたが、たぶんラッキーな1日になると思いますよ。" },
                            ShouldEndSession = true
                        }
                    };
                }
            }
            else
            {
                // エラー
                response = new SkillResponse()
                {
                    Version = "1.0",
                    Response = new ResponseBody()
                    {
                        OutputSpeech = new PlainTextOutputSpeech { Text = "よくわかりませんでしたが、きっと大丈夫ですよ。" },
                        ShouldEndSession = true
                    }
                };
            }
            return new OkObjectResult(response);

        }

        /// <summary>
        /// リクエストの検証処理。
        /// https://blogs.msdn.microsoft.com/appconsult/2018/11/03/build-your-first-alexa-skill-with-alexa-net-and-azure-functions-the-certification/
        /// </summary>
        /// <param name="request"></param>
        /// <param name="log"></param>
        /// <param name="skillRequest"></param>
        /// <returns></returns>
        private static async Task<bool> ValidateRequest(HttpRequest request, ILogger log, SkillRequest skillRequest)
        {
            request.Headers.TryGetValue("SignatureCertChainUrl", out var signatureChainUrl);
            if (string.IsNullOrWhiteSpace(signatureChainUrl))
            {
                log.LogError("Validation failed. Empty SignatureCertChainUrl header");
                return false;
            }

            Uri certUrl;
            try
            {
                certUrl = new Uri(signatureChainUrl);
            }
            catch
            {
                log.LogError($"Validation failed. SignatureChainUrl not valid: {signatureChainUrl}");
                return false;
            }

            request.Headers.TryGetValue("Signature", out var signature);
            if (string.IsNullOrWhiteSpace(signature))
            {
                log.LogError("Validation failed - Empty Signature header");
                return false;
            }

            request.Body.Position = 0;
            var body = await request.ReadAsStringAsync();
            request.Body.Position = 0;

            if (string.IsNullOrWhiteSpace(body))
            {
                log.LogError("Validation failed - the JSON is empty");
                return false;
            }

            bool isTimestampValid = RequestVerification.RequestTimestampWithinTolerance(skillRequest);
            bool valid = await RequestVerification.Verify(signature, certUrl, body);

            if (!valid || !isTimestampValid)
            {
                log.LogError("Validation failed - RequestVerification failed");
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
