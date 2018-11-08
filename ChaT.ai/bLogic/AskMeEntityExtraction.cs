using AskMe.ai;
using ChaT.ai.Dto;
using ChaT.db;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using static ChaT.ai.bLogic.AskMeEntityExtracttionNLP;

namespace ChaT.ai.bLogic
{
    public class AskMeEntityExtraction
    {
        private string Message = string.Empty;
        private int Node = 0;
        private int SessionId = 0;
        private ChatDatabaseModel db;
        TextInfo textInfoMain = new CultureInfo("en-US", false).TextInfo;

        public AskMeEntityExtraction(string message, int node, int sessionId)
        {
            this.Message = textInfoMain.ToTitleCase(message);
            this.Node = node;
            this.SessionId = sessionId;
            db = new ChatDatabaseModel();
        }

        public KeyValuePair<int, string> GetEntityforIntent(ChatIntent intent)
        {
            string entity = string.Empty;
            List<ChatEntity> entityList = new List<ChatEntity>(); // db.ChatEntity.Where(z => z.ChatIntentId == chatIntentId.ToString()).ToList();
            List<string> questionList = db.ChatIntentQuestion.Where(x => x.ChatIntentId == intent.ChatIntentId && x.QuestionDesc.ToLower().Contains("entity")).Select(y => y.QuestionDesc).ToList();
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            KeyValuePair<int, string> responseIntent = new KeyValuePair<int, string>();
            int foundEntityMatch = 0;
            string extractedEntityName = string.Empty;
            string extractedEntityValue = string.Empty;

            foreach (string question in questionList)
            {
                string textPriortoEntityinQuestion = string.Empty;
                string textPriortoEntityinMessage = string.Empty;
                int iStart = question.IndexOf("[");
                int iEnd = question.IndexOf("]");
                if (iStart != -1 && iEnd != -1)
                {
                    extractedEntityName = question.Substring(iStart + 1, iEnd - iStart - 1);
                    textPriortoEntityinQuestion = question.Substring(0, iStart);
                    textPriortoEntityinMessage = Message.Substring(0, iStart);
                    LevenshteinDistance dist = new LevenshteinDistance();
                    int matching = dist.Compute(textPriortoEntityinQuestion.ToLower(), textPriortoEntityinMessage.ToLower());
                    if (matching <= 6)
                    {
                        int iStartSpace = textPriortoEntityinMessage.LastIndexOf(" ");
                        int iEndSpace = Message.IndexOf(",", textPriortoEntityinMessage.Length);
                        if (iEndSpace == -1)
                            iEndSpace = Message.Length;
                        extractedEntityValue = Message.Substring(iStartSpace, iEndSpace - iStartSpace);
                        extractedEntityValue = textInfo.ToTitleCase(extractedEntityValue);
                        AskMeOnlineApi online = new AskMeOnlineApi();
                        responseIntent = online.OnlineApiChannel(intent, extractedEntityName, extractedEntityValue);
                        foundEntityMatch = foundEntityMatch + 1;
                        break;
                    }
                }
            }

            if (foundEntityMatch ==0)
            {
                ChatIntent noEntityFound = db.ChatIntent.Where(x => x.ParentId == intent.ChatIntentId).Where(x => x.IntentName.Contains("Entity")).FirstOrDefault();
                HttpContext httpContext = HttpContext.Current;
                httpContext.Session["EntityIssue"] = extractedEntityName;
                responseIntent = new KeyValuePair<int, string>(noEntityFound.ChatIntentId, noEntityFound.Response);
            }

            return responseIntent;
        }

        public KeyValuePair<int, string> PutEntityforIntent(ChatIntent intent)
        {
            KeyValuePair<int, string> responseIntent = new KeyValuePair<int, string>();
            HttpContext httpContext = HttpContext.Current;
            if (httpContext.Session["EntityIssue"] != null)
            {
                string extractedEntityName = httpContext.Session["EntityIssue"].ToString();
                string extractedEntityValue = Message;
                AskMeOnlineApi online = new AskMeOnlineApi();
                responseIntent = online.OnlineApiChannel(intent, extractedEntityName, extractedEntityValue);
                httpContext.Session["EntityIssue"] = null;
            }
            else
            {
                responseIntent = new KeyValuePair<int, string>(intent.ChatIntentId, "Sorry, i did not understand");
            }
            return responseIntent;
        }


