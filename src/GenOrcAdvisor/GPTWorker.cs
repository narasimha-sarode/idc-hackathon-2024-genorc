using Azure.AI.OpenAI;
using Azure;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using OpenAI.Chat;
using Microsoft.Extensions.Options;

namespace GenOrcAdvisor
{
    internal class GPTWorker
    {
        private readonly IOptions<AzureAISettings> _azureAiSettings;

        private readonly MongoDataReader _mongoDataReader;

        public GPTWorker(IOptions<AzureAISettings> azureAiSettings,
                         MongoDataReader mongoDataReader)
        {
            _azureAiSettings = azureAiSettings;

            _mongoDataReader = mongoDataReader;
        }

        public async Task DoWork(CancellationToken cancellationToken)
        {
            while (true)
            {
                var orderMessage = await _mongoDataReader.GetNextDocument();
                if (orderMessage is null || orderMessage.IsBsonNull)
                {
                    await Task.Delay(5000, cancellationToken);
                    continue;
                }

                var instrumentState = await _mongoDataReader.GetInstrumentStateData();
                if (instrumentState is null || instrumentState.IsBsonNull)
                {
                    throw new InvalidDataException("Instrument State Data Not available...");
                }

                AzureOpenAIClient azureClient = new(_azureAiSettings.Value.Uri,
                                            new AzureKeyCredential(_azureAiSettings.Value.API_KEY));

                ChatClient chatClient = azureClient.GetChatClient(_azureAiSettings.Value.DeploymentName);

                if (!File.Exists(_azureAiSettings.Value.PromptTemplateFilePath))
                    throw new FileNotFoundException($"File Given '{_azureAiSettings.Value.PromptTemplateFilePath}' not found.");

                string promptText = await File.ReadAllTextAsync(_azureAiSettings.Value.PromptTemplateFilePath, cancellationToken);

                promptText = string.Format(promptText, instrumentState.ToJson(), orderMessage.ToJson());

                var res = await chatClient.CompleteChatAsync(promptText);

                if (res != null && res.Value.Content.Count > 0)
                    Console.WriteLine($"AI Responded as... \n\n {res.Value.Content[0].Text}");
                else
                    Console.WriteLine("Response is returned empty..");

            }



        }

    }
}
