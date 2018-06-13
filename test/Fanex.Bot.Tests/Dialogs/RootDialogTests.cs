using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Fanex.Bot.Dialogs;
using Fanex.Bot.Dialogs.Impl;
using Microsoft.Bot.Connector;
using NSubstitute;
using Xunit;

namespace Fanex.Bot.Tests.Dialogs
{
    public class RootDialogTests
    {
        [Fact]
        public async Task HandleMessageAsync_MessageGroup_SendGroupId()
        {
            // Arrange
            var message = "group";

            // Act

            // Assert
        }
    }
}