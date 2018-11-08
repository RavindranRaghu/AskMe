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
using System.Threading.Tasks;
using ChaT.ai.cAuth;

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

        public ActionResult Chat(int sessionId, string message, int node)
        {

            ChatResponseDto responseDto = new ChatResponseDto().GetChatResponse(sessionId, message, node);
            var result = new { node = responseDto.Node, response = responseDto.Message, suggest = responseDto.Suggestion };
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        public ActionResult StartChat()
        {
            AskMeCommon common = new AskMeCommon("hello", 0);
            List<string> suggest = common.GetSuggestionList(0);
            ChatSession session = new ChatSession();
            session.SessionStart = DateTime.UtcNow;
            session.SessionUd = "";
            session.isAuth = false;
            db.ChatSession.Add(session);
            db.SaveChanges();
            int sessionId = session.SessionId;
            string finalResponse = db.ChatIntent.Where(x => x.ChatIntentId == 0).Select(y=>y.Response).FirstOrDefault();
            var result = new { node = 0, response = finalResponse, suggest = suggest, sessionId= sessionId };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index()
        {
            //ViewBag.nodeLevel = 0;
            //ViewBag.Welcome = db.ChatIntent.Where(x => x.ChatIntentId == 0).Select(x => x.Response).FirstOrDefault();
            List<ChatIntent> intentList = db.ChatIntent.Where(x => x.ParentId == 0 & x.ChatIntentId > 2 & !x.IsRedirect).ToList();
            return View(intentList);
        }

        [HttpGet]
        public ActionResult Login()
        {
            if (Session["loginsuccess"] != null)
            {
                if (Session["loginsuccess"].ToString() == "yes")
                {
                    return RedirectToAction("index","Admin");
                }
            }
            return View();
        }

        [HttpPost]
        public ActionResult Login(string ud, string pd)
        {
            if (ud.ToLower() == "admin" && pd == "1234")
            {
                Session["loginsuccess"] = "yes";
                return RedirectToAction("index", "Admin");
            }
            else
            {
                Session["loginsuccess"] = "no";
            }

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> UploadAudio()
        {
            string text = string.Empty;
            var file = Request.Files[0];
            // Send an audio file by 1024 byte chunks
            //Guid guid = Guid.NewGuid();
            //string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content\\sound\\myRecording01.wav");
            //string filepath2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content\\sound\\" + guid.ToString() + ".wav");
            //string filepath3 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content\\sound\\9458bba2-e3ff-4c12-bdbd-f1435192ac5f.wav");
            string finalresponse = "hello";
            AskMeSpeechtoText speechtoText = new AskMeSpeechtoText();
            await Task.Run(() => speechtoText.SpeechProcessing(file));

            return Json(finalresponse, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UploadAudioBingSpeech()
        {
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
            request.Headers["Ocp-Apim-Subscription-Key"] = "a7e75b07b0064ed596d39fcdb21dd482";

            // Send an audio file by 1024 byte chunks
            Guid guid = Guid.NewGuid();
            string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content\\sound\\myRecording01.wav");
            string filepath2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content\\sound\\" + guid.ToString() + ".wav");
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