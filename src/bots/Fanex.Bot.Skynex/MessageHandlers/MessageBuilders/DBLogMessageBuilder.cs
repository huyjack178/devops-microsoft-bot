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
                return dbLog.MsgInfo + MessageFormatSignal.NewLine + MessageFormatSignal.BreakLine;
            }

            var builder = new StringBuilder();

            builder.Append(MessageFormatSignal.BeginBold).Append("Server:").Append(MessageFormatSignal.EndBold).Append(" ")
                .Append(dbLog.ServerName).Append(MessageFormatSignal.NewLine);
            builder.Append(MessageFormatSignal.BeginBold).Append("Title:").Append(MessageFormatSignal.EndBold).Append(" ")
                .Append(dbLog.Title).Append(MessageFormatSignal.NewLine);
            builder.Append(MessageFormatSignal.BeginBold).Append("DateTime:").Append(MessageFormatSignal.EndBold).Append(" ")
                .Append(dbLog.LogDate).Append(MessageFormatSignal.DoubleNewLine);
            builder.Append(dbLog.MsgInfo).Append(MessageFormatSignal.NewLine).Append(MessageFormatSignal.BreakLine);

            return builder.ToString();
        }
    }
}