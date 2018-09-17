# Fanex.Bot.Client
Use for send your specific message to Skynex bot chat via Skype channel for notifications, logs, etc.

## Install
NuGet package is available [here](http://nuget.nexdev.net/packages/Fanex.Bot.Client/1.0.3.4)

## Getting Started
1. Add these keys to **web.config** or **appsettings.json**

    **ASP.NET MVC**
    ```json
        <appSettings
        	<add key="FanexBotClient:BotServiceUrl" value="https://bot.nexdev.net:6969/skynex/api" />
        	<add key="FanexBotClient:ClientId" value="YOUR_CLIENT_ID_PROVIDED_BY_NEXOPS" />
        	<add key="FanexBotClient:ClientPassword" value="YOUR_CLIENT_PASSWORD_PROVIDED_BY_NEXOPS" />
        </appSettings>
    ```

    **ASP.NET Core**
    ```json
        "FanexBotClient:BotServiceUrl": "https://bot.nexdev.net:6969/skynex/api"
        "FanexBotClient:ClientId": "YOUR_CLIENT_ID_PROVIDED_BY_NEXOPS",
        "FanexBotClient:ClientPassword": "YOUR_CLIENT_PASSWORD_PROVIDED_BY_NEXOPS"
    ```

2. Find and add **Skynex** to your group https://join.skype.com/bot/74cab44c-d551-42a7-9bbe-4d460d320516
3. Call **@Skynex** and send **group** to get **Conversation Id**
4. Add this code to your Global.aspx (ASP.NET MVC) or Startup.cs (ASP.NET Core)

    **ASP.NET MVC**
    ```csharp
    BotClientManager.UseConfig(new Configuration.BotSettings(
        new Uri(ConfigurationManager.AppSettings["FanexBotClient:BotServiceUrl"]),
        ConfigurationManager.AppSettings["FanexBotClient:ClientId"],
        ConfigurationManager.AppSettings["FanexBotClient:ClientPassword"]));
    ```

    **ASP.NET Core**
    ```csharp
    BotClientManager.UseConfig(new Configuration.BotSettings(
            new Uri(Configuration.GetSection("FanexBotClient:BotServiceUrl").Value),
            Configuration.GetSection("FanexBotClient:ClientId").Value,
            Configuration.GetSection("FanexBotClient:ClientPassword").Value));
    ```

5. Run this C# code

    ```csharp
	    var botConnector = new BotConnector();
	    botConnector.Send(message: "hello", "conversationId");
    ```