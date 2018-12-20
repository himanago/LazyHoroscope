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
    public static class ClovaIntentHandlerFunction
    {
        [FunctionName("ClovaIntentHandlerFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var response = new CEKResponse();

            if (req.Headers.TryGetValue("SignatureCEK", out var signature))
            {
                var client = new ClovaClient();
                var request = await client.GetRequest(signature, req.Body);

                // IntentRequest�݂̂����ɂ���
                if (request.Request.Type == RequestType.IntentRequest)
                {
                    log.LogInformation(request.Request.Intent.Name);
                    switch (request.Request.Intent.Name)
                    {
                        case "ZodiacIntent":
                            // �肢���ʂ�Ԃ��B����͓K���肢�Ȃ̂Ń����_���ɂǂ��炩
                            // (�{���͂����ŃX���b�g�̐������󂯎���Ă���Ɋ�Â����������s��)
                            if (new System.Random().Next() % 2 == 0)
                            {
                                response.AddText("�����͐�D���I�ƂĂ��悢1���ɂȂ�܂���I");
                            }
                            else
                            {
                                response.AddText("�����͂��܂���ĂȂ������B�]�΂Ȃ��悤�ɋC��t���Ă��������ˁB");
                            }
                            response.ShouldEndSession = true;
                            break;

                        case "Clova.GuideIntent":
                            // �g����
                            response.AddText("���Ȃ��̐����������Ă��������B����Ă����܂��B");
                            response.ShouldEndSession = false;
                            break;

                        default:
                            // ���̑��̃C���e���g�̏ꍇ
                            response.AddText("�悭�킩��܂���ł������A�܂��܂����Ǝv���܂��B");
                            response.ShouldEndSession = false;
                            break;
                    }
                }
                else
                {
                    // �G���[
                    response.AddText("�悭�킩��܂���ł������A���Ԃ񃉃b�L�[��1���ɂȂ�Ǝv���܂���B");
                    response.ShouldEndSession = true;
                }
            }
            else
            {
                // �G���[
                response.AddText("�悭�킩��܂���ł������A�����Ƒ��v�ł���B");
                response.ShouldEndSession = true;
            }
            return new OkObjectResult(response);
        }
    }
}
