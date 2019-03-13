using AskMe.ai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChaT.db;
using System.Globalization;

namespace ChaT.ai.bLogic
{
    public class AskMeChannel
    {
        private string Message = string.Empty;
        private int Node = 0;
        private int SessionId = 0;
        private ChatDatabaseModel db;
        private AskMeHiBye hiBye;
        private AskMeSuggestionMatch suggestionMatch;
        private AskMeCommon common;
        private AskMezPossibleMatch zPossibleMatch;
        private AskMeSynonymMatch synonymMatch;
        private AskMeContentManager contentManager = new AskMeContentManager();
        ChatIntent finalResponse = new ChatIntent();        
        public AskMeChannel(string message, int node, int sessionId)
        {
            this.Message = message.ToLower();
            this.Node = node;
            this.SessionId = sessionId;
            db = new ChatDatabaseModel();
            hiBye = new AskMeHiBye(Message, Node);
            suggestionMatch = new AskMeSuggestionMatch(Message, Node);
            common = new AskMeCommon(Message, Node);
            zPossibleMatch = new AskMezPossibleMatch(Message, Node);
            synonymMatch = new AskMeSynonymMatch(Message, Node);
        }

        //ChaT Initializer
        public ChatIntent ChatInitializer()
        {
            finalResponse = ChatResponseMain();
            if (finalResponse.Response == contentManager.NoIntentMatchedResponse)
            {
                var hasParentNode = db.ChatIntent.Where(x => x.ChatIntentId == Node && Node != 0);
                if (hasParentNode.Any())
                {
                    Node = hasParentNode.Select(y => y.ParentId).FirstOrDefault();
                    finalResponse = ChatInitializer();
                }
            }
            if (finalResponse.Response == contentManager.NoIntentMatchedResponse)
            {
                common.LogFailureResponse();
            }
            return finalResponse;
        }

