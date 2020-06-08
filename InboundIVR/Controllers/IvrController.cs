using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio.AspNet.Common;
using Twilio.AspNet.Mvc;
using Twilio.TwiML;
using Twilio.TwiML.Voice;
using Twilio.Rest.Api.V2010.Account;

namespace InboundIVR.Controllers
{
    public class IVRController : TwilioController
    {
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
        public ActionResult Menu(string digits)
        {
            var selectedOption = digits;

            switch(digits)
            {
                case "1":
                    return ReturnAddressHours();
                case "2":
                    return DialRepresentative();
                default:
                    return RedirectMain();
            }
        }

        // POST: IVR/Address
        public ActionResult Address(string digits)
        {
            var selectedOption = digits;

            switch(digits)
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
                action: Url.ActionUri("Menu", "IVR"),
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

            response.Redirect(Url.ActionUri("EntryRetry", "IVR"));

            return TwiML(response);
        }

        private TwiMLResult ReturnAddressHours()
        {
            var response = new VoiceResponse();

            response.Say("Our address is 123 Lincoln St, Omaha, Nebraska 56789. " +
                "We are open Monday through Friday, from 8 A M to 5 P M, Eastern Standard Time.");

            var gather = new Gather(
                action: Url.ActionUri("Address", "IVR"),
                numDigits: 1,
                timeout: 5);

            gather.Say("To hear this again, press 1. To return to the main menu, press 2.");

            response.Append(gather);

            response.Say("Thank you for calling. Have a great day.");

            response.Hangup();

            return TwiML(response);
        }

        private TwiMLResult DialRepresentative()
        {
            var response = new VoiceResponse();

            response.Say("Please wait one moment while I connect you with the next available representative.");

            response.Redirect(new Uri("http://com.twilio.sounds.music.s3.amazonaws.com/index.xml"));

            return TwiML(response);
        }

        private TwiMLResult Hangup()
        {
            var response = new VoiceResponse();

            response.Hangup();

            return TwiML(response);
        }

        private async void UpdateCallAfterSleep(string callSid)
        {
            // Perform any additional API calls here, such as looking
            // up which representative should receive the call
            await System.Threading.Tasks.Task.Delay(5000);

            var dial = new Dial(
                callerId: "+12062029455",
                number: "+15551234567",
                action: Url.ActionUri("Index", "DialStatus"));

            response.Append(dial);

        }
    }
}