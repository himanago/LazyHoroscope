using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Alexa.NET.Response;
using Google.Cloud.Dialogflow.V2;
using CEK.CSharp.Models;

namespace LazyHoroscope.Models
{
    public class IntentResult
    {
        private readonly List<string> messages = new List<string>();

        public bool ShouldEndSession { get; set; }

        public void AddMessage(string message) => messages.Add(message);

        public SkillResponse GetAlexaResponse()
        {
            return new SkillResponse()
            {
                Version = "1.0",
                Response = new ResponseBody()
                {
                    OutputSpeech = new PlainTextOutputSpeech
                    {
                        Text = string.Join('.', messages)
                    },
                    ShouldEndSession = ShouldEndSession
                }
            };
        }

        public CEKResponse GetClovaResponse()
        {
            var res = new CEKResponse()
            {
                ShouldEndSession = ShouldEndSession
            };
            messages.ForEach(m => res.AddText(m));
            return res;
        }

        public WebhookResponse GetGoogleResponse()
        {
            var webhookResponse = new WebhookResponse();
            webhookResponse.FulfillmentText = string.Join('.', messages);
            return webhookResponse;
        }
    }
}
