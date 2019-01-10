namespace Fanex.Bot.Skynex.MessageHandlers.MessageBuilders
{
    using System.Text;
    using Fanex.Bot.Helpers;
    using Fanex.Bot.Models.Log;

    public interface IMessageBuilder
    {
        string BuildMessage(object model);
    }

    public interface IDBLogMessageBuilder : IMessageBuilder
    {
    }

    public class DBLogMessageBuilder : IDBLogMessageBuilder
    {
        public string BuildMessage(object model)
        {
            var dbLog = DataHelper.Parse<DBLog>(model);

            if (dbLog.IsSimple)
            {
                return dbLog.MsgInfo + MessageFormatSignal.NEWLINE + MessageFormatSignal.DIVIDER;
            }

            var builder = new StringBuilder();

            builder.Append(MessageFormatSignal.BOLD_START).Append("Server:").Append(MessageFormatSignal.BOLD_END).Append(" ")
                .Append(dbLog.ServerName).Append(MessageFormatSignal.NEWLINE);
            builder.Append(MessageFormatSignal.BOLD_START).Append("Title:").Append(MessageFormatSignal.BOLD_END).Append(" ")
                .Append(dbLog.Title).Append(MessageFormatSignal.NEWLINE);
            builder.Append(MessageFormatSignal.BOLD_START).Append("DateTime:").Append(MessageFormatSignal.BOLD_END).Append(" ")
                .Append(dbLog.LogDate).Append(MessageFormatSignal.DOUBLE_NEWLINE);
            builder.Append(dbLog.MsgInfo).Append(MessageFormatSignal.NEWLINE).Append(MessageFormatSignal.DIVIDER);

            return builder.ToString();
        }
    }
}