# Fanex.Bot.Client
Use for send your specific message to Skynex bot chat via Skype channel for notifications, logs, etc.

## Install
NuGet package is available [here](http://nuget.nexdev.net/packages/Fanex.Bot.Client/1.0.3.4)

## Getting Started
1. Add appSettings to Web.config

```json
<appSettings
	<add key="FanexBotClient:BotServiceUrl" value="https://bot.nexdev.net:6969/skynex/api" />
	<add key="FanexBotClient:ClientId" value="96ece90f-deab-4f11-b8ef-cdabf58542fb" />
	<add key="FanexBotClient:ClientPassword" value="pbJBPC295+afgvaCLR46+{_" />
	<add key="FanexBotClient:TokenUrl" value="https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token" />
</appSettings>
```

2. Find and add **Skynex** to your group https://join.skype.com/bot/74cab44c-d551-42a7-9bbe-4d460d320516
3. Send **@Skynex group** to get **Conversation Id**
4. Run this C# code
```csharp
	var botConnector = new BotConnector();
	botConnector.Send(message: "hello", "conversationId");
```