New-Item -Path \\10.18.200.122\d$\WWW\Fanex.Bot\Skynex.Test\app_offline.htm -type file -Force 

dotnet publish -o \\10.18.200.122\d$\WWW\Fanex.Bot\Skynex.Test -f netcoreapp2.0 ..\Fanex.Bot.Skynex.csproj

Remove-Item -Path \\10.18.200.122\d$\WWW\Fanex.Bot\Skynex.Test\app_offline.htm -Force 



New-Item -Path \\10.18.200.123\d$\WWW\Fanex.Bot\Skynex.Test\app_offline.htm -type file -Force 

dotnet publish -o \\10.18.200.123\d$\WWW\Fanex.Bot\Skynex.Test -f netcoreapp2.0 ..\Fanex.Bot.Skynex.csproj

Remove-Item -Path \\10.18.200.123\d$\WWW\Fanex.Bot\Skynex.Test\app_offline.htm -Force 
