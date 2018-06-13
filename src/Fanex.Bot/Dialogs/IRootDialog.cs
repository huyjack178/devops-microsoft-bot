﻿namespace Fanex.Bot.Dialogs
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;

    public interface IRootDialog
    {
        Task HandleMessageAsync(IMessageActivity activity, string messageCmd);
    }
}