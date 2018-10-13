using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Speech.Recognition;
using System.Speech.AudioFormat;
using System.Threading.Tasks;

namespace ChaT.ai.bLogic
{
    public class AskMeSpeechOls
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

    public class AskMeSpeechtoText
    {
        private bool completed;

        public async void SpeechProcessing(HttpPostedFileBase file)

        // Initialize an in-process speech recognition engine.  
        {
            Guid guid = Guid.NewGuid();            
            string filepath2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content\\sound\\" + guid.ToString() + ".wav");
            file.SaveAs(filepath2);
            var cultureInfo = new System.Globalization.CultureInfo("en-US");
            using (SpeechRecognitionEngine recognizer =
               new SpeechRecognitionEngine())
            {

                // Create and load a grammar. 
                Choices choiceList = new Choices();

                choiceList.Add(new string[] { "Open", "Open", "Close", "Firefox" });
                Grammar grammar = new Grammar(choiceList);

                Grammar dictation = new DictationGrammar();
                dictation.Name = "Dictation Grammar";
                dictation.Enabled = true;

                DictationGrammar defaultDictationGrammar = new DictationGrammar();
                defaultDictationGrammar.Name = "default dictation";
                defaultDictationGrammar.Enabled = true;

                // Create the spelling dictation grammar.  
                DictationGrammar spellingDictationGrammar =
                  new DictationGrammar("grammar:dictation#spelling");
                spellingDictationGrammar.Name = "spelling dictation";
                spellingDictationGrammar.Enabled = true;

                // Create the question dictation grammar.  
                DictationGrammar customDictationGrammar =
                  new DictationGrammar("grammar:dictation");
                customDictationGrammar.Name = "question dictation";
                customDictationGrammar.Enabled = true;


                //await Task.Run(() => recognizer.LoadGrammar(dictation));
                //await Task.Run(() => recognizer.LoadGrammar(defaultDictationGrammar));
                //await Task.Run(() => recognizer.LoadGrammar(spellingDictationGrammar));
                //await Task.Run(() => recognizer.LoadGrammar(customDictationGrammar));
                await Task.Run(() => recognizer.LoadGrammar(grammar));                

                // Add a context to customDictationGrammar.  
                //customDictationGrammar.SetDictationContext("How do you", null);

                // Configure the input to the recognizer.  
                recognizer.SetInputToWaveFile(filepath2);

                // Attach event handlers for the results of recognition.  
                recognizer.SpeechRecognized +=
                  new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);
                recognizer.RecognizeCompleted +=
                  new EventHandler<RecognizeCompletedEventArgs>(recognizer_RecognizeCompleted);

                // Perform recognition on the entire file.  
                Console.WriteLine("Starting asynchronous recognition...");
                completed = false;
                recognizer.RecognizeAsync(RecognizeMode.Multiple);

                // Keep the console window open.  
                while (!completed)
                {
                    Console.ReadLine();
                }
                if (System.IO.File.Exists(filepath2))
                {
                    System.IO.File.Delete(filepath2);
                }
                Console.WriteLine("Done.");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        // Handle the SpeechRecognized event.  
        public void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result != null && e.Result.Text != null && e.Result.Confidence >= 0.7)
            {
                Console.WriteLine("  Recognized text =  {0}", e.Result.Text);
            }
            else
            {
                Console.WriteLine("  Recognized text not available.");
            }
        }

        // Handle the RecognizeCompleted event.  
        public void recognizer_RecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Console.WriteLine("  Error encountered, {0}: {1}",
                e.Error.GetType().Name, e.Error.Message);
            }
            if (e.Cancelled)
            {
                Console.WriteLine("  Operation cancelled.");
            }
            if (e.InputStreamEnded)
            {
                Console.WriteLine("  End of stream encountered.");
            }
            completed = true;
        }
    }
}


