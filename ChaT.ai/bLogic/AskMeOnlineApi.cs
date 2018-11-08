using ChaT.ai.Dto;
using ChaT.db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChaT.ai.bLogic
{
    public class AskMeOnlineApi
    {
        private ChatDatabaseModel db;

        public AskMeOnlineApi()
        {
            db = new ChatDatabaseModel();
        }

        public KeyValuePair<int, string> OnlineApiChannel(ChatIntent intent, string extractEntityName, string extractedEntityValue)
        {
            string response = string.Empty;            
            ChatIntent respondIntent = new ChatIntent();

            if (intent.IntentName == "ChangeAddressMeIntent" )
            {
                if (extractedEntityValue.ToLower().Trim() == "ravi raghu")
                {
                    respondIntent = db.ChatIntent.Where(x => x.ParentId == intent.ChatIntentId && x.IntentName.ToLower().Contains("yes")).FirstOrDefault();
                }
                else
                {
                    respondIntent = db.ChatIntent.Where(x => x.ParentId == intent.ChatIntentId && x.IntentName.ToLower().Contains("no")).FirstOrDefault();                    
                }
            }

            else if (intent.IntentName == "NoOnlineAccount")
            {
                if (extractedEntityValue.ToLower().Trim() == "chennai")
                {
                    respondIntent = db.ChatIntent.Where(x => x.ParentId == intent.ChatIntentId && x.IntentName.ToLower().Contains("yes")).FirstOrDefault();
                }
                else
                {
                    respondIntent = db.ChatIntent.Where(x => x.ParentId == intent.ChatIntentId && x.IntentName.ToLower().Contains("no")).FirstOrDefault();
                }
            }
            else
            {
                return new KeyValuePair<int, string>(intent.ChatIntentId, intent.Response);
            }
            respondIntent.Response = respondIntent.Response.Replace(extractEntityName, extractedEntityValue);
            return new KeyValuePair<int, string>(respondIntent.ChatIntentId, respondIntent.Response);
        }

        public KeyValuePair<int, string> OnlineApiChannelforNLP(ChatIntent intent, List<ChatSessionEntity> entityList)
        {
            string response = string.Empty;
            ChatIntent respondIntent = new ChatIntent();
            HttpContext httpContext = HttpContext.Current;
            int sessionId = entityList.Select(x => x.SessionId).FirstOrDefault();

            if (intent.IntentName.Trim() == "ChangeAddressMeIntent")
            {
                string entityValue = entityList.Where(x => x.EntityType == "PERSON").Select(y => y.EntityValue).FirstOrDefault().ToLower().Trim();
                if (entityValue == "ravi raghu")
                {
                    respondIntent = db.ChatIntent.Where(x => x.ParentId == intent.ChatIntentId && x.IntentName.ToLower().Contains("yes")).FirstOrDefault();
                }
                else
                {
                    respondIntent = db.ChatIntent.Where(x => x.ParentId == intent.ChatIntentId && x.IntentName.ToLower().Contains("no")).FirstOrDefault();
                }
            }

            else if (intent.IntentName.Trim() == "NoOnlineAccount")
            {
                string entityValue = entityList.Where(x => x.EntityType == "LOCATION").Select(y => y.EntityValue).FirstOrDefault().ToLower().Trim();
                if (entityValue == "chennai")
                {
                    respondIntent = db.ChatIntent.Where(x => x.ParentId == intent.ChatIntentId && x.IntentName.ToLower().Contains("yes")).FirstOrDefault();
                }
                else
                {
                    respondIntent = db.ChatIntent.Where(x => x.ParentId == intent.ChatIntentId && x.IntentName.ToLower().Contains("no")).FirstOrDefault();
                }
            }
            else
            {
                respondIntent = intent;
            }

            httpContext.Session[intent.ChatIntentId.ToString()] = null;
            List<ChatSessionEntity> recognizedList = db.ChatSessionEntity.Where(x => x.SessionId == sessionId).ToList();

            foreach (ChatSessionEntity chatEntity in recognizedList)
            {
                chatEntity.EntityType = "recog";
                chatEntity.NotRecognized = false;
            }
            db.SaveChanges();

            foreach (var recog in entityList)
            {
                respondIntent.Response = respondIntent.Response.Replace(recog.EntityName, recog.EntityValue);
            }            
            return new KeyValuePair<int, string>(respondIntent.ChatIntentId, respondIntent.Response);
        }

    }
}