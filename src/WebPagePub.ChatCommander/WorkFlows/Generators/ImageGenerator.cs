using WebPagePub.ChatCommander.ChatModels;
using WebPagePub.ChatCommander.SettingsModels;
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
            ChatGptSettings chatGptSettings,
            ISitePageManager sitePageManager) :
            base(chatGptSettings, sitePageManager)
        {
            base.chatGptSettings = chatGptSettings;
            base.sitePageManager = sitePageManager;
        }
     
        public async Task<DalleImagesResponseModel> GenerateImage(ImageInputRequest input)
        {
            chatGPT = new ChatGPT(chatGptSettings);

            return await chatGPT.GenerateImage(input);
        }
    }
}
