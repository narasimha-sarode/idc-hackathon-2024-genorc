using Azure.AI.OpenAI;
using Azure;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using OpenAI.Chat;
using Microsoft.Extensions.Options;

namespace GenOrcAdvisor
{
    internal class OrderAdviceGenerator
    {
        private readonly IOptions<AzureAISettings> _azureAiSettings;

        private readonly MongoDataAccessorService _mongoDataAccessorService;

        public OrderAdviceGenerator(IOptions<AzureAISettings> azureAiSettings,
                         MongoDataAccessorService mongoDataReader)
        {
            _azureAiSettings = azureAiSettings;

            _mongoDataAccessorService = mongoDataReader;
        }

        public async Task DoWork(CancellationToken cancellationToken)
        {
            while (true)
            {
                try
                {
                    #region TryGetNextAvailableOrder

                    var orderMessage = await _mongoDataAccessorService.GetNextOrderToProcess();
                    if (orderMessage is null || orderMessage.ElementCount == 0)
                    {
                        await Task.Delay(5000, cancellationToken);
                        continue;
                    }
                    await Console.Out.WriteLineAsync($"Received Order Message:: \n\n{orderMessage.ToJson()}\n\n");

                    var instrumentState = await _mongoDataAccessorService.GetCurrentInstrumentState();
                    if (instrumentState is null || instrumentState.ElementCount == 0)
                    {
                        throw new InvalidDataException("Instrument State Data Not available...");
                    }

                    await Console.Out.WriteLineAsync($"Received Instrument State Message:: \n\n{instrumentState.ToJson()}\n\n");

                    #endregion

                    #region OpenAI Call

                    AzureOpenAIClient azureClient = new(_azureAiSettings.Value.Uri,
                                                new AzureKeyCredential(_azureAiSettings.Value.API_KEY));

                    ChatClient chatClient = azureClient.GetChatClient(_azureAiSettings.Value.DeploymentName);

                    if (!File.Exists(_azureAiSettings.Value.PromptTemplateFilePath))
                        throw new FileNotFoundException($"File Given '{_azureAiSettings.Value.PromptTemplateFilePath}' not found.");

                    string promptText = await File.ReadAllTextAsync(_azureAiSettings.Value.PromptTemplateFilePath, cancellationToken);

                    promptText = string.Format(promptText, instrumentState.ElementAt(1).ToJson(), orderMessage.ElementAt(1).ToJson());

                    await Console.Out.WriteLineAsync($"Generating response for::\n\n{promptText}\n\n");

                    var aiResponse = await chatClient.CompleteChatAsync(promptText);

                    if (aiResponse != null && aiResponse.Value.Content.Count > 0)
                        Console.WriteLine($"AI Responded as... \n\n {aiResponse.Value.Content[0].Text}");
                    else
                        Console.WriteLine("Response is returned empty..\n\n");


                    #endregion

                    #region Updating Order with AI Response

                    var respContent = aiResponse?.Value?.Content[0].Text ?? string.Empty;
                    await _mongoDataAccessorService.UpdateAIResponse(orderMessage, respContent);

                    #endregion

                    await Task.Delay(2000, cancellationToken);
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync($"Exception Occured. Details::{ex.ToString()}");
                }
            }



        }

    }
}
