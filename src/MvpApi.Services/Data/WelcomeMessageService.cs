using System;

namespace MvpApi.Services.Data
{
    public class WelcomeMessageService
    {
        private readonly Random rand;
        private readonly string[] messages = 
        {
            "You da real MVP!",
            "Hello, MVP!",
            "Hi Most Valuable Player.. oh wait, wrong MVP :/",
            "Welcome back!",
            "WOW SUCH AWESOME",
            "The Struggle Is Real",
            "Sloth Has Been Saved",
            "ALL YOUR CONTRIBUTIONS ARE BELONG TO US",
            "ERMAHGERD, contributions!"
        };

        public WelcomeMessageService()
        {
            rand = new Random();
        }

        public string GetRandomMessage()
        {
            return messages[rand.Next(messages.Length)];
        }

        public string GetMessage(int i)
        {
            if (i >= messages.Length)
            {
                i = messages.Length - 1;
            }
            return messages[i];
        }
    }
}