        public ChatIntent GetEntityforIntentfromNLP(ChatIntent responseIntent)
        {
            AskMeEntityExtracttionNLP nlp = new AskMeEntityExtracttionNLP();
            List<EntityRecognition> entityListMessage = new List<EntityRecognition>();
            List<EntityRecognized> entityListRecognized = new List<EntityRecognized>();
            List<ChatEntity> entityListDb = db.ChatEntity.Where(x=>x.ChatIntentId == Node).ToList();
            if (entityListDb.Where(x=>x.EntityType.Contains("PERSON")).Any() || entityListDb.Where(x => x.EntityType.Contains("LOCATION")).Any())
            {
                entityListMessage = nlp.ExtractionChannel(Message);
            }
            HttpContext httpContext = HttpContext.Current;

            foreach(ChatEntity entity in entityListDb)
            {
                if (entity.EntityType.ToUpper().Contains("PERSON") && entityListMessage.Count >0)
                {
                    var hasRecognition = entityListMessage.Where(x => x.EntityType == "PERSON");
                    if (hasRecognition.Any())
                    {
                        EntityRecognition recog = hasRecognition.FirstOrDefault();
                        EntityRecognized recognized = new EntityRecognized();
                        recognized.EntityType = entity.EntityType;
                        recognized.EntityName = entity.EntityName;
                        recognized.EntityValue = recog.EntityValue;
                        recognized.NotRecognized = false;
                        entityListRecognized.Add(recognized);
                        if (entity.EntityName.ToUpper().Contains("USERNAME"))
                        {
                            httpContext.Session["authUser"] = recog.EntityValue;
                            ChatSession chatSession = db.ChatSession.Where(x => x.SessionId == SessionId).FirstOrDefault();
                            chatSession.SessionUd = recog.EntityValue;
                            db.SaveChanges();
                        }                        
                    }
                }
                else if (entity.EntityType.ToUpper().Contains("LOCATION") && entityListMessage.Count > 0)
                {
                    var hasRecognition = entityListMessage.Where(x => x.EntityType == "LOCATION");
                    if (hasRecognition.Any())
                    {
                        EntityRecognition recog = hasRecognition.FirstOrDefault();
                        EntityRecognized recognized = new EntityRecognized();
                        recognized.EntityType = entity.EntityType;
                        recognized.EntityName = entity.EntityName;
                        recognized.EntityValue = recog.EntityValue;
                        recognized.NotRecognized = false;
                        entityListRecognized.Add(recognized);
                    }
                }
                else if (entity.EntityType.ToUpper().Contains("NUMBER") && entityListMessage.Count > 0)
                {
                    string numericValue = new String(Message.Where(Char.IsDigit).ToArray());
                    if (numericValue.Length > 0)
                    {
                        EntityRecognized recognized = new EntityRecognized();
                        recognized.EntityType = entity.EntityType;
                        recognized.EntityName = entity.EntityName;
                        recognized.EntityValue = numericValue;
                        recognized.NotRecognized = false;
                        entityListRecognized.Add(recognized);
                    }
                }
                else
                {
                    EntityRecognized recognized = new EntityRecognized();
                    recognized.EntityType = entity.EntityType;
                    recognized.EntityName = entity.EntityName;
                    recognized.EntityValue = entity.EntityDescription;
                    recognized.NotRecognized = true;
                    entityListRecognized.Add(recognized);
                }
            }

            string noEntityMessage = string.Empty;

            List<ChatSessionEntity> sessionEntityList = new List<ChatSessionEntity>();
            foreach (EntityRecognized recognized in entityListRecognized)
            {
                ChatSessionEntity sessionEntity = new ChatSessionEntity();
                sessionEntity.SessionId = SessionId;
                sessionEntity.EntityType = recognized.EntityType;
                sessionEntity.EntityName = recognized.EntityName;
                sessionEntity.EntityValue = recognized.EntityValue;
                sessionEntity.NotRecognized = recognized.NotRecognized;
                db.ChatSessionEntity.Add(sessionEntity);
                sessionEntityList.Add(sessionEntity);
                if (recognized.NotRecognized)
                {
                    noEntityMessage = noEntityMessage + recognized.EntityValue+ " ";                    
                }
            }
            db.SaveChanges();
            if (noEntityMessage.Length>1)
            {
                Message = noEntityMessage;
                httpContext.Session[Node.ToString()] = entityListRecognized;
            }
            else
            {
                AskMeOnlineApi onlineApi = new AskMeOnlineApi();
                ChatIntent forOnline = db.ChatIntent.Where(x => x.ChatIntentId == Node).FirstOrDefault();
                KeyValuePair<int, string> onlineResponse=  onlineApi.OnlineApiChannelforNLP(forOnline, sessionEntityList);
                Node = onlineResponse.Key;
                Message = onlineResponse.Value;
            }

            responseIntent.ChatIntentId = Node;
            responseIntent.Response = Message;
            return responseIntent;
        }

