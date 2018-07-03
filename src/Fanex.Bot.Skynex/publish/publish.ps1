
PsExec.exe \\10.18.200.103  -s -i C:\windows\system32\inetsrv\appcmd.exe recycle apppool NexOps.NexBot.Site
PsExec.exe \\10.18.200.103  -s -i C:\windows\system32\inetsrv\appcmd.exe stop site NexOps.NexBot.Site
Start-Sleep 2
dotnet publish -o \\10.18.200.103\d$\WWW\NexOps\Skynex -f netcoreapp2.0 ..\Fanex.Bot.Skynex.csproj

PsExec.exe \\10.18.200.103  -s -i C:\windows\system32\inetsrv\appcmd.exe start site NexOps.NexBot.Site

Start-Sleep 5



PsExec.exe \\10.18.200.104  -s -i C:\windows\system32\inetsrv\appcmd.exe recycle apppool NexOps.NexBot.Site
PsExec.exe \\10.18.200.104  -s -i C:\windows\system32\inetsrv\appcmd.exe stop site NexOps.NexBot.Site
Start-Sleep 2
dotnet publish -o \\10.18.200.104\d$\WWW\NexOps\Skynex -f netcoreapp2.0 ..\Fanex.Bot.Skynex.csproj

PsExec.exe \\10.18.200.104  -s -i C:\windows\system32\inetsrv\appcmd.exe start site NexOps.NexBot.Site


