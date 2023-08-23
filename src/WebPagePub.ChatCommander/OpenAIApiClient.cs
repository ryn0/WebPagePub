using Newtonsoft.Json;
using System.Text;
using WebPagePub.ChatCommander.ChatModels;
using WebPagePub.ChatCommander.SettingsModels;

namespace WebPagePub.ChatCommander
{
    public class ChatGPT
    {
        private readonly string _apiKey;
        
        private readonly string textModel;
        private readonly int maxTokens = 1000;
        const string contentType = "application/json";
        const string completionsEndPoint = "https://api.openai.com/v1/completions";
        const string imageGenerationsEndPoint = "https://api.openai.com/v1/images/generations";

        private static readonly HttpClient Client = new HttpClient();

        public ChatGPT(ChatGptSettings chatGptSettings)
        {
            _apiKey = chatGptSettings.ApiKey;
            this.maxTokens = chatGptSettings.MaxTokens;
            this.textModel = chatGptSettings.TextModel;
            Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<string> SubmitMessage(string prompt)
        {
            var jsonContent = new
            {
                prompt = prompt,
                model = textModel,
                max_tokens = maxTokens
            };

            try
            {
                var responseContent = await Client.PostAsync(
                    completionsEndPoint,
                    new StringContent(JsonConvert.SerializeObject(jsonContent),
                    Encoding.UTF8,
                    contentType));

                // Read the response as a string
                var resContext = await responseContent.Content.ReadAsStringAsync();

                // Deserialize the response into a dynamic object
                var data = JsonConvert.DeserializeObject<dynamic>(resContext);

                return data.choices[0].text;
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
                var Message = await Client.PostAsync(
                    imageGenerationsEndPoint,
                    new StringContent(JsonConvert.SerializeObject(prompt),
                    Encoding.UTF8,
                    contentType));

                if (Message.IsSuccessStatusCode)
                {
                    var content = await Message.Content.ReadAsStringAsync();
                    resp = JsonConvert.DeserializeObject<DalleImagesResponseModel>(content);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(nameof(GenerateImage), ex.InnerException);
            }

            return resp;
        }
    }
}