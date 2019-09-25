using System;
using Fanex.Bot.Core._Shared.Enumerations;
using Fanex.Bot.Core.Bot.Models;
using Fanex.Bot.Skynex._Shared.Base;
using Fanex.Bot.Skynex.Bot;
using Fanex.Bot.Skynex.GitLab;
using Fanex.Bot.Skynex.Log;
using Fanex.Bot.Skynex.Sentry;
using Fanex.Bot.Skynex.UM;
using Fanex.Bot.Skynex.Zabbix;

// ReSharper disable All

namespace Fanex.Bot.Skynex.Tests.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fixtures;
    using Microsoft.Bot.Connector;
    using NSubstitute;
    using Xunit;

    public class MessagesControllerTests : IClassFixture<BotConversationFixture>
    {
        private readonly BotConversationFixture conversationFixture;
        private readonly ICommonDialog commonDialog;
        private readonly ILogDialog logDialog;
        private readonly IGitLabDialog gitLabDialog;
        private readonly IUnderMaintenanceDialog umDialog;
        private readonly IDBLogDialog dbLogDialog;
        private readonly IZabbixDialog zabbixDialog;
        private readonly ISentryDialog sentryDialog;
        private readonly MessagesController messagesController;

        public MessagesControllerTests(BotConversationFixture conversationFixture)
        {
            this.conversationFixture = conversationFixture;
            commonDialog = Substitute.For<ICommonDialog>();
            logDialog = Substitute.For<ILogDialog>();
            gitLabDialog = Substitute.For<IGitLabDialog>();
            umDialog = Substitute.For<IUnderMaintenanceDialog>();
            dbLogDialog = Substitute.For<IDBLogDialog>();
            zabbixDialog = Substitute.For<IZabbixDialog>();
            sentryDialog = Substitute.For<ISentryDialog>();

            var functionFactory = new Func<string, IDialog>((functionName) =>
            {
                switch (functionName)
                {
                    case FunctionType.LogMSiteFunctionName:
                        return logDialog;

                    case FunctionType.LogDbFunctionName:
                        return dbLogDialog;

                    case FunctionType.LogSentryFunctionName:
                        return sentryDialog;

                    case FunctionType.UnderMaintenanceFunctionName:
                        return umDialog;

                    case FunctionType.GitLabFunctionName:
                        return gitLabDialog;

                    case FunctionType.ZabbixFunctionName:
                        return zabbixDialog;

                    default:
                        return commonDialog;
                }
            });
            messagesController = new MessagesController(
                this.conversationFixture.Conversation,
                this.conversationFixture.Configuration,
                commonDialog,
                functionFactory);
        }

        [Fact]
        public async Task Post_ActivityMessage_HandMessageCommand_LogCommand_CallLogDialog()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = "log_msite add" };

            // Act
            await messagesController.Post(activity);

            // Asserts
            await logDialog.Received().HandleMessage(Arg.Is(activity), "log_msite add");
        }

        [Fact]
        public async Task Post_ActivityMessage_HandMessageCommand_GitlabCommand_CallGitLabDialog()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = "gitlab" };

            // Act
            await messagesController.Post(activity);

            // Asserts
            await gitLabDialog.Received().HandleMessage(Arg.Is(activity), "gitlab");
        }

        [Fact]
        public async Task Post_ActivityMessage_HandMessageCommand_UMCommand_CallUMDialog()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = "um" };

            // Act
            await messagesController.Post(activity);

            // Asserts
            await umDialog.Received().HandleMessage(Arg.Is(activity), "um");
        }

        [Fact]
        public async Task Post_ActivityMessage_HandMessageCommand_LogDbCommand_CallDbLogDialog()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = "log_database add" };

            // Act
            await messagesController.Post(activity);

            // Asserts
            await dbLogDialog.Received().HandleMessage(Arg.Is(activity), "log_database add");
        }

        [Fact]
        public async Task Post_ActivityMessage_HandMessageCommand_LogSentryCommand_CallSentryDialog()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = "log_sentry add" };

            // Act
            await messagesController.Post(activity);

            // Asserts
            await sentryDialog.Received().HandleMessage(Arg.Is(activity), "log_sentry add");
        }

        [Fact]
        public async Task Post_ActivityMessage_HandMessageCommand_ZabbixCommand_CallZabbixDialog()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = "zabbix add" };

            // Act
            await messagesController.Post(activity);

            // Asserts
            await zabbixDialog.Received().HandleMessage(Arg.Is(activity), "zabbix add");
        }

        [Fact]
        public async Task Post_ActivityMessage_HandMessageCommand_OtherCommand_CallDialog()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = "group" };

            // Act
            await messagesController.Post(activity);

            // Asserts
            await commonDialog.Received().HandleMessage(Arg.Is(activity), "group");
        }

        [Theory]
        [InlineData("@Skynex group")]
        [InlineData("Skynex group")]
        public async Task Post_ActivityMessage_HandMessageCommand_TextHasBotName_RemoveBotName(string message)
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = message };
            conversationFixture.Configuration.GetSection("BotName")?.Value.Returns("Skynex");

            // Act
            await messagesController.Post(activity);

            // Asserts
            await commonDialog.Received().HandleMessage(Arg.Is(activity), "group");
        }

        [Fact]
        public async Task Post_ActivityContactRelationUpdate_CallHandleContactRelationUpdate()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.ContactRelationUpdate,
                Conversation = new ConversationAccount { Id = "43235grerew" },
            };

            // Act
            await messagesController.Post(activity);

            // Asserts
            await commonDialog.Received().HandleContactRelationUpdate(Arg.Is(activity));
        }

        [Fact]
        public async Task Post_ActivityConverstationUpdate_CallHandleConversationUpdate()
        {
            // Arrange
            conversationFixture.Configuration.GetSection("BotId").Value.Returns("12324342345");
            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                Conversation = new ConversationAccount { Id = "43235grerew" },
            };

            // Act
            await messagesController.Post(activity);

            // Asserts
            await commonDialog.Received().HandleConversationUpdate(Arg.Is(activity));
        }

        [Fact]
        public async Task Forward_Always_SendToConversation_ReturnOk()
        {
            // Arrange
            var conversationId = "@#423424";
            var message = "hello";
            var expectedResult = Result.CreateSuccessfulResult();
            conversationFixture.Conversation.SendAsync(conversationId, message).Returns(expectedResult);

            // Act
            var result = await messagesController.Forward(message, conversationId);

            // Assert
            await conversationFixture.Conversation.Received(1).SendAsync(conversationId, message);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(expectedResult, result.Value);
        }
    }
}