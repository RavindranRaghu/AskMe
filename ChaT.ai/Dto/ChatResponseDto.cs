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

namespace ChaT.ai.Dto
{
    public class ChatResponseDto
    {
        public int Node { get; set; }
        public string Message { get; set; }
        public List<string> Suggestion {get; set;}

        private ChatDatabaseModel db = new ChatDatabaseModel();

        public ChatResponseDto GetChatResponse(int sessionId, string message, int node)
        {
            ChatDatabaseModel db = new ChatDatabaseModel();
            ChatResponseDto responseDto = new ChatResponseDto();

            #region Declaration
            AskMeContentManager contentManager = new AskMeContentManager();
            AskMeCommon common = new AskMeCommon(message, node);
            string finalResponse = string.Empty;
            string phoneNumber = string.Empty;
            ChatIntent responseIntent = new ChatIntent();
            List<ChatIntent> intentList = db.ChatIntent.ToList();
            List<ChatEntity> entityList = db.ChatEntity.ToList();
            ChatSession chatSession = db.ChatSession.Where(x => x.SessionId == sessionId).FirstOrDefault();

            #endregion

            #region Prerequsite Check
            if (common.CheckEmptyMessage())
            {
                finalResponse = "Sorry i did not understand";
                List<string> suggestforEmpty = common.GetSuggestionList();
                responseDto.Node = node;
                responseDto.Message = finalResponse;
                responseDto.Suggestion = suggestforEmpty;
                return responseDto;
            }
            else
            {
                message = message.Trim();
            }
            #endregion             

            #region Has Entity
            var hasEntity = (from inte in intentList
                             join ent in entityList on inte.ChatIntentId equals ent.ChatIntentId
                             where inte.ChatIntentId == node
                             select inte).ToList();

            // Check if Chat has one Intent with Entity

            var hasUnrecognizedEntity = db.ChatSessionEntity.Where(x => x.SessionId == sessionId && x.EntityType == "unrecog").ToList();

            if (hasEntity.Count > 0 && hasUnrecognizedEntity.Count > 0)
            {
                List<ChatSessionEntity> entityRecognized = hasUnrecognizedEntity;

                AskMeEntityExtraction entityMatch = new AskMeEntityExtraction(message, node, sessionId);
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

                responseDto.Node = node;
                responseDto.Message = finalResponse;
                responseDto.Suggestion = suggestforEntity;
                return responseDto;
            }
            

            var hasOneChildIntent = (from inte in intentList
                                     where inte.ParentId == node
                                     select inte).ToList();

            // Check if Chat has one Intent with Entity
            if (hasOneChildIntent.Count == 1)
            {
                ChatIntent childIntent = hasOneChildIntent.FirstOrDefault();

                var hasOneIntentwithEntity = (from ent in entityList
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

                    responseDto.Node = node;
                    responseDto.Message = finalResponse;
                    responseDto.Suggestion = suggestforEntity;
                    return responseDto;
                }
            }
            #endregion

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
            responseDto.Node = node;
            responseDto.Message = finalResponse;
            responseDto.Suggestion = suggest;
            return responseDto;
            #endregion
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

    }
}