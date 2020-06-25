using System;
using System.Configuration;
using Twilio;
using Twilio.AspNet.Common;
using Twilio.AspNet.Mvc;
using Twilio.TwiML;
using Twilio.TwiML.Voice;
using System.Diagnostics;

namespace InboundIVR.Controllers
{
    public class QueueController : TwilioController
    {
        private readonly string BaseUrl = ConfigurationManager.AppSettings["BaseUrl"];
        private readonly Say.VoiceEnum Voice = Say.VoiceEnum.PollyJoanna;
        private readonly Say.LanguageEnum Language = Say.LanguageEnum.EnUs;
        private readonly int MaxLoops = 10;
        private readonly Uri HoldMusic;

        public QueueController()
        {
            var accountSid = ConfigurationManager.AppSettings["AccountSid"];
            var authToken = ConfigurationManager.AppSettings["AuthToken"];

            HoldMusic = new Uri(ConfigurationManager.AppSettings["HoldMusicUrl"]);

            TwilioClient.Init(accountSid, authToken);
        }

        // POST: Queue/Entry
        public TwiMLResult Entry()
        {
            var response = new VoiceResponse();

            response.Play(HoldMusic);

            var gather = new Gather(
                action: new Uri($"{BaseUrl}/Queue/VoicemailResponse?loopCount=1"),
                numDigits: 1,
                timeout: 5,
                actionOnEmptyResult: true);

            var message = "If you would like to leave a voicemail, press 1. "
                + "Otherwise, please continue to hold for a representative.";
            gather.Say(message, Voice, null, Language);

            response.Append(gather);

            return TwiML(response);
        }

        // POST: Queue/VoicemailResponse
        public TwiMLResult VoicemailResponse(VoiceRequest request, int loopCount)
        {
            var response = new VoiceResponse();

            var selectedOption = request.Digits;
            Debug.WriteLine($"Digits: {selectedOption}, LoopCount: {loopCount}");

            switch(selectedOption)
            {
                case "1":
                    {
                        var message = "Please leave your message after the tone.";
                        response.Say(message, Voice, null, Language);

                        response.Record(
                            action: new Uri($"{BaseUrl}/Queue/RecordingComplete"),
                            recordingStatusCallback: new Uri($"{BaseUrl}/RecordingStatus"),
                            timeout: 10);

                        break;
                    }
                default:
                    {
                        if (loopCount == MaxLoops)
                        {
                            var message = "We're sorry, we're unable to take your call at this time. "
                                + "Please try your call again later. Goodbye.";
                            response.Say(message, Voice, null, Language);
                        }
                        else
                        {
                            var newLoopCount = loopCount + 1;
                            response.Play(HoldMusic);

                            var gather = new Gather(
                                action: new Uri($"{BaseUrl}/Queue/VoicemailResponse?loopCount={newLoopCount}"),
                                numDigits: 1,
                                timeout: 5,
                                actionOnEmptyResult: true);

                            var message = "If you would like to leave a voicemail, press 1. "
                                + "Otherwise, please continue to hold for a representative.";
                            gather.Say(message, Voice, null, Language);

                            response.Append(gather);
                        }

                        break;
                    }
            }

            return TwiML(response);
        }

        // POST Queue/RecordingComplete
        public TwiMLResult RecordingComplete()
        {
            var response = new VoiceResponse();

            var message = "Your message has been recorded. Thank you for calling. Goodbye.";
            response.Say(message, Voice, null, Language);

            response.Hangup();

            return TwiML(response);
        }
    }
}