using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio.AspNet.Common;
using Twilio.AspNet.Mvc;
using Twilio.TwiML;
using Twilio.TwiML.Voice;

namespace InboundIVR.Controllers
{
    public class DialStatusController : TwilioController
    {
        // POST: DialStatus
        public TwiMLResult Index(VoiceRequest request)
        {
            var response = new VoiceResponse();

            switch(request.DialCallStatus)
            {
                case "busy":
                case "no-answer":
                case "failed":
                    response.Say("We were unable to reach a representative. Please try your call again later.");
                    response.Hangup();
                    break;
                default:
                    response.Hangup();
                    break;
            }

            return TwiML(response);
        }
    }
}