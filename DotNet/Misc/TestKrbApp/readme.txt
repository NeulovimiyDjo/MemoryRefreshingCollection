#bin/appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "LdapServer": "computer_name_of_domain_controller",
  "LdapSearchBase": "cn=users,dc=adtestdomain,dc=com",
  "LdapSearchFilter": "cn=user4",
  "LdapAttributesList": ["cn", "distinguishedName", "samaccountname", "objectSid"],
  "TestClientHost": "localhost",
  "TestClientUser": "user4@ADTESTDOMAIN.COM",
  "NetShareHost": "orion",
  "NetShareName": "exchange",
  "AllowedHosts": "*"
}

#bin/hosts (LF endings)
127.0.0.1 localhost
ip_of_domain_controller computer_name_of_domain_controller

#bin/krb5.conf (LF endings)
[libdefaults]
    default_realm = ADTESTDOMAIN.COM
    kdc_timesync = 1
    ccache_type = 4
    forwardable = true
    proxiable = true
    dns_lookup_realm = true
    dns_lookup_kdc = true
[realms]
    ADTESTDOMAIN.COM = {
        kdc = computer_name_of_domain_controller
        admin_server = computer_name_of_domain_controller
        default_domain = ADTESTDOMAIN.COM
    }
[domain_realm]
    .adtestdomain.com = ADTESTDOMAIN.COM
    adtestdomain.com = ADTESTDOMAIN.COM

#bin/service.keytab (configure IE (truested + intranet) to always prompt user and pass)
setspn -S HTTP/localhost user3
setspn -L user3
ktutil
add_entry -password -p HTTP/localhost@ADTESTDOMAIN.COM -k 1 -e arcfour-hmac
add_entry -password -p HTTP/localhost@ADTESTDOMAIN.COM -k 1 -e aes128-cts-hmac-sha1-96
add_entry -password -p HTTP/localhost@ADTESTDOMAIN.COM -k 1 -e aes256-cts-hmac-sha1-96
wkt service.keytab
q

#bin/client.keytab
ktutil
add_entry -password -p user2@ADTESTDOMAIN.COM -k 1 -e arcfour-hmac
add_entry -password -p user2@ADTESTDOMAIN.COM -k 1 -e aes128-cts-hmac-sha1-96
add_entry -password -p user2@ADTESTDOMAIN.COM -k 1 -e aes256-cts-hmac-sha1-96
wkt client.keytab
q
