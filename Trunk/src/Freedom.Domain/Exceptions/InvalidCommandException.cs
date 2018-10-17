using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Freedom.Domain.CommandModel;

namespace Freedom.Domain.Exceptions
{
    [Serializable]
    public class InvalidCommandException : ArgumentException
    {
        public InvalidCommandException()
        {
        }

        public InvalidCommandException(string message)
            : base(message)
        {
        }

        public InvalidCommandException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public InvalidCommandException(CommandBase command)
        {
            Command = command;
        }

        public InvalidCommandException(CommandBase command, string message)
            : base(message)
        {
            Command = command;

        }

        public InvalidCommandException(CommandBase command, string message, Exception inner)
            : base(message, inner)
        {
            Command = command;
        }

        protected InvalidCommandException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            SetCommandMemento(info.GetString(nameof(Command)));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(Command), GetCommandMemento());
        }

        public CommandBase Command { get; set; }

        private string GetCommandMemento()
        {
            if (Command == null)
                return null;
    
            DataContractSerializer serializer = new DataContractSerializer(typeof(CommandBase));

            StringBuilder stringBuilder = new StringBuilder();

            using (XmlWriter writer = XmlWriter.Create(stringBuilder))
                serializer.WriteObject(writer, Command);

            return stringBuilder.ToString();
        }

        private void SetCommandMemento(string commandMemento)
        {
            if (string.IsNullOrEmpty(commandMemento))
            {
                Command = null;
            }
            else
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(CommandBase));

                using (XmlReader xmlReader = XmlReader.Create(new StringReader(commandMemento)))
                    Command = serializer.ReadObject(xmlReader) as CommandBase;
            }
        }
    }
}
