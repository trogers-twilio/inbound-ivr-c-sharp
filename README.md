# Inbound IVR Sample Using C# Twilio SDK
Project is built using ASP.NET. The Twilio Quickstart guide for C#/.NET can be found here:

https://www.twilio.com/docs/voice/quickstart/csharp

## Overview
This app provides an example of a simple IVR built with TwiML, leveraging the Twilio C#/.NET SDK for TwiML generation. It includes an example of connecting a caller with another phone number, and error handling iff that party is unreachable for some reason.

## Config Files and AppSettings
* web.config
  * BaseUrl: The base domain of the web service hosting this app
  * FromNumber: The Twilio number to use as the caller ID when making a call
  * ToNumberA: The first number to try connecting a caller to if they choose to speak with someone
  * ToNumberB: The second number to try connecting a caller to if the call to NumberA failed for any reason
* web.SECRETS.config
  * AccountSid: The account SID of the Twilio project to use in this app
  * AuthToken: The auth token of the Twilio project to use in this app