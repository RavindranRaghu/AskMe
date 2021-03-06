﻿using ChaT.ai.bLogic;
using ChaT.ai.cAuth;
using ChaT.ai.Dto;
using ChaT.db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ChaT.ai.Controllers
{
    [AskMeAuthorize()]
    public class AdminController : Controller
    {

        private ChatDatabaseModel db;
        private List<ChatIntentTreeDto> intentTreeDtos;
        public AdminController()
        {
            db = new ChatDatabaseModel();
            intentTreeDtos = new List<ChatIntentTreeDto>();
        }
        // GET: Admin        
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
                                               NeedAuth = inte.NeedAuth,
                                               IsRedirect = inte.IsRedirect,
                                               RedirectIntent = inte.RedirectIntent,
                                               RedirectIntentName = (inte.RedirectIntent == null) ? string.Empty : db.ChatIntent.Where(x => x.ChatIntentId == inte.RedirectIntent.Value).Select(x => x.IntentName).FirstOrDefault(),
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

        public ActionResult IntentNew()
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
                                               NeedAuth = inte.NeedAuth,
                                               IsRedirect = inte.IsRedirect,
                                               RedirectIntent = inte.RedirectIntent,
                                               RedirectIntentName = (inte.RedirectIntent == null) ? string.Empty : db.ChatIntent.Where(x => x.ChatIntentId == inte.RedirectIntent.Value).Select(x => x.IntentName).FirstOrDefault(),
                                               UpdatedDate = inte.UpdatedDate
                                           }).OrderBy(y => y.ChatIntentId).ToList();

            SelectListItem selectListItem = new SelectListItem();
            selectListItem.Text = "Select Intent";
            selectListItem.Value = "9999";
            List<SelectListItem> intentNames = new List<SelectListItem>();
            intentNames.Add(selectListItem);
            List<SelectListItem> intentNamesDB = db.ChatIntent.ToList().Select(u => new SelectListItem
            {
                Text = u.IntentName,
                Value = u.ChatIntentId.ToString()
            }).ToList();

            intentNames.AddRange(intentNamesDB);

            ViewBag.intentNames = intentNames;

            return View(intents);
        }

        public ActionResult IntentTree()
        {            
            List<ChatIntentDto> intents = (from inte in db.ChatIntent
                                           join par in db.ChatIntent on inte.ParentId equals par.ChatIntentId
                                           where inte.ChatIntentId > 0
                                           select new ChatIntentDto
                                           {
                                               ChatIntentId = inte.ChatIntentId,
                                               IntentName = inte.IntentName,
                                               IntentDescription = inte.IntentDescription,
                                               ParentId = inte.ParentId,
                                               ParentName = par.IntentName,
                                               Response = inte.Response,
                                               NeedAuth = inte.NeedAuth,
                                               IsRedirect = inte.IsRedirect,
                                               RedirectIntent = inte.RedirectIntent,
                                               RedirectIntentName = (inte.RedirectIntent == null) ? string.Empty : db.ChatIntent.Where(x => x.ChatIntentId == inte.RedirectIntent.Value).Select(x => x.IntentName).FirstOrDefault(),
                                               UpdatedDate = inte.UpdatedDate
                                           }).OrderBy(y => y.ChatIntentId).ToList();
            
            List<ChatIntentTreeDto> intentTree = FillRecursive(intents, 0);
            return Json(intentTree, JsonRequestBehavior.AllowGet);
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
                                       select new EntitytDto
                                       {
                                           ChatEntityId = ent.ChatEntityId,
                                           EntityName = ent.EntityName,
                                           EntityDescription = ent.EntityDescription,
                                           EntityType = ent.EntityType,
                                           ChatIntentId = ent.ChatIntentId,
                                           ChatIntentName = intent.IntentName,
                                           UpdatedDate = ent.UpdatedDate
                                       }).ToList();
            List<SelectListItem> intents = db.ChatIntent.ToList().Select(u => new SelectListItem
            {
                Text = u.IntentName,
                Value = u.ChatIntentId.ToString()
            }).ToList();

            AskMeCommon common = new AskMeCommon("hello",0);
            List<SelectListItem> entityTypes = common.GetEntityTypeSelectList();

            ViewBag.intents = intents;
            ViewBag.entityTypes = entityTypes;

            return View(entity);
        }
        
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

        public ActionResult Index()
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
                    finalIntent.NeedAuth = intent.NeedAuth;
                    finalIntent.IsRedirect = intent.IsRedirect;
                    finalIntent.RedirectIntent = intent.RedirectIntent;
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
                    finalIntent.NeedAuth = intent.NeedAuth;
                    finalIntent.IsRedirect = intent.IsRedirect;
                    finalIntent.RedirectIntent = intent.RedirectIntent;
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
                    entity.EntityType = entitydto.EntityType;
                    entity.ChatIntentId = entitydto.ChatIntentId;
                    entity.UpdatedDate = DateTime.Now;
                    db.ChatEntity.Add(entity);
                }
                else if (operation == "u")
                {
                    entity = db.ChatEntity.Where(x => x.ChatEntityId == entitydto.ChatEntityId).FirstOrDefault();
                    entity.EntityName = entitydto.EntityName;
                    entity.EntityDescription = entitydto.EntityDescription;
                    entity.EntityType = entitydto.EntityType;
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

        public List<ChatIntentTreeDto> FillRecursive(List<ChatIntentDto> flatObjects, int parentId)
        {
            return flatObjects.Where(x => x.ParentId.Equals(parentId)).Select(inte => new ChatIntentTreeDto
            {
                ChatIntentId = inte.ChatIntentId,
                IntentName = inte.IntentName,
                IntentDescription = inte.IntentDescription,
                ParentId = inte.ParentId,
                ParentName = (db.ChatIntent.Where(x => x.ChatIntentId == inte.ParentId).Select(x => x.IntentName).FirstOrDefault()),
                Response = inte.Response,
                NeedAuth = inte.NeedAuth,
                IsRedirect = inte.IsRedirect,
                RedirectIntent = inte.RedirectIntent,
                RedirectIntentName = (inte.RedirectIntent == null) ? string.Empty : db.ChatIntent.Where(x => x.ChatIntentId == inte.RedirectIntent.Value).Select(x => x.IntentName).FirstOrDefault(),
                UpdatedDate = inte.UpdatedDate,
                Children = FillRecursive(flatObjects, inte.ChatIntentId)
            }).ToList();
        }
    }
}