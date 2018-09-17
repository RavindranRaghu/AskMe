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
            ViewBag.Welcome = db.ChatIntent.Where(x => x.ChatIntentId == 0).Select(x => x.Response).FirstOrDefault();
            List<ChatIntent> intentList = db.ChatIntent.Where(x => x.ParentId == 0 & x.ChatIntentId > 2).ToList();
            return View(intentList);
        }

        public ActionResult Chat(string sender, string message, int node)
        {
            message = message.ToLower();
            string finalResponse = string.Empty;
            string phoneNumber = string.Empty;
            KeyValuePair<int, string> responseMessage = new KeyValuePair<int, string>();
            ChatIntent nodeDetail = db.ChatIntent.Where(x => x.ChatIntentId == node).FirstOrDefault();
            if (nodeDetail.IntentName == "askpaymentspecialist")
            {
                Session["phone"] = message;
                nodeDetail = db.ChatIntent.Where(x => x.ParentId == node).FirstOrDefault();
                finalResponse = nodeDetail.Response;
                node = nodeDetail.ChatIntentId;
            }
            else if (nodeDetail.IntentName == "askphonenumber")
            {
                nodeDetail = db.ChatIntent.Where(x => x.ParentId == node).FirstOrDefault();
                if (Session["phone"] != null)
                    phoneNumber = Session["phone"].ToString();
                else
                    phoneNumber = "+1 980 233 7575";

                finalResponse = nodeDetail.Response;
                finalResponse = finalResponse.Replace("paramphonenumber", phoneNumber);
                finalResponse = finalResponse.Replace("paramappointmenttime", message);
                node = nodeDetail.ChatIntentId;
            }
            else
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

        public ActionResult param()
        {
            return View(db.ChatParameter.ToList());
        }

        public ActionResult Entity()
        {
            List<EntitytDto> entity = (from ent in db.ChatEntity
                                       join intent in db.ChatIntent on ent.ChatIntentId equals intent.ChatIntentId
                                       select new EntitytDto {
                                           ChatEntityId = ent.ChatEntityId,
                                           EntityName = ent.EntityName,
                                           EntityDescription = ent.EntityDescription,
                                           ChatIntentId = ent.ChatIntentId,
                                           ChatIntentName = intent.IntentName,
                                           UpdatedDate =  ent.UpdatedDate
                                       }).ToList();
            List<SelectListItem> intents = db.ChatIntent.ToList().Select(u => new SelectListItem
            {
                Text = u.IntentName,
                Value = u.ChatIntentId.ToString()
            }).ToList();

            ViewBag.intents = intents;

            return View(entity);
        }

        public ActionResult fail()
        {
            return View(db.ChatFailureResponse.ToList());
        }

        public ActionResult Admin()
        {
            FeatureDto featureDto = new FeatureDto();
            featureDto.feature = db.ChatFeatureList.ToList();
            featureDto.subfeature = db.ChatSubFeatureList.ToList();
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
                    finalParam = db.ChatParameter.Where(x => x.ParameterId== param.ParameterId).FirstOrDefault();
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
                    entity.EntityName = entitydto.EntityName ;
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

        public JsonResult FailReviewUpdate(int questionId)
        {
            bool changed = true;
            try
            {
                ChatFailureResponse fail = db.ChatFailureResponse.Where(x => x.DefectId == questionId).FirstOrDefault();
                fail.Reviewed = true;
                fail.UpdatedDate = DateTime.Now;
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
        public ActionResult UploadAudio()
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
            request.Headers["Ocp-Apim-Subscription-Key"] = "7bae8a4d0172428d8ecfb906d17cdd56";

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


        [HttpPost]
        public ActionResult UploadAudio1()
        {
            string finalresponse = string.Empty;
            var file = Request.Files[0];

            // Send an audio file by 1024 byte chunks
            Guid guid = Guid.NewGuid();
            string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content\\sound\\myRecording01.wav");
            string filepath2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content\\sound\\" + guid.ToString() + ".wav");
            file.SaveAs(filepath2);
            AskMeSpeech speech = new AskMeSpeech();
            //finalresponse = speech.SpeechToTextGoogle(filepath2);
            //speech.Voice2Text(filepath2);

            if (System.IO.File.Exists(filepath2))
            {
                System.IO.File.Delete(filepath2);
            }

            return Json(finalresponse, JsonRequestBehavior.AllowGet);
        }
    }
}