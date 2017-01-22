# DynDnsUpdater
A command line update client for dyn.com or any other provider with support for [Remote Access Update API](https://help.dyn.com/remote-access-api) (i.e. strato.com).

Usage:
```
dyndnsupdater.exe /u:USERNAME /p:PASSWORD /h:HOSTNAME /f
```

Next to this one time call you can place the command into a bat/cmd file and execute this job periodically, using the Windows in-build scheduled task functionality.

As proposed by the [Remote Access Update API](https://help.dyn.com/remote-access-api) DynDnsUpdater will check the external ip of the current host by calling http://checkip.dyndns.com. The current ip will be saved under %LocalAppData%/DynDnsUpdater. At the next runtime DynDnsUpdater copmares the current ip and the saved ip. Only if the ip has changed since the last time an update request is send to the ISP.

By default DynDnsUpdater will uses dyn.com. If you like to use another ISP (with support for the protocol) open DnyDnsUpdater.exe.config and change the following user setting:

```
<setting name="UpdateServerName" serializeAs="String">
    <value>https://dyndns.strato.com</value>
</setting>
```

While running in background - as proposed via Windows scheduled task - you won't be able to view the console ouput. Therefore a log4net log file could be activated in log4net.config file.

log output:

```
2016-04-28 12:00:09,646 [1] INFO  DynDnsUpdater.Program (null) - Last Recorded IP Address: xx.xxx.x.xx
2016-04-28 12:00:09,686 [1] INFO  DynDnsUpdater.Program (null) - Detected IP Address: yy.yyy.yy.yyy
2016-04-28 12:00:09,946 [1] INFO  DynDnsUpdater.Program (null) - New IP Address will be propagated to: https://dyndns.strato.com
2016-04-28 12:00:10,816 [1] INFO  DynDnsUpdater.Program (null) - good  yy.yyy.yy.yyy
2016-04-29 12:00:27,395 [1] INFO  DynDnsUpdater.Program (null) - Last Recorded IP Address:  yy.yyy.yy.yyy
2016-04-29 12:00:27,435 [1] INFO  DynDnsUpdater.Program (null) - Detected IP Address:  yy.yyy.yy.yyy
2016-04-29 12:00:27,435 [1] INFO  DynDnsUpdater.Program (null) - Update not necessary
```