        public KeyValuePair<int, string> CheckIfAtleastOneEntityHasValue(List<ChatSessionEntity> entityListRecognized)
        {
            AskMeEntityExtracttionNLP nlp = new AskMeEntityExtracttionNLP();
            List<EntityRecognition> entityListMessage = new List<EntityRecognition>();
            List<ChatEntity> entityListDb = db.ChatEntity.Where(x => x.ChatIntentId == Node).ToList();
            if (entityListDb.Where(x => x.EntityType.Contains("PERSON")).Any() || entityListDb.Where(x => x.EntityType.Contains("LOCATION")).Any())
            {
                entityListMessage = nlp.ExtractionChannel(Message);
            }
            HttpContext httpContext = HttpContext.Current;

            foreach(ChatSessionEntity entityRecognized in entityListRecognized)
            {
                if (entityRecognized.NotRecognized)
                {
                    if (entityRecognized.EntityName.ToUpper().Contains("PERSON"))
                    {
                        if (entityListMessage.Where(x => x.EntityType == "PERSON").Any())
                        {
                            entityRecognized.EntityValue = entityListMessage.Where(x => x.EntityType == "PERSON").Select(Y => Y.EntityValue).FirstOrDefault();
                            entityRecognized.EntityType = "PERSON";
                            entityRecognized.NotRecognized = false;
                            if (entityRecognized.EntityName.ToUpper().Contains("USERNAME"))
                            {
                                httpContext.Session["authUser"] = entityRecognized.EntityValue;
                                ChatSession chatSession = db.ChatSession.Where(x => x.SessionId == SessionId).FirstOrDefault();
                                chatSession.SessionUd = entityRecognized.EntityValue;
                                db.SaveChanges();
                            }
                        }
                    }
                    else if (entityRecognized.EntityName.ToUpper().Contains("LOCATION"))
                    {
                        if (entityListMessage.Where(x => x.EntityType == "LOCATION").Any())
                        {
                            entityRecognized.EntityValue = entityListMessage.Where(x => x.EntityType == "LOCATION").Select(Y => Y.EntityValue).FirstOrDefault();
                            entityRecognized.EntityType = "LOCATION";
                            entityRecognized.NotRecognized = false;
                        }
                    }
                    else if (entityRecognized.EntityName.ToUpper().Contains("NUMBER"))
                    {
                        string numericValue = new String(Message.Where(Char.IsDigit).ToArray());
                        if (numericValue.Length > 0)
                        {
                            entityRecognized.EntityValue = numericValue;
                            entityRecognized.EntityType = "NUMBER";
                            entityRecognized.NotRecognized = false;
                        }
                    }
                }
            }

            string noEntityMessage = string.Empty;
            foreach (ChatSessionEntity recognized in entityListRecognized)
            {
                ChatSessionEntity sessionEntity = new ChatSessionEntity();
                sessionEntity.SessionId = SessionId;
                sessionEntity.EntityType = recognized.EntityType;
                sessionEntity.EntityName = recognized.EntityName;
                sessionEntity.EntityValue = recognized.EntityValue;
                sessionEntity.NotRecognized = recognized.NotRecognized;
                if (recognized.NotRecognized)
                {
                    noEntityMessage = noEntityMessage + recognized.EntityValue + " ";
                }
            }
            db.SaveChanges();
            if (noEntityMessage.Length > 1)
            {
                Message = noEntityMessage;
                httpContext.Session[Node.ToString()] = entityListRecognized;
            }
            else
            {
                AskMeOnlineApi onlineApi = new AskMeOnlineApi();
                ChatIntent forOnline = db.ChatIntent.Where(x => x.ChatIntentId == Node).FirstOrDefault();
                return onlineApi.OnlineApiChannelforNLP(forOnline, entityListRecognized);
            }

            return new KeyValuePair<int, string>(Node, Message);
        }


        public KeyValuePair<int, string> CheckIfAtleastOneEntitywithPasscode(List<ChatSessionEntity> entityListRecognized)
        {
            ChatEntity entity = db.ChatEntity.Where(x => x.ChatIntentId == Node && x.EntityType.Contains("PASSCODE")).FirstOrDefault();
            HttpContext httpContext = HttpContext.Current;

            ChatSessionEntity entityRecognized = entityListRecognized.Where(x => x.EntityType.Contains("PASSCODE")).FirstOrDefault();
            if (Message.ToLower()=="apollo")
            {
                ChatIntent passwordIntent = db.ChatIntent.Where(x => x.ChatIntentId == Node).FirstOrDefault();
                Message = passwordIntent.Response;
                Node = passwordIntent.ChatIntentId;
                httpContext.Session[passwordIntent.ChatIntentId.ToString()] = null;
                httpContext.Session["auth"] = true;
                ChatSession chatSession = db.ChatSession.Where(x => x.SessionId == SessionId).FirstOrDefault();                
                chatSession.isAuth = true;
                ChatSessionEntity chatSessionEntity = db.ChatSessionEntity.Where(x => x.SessionEntId == entityRecognized.SessionEntId).FirstOrDefault();
                chatSessionEntity.EntityType = "recog";
                chatSessionEntity.NotRecognized = false;
                db.SaveChanges();
                int authIntentId = (chatSession.IntentBeforeAuth != null) ? chatSession.IntentBeforeAuth.Value : 0;
                if (authIntentId != 0)
                {                    
                    ChatIntent authIntent = db.ChatIntent.Where(x => x.ChatIntentId == authIntentId).FirstOrDefault();
                    Node = authIntent.ChatIntentId;
                    Message = authIntent.Response;
                }
                return new KeyValuePair<int, string>(Node, Message);
            }
            else
            {
                httpContext.Session[Node.ToString()] = entityListRecognized;                
                return new KeyValuePair<int, string>(Node, entity.EntityDescription);
            }

        }

    }
}