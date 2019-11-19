using System;
namespace Feels.Objects
{
    public class User
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public string Image { get; set; }

        public string Location { get; set; }

        public string Tone { get; set; }

        public string ToneSrc { get; set; }
    }

    public class Person : User
    {
        public string Conversation { get; set; }
    }
}
