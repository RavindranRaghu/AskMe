using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace ChaT.ai.bLogic
{
    public class AskMeSpeech
    {

        public string SpeechToText(byte[] buffer)
        {
            string requestUri = "https://speech.platform.bing.com/speech/recognition/conversation/cognitiveservices/v1?language=en-US&format=detailed";
            HttpWebRequest request = null;
            request = (HttpWebRequest)HttpWebRequest.Create(requestUri);
            request.SendChunked = true;
            request.Accept = @"application/json;text/xml";
            request.Method = "POST";
            request.ProtocolVersion = HttpVersion.Version11;
            request.ContentType = @"audio/wav; codec=audio/pcm; samplerate=16000";
            request.Headers["Ocp-Apim-Subscription-Key"] = "7bae8a4d0172428d8ecfb906d17cdd56";

            // Send an audio file by 1024 byte chunks
            int bytesRead = 0;
            using (Stream requestStream = request.GetRequestStream())
            {
                /*
                * Read 1024 raw bytes from the input audio file.
                */
                requestStream.Write(buffer, 0, bytesRead);

                // Flush
                requestStream.Flush();
            }

            string responseString = string.Empty;
            using (WebResponse response = request.GetResponse())
            {
                Console.WriteLine(((HttpWebResponse)response).StatusCode);

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    responseString = sr.ReadToEnd();
                }

                Console.WriteLine(responseString);
                Console.ReadLine();
            }

            return responseString;
        }
    }

}

