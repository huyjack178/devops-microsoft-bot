using System.Text;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core.Log.Models;
using Fanex.Bot.Helpers;
using Fanex.Bot.Skynex._Shared.Base;

namespace Fanex.Bot.Skynex.Log
{
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
                return dbLog.MsgInfo + MessageFormatSymbol.NEWLINE + MessageFormatSymbol.DIVIDER;
            }

            var builder = new StringBuilder();

            builder.Append(MessageFormatSymbol.BOLD_START).Append("Server:").Append(MessageFormatSymbol.BOLD_END).Append(" ")
                .Append(dbLog.ServerName).Append(MessageFormatSymbol.NEWLINE);
            builder.Append(MessageFormatSymbol.BOLD_START).Append("Title:").Append(MessageFormatSymbol.BOLD_END).Append(" ")
                .Append(dbLog.Title).Append(MessageFormatSymbol.NEWLINE);
            builder.Append(MessageFormatSymbol.BOLD_START).Append("DateTime:").Append(MessageFormatSymbol.BOLD_END).Append(" ")
                .Append(dbLog.LogDate).Append(MessageFormatSymbol.DOUBLE_NEWLINE);
            builder.Append(dbLog.MsgInfo).Append(MessageFormatSymbol.NEWLINE).Append(MessageFormatSymbol.DIVIDER);

            return builder.ToString();
        }
    }
}