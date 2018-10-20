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

        public ActionResult Chatold(string sender, string message, int node)
        {
            AskMeContentManager contentManager = new AskMeContentManager();
            if (message.Length > 0)
            {
                message = message.Trim();
            }
            message = message.ToLower();
            string finalResponse = string.Empty;
            string phoneNumber = string.Empty;
            KeyValuePair<int, string> responseMessage = new KeyValuePair<int, string>();

            var hasOneIntentwithEntity = (from inte in db.ChatIntent
                                          where inte.ParentId == node
                                          && inte.IntentName.Contains("entity")
                                          select inte).ToList();

            // Check if Chat has one Intent with Entity
            if (hasOneIntentwithEntity.Count == 1)
            {
                ChatIntent intent = hasOneIntentwithEntity.FirstOrDefault();
                ChatEntity entity = db.ChatEntity.Where(x => x.ChatIntentId == intent.ChatIntentId).FirstOrDefault();
                AskMeEntityMatch entityMatch = new AskMeEntityMatch(message, node);
                EntityIdentifiedDto entityIdentifedDto = new EntityIdentifiedDto();
                if (entity.EntityName != null)
                {
                    entityIdentifedDto = entityMatch.HasOneChildIntentWithOneEntity(entity, intent);
                    finalResponse = entityIdentifedDto.ChatResponse;
                }
                else
                {
                    finalResponse = intent.Response;
                }
                node = intent.ChatIntentId;
            }
            else // 
            {
                AskMeChannel channel = new AskMeChannel(message, node);
                responseMessage = channel.ChatInitializer();
                node = responseMessage.Key;
                finalResponse = responseMessage.Value;

                if (finalResponse == "triggerpaymentflow") // askpaymentspecialist)
                {
                    channel = new AskMeChannel("askpaymentspecialist", node);
                    responseMessage = channel.ChatInitializer();
                    node = responseMessage.Key;
                    finalResponse = responseMessage.Value;
                }

                if (!string.IsNullOrEmpty(finalResponse))
                {
                    string custName = db.ChatParameter.Where(x => x.ParameterName == "customername").Select(x => x.ParameterValue).FirstOrDefault();
                    finalResponse = finalResponse.Replace("parametercustomername", custName);
                }
            }

            // Get Suggestions List
            List<string> suggest = new List<string>();
            var hasSuggest = db.ChatIntent.Where(x => x.ParentId == node && x.ChatIntentId > 2).Select(y => y.IntentDescription);
            if (hasSuggest.Any())
            {
                suggest = hasSuggest.ToList();
            }
            else
            {
                suggest = db.ChatIntent.Where(x => x.ParentId == 0 && x.ChatIntentId > 2).Select(y => y.IntentDescription).ToList();
            }
            var result = new { node = node, response = finalResponse, suggest = suggest };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Chat(string sender, string message, int node)
        {

            #region Declaration
            AskMeContentManager contentManager = new AskMeContentManager();
            AskMeCommon common = new AskMeCommon(message, node);
            string finalResponse = string.Empty;
            string phoneNumber = string.Empty;
            KeyValuePair<int, string> responseMessage = new KeyValuePair<int, string>();
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

            if (hasEntity.Count>0 && Session[node.ToString()] != null)
            {
                List<EntityRecognized> entityRecognized = new List<EntityRecognized>();
                entityRecognized = (List<EntityRecognized>)Session[node.ToString()];

                AskMeEntityExtraction entityMatch = new AskMeEntityExtraction(message, node);
                KeyValuePair<int, string> oneEntityResponse = new KeyValuePair<int, string>();
                if (entityRecognized.Where(x => x.EntityName.Contains("PASSCODE")).Any())
                {
                    oneEntityResponse = entityMatch.CheckIfAtleastOneEntitywithPasscode(entityRecognized);
                }
                else
                {
                    oneEntityResponse = entityMatch.CheckIfAtleastOneEntityHasValue(entityRecognized);
                }
                finalResponse = oneEntityResponse.Value;
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
                    List<EntityRecognized> entityRecognized = new List<EntityRecognized>();
                    List<ChatEntity> possibleEntities = hasOneIntentwithEntity;
                    foreach (ChatEntity entity in possibleEntities)
                    {
                        EntityRecognized recognized = new EntityRecognized();
                        recognized.EntityType = "unrecog";
                        recognized.EntityName = entity.EntityName;
                        recognized.EntityValue = entity.EntityDescription;
                        entityRecognized.Add(recognized);
                    }

                    AskMeEntityExtraction entityMatch = new AskMeEntityExtraction(message, childIntent.ChatIntentId);
                    KeyValuePair<int, string> oneEntityResponse = new KeyValuePair<int, string>();
                    if (entityRecognized.Where(x=>x.EntityName.Contains("PASSCODE")).Any())
                    {
                        oneEntityResponse = entityMatch.CheckIfAtleastOneEntitywithPasscode(entityRecognized);
                    }
                    else
                    {
                        oneEntityResponse = entityMatch.CheckIfAtleastOneEntityHasValue(entityRecognized);
                    }                                    
                    finalResponse = oneEntityResponse.Value;
                    node = oneEntityResponse.Key;

                    List<string> suggestforEntity = common.GetSuggestionList(node);
                    var resultforEntity = new { node = node, response = finalResponse, suggest = suggestforEntity };
                    return Json(resultforEntity, JsonRequestBehavior.AllowGet);
                }
            }

            #region Main Channel
            AskMeChannel channel = new AskMeChannel(message, node);
            responseMessage = channel.ChatInitializer();
            node = responseMessage.Key;
            finalResponse = responseMessage.Value;

            if (finalResponse == "triggerpaymentflow") // askpaymentspecialist)
            {
                channel = new AskMeChannel("askpaymentspecialist", node);
                responseMessage = channel.ChatInitializer();
                node = responseMessage.Key;
                finalResponse = responseMessage.Value;
            }

            if (!string.IsNullOrEmpty(finalResponse))
            {
                string custName = db.ChatParameter.Where(x => x.ParameterName == "customername").Select(x => x.ParameterValue).FirstOrDefault();
                finalResponse = finalResponse.Replace("parametercustomername", custName);
            }

            // Get Suggestions List
            List<string> suggest = common.GetSuggestionList(node);
            var result = new { node = node, response = finalResponse, suggest = suggest };
            return Json(result, JsonRequestBehavior.AllowGet);
            #endregion
        }

        public ActionResult Index()
        {
            ViewBag.nodeLevel = 0;
            ViewBag.Welcome = db.ChatIntent.Where(x => x.ChatIntentId == 0).Select(x => x.Response).FirstOrDefault();
            List<ChatIntent> intentList = db.ChatIntent.Where(x => x.ParentId == 0 & x.ChatIntentId > 2).ToList();
            return View(intentList);
        }

        public ActionResult Index1()
        {
            ViewBag.nodeLevel = 0;
            ViewBag.Welcome = db.ChatIntent.Where(x => x.ChatIntentId == 0).Select(x => x.Response).FirstOrDefault();
            List<ChatIntent> intentList = db.ChatIntent.Where(x => x.ParentId == 0 & x.ChatIntentId > 2).ToList();
            return View(intentList);
        }

        public ActionResult Index2()
        {
            ViewBag.nodeLevel = 0;
            ViewBag.Welcome = db.ChatIntent.Where(x => x.ChatIntentId == 0).Select(x => x.Response).FirstOrDefault();
            List<ChatIntent> intentList = db.ChatIntent.Where(x => x.ParentId == 0 & x.ChatIntentId > 2).ToList();
            return View(intentList);
        }

        [HttpGet]
        public ActionResult Login()
        {
            if (Session["loginsuccess"] != null)
            {
                if (Session["loginsuccess"].ToString() == "yes")
                {
                    return RedirectToAction("Admin");
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
                return RedirectToAction("Admin");
            }
            else
            {
                Session["loginsuccess"] = "no";
            }

            return View();
        }

        [AskMeAuthorize()]
        public ActionResult Intent()
        {
            List<ChatIntent> intent = db.ChatIntent.ToList();
            List<ChatIntentDto> intents = (from inte in db.ChatIntent
                                           join par in db.ChatIntent on inte.ParentId equals par.ChatIntentId
                                           select new ChatIntentDto
                                           {
                                               ChatIntentId = inte.ChatIntentId,
                                               IntentName = inte.IntentName,
                                               IntentDescription = inte.IntentDescription,
                                               ParentId = inte.ParentId,
                                               ParentName = par.IntentName,
                                               Response = inte.Response,
                                               UpdatedDate = inte.UpdatedDate
                                           }).OrderBy(y => y.ChatIntentId).ToList();

            List<SelectListItem> intentNames = db.ChatIntent.ToList().Select(u => new SelectListItem
            {
                Text = u.IntentName,
                Value = u.ChatIntentId.ToString()
            }).ToList();

            ViewBag.intentNames = intentNames;

            return View(intents);
        }

        [AskMeAuthorize()]
        public ActionResult ask()
        {

            AskDtoWithIntentList askWithIntent = new AskDtoWithIntentList();
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

            List<SelectListItem> intents = db.ChatIntent.ToList().Select(u => new SelectListItem
            {
                Text = u.IntentName,
                Value = u.ChatIntentId.ToString()
            }).ToList();

            askWithIntent.askList = question;
            askWithIntent.intents = intents;

            ViewBag.intents = intents;

            return View(question);
        }

        [AskMeAuthorize()]
        public ActionResult param()
        {
            return View(db.ChatParameter.ToList());
        }

        [AskMeAuthorize()]
        public ActionResult Entity()
        {
            List<EntitytDto> entity = (from ent in db.ChatEntity
                                       join intent in db.ChatIntent on ent.ChatIntentId equals intent.ChatIntentId
                                       select new EntitytDto
                                       {
                                           ChatEntityId = ent.ChatEntityId,
                                           EntityName = ent.EntityName,
                                           EntityDescription = ent.EntityDescription,
                                           ChatIntentId = ent.ChatIntentId,
                                           ChatIntentName = intent.IntentName,
                                           UpdatedDate = ent.UpdatedDate
                                       }).ToList();
            List<SelectListItem> intents = db.ChatIntent.ToList().Select(u => new SelectListItem
            {
                Text = u.IntentName,
                Value = u.ChatIntentId.ToString()
            }).ToList();

            ViewBag.intents = intents;

            return View(entity);
        }

        [AskMeAuthorize()]
        public ActionResult fail()
        {
            List<SelectListItem> intentNames = db.ChatIntent.ToList().Select(u => new SelectListItem
            {
                Text = u.IntentName,
                Value = u.ChatIntentId.ToString()
            }).ToList();

            ViewBag.intentNames = intentNames;
            return View(db.ChatFailureResponse.ToList());
        }

        [AskMeAuthorize()]
        public ActionResult Admin()
        {
            FeatureDto featureDto = new FeatureDto();
            featureDto.feature = db.ChatFeatureList.ToList();
            featureDto.subfeature = db.ChatSubFeatureList.ToList();
            featureDto.FeaturesDeveloped = featureDto.subfeature.Where(x => x.DevelopmentComplete).Count();
            if (featureDto.subfeature.Where(x => x.EffortActual > 0).Any())
            {
                featureDto.ActualHrs = featureDto.subfeature.Select(x => x.EffortActual).ToList().Sum();
            }
            else
            {
                featureDto.ActualHrs = 0;
            }

            return View(featureDto);
        }

        public JsonResult QuestionUpdate(AskOperation askOperation)
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

        public JsonResult IntentUpdate(IntentOperation intOperation)
        {
            ChatIntentDto intent = intOperation.intent;
            string operation = intOperation.Operation;
            ChatIntent finalIntent = new ChatIntent();
            bool changed = false;
            intent.UpdatedDate = DateTime.UtcNow;
            try
            {
                if (operation == "a")
                {
                    finalIntent.IntentName = intent.IntentName;
                    finalIntent.IntentDescription = intent.IntentDescription;
                    finalIntent.Response = intent.Response;
                    finalIntent.ParentId = intent.ParentId;
                    finalIntent.UpdatedDate = DateTime.Now;
                    db.ChatIntent.Add(finalIntent);
                }
                else if (operation == "u")
                {
                    finalIntent = db.ChatIntent.Where(x => x.ChatIntentId == intent.ChatIntentId).FirstOrDefault();
                    finalIntent.IntentName = intent.IntentName;
                    finalIntent.IntentDescription = intent.IntentDescription;
                    finalIntent.Response = intent.Response;
                    finalIntent.ParentId = intent.ParentId;
                    finalIntent.UpdatedDate = DateTime.Now;
                }
                else
                {
                    finalIntent = db.ChatIntent.Where(x => x.ChatIntentId == intent.ChatIntentId).FirstOrDefault();
                    db.ChatIntent.Attach(finalIntent);
                    db.ChatIntent.Remove(finalIntent);
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

        public JsonResult ParamUpdate(ParamOperation parmOperation)
        {
            ChatParameter param = parmOperation.param;
            string operation = parmOperation.Operation;
            ChatParameter finalParam = new ChatParameter();
            bool changed = false;
            param.UpdatedDate = DateTime.UtcNow;
            try
            {
                if (operation == "a")
                {
                    finalParam.ParameterName = param.ParameterName;
                    finalParam.ParameterValue = param.ParameterValue;
                    finalParam.UpdatedDate = DateTime.Now;
                    db.ChatParameter.Add(finalParam);
                }
                else if (operation == "u")
                {
                    finalParam = db.ChatParameter.Where(x => x.ParameterId == param.ParameterId).FirstOrDefault();
                    finalParam.ParameterName = param.ParameterName;
                    finalParam.ParameterValue = param.ParameterValue;
                    finalParam.UpdatedDate = DateTime.Now;
                }
                else
                {
                    finalParam = db.ChatParameter.Where(x => x.ParameterId == param.ParameterId).FirstOrDefault();
                    db.ChatParameter.Attach(finalParam);
                    db.ChatParameter.Remove(finalParam);
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

        public JsonResult EntityUpdate(EntityOperation entOperation)
        {
            EntitytDto entitydto = entOperation.entity;
            string operation = entOperation.Operation;
            ChatEntity entity = new ChatEntity();
            bool changed = false;
            entity.UpdatedDate = DateTime.UtcNow;
            try
            {
                if (operation == "a")
                {
                    entity.EntityName = entitydto.EntityName;
                    entity.EntityDescription = entitydto.EntityDescription;
                    entity.ChatIntentId = entitydto.ChatIntentId;
                    entity.UpdatedDate = DateTime.Now;
                    db.ChatEntity.Add(entity);
                }
                else if (operation == "u")
                {
                    entity = db.ChatEntity.Where(x => x.ChatEntityId == entitydto.ChatEntityId).FirstOrDefault();
                    entity.EntityName = entitydto.EntityName;
                    entity.EntityDescription = entitydto.EntityDescription;
                    entity.ChatIntentId = entitydto.ChatIntentId;
                    entity.UpdatedDate = DateTime.Now;
                }
                else
                {
                    entity = db.ChatEntity.Where(x => x.ChatEntityId == entitydto.ChatEntityId).FirstOrDefault();
                    db.ChatEntity.Attach(entity);
                    db.ChatEntity.Remove(entity);
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

        public JsonResult FailReviewUpdate(int questionId, string questionName, int ChatIntentId)
        {
            bool changed = true;
            try
            {
                ChatFailureResponse fail = db.ChatFailureResponse.Where(x => x.DefectId == questionId).FirstOrDefault();
                fail.Reviewed = true;
                fail.UpdatedDate = DateTime.Now;
                changed = true;

                ChatIntentQuestion question = new ChatIntentQuestion();
                question.QuestionDesc = questionName;
                question.ChatIntentId = ChatIntentId;
                db.ChatIntentQuestion.Add(question);

                db.SaveChanges();
                return Json(changed, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)

            {
                Console.WriteLine(e.Message);
                return Json(changed, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult FeatureUpdate(FeatureOperation featureOperation)
        {
            ChatFeatureList featureDto = featureOperation.feature;
            string operation = featureOperation.Operation;
            ChatFeatureList feature = new ChatFeatureList();
            bool changed = false;
            feature.UpdatedDate = DateTime.UtcNow;
            try
            {
                if (operation == "a")
                {
                    feature.FeatureName = featureDto.FeatureName;
                    feature.FeatureDesc = featureDto.FeatureDesc;
                    feature.StoryGroomed = featureDto.StoryGroomed;
                    feature.DevelopmentComplete = featureDto.DevelopmentComplete;
                    feature.UpdatedDate = DateTime.Now;
                    db.ChatFeatureList.Add(feature);
                }
                else if (operation == "u")
                {
                    feature = db.ChatFeatureList.Where(x => x.FeatureId == featureDto.FeatureId).FirstOrDefault();
                    feature.FeatureName = featureDto.FeatureName;
                    feature.FeatureDesc = featureDto.FeatureDesc;
                    feature.StoryGroomed = featureDto.StoryGroomed;
                    feature.DevelopmentComplete = featureDto.DevelopmentComplete;
                    feature.UpdatedDate = DateTime.Now;
                }
                else
                {
                    feature = db.ChatFeatureList.Where(x => x.FeatureId == featureDto.FeatureId).FirstOrDefault();
                    db.ChatFeatureList.Attach(feature);
                    db.ChatFeatureList.Remove(feature);
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

        public JsonResult SubFeatureUpdate(SubFeatureOperation subfeatureOperation)
        {
            ChatSubFeatureList featureDto = subfeatureOperation.subfeature;
            string operation = subfeatureOperation.Operation;
            ChatSubFeatureList feature = new ChatSubFeatureList();
            bool changed = false;
            feature.UpdatedDate = DateTime.UtcNow;
            try
            {
                if (operation == "a")
                {
                    feature.FeatureId = featureDto.FeatureId;
                    feature.SubFeatureName = featureDto.SubFeatureName;
                    feature.SubFeatureDesc = featureDto.SubFeatureDesc;
                    feature.StoryGroomed = featureDto.StoryGroomed;
                    feature.DevelopmentComplete = featureDto.DevelopmentComplete;
                    feature.EffortEstimated = featureDto.EffortEstimated;
                    feature.EffortActual = featureDto.EffortActual;
                    feature.UpdatedDate = DateTime.Now;
                    db.ChatSubFeatureList.Add(feature);
                }
                else if (operation == "u")
                {
                    feature = db.ChatSubFeatureList.Where(x => x.SubFeatureId == featureDto.SubFeatureId).FirstOrDefault();
                    feature.FeatureId = featureDto.FeatureId;
                    feature.SubFeatureName = featureDto.SubFeatureName;
                    feature.SubFeatureDesc = featureDto.SubFeatureDesc;
                    feature.StoryGroomed = featureDto.StoryGroomed;
                    feature.DevelopmentComplete = featureDto.DevelopmentComplete;
                    feature.EffortEstimated = featureDto.EffortEstimated;
                    feature.EffortActual = featureDto.EffortActual;
                    feature.UpdatedDate = DateTime.Now;
                }
                else
                {
                    feature = db.ChatSubFeatureList.Where(x => x.SubFeatureId == featureDto.SubFeatureId).FirstOrDefault();
                    db.ChatSubFeatureList.Attach(feature);
                    db.ChatSubFeatureList.Remove(feature);
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