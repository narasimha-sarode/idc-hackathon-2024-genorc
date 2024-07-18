using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenOrcAdvisor
{
    internal class AzureAISettings
    {
        public Uri Uri { get; set; }
        public string DeploymentName { get; set; }
        public string API_KEY { get; set; }
        public string PromptTemplateFilePath { get; set; }


    }

    internal class MongoDBSettings
    {
        public string DatabaseName { get; set; }
        public string ConnectionString { get; set; }

        public string InstrumentStateCollectionName { get; set; }

        public string OrderMessagesCollectionName { get; set; }


    }
}
