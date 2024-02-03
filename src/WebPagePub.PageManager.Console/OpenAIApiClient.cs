using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Images;
using WebPagePub.Core.Utilities;
using WebPagePub.PageManager.Console.Models.ChatModels;
using WebPagePub.PageManager.Console.Models.SettingsModels;

namespace WebPagePub.PageManager.Console
{
    public class OpenAiApiClient
    {
        private readonly string _apiKey;
        private readonly OpenAIAPI ApiClient;

        private readonly string textModel;
        public int MaxTokens { get; set; } = 4096;
        const string contentType = "application/json";

        public OpenAiApiClient(OpenAiApiSettings settings)
        {
            _apiKey = settings.ApiKey;
            this.MaxTokens = settings.MaxTokens;
            this.textModel = settings.TextModel;
            this.ApiClient = new OpenAIAPI(_apiKey);
        }

        public async Task<string> SubmitMessage(string prompt)
        {
            try
            {
                var result = await this.ApiClient.Chat.CreateChatCompletionAsync(new ChatRequest()
                {
                    Model = this.textModel,
                    Temperature = 0.1,
                    //MaxTokens = MaxTokens,
                    Messages = new ChatMessage[] {
                        new ChatMessage(ChatMessageRole.User, prompt)
                    }
                });
                var message = result?.Choices[0]?.Message?.TextContent?.Trim();

                return message;
            }
            catch (Exception ex)
            {
                throw new Exception(nameof(SubmitMessage), ex.InnerException);
            }
        }

        public async Task<DalleImagesResponseModel> GenerateImage(ImageInputRequest prompt)
        {
            var resp = new DalleImagesResponseModel();

            try
            {
                var result = await this.ApiClient
                                       .ImageGenerations
                                       .CreateImageAsync(
                    new ImageGenerationRequest()
                    {
                        Prompt = TextUtilities.RemoveNonAlphaNumeric(prompt.Prompt),
                        Model = OpenAI_API.Models.Model.DALLE3,
                        NumOfImages = prompt.Quanaity,
                        Size = prompt.ImageSize
                    });

                var imageUrl = result.Data[0].Url;

                resp.Data = [new Link() { Url = imageUrl }];
            }
            catch //(Exception ex)
            {
                resp.Success = false;
            }

            resp.Success = true;

            return resp;
        }

    }
}