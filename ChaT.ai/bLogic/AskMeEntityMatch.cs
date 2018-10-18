using ChaT.ai.Dto;
using ChaT.db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChaT.ai.bLogic
{
    public class AskMeEntityMatch
    {
        private string Message = string.Empty;
        private int Node = 0;
        private ChatDatabaseModel db;

        public AskMeEntityMatch(string message, int node)
        {
            this.Message = message.ToLower();
            this.Node = node;
            db = new ChatDatabaseModel();
        }

        public EntityIdentifiedDto HasOneChildIntentWithOneEntity(ChatEntity entity, ChatIntent intent)
        {
            var hasEntity = db.ChatEntity.Where(x => x.ChatIntentId == Node);
            EntityIdentifiedDto dto = new EntityIdentifiedDto();
            string finalResponse = string.Empty;
            HttpContext context = HttpContext.Current;
            finalResponse = intent.Response;

            dto.EntityName = entity.EntityName;
            dto.EntityValue = EntityPatternMatch(entity);
            dto.MatchConfidence = "high";
            dto.ChatResponse = finalResponse;

            dto = SetEntityinResponse(dto);

            //phoneNumber = (context.Session["phone"] != null ? context.Session["phone"].ToString() : "980 000 000");
            //message = (context.Session["datetime"] != null ? context.Session["datetime"].ToString() : "12:00 AM EST");
            //finalResponse = finalResponse.Replace("paramphonenumber", phoneNumber);
            //finalResponse = finalResponse.Replace("paramappointmenttime", message);
            //dto.ChatResponse = finalResponse;
            return dto;
        }

        public EntityIdentifiedDto SetEntityinResponse(EntityIdentifiedDto dto)
        {
            string entityName = dto.EntityName;
            string response = dto.ChatResponse;
            int counter = 0;
            HttpContext context = HttpContext.Current;
            
            while (response.IndexOf("[") !=-1 && response.IndexOf("]") != -1)
            {
                string replace = ExtractBetween(response, "[", "]", counter);
                string entity = (context.Session[replace] != null ? context.Session[replace].ToString() : replace);
                response = response.Replace("[" + replace+"]", entity);
            }
            dto.ChatResponse = response;
            return dto;
        }

        public string ExtractBetween(string text, string start, string end, int counter)
        {
            int iStart = text.IndexOf(start);
            int iEnd = text.IndexOf(end);
            if (iStart != -1 && iEnd != -1)
            {
                return text.Substring(iStart+1, iEnd-iStart-1);
            }
            int len = iEnd - iStart;

            return "noentityfound";
        }

        public string EntityPatternMatch(List<ChatEntity> entityList)
        {
            foreach (ChatEntity entity in entityList)
            {
                
            }
            return "";           
        }


        public string EntityPatternMatch(ChatEntity entity)
        {
            string extractedEntityMatch = string.Empty;
            if (entity.EntityName.Contains("phone"))
                extractedEntityMatch = ExtractedEntityPhone(entity);
            else if (entity.EntityName.Contains("email"))
                    extractedEntityMatch = ExtractedEntityEmail(entity);
            else if (entity.EntityName.Contains("datetime"))
                extractedEntityMatch = ExtractedEntityDateTime(entity);
            else if (entity.EntityName.Contains("city"))
                extractedEntityMatch = ExtractedEntityCity(entity);
            return extractedEntityMatch;
        }

        private string ExtractedEntityCity(ChatEntity entity)
        {
            throw new NotImplementedException();
        }

        private string ExtractedEntityDateTime(ChatEntity entity)
        {
            string schedule = Message;
            HttpContext context = HttpContext.Current;
            context.Session[entity.EntityName] = schedule;
            return schedule;
        }

        private string ExtractedEntityEmail(ChatEntity entity)
        {
            throw new NotImplementedException();
        }

        private string ExtractedEntityPhone(ChatEntity entity)
        {
            string phone = new string(Message.Where(Char.IsDigit).ToArray());
            HttpContext context = HttpContext.Current;
            context.Session[entity.EntityName] = phone;
            return phone;
        }



    }
}