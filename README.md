log4net-appender
================

log4net.Appender.InitialStateAppender for easily shipping event logs from log4net to Initial State's log analysis service


###Example Config
> Note, this example config also references nuget package Log4Net.SyslogLayout

```
	<appender name="InitialStateAppender" type="Log4Net.InitialStateAppender.ApiAppender, Log4Net.InitialStateAppender">
      <ApiKey value="someapikey" />
      <ApiRootUrl value="https://dev.initialstate.com/api/v1/logs/" />
      <BucketId value="dbf1772a-b742-45b0-899f-488b0cfaab53" />
      <layout type="Log4Net.SyslogLayout.SyslogLayout, Log4Net.SyslogLayout">
        <structuredDataPrefix value="IS@50000" />
      </layout>
    </appender>
```