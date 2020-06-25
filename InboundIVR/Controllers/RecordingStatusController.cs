using System.Web.Mvc;
using Twilio.AspNet.Common;
using Twilio.AspNet.Mvc;
using System.Diagnostics;

namespace InboundIVR.Controllers
{
    public class RecordingStatusController : TwilioController
    {
        // GET: RecordingStatus
        public ActionResult Index(VoiceRequest request)
        {
            // Handle the recording status here
            Debug.WriteLine($"Recording is {request.RecordingStatus} "
                + $"and is available at {request.RecordingUrl}");

            return View();
        }
    }
}