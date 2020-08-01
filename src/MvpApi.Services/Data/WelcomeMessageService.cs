using System;
using MvpApi.Services.Models;

namespace MvpApi.Services.Data
{
    public class WelcomeMessageService
    {
        private readonly Random rand;

        private readonly WelcomeMessage[] messages = 
        {
            new WelcomeMessage { Message = "You da real MVP!", GifUrl = ""}, 
            new WelcomeMessage { Message = "Hello, MVP!", GifUrl = "" }, 
            new WelcomeMessage { Message = "Will the real MVP please stand up?", GifUrl = "" }, 
            new WelcomeMessage { Message = "Welcome back!", GifUrl = "" }, 
            new WelcomeMessage { Message = "WOW SUCH AWESOME", GifUrl = "" }, 
            new WelcomeMessage { Message = "The Struggle Is Real", GifUrl = "" }, 
            new WelcomeMessage { Message = "Sloth Has Been Saved", GifUrl = "" }, 
            new WelcomeMessage { Message = "ALL YOUR CONTRIBUTIONS ARE BELONG TO US", GifUrl = "" }, 
            new WelcomeMessage { Message = "ERMAHGERD, contributions!", GifUrl = "" }, 
            new WelcomeMessage { Message = "Wow Much Valuable", GifUrl = "" }, 
            new WelcomeMessage { Message = "May the Code be with You!", GifUrl = "" }, 
            new WelcomeMessage { Message = "Most Valuable, you are", GifUrl = "" }
        };

        public WelcomeMessageService()
        {
            rand = new Random();
        }

        public WelcomeMessage GetRandomMessage()
        {
            return messages[rand.Next(messages.Length)];
        }

        public WelcomeMessage GetMessage(int i)
        {
            if (i >= messages.Length)
            {
                i = messages.Length - 1;
            }
            return messages[i];
        }
    }
}
