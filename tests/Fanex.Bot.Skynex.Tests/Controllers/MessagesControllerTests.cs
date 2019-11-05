using System;
using System.Text.RegularExpressions;
using Fanex.Bot.Core._Shared.Enumerations;
using Fanex.Bot.Core.Bot.Models;
using Fanex.Bot.Skynex._Shared.Base;
using Fanex.Bot.Skynex.Bot;
using Fanex.Bot.Skynex.ExecuteSP;
using Fanex.Bot.Skynex.GitLab;
using Fanex.Bot.Skynex.Log;
using Fanex.Bot.Skynex.Sentry;
using Fanex.Bot.Skynex.UM;
using Fanex.Bot.Skynex.Zabbix;
using Microsoft.Extensions.Configuration;

// ReSharper disable All

namespace Fanex.Bot.Skynex.Tests.Controllers
{
    using System.Threading.Tasks;
    using Fixtures;
    using Microsoft.Bot.Connector;
    using NSubstitute;
    using Xunit;

#pragma warning disable S2699 // Tests should include assertions

    public class MessagesControllerTests : IClassFixture<BotConversationFixture>
    {
        private readonly BotConversationFixture conversationFixture;
        private readonly IMessengerDialog skypeDialog;
        private readonly ITelegramDialog telegramDialog;
        private readonly ILogDialog logDialog;
        private readonly IGitLabDialog gitLabDialog;
        private readonly IUnderMaintenanceDialog umDialog;
        private readonly IDBLogDialog dbLogDialog;
        private readonly IZabbixDialog zabbixDialog;
        private readonly ISentryDialog sentryDialog;
        private readonly IExecuteSpDialog executeSpDialog;
        private readonly MessagesController messagesController;
        private readonly IConfiguration configuration;

        public MessagesControllerTests(BotConversationFixture conversationFixture)
        {
            configuration = new ConfigurationBuilder()
                .AddJsonFile($"{AppDomain.CurrentDomain.BaseDirectory}/../../../appsettings.json")
                .Build();
            this.conversationFixture = conversationFixture;
            skypeDialog = Substitute.For<IMessengerDialog>();
            telegramDialog = Substitute.For<ITelegramDialog>();
            logDialog = Substitute.For<ILogDialog>();
            gitLabDialog = Substitute.For<IGitLabDialog>();
            umDialog = Substitute.For<IUnderMaintenanceDialog>();
            dbLogDialog = Substitute.For<IDBLogDialog>();
            zabbixDialog = Substitute.For<IZabbixDialog>();
            sentryDialog = Substitute.For<ISentryDialog>();
            executeSpDialog = Substitute.For<IExecuteSpDialog>();

            var functionDialogFactory = new Func<string, string, IDialog>((functionName, messengerName) =>
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

                    case FunctionType.ExecuteSpFunctionName:
                        return executeSpDialog;

                    default:
                        switch (messengerName)
                        {
                            case MessengerType.TelegramMessengerTypeName:
                                return telegramDialog;

                            default:
                                return skypeDialog;
                        }
                }
            });

            var messengerDialogFactory = new Func<string, IMessengerDialog>(messengerTypeName =>
            {
                switch (messengerTypeName)
                {
                    case MessengerType.TelegramMessengerTypeName:
                        return telegramDialog;

                    default:
                        return skypeDialog;
                }
            });

            messagesController = new MessagesController(
                this.conversationFixture.Conversation,
                configuration,
                messengerDialogFactory,
                functionDialogFactory);
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
        public async Task Post_ActivityMessage_HandMessageCommand_ExecuteSpCommand_CallExecuteDialog()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = "query add" };

            // Act
            await messagesController.Post(activity);

            // Asserts
            await executeSpDialog.Received().HandleMessage(Arg.Is(activity), "query add");
        }

        [Fact]
        public async Task Post_ActivityMessage_HandMessageCommand_OtherCommand_CallDialog()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = "group" };

            // Act
            await messagesController.Post(activity);

            // Asserts
            await skypeDialog.Received().HandleMessage(Arg.Is(activity), "group");
        }

        [Theory]
        [InlineData("@Skynex-Bot-Test log_sentry", "log_sentry")]
        [InlineData("Skynex-Bot-Test log_sentry", "log_sentry")]
        [InlineData("SkynexTestBot log_sentry", "log_sentry")]
        [InlineData("@Skynex log_sentry start score247", "log_sentry start score247")]
        [InlineData("log_sentry", "log_sentry")]
        public async Task Post_ActivityMessage_HandMessageCommand_TextHasBotName_RemoveBotName(string message, string expectedMessage)
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = message };

            // Act
            await messagesController.Post(activity);

            // Asserts
            await sentryDialog.Received().HandleMessage(Arg.Is(activity), expectedMessage);
        }

        [Fact]
#pragma warning disable S2699 // Tests should include assertions
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
            await skypeDialog.Received().HandleContactRelationUpdate(Arg.Is(activity));
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
            await skypeDialog.Received().HandleConversationUpdate(Arg.Is(activity));
        }

#pragma warning restore S2699 // Tests should include assertions

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

#pragma warning restore S2699 // Tests should include assertions
}