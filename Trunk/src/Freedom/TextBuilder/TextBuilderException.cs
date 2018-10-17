using System;
using System.Runtime.Serialization;

namespace Freedom.TextBuilder
{
    [Serializable]
    public class TextBuilderException : FormatException
    {
        internal static readonly TextBuilderError[] DefaultErrors = new TextBuilderError[0];

        private readonly TextBuilderError[] _errors;

        public TextBuilderException()
        {
        }

        public TextBuilderException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public TextBuilderException(TextBuilderError[] errors)
        {
            _errors = errors;
        }

        public TextBuilderException(string message, TextBuilderError[] errors)
            : base(message)
        {
            _errors = errors;
        }

        protected TextBuilderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _errors = (TextBuilderError[]) info.GetValue("errors", typeof (TextBuilderError[]));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("errors", _errors);
        }

        public TextBuilderError[] Errors => _errors ?? DefaultErrors;
    }
}
