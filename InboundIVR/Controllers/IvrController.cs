using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Twilio;
using Twilio.AspNet.Common;
using Twilio.AspNet.Mvc;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using Twilio.TwiML.Voice;
using System.Diagnostics;

namespace InboundIVR.Controllers
{
    public class IVRController : TwilioController
    {
        private readonly string BaseUrl = ConfigurationManager.AppSettings["BaseUrl"];

        public IVRController()
        {
            var accountSid = ConfigurationManager.AppSettings["AccountSid"];
            var authToken = ConfigurationManager.AppSettings["AuthToken"];

            TwilioClient.Init(accountSid, authToken);
        }
        // POST: IVR/Entry
        public ActionResult Entry()
        {
            return ReturnEntry(true);
        }

        // POST: IVR/EntryRetry
        public ActionResult EntryRetry()
        {
            return ReturnEntry(false);
        }

        // POST: IVR/Menu 
        public ActionResult Menu(VoiceRequest request)
        {
            var selectedOption = request.Digits;

            switch(selectedOption)
            {
                case "1":
                    return ReturnAddressHours();
                case "2":
                    return DialRepresentative(request.CallSid);
                default:
                    return RedirectMain();
            }
        }

        // POST: IVR/Address
        public ActionResult Address(VoiceRequest request)
        {
            var selectedOption = request.Digits;

            switch(selectedOption)
            {
                case "1":
                    return ReturnAddressHours();
                case "2":
                    return ReturnEntry(false);
                default:
                    return Hangup();
            }
        }

        private TwiMLResult ReturnEntry(bool includeWelcome)
        {
            var response = new VoiceResponse();

            var gather = new Gather(
                action: new Uri($"{BaseUrl}/IVR/Menu"),
                numDigits: 1,
                timeout: 5,
                actionOnEmptyResult: true);

            var welcomeGreeting = "Hello, thank you for calling.";

            if (includeWelcome)
            {
                gather.Say(welcomeGreeting);
            }

            gather.Say("Press 1 for our address and hours, press 2 to speak with a representative.");

            response.Append(gather);

            return TwiML(response);
        }

        private TwiMLResult RedirectMain()
        {
            var response = new VoiceResponse();

            response.Say("I'm sorry, I didn't hear a selection. Please try again.");
            
            response.Redirect(new Uri($"{BaseUrl}/IVR/EntryRetry"));

            return TwiML(response);
        }

        private TwiMLResult ReturnAddressHours()
        {
            var response = new VoiceResponse();

            response.Say("Our address is 123 Lincoln St, Omaha, Nebraska 56789. " +
                "We are open Monday through Friday, from 8 A M to 5 P M, Eastern Standard Time.");
            
            var gather = new Gather(
                action: new Uri($"{BaseUrl}/IVR/Address"),
                numDigits: 1,
                timeout: 5);

            gather.Say("To hear this again, press 1. To return to the main menu, press 2.");

            response.Append(gather);

            response.Say("Thank you for calling. Have a great day.");

            response.Hangup();

            return TwiML(response);
        }

        private TwiMLResult DialRepresentative(string callSid)
        {
            var response = new VoiceResponse();

            response.Say("Please wait one moment while I connect you with the next available representative.");

            response.Redirect(
                new Uri("http://com.twilio.sounds.music.s3.amazonaws.com/index.xml"),
                method: Twilio.Http.HttpMethod.Get);
            
            System.Threading.Tasks.Task.Run(() => { UpdateCallAfterSleep(callSid); });

            return TwiML(response);
        }

        private TwiMLResult Hangup()
        {
            var response = new VoiceResponse();

            response.Hangup();

            return TwiML(response);
        }

        private async System.Threading.Tasks.Task UpdateCallAfterSleep(string callSid)
        {
            // Perform any additional API calls here, such as looking
            // up which representative should receive the call
            Debug.WriteLine("Waiting 10 seconds");
            await System.Threading.Tasks.Task.Delay(10000);
            Debug.WriteLine("Done waiting");

            var response = new VoiceResponse();

            response.Dial(
                callerId: ConfigurationManager.AppSettings["FromNumber"],
                number: ConfigurationManager.AppSettings["ToNumberA"],
                action: new Uri($"{BaseUrl}/DialStatus"));

            var call = CallResource.Update(
                twiml: new Twilio.Types.Twiml(response.ToString()),
                pathSid: callSid);

            Debug.WriteLine(string.Format("Updated call {0}", call.Sid));
        }
    }
}