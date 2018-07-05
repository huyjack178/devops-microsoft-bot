
PsExec.exe \\10.18.200.122  -s -i C:\windows\system32\inetsrv\appcmd.exe recycle apppool Fanex.Bot
PsExec.exe \\10.18.200.122  -s -i C:\windows\system32\inetsrv\appcmd.exe stop site Fanex.Bot

dotnet publish -o \\10.18.200.122\d$\WWW\Fanex.Bot\Letstalk -f netcoreapp2.0 ..\Fanex.Bot.Letstalk.csproj

PsExec.exe \\10.18.200.122  -s -i C:\windows\system32\inetsrv\appcmd.exe start site Fanex.Bot

Start-Sleep 5



PsExec.exe \\10.18.200.123  -s -i C:\windows\system32\inetsrv\appcmd.exe recycle apppool Fanex.Bot
PsExec.exe \\10.18.200.123  -s -i C:\windows\system32\inetsrv\appcmd.exe stop site Fanex.Bot

dotnet publish -o \\10.18.200.123\d$\WWW\Fanex.Bot\Letstalk -f netcoreapp2.0 ..\Fanex.Bot.Letstalk.csproj

PsExec.exe \\10.18.200.123  -s -i C:\windows\system32\inetsrv\appcmd.exe start site Fanex.Bot

