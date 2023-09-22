using WebPagePub.ChatCommander.Models.ChatModels;
using WebPagePub.ChatCommander.Models.SettingsModels;
using WebPagePub.Managers.Interfaces;

namespace WebPagePub.ChatCommander.WorkFlows.Generators
{
    public class ImageGenerator : BaseGenerator
    {
        /////////////////////////////////
        //var imageGen = new ImageGenerator(settings, sitePageManager);

        //var resp = await imageGen.GenerateImage(new input()
        //{
        //    prompt = "boxers fighting",
        //    size = ",
        //    n = 1
        //});

        public ImageGenerator(
            OpenAiApiSettings chatGptSettings,
            ISitePageManager sitePageManager) :
            base(chatGptSettings, sitePageManager)
        {
            base.OpenAiApiSettings = chatGptSettings;
            base.sitePageManager = sitePageManager;
        }
     
        public async Task<DalleImagesResponseModel> GenerateImage(ImageInputRequest input)
        {
            if (OpenAiApiSettings == null)
            {
                throw new NullReferenceException($"{OpenAiApiSettings}");
            }

            openAiApiClient = new OpenAiApiClient(OpenAiApiSettings);

            return await openAiApiClient.GenerateImage(input);
        }
    }
}
