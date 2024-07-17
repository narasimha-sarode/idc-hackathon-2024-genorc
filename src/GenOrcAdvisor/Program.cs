using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

internal class Program
{
    private static void Main(string[] args)
    {
        try
        {
            string keyFromEnvironment = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? string.Empty;

            keyFromEnvironment = "971ec1b405084f5c9fe4da9d135c4cae";
            string uri = "https://becopenaidev6.openai.azure.com/";

            AzureOpenAIClient azureClient = new(
                new Uri(uri),
                new AzureKeyCredential(keyFromEnvironment));
            ChatClient chatClient = azureClient.GetChatClient("gpt-4o");

            string prompt = File.ReadAllText("prompt.txt");
            prompt = string.Format(prompt, File.ReadAllText("InstrumentState.json"), File.ReadAllText("OrderMessageSample.json"));

            var res = chatClient.CompleteChat(prompt);

            if (res != null && res.Value.Content.Count > 0)
                Console.WriteLine($"AI Responded as... \n\n {res.Value.Content[0].Text}");
            else
                Console.WriteLine("Response is returned empty..");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occured...\n\n Details::{ex.ToString()}");
        }
    }
}