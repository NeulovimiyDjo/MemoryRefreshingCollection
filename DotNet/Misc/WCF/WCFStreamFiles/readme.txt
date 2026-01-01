IMPORTANT: this answer is available with HTTPS

Goto configure directory

C:/Windows/System32/inetsrv/config/

Open applicationHost.config file in edit mode.

Add this configure for your application

Example: configure for 1 MB uploading

<location path="SiteName" overrideMode="Allow">
   <system.webServer>
     ......
      <serverRuntime enabled="true" uploadReadAheadSize="1024000" />     
     .....
   </system.webServer>
</location>
Restart your site in the IIS