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

            #region Declaration
            AskMeContentManager contentManager = new AskMeContentManager();
            AskMeCommon common = new AskMeCommon(message, node);
            string finalResponse = string.Empty;
            string phoneNumber = string.Empty;            
            ChatIntent responseIntent = new ChatIntent();
            List<ChatIntent> intentList = db.ChatIntent.ToList();
            ChatSession chatSession = db.ChatSession.Where(x => x.SessionId == sessionId).FirstOrDefault();

            #endregion

            #region Prerequsite Check
            if (common.CheckEmptyMessage())
            {
                finalResponse = "Sorry i did not understand";
                List<string> suggestforEmpty = common.GetSuggestionList();
                var resultforEmpty = new { node = node, response = finalResponse, suggest = suggestforEmpty };
                return Json(resultforEmpty, JsonRequestBehavior.AllowGet);
            }
            else
            {
                message = message.Trim();
            }
            #endregion             

            #region Has Only One Intent with Entity
            var hasEntity = (from inte in db.ChatIntent        
                             join ent in db.ChatEntity on inte.ChatIntentId equals ent.ChatIntentId
                             where inte.ChatIntentId == node
                             select inte).ToList();

            // Check if Chat has one Intent with Entity

            var hasUnrecognizedEntity = db.ChatSessionEntity.Where(x => x.SessionId == sessionId && x.EntityType == "unrecog").ToList();

            if (hasEntity.Count>0 && hasUnrecognizedEntity.Count > 0)
            {
                List<ChatSessionEntity> entityRecognized = hasUnrecognizedEntity;                

                AskMeEntityExtraction entityMatch = new AskMeEntityExtraction(message, node,sessionId);
                KeyValuePair<int, string> oneEntityResponse = new KeyValuePair<int, string>();
                if (entityRecognized.Where(x => x.EntityName.Contains("PASSCODE")).Any())
                {
                    oneEntityResponse = entityMatch.CheckIfAtleastOneEntitywithPasscode(entityRecognized);
                }
                else
                {
                    oneEntityResponse = entityMatch.CheckIfAtleastOneEntityHasValue(entityRecognized);
                }
                finalResponse = replaceParam(oneEntityResponse.Value);
                node = oneEntityResponse.Key;

                List<string> suggestforEntity = common.GetSuggestionList(node);
                var resultforEntity = new { node = node, response = finalResponse, suggest = suggestforEntity };
                return Json(resultforEntity, JsonRequestBehavior.AllowGet);
            }
            #endregion

            var hasOneChildIntent = (from inte in db.ChatIntent                                          
                                          where inte.ParentId == node
                                          select inte).ToList();

            // Check if Chat has one Intent with Entity
            if (hasOneChildIntent.Count ==1)
            {
                ChatIntent childIntent = hasOneChildIntent.FirstOrDefault();

                var hasOneIntentwithEntity = (from ent in db.ChatEntity
                                              where ent.ChatIntentId == childIntent.ChatIntentId
                                              select ent).ToList();
                if (hasOneIntentwithEntity.Count > 0)
                {
                    List<ChatSessionEntity> entityRecognized = new List<ChatSessionEntity>();
                    List<ChatEntity> possibleEntities = hasOneIntentwithEntity;
                    foreach (ChatEntity entity in possibleEntities)
                    {
                        ChatSessionEntity recognized = new ChatSessionEntity();
                        recognized.SessionId = sessionId;
                        recognized.EntityType = "unrecog";
                        recognized.EntityName = entity.EntityName;
                        recognized.EntityValue = entity.EntityDescription;
                        entityRecognized.Add(recognized);
                        db.ChatSessionEntity.Add(recognized);
                    }
                    db.SaveChanges();
                    AskMeEntityExtraction entityMatch = new AskMeEntityExtraction(message, childIntent.ChatIntentId, sessionId);
                    KeyValuePair<int, string> oneEntityResponse = new KeyValuePair<int, string>();
                    if (entityRecognized.Where(x=>x.EntityName.Contains("PASSCODE")).Any())
                    {
                        oneEntityResponse = entityMatch.CheckIfAtleastOneEntitywithPasscode(entityRecognized);
                    }
                    else
                    {
                        oneEntityResponse = entityMatch.CheckIfAtleastOneEntityHasValue(entityRecognized);
                    }                                    
                    finalResponse = replaceParam(oneEntityResponse.Value);
                    node = oneEntityResponse.Key;

                    List<string> suggestforEntity = common.GetSuggestionList(node);
                    var resultforEntity = new { node = node, response = finalResponse, suggest = suggestforEntity };
                    return Json(resultforEntity, JsonRequestBehavior.AllowGet);
                }
            }

            #region Main Channel
            AskMeChannel channel = new AskMeChannel(message, node, sessionId);            
            responseIntent = channel.ChatInitializer();            
            bool hasRedirect = (responseIntent.RedirectIntent.HasValue) ? true : false;            

            node = responseIntent.ChatIntentId;
            finalResponse = responseIntent.Response;

            if (responseIntent.NeedAuth)
            {
                bool hasBeenAuth = chatSession.isAuth && chatSession.SessionStart.AddDays(2) > DateTime.UtcNow;                
                if (!hasBeenAuth)
                {
                    ChatIntent authIntent = intentList.Where(x => x.IntentName.ToLower().Contains("auth")).FirstOrDefault();
                    node = authIntent.ChatIntentId;
                    finalResponse = authIntent.Response;
                    chatSession.IntentBeforeAuth = responseIntent.ChatIntentId;
                    db.SaveChanges();
                }
            }

            if (hasRedirect) // askpaymentspecialist)
            {
                int redirectIntentId = Convert.ToInt32(responseIntent.RedirectIntent);
                ChatIntent redirectIntent = intentList.Where(x => x.ChatIntentId == redirectIntentId).FirstOrDefault();                
                node = redirectIntent.ChatIntentId;
                finalResponse = redirectIntent.Response;                
            }

            finalResponse = replaceParam(finalResponse);

            // Get Suggestions List
            List<string> suggest = common.GetSuggestionList(node);
            var result = new { node = node, response = finalResponse, suggest = suggest };
            return Json(result, JsonRequestBehavior.AllowGet);
            #endregion
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

        private string replaceParam(string response)
        {
            if (!string.IsNullOrEmpty(response))
            {
                string custName = db.ChatParameter.Where(x => x.ParameterName == "customername").Select(x => x.ParameterValue).FirstOrDefault();
                response = response.Replace("parametercustomername", custName);
            }
            return response;
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