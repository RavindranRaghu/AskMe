using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using edu.stanford.nlp.parser;
using java.util;
using edu.stanford.nlp.ie.crf;
using edu.stanford.nlp.pipeline;
using edu.stanford.nlp.util;
using System.IO;
using System.Globalization;

namespace ChaT.ai.bLogic
{
    public class AskMeEntityExtracttionNLP
    {
        public List<EntityRecognition> ExtractionChannel(string message)
        {
            // find people
            List<EntityRecognition> entity = new List<EntityRecognition>();

            // Path to the folder with classifiers models
            //var jarRoot = @"C:\\Users\\Sai\\Downloads\\stanford-ner-2016-10-31\\stanford-ner-2016-10-31";
            //var classifiersDirecrory = jarRoot + @"\classifiers";

            string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content\\classifier\\english.all.3class.distsim.crf.ser.gz");

            var classifier = CRFClassifier.getClassifierNoExceptions(filepath);
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            message = textInfo.ToTitleCase(message);
            string result = classifier.classifyWithInlineXML(message);

            entity = getEntityList(result);

            return entity;

        }

        public List<EntityRecognition> getEntityList(string result)
        {
            List<EntityRecognition> entity = new List<EntityRecognition>();
            int currentlength = 0;
            while (currentlength < result.Length)
            {
                int StartIndex = result.IndexOf("<PERSON>", currentlength);
                int EndIndex = result.IndexOf("</PERSON>", currentlength);
                if (StartIndex != -1 || EndIndex != -1)
                {
                    StartIndex = StartIndex + 8;
                    string entityValue = (result.Substring(StartIndex, EndIndex - StartIndex));
                    EntityRecognition recog = new EntityRecognition();
                    recog.EntityType = "PERSON";
                    recog.EntityValue = entityValue;
                    entity.Add(recog);
                    currentlength = currentlength + EndIndex + 1;
                }
                else
                {
                    break;
                }
            }


            currentlength = 0;
            while (currentlength < result.Length)
            {
                int StartIndex = result.IndexOf("<LOCATION>", currentlength);
                int EndIndex = result.IndexOf("</LOCATION>", currentlength);
                if (StartIndex != -1 || EndIndex != -1)
                {
                    StartIndex = StartIndex + 10;
                    string entityValue = (result.Substring(StartIndex, EndIndex - StartIndex));
                    EntityRecognition recog = new EntityRecognition();
                    recog.EntityType = "LOCATION";
                    recog.EntityValue = entityValue;
                    entity.Add(recog);
                    currentlength = currentlength + EndIndex + 1;
                }
                else
                {
                    break;
                }
            }

            return entity;
        }
        public class EntityRecognition
        {
            public string EntityType { get; set; }
            public string EntityValue { get; set; }

        }

    }
}