using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio;
using Twilio.AspNet.Common;
using Twilio.AspNet.Mvc;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using Twilio.TwiML.Voice;
using System.Configuration;
using System.Diagnostics;

namespace InboundIVR.Controllers
{
    public class DialStatusController : TwilioController
    {
        private readonly string BaseUrl = "https://3d09f9b20260.ngrok.io";

        public DialStatusController()
        {
            var accountSid = ConfigurationManager.AppSettings["AccountSid"];
            var authToken = ConfigurationManager.AppSettings["AuthToken"];

            TwilioClient.Init(accountSid, authToken);
        }

        // POST: DialStatus
        public TwiMLResult Index(VoiceRequest request)
        {
            var response = new VoiceResponse();

            switch(request.DialCallStatus)
            {
                case "busy":
                case "no-answer":
                case "failed":
                    response.Redirect(
                        new Uri("http://com.twilio.sounds.music.s3.amazonaws.com/index.xml"),
                        method: Twilio.Http.HttpMethod.Get);

                    System.Threading.Tasks.Task.Run(() => { UpdateCallAfterSleep(request.CallSid); });
                    break;
                default:
                    response.Hangup();
                    break;
            }

            return TwiML(response);
        }

        private async System.Threading.Tasks.Task UpdateCallAfterSleep(string callSid)
        {
            // Perform any additional API calls here, such as looking
            // up which representative should receive the call
            Debug.WriteLine("Waiting 5 seconds");
            await System.Threading.Tasks.Task.Delay(5000);
            Debug.WriteLine("Done waiting");

            var response = new VoiceResponse();

            response.Dial(
                callerId: "+12062029455",
                number: "+18136357750",
                action: new Uri($"{BaseUrl}/DialStatus"));

            var call = CallResource.Update(
                twiml: new Twilio.Types.Twiml(response.ToString()),
                pathSid: callSid);

            Debug.WriteLine(string.Format("Updated call {0}", call.Sid));
        }
    }
}