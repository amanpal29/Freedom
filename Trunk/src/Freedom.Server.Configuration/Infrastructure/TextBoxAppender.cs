using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Freedom.Server.Tools.Infrastructure
{
    public sealed class TextBoxAppender : AppenderSkeleton
    {
        private readonly TextBox _textBox;

        public TextBoxAppender(TextBox textBox)
        {
            Name = GetType().Name;

            _textBox = textBox;

            Layout = new SimpleLayout();
        }

        protected override bool RequiresLayout => true;

        protected override void Append(LoggingEvent loggingEvent)
        {
            StringWriter writer = new StringWriter();

            RenderLoggingEvent(writer, loggingEvent);

            Dispatcher dispatcher = _textBox.Dispatcher;

            if (dispatcher == null || dispatcher.CheckAccess())
                Append(writer.ToString());
            else
                dispatcher.Invoke((Action<string>)Append, writer.ToString());
        }

        protected override void Append(LoggingEvent[] loggingEvents)
        {
            StringWriter writer = new StringWriter();

            foreach (LoggingEvent loggingEvent in loggingEvents)
                RenderLoggingEvent(writer, loggingEvent);

            Dispatcher dispatcher = _textBox.Dispatcher;

            if (dispatcher == null || dispatcher.CheckAccess())
                Append(writer.ToString());
            else
                dispatcher.Invoke((Action<string>)Append, writer.ToString());
        }

        private void Append(string text)
        {
            _textBox.AppendText(text);
            _textBox.ScrollToEnd();
        }
    }
}