        //ChaT Bot Reponse Main Entry
        public ChatIntent ChatResponseMain()
        {
            string responseMessage = contentManager.NoIntentMatchedResponse;
            TFIDF getVocab = new TFIDF();
            Dictionary<string, string> reponseDict = new Dictionary<string, string>();
            List<ChatIntent> intentListAll = db.ChatIntent.ToList();

            ChatIntent responseIntent = intentListAll.Where(x => x.ChatIntentId == 0).FirstOrDefault();

            #region 1.CheckIntentGreetingOrGoodbye
            if (hiBye.Greet())                
                return UpdateIntent(Node, contentManager.GreetResponse, responseIntent);
            else if (hiBye.GoodBye())
                return UpdateIntent(Node, contentManager.GoodbyeResponse, responseIntent);        
            #endregion

            List<ChatIntent> intentList = (from intention in intentListAll
                                           where intention.ChatIntentId > 2 && intention.ParentId == Node
                                           select intention).ToList();

            #region 2.CheckIntentFullMatchbySuggestion
            KeyValuePair<int, bool> fullMatch = suggestionMatch.FullSuggestionMatch(intentList);
            if (fullMatch.Value)
            {
                ChatIntent fullMatchIntent = intentList.Where(x => x.ChatIntentId == fullMatch.Key).FirstOrDefault();
                responseMessage = fullMatchIntent.Response;
                var hasEntity = (from ent in db.ChatEntity where ent.ChatIntentId == fullMatchIntent.ChatIntentId
                                 select ent);
                if (hasEntity.Any())
                {
                    AskMeEntityExtraction entity = new AskMeEntityExtraction(Message, fullMatchIntent.ChatIntentId, SessionId);                    
                    return entity.GetEntityforIntentfromNLP(fullMatchIntent);
                }
                return fullMatchIntent;
            }

            KeyValuePair<int, bool> partialMatch = suggestionMatch.PartialSuggestionMatch(intentList);
            if (partialMatch.Value)
            {
                ChatIntent partialMatchIntent = intentList.Where(x => x.ChatIntentId == partialMatch.Key).FirstOrDefault();
                responseMessage = partialMatchIntent.Response;
                var hasEntity = (from ent in db.ChatEntity
                                 where ent.ChatIntentId == partialMatchIntent.ChatIntentId
                                 select ent);
                if (hasEntity.Any())
                {
                    AskMeEntityExtraction entity = new AskMeEntityExtraction(Message, partialMatchIntent.ChatIntentId, SessionId);
                    return entity.GetEntityforIntentfromNLP(partialMatchIntent);
                }
                return partialMatchIntent;
            }
            #endregion


            List<string> vocabList = getVocab.GetVocabulary(Message);
            if (vocabList.Count == 0)
                return UpdateIntent(Node, contentManager.NoIntentMatchedResponse, responseIntent);

            if (Message.ToLower() == "yes" || Message.ToLower() == "no")            
                return UpdateIntent(Node, contentManager.NoIntentMatchedResponse, responseIntent);

            #region 3.TFIDF Match Process
            SimilarityCalculator similarityCalculator = new SimilarityCalculator();
            List<ChatIntentQuestion> questionList = db.ChatIntentQuestion.ToList();
            Dictionary<int, double> scoreDict = new Dictionary<int, double>();
            foreach (ChatIntentQuestion question in questionList)
            {
                double compare = similarityCalculator.CompareString(Message, question.QuestionDesc, 1);
                KeyValuePair<int, double> score = new KeyValuePair<int, double>(question.ChatIntentId, compare);
                if (scoreDict.ContainsKey(score.Key))
                {
                    if (scoreDict[score.Key] < compare)
                    {
                        scoreDict[score.Key] = compare;
                    }
                }
                else
                {
                    scoreDict.Add(score.Key, score.Value);
                }
            }

            if (scoreDict.Where(x => x.Value > 0.45).Any())
            {
                int maxScoreChatIntentId = scoreDict.OrderByDescending(x => x.Value).Select(y => y.Key).FirstOrDefault();
                ChatIntent maxIntent = intentListAll.Where(x => x.ChatIntentId == maxScoreChatIntentId).FirstOrDefault();
                Node = maxScoreChatIntentId;

                var hasEntity = (from ent in db.ChatEntity
                                 where ent.ChatIntentId == maxIntent.ChatIntentId
                                 select ent);
                if (hasEntity.Any())
                {
                    AskMeEntityExtraction entity = new AskMeEntityExtraction(Message, maxIntent.ChatIntentId, SessionId);                    
                    return entity.GetEntityforIntentfromNLP(maxIntent);
                }

                //KeyValuePair<int, string> responseIntent = GetEntityforIntent(Node, maxIntent.Response);
                return maxIntent;
            }
            else if (scoreDict.Where(x => x.Value >= 0.23).Any())
            {
                List<int> possibeMatch = scoreDict.OrderByDescending(x => x.Value).Where(x => x.Value >= 0.23).Select(y => y.Key).ToList();
                responseMessage = contentManager.IntentPossibleMatchedResponse;
                foreach (int match in possibeMatch)
                {
                    responseMessage = responseMessage + ", ";
                    string suggestion = intentListAll.Where(x => x.ChatIntentId == match).Select(y => y.IntentDescription).FirstOrDefault();
                    responseMessage = responseMessage + suggestion;
                }
                responseMessage = responseMessage + ", " + contentManager.IntentSuggestionResponse;
                return UpdateIntent(Node, responseMessage, responseIntent);
            }
            #endregion

            #region 4.Probable Match Process
            KeyValuePair<string, bool> probableMatchCorrect = zPossibleMatch.ProbableMatchCorrectSpelling(vocabList, intentListAll);
            if (probableMatchCorrect.Value)
            {
                common.LogFailureResponse();
                responseMessage = probableMatchCorrect.Key;
                return UpdateIntent(Node, responseMessage, responseIntent);
            }

            KeyValuePair<string, bool> probableMatchTypo = zPossibleMatch.ProbableMatchTypoError(vocabList, intentListAll);
            if (probableMatchTypo.Value)
            {
                common.LogFailureResponse();
                responseMessage = probableMatchTypo.Key;
                return UpdateIntent(Node, responseMessage, responseIntent);
            }
            #endregion

            #region 4.Synonym Match Process
            KeyValuePair<string, bool> synMatch = synonymMatch.SynonymMatch(vocabList, intentListAll);
            if (synMatch.Value)
            {
                common.LogFailureResponse();
                responseMessage = synMatch.Key;
                return UpdateIntent(Node, responseMessage, responseIntent);
            }
            #endregion

            if (responseIntent != null)
            {
                responseIntent.ChatIntentId = Node;
                responseIntent.Response = responseMessage;
            }
            else
            {
                responseIntent = new ChatIntent();
                responseIntent.ChatIntentId = Node;
                responseIntent.Response = "Sorry I did not understand, Please enter one of the suggestions";
            }
            return responseIntent;
        }


        private ChatIntent UpdateIntent (int node, string message, ChatIntent respondIntent)
        {
            if (respondIntent != null)
            {
                respondIntent.ChatIntentId = node;
                respondIntent.Response = message;
            }
            else
            {
                respondIntent = new ChatIntent();
                respondIntent.ChatIntentId = node;
                respondIntent.Response = "Sorry I did not understand, Please enter one of the suggestions";
            }
            return respondIntent;
        }
    }
}