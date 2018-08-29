using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ChaT.db;
using System.Collections;
using AskMe.ai;
using ChaT.ai.bLogic;
using System.IO;
using ChaT.ai.Dto;
using System.Net;
using Newtonsoft.Json;

namespace ChaT.ai.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        private ChatDatabaseModel db;

        public HomeController()
        {
            db = new ChatDatabaseModel(); 
        }

        public ActionResult Index()
        {
            ViewBag.nodeLevel = 0;
            List<ChatIntent> intentList = db.ChatIntent.Where(x => x.ParentId == 0 & x.ChatIntentId > 2).ToList();
            return View(intentList);
        }

        public ActionResult Chat(string sender, string message, int node)
        {
            AskMeChannel channel = new AskMeChannel(message, node);
            KeyValuePair<int, string> responseMessage = channel.ChatInitializer();
            node = responseMessage.Key;
            List<string> suggest = new List<string>();
            var hasSuggest = db.ChatIntent.Where(x => x.ParentId == node && x.ChatIntentId > 2).Select(y => y.IntentName);
            if (hasSuggest.Any())
            {
                suggest = hasSuggest.ToList();
            }
            else
            {
                suggest = db.ChatIntent.Where(x => x.ParentId == 0 && x.ChatIntentId > 2 ).Select(y => y.IntentName).ToList();
            }
            var result = new { node = node, response = responseMessage.Value, suggest = suggest };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Intent()
        {
            List<ChatIntent> intent = db.ChatIntent.ToList();
            return View(intent); 
        }

        public ActionResult ask()
        {
            List<AskDto> question = (from ques in db.ChatIntentQuestion
                                     join inte in db.ChatIntent on ques.ChatIntentId equals inte.ChatIntentId
                                     select new AskDto
                                     {
                                         QuestionId = ques.QuestionId,
                                         QuestionDesc = ques.QuestionDesc,
                                         ChatIntentId = inte.ChatIntentId,
                                         ChatIntentName = inte.IntentName,
                                         UpdatedDate = ques.UpdatedDate
                                     }).ToList();
            return View(question);
        }

        public JsonResult ProjectUpdate(AskOperation askOperation)
        {
            AskDto ask = askOperation.ask;
            string operation = askOperation.Operation;
            ChatIntentQuestion question = new ChatIntentQuestion();
            bool changed = false;
            ask.UpdatedDate = DateTime.UtcNow;
            try
            {
                if (operation == "a")
                {
                    question.QuestionDesc = ask.QuestionDesc;
                    question.ChatIntentId = ask.ChatIntentId;
                    question.UpdatedDate = DateTime.Now;
                    db.ChatIntentQuestion.Add(question);
                }
                else if (operation == "u")
                {
                    question = db.ChatIntentQuestion.Where(x => x.QuestionId == ask.QuestionId).FirstOrDefault();
                    question.QuestionDesc = ask.QuestionDesc;
                    question.ChatIntentId = ask.ChatIntentId;
                    question.UpdatedDate = DateTime.Now;
                }
                else
                {
                    question = db.ChatIntentQuestion.Where(x => x.QuestionId == ask.QuestionId).FirstOrDefault();
                    db.ChatIntentQuestion.Attach(question);
                    db.ChatIntentQuestion.Remove(question);
                }
                changed = true;
                db.SaveChanges();
                return Json(changed, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)

            {
                Console.WriteLine(e.Message);
                return Json(changed, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult Voice()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadAudio()
        {
            string content = "";
            string text = string.Empty;
            var file = Request.Files[0];
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
            Guid guid = Guid.NewGuid();
            string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content\\sound\\myRecording01.wav");
            string filepath2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content\\sound\\"+ guid.ToString()+".wav");
            file.SaveAs(filepath2);
            using (FileStream fs = new FileStream(filepath2, FileMode.Open, FileAccess.Read))
            {

                /*
                * Open a request stream and write 1024 byte chunks in the stream one at a time.
                */
                byte[] buffer = null;
                int bytesRead = 0;
                using (Stream requestStream = request.GetRequestStream())
                {
                    /*
                    * Read 1024 raw bytes from the input audio file.
                    */
                    buffer = new Byte[checked((uint)Math.Min(1024, (int)fs.Length))];
                    while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        requestStream.Write(buffer, 0, bytesRead);
                    }

                    // Flush
                    requestStream.Flush();
                }
            }

            string responseString = string.Empty;
            using (WebResponse response = request.GetResponse())
            {
                Console.WriteLine(((HttpWebResponse)response).StatusCode);

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    responseString = sr.ReadToEnd();

                }                
            }
            string finalresponse = string.Empty;
            SpeechResponseDto speechResponse = JsonConvert.DeserializeObject<SpeechResponseDto>(responseString);
            if (speechResponse.Duration > 0)
                finalresponse = speechResponse.nBest[0].Display;
            else
                finalresponse = "Sorry i did not understand";

            if (System.IO.File.Exists(filepath2))
            {
                System.IO.File.Delete(filepath2);
            }

            return Json(finalresponse, JsonRequestBehavior.AllowGet);
        }
    }
}