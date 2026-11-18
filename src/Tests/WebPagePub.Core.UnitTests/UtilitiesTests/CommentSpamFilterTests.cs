using WebPagePub.Core.Utilities;
using Xunit;

namespace WebPagePub.Core.UnitTests.UtilitiesTests
{
    public class CommentSpamFilterTests
    {
        [Theory]
        [InlineData("get gsa search engine ranker", "moriceallen335@yahoo.com", "https://www.facebook.com/gsase", "Wonderful blog! I found it while surfing around on Yahoo News. Do you have any tips on how to get listed in Yahoo News?")]
        [InlineData("GSA URL list", "lourdessisson739@yahoo.com", "https://www.facebook.com/gsase", "Very good post! We will be linking to this particularly great post on our website. Keep up the good writing.")]
        [InlineData("BUY XANAX ONLINE", "belenklein519@gmail.com", "https://zozibd.com", "Top xxx sites offer secure and premium content for adults.")]
        [InlineData("aplikasi mega888", "leilanihirsch641@gmail.com", "https://www.jic-magic.com/ques", "Can you tell us more about this? I'd care to find out some additional information.")]
        [InlineData("Extra resources", "lucretiarennie630@gmail.com", "https://www.lovepampercompany.com", "Good way of describing, and good piece of writing to get data on the topic of my presentation subject matter.")]
        [InlineData("女同性恋色情影片", "catalanzora189@live.fr", "https://Phrsxpll.com/", "新成人网站 提供创新的成人娱乐内容。")]
        [InlineData("Купить фентанил", "mitchellkirsova101@gmail.com", "https://www.1337games.org/us", "Порносайт предлагает широкий выбор видео для взрослых развлечений.")]
        [InlineData("youtube video downloader", "joanlyon813@bigpond.com", "https://savefrom.app/2-youtube", "We recognize the value of your time, which is why we have incorporated a Turbo Mode feature into Easy Videos Downloader.")]
        [InlineData("QA Probe", "qa@example.com", "", "<b>QA658BOLD</b> <script>alert(\"qa658\")</script>")]
        [InlineData("x'; SELECT pg_sleep(5)--", "qa@example.com", "", "probe")]
        [InlineData("lethbridge.casino", "felicitaswestgarth82@gmail.com", "https://lethbridge.casino/", "Howdy! I could have sworn I've been to this website before but after going through many of the articles I realized it's new to me.")]
        [InlineData("KasieKed", "devon.kelley15@mail.com", "", "Purchase Website Visits")]
        [InlineData("AlbertTaw", "t.hughr.ahytj57h7@gmail.com", "", "t.hughr.ahytj57h7@gmail.com")]
        [InlineData("NAYUYUTY854716NEYRTHYT", "cooke694303@notlettersmail.com", "", "MEKYUJTYJ854716MAYTRYR")]
        [InlineData("Geraldfap", "linda.barnette.lb@gmail.com", "", "Hello.")]
        [InlineData("Rogertwisp", "vmavrina46@gmail.com", "", "Продаю PlayStation последнего поколения вместе с кучей игр, переезжаю и взять всё это с собой не получится")]
        [InlineData("Geraldheape", "jacksrenome@gmx.com", "", "YyErjcwdkdjwjjwjjdwjddjwsjf ndsaKAqwdweihduncbbwebidaa iudwnishqwuvdwqihbfvweuiojsqjqioqdefiw")]
        public void Real_spam_from_the_blog_is_caught(string name, string email, string website, string comment)
        {
            Assert.True(CommentSpamFilter.Evaluate(name, email, website, comment).IsSpam);
        }

        [Theory]
        [InlineData("Sam", "sam@proton.me", "", "Does Feather still work with a Trezor after the 2.9 update? Mine stopped detecting it.")]
        [InlineData("anon", "a@b.co", "", "Thanks, the pruned node took about 14 hours to sync on my NUC. One note: port 18080 needs to be open.")]
        [InlineData("Maria K", "maria.k@gmail.com", "", "I used the second exchange in the list last week, 0.5 XMR to BTC took 25 minutes.")]
        [InlineData("J", "j77@tutanota.com", "https://myblog.net", "The table is missing Cake Wallet's desktop version, it came out this year.")]
        [InlineData("Lee", "lee@example.org", "", "Great post, but the mining numbers look off for a Ryzen 5950X: I get 15 kH/s, not 12.")]
        public void Real_comments_pass(string name, string email, string website, string comment)
        {
            Assert.False(CommentSpamFilter.Evaluate(name, email, website, comment).IsSpam);
        }

        [Fact]
        public void Hidden_field_alone_marks_spam()
        {
            Assert.True(CommentSpamFilter.Evaluate("Sam", "sam@proton.me", string.Empty, "A normal looking comment.", "filled").IsSpam);
        }
    }
}
