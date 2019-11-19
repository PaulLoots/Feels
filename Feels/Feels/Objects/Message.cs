using System;
namespace Feels.Objects
{
    public class Conversation
    {
        public string Person1 { get; set; }

        public string Person2 { get; set; }

        public string Tone { get; set; }
    }

    public class Chat : Conversation
    {
        public string ConversationID { get; set; }
    }

    public class Message
    {
        public string Sender { get; set; }

        public string Text { get; set; }

        public string Tone { get; set; }

        public string ToneSrc { get; set; }

        public bool IsImage { get; set; }
    }

    public class ConversationStatus
    {
        public string PersonID { get; set; }

        public bool Live { get; set; }

        public bool Typing { get; set; }
    }
}
