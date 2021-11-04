
#powershell
https://github.com/PowerShell/PowerShell/releases/download/v7.2.1/powershell-lts-7.2.1-1.rh.x86_64.rpm

#fluentd
https://calyptia-fluentd.s3.us-east-2.amazonaws.com/1/redhat/8/x86_64/calyptia-fluentd-1.3.4-1.el8.x86_64.rpm

# cyrus-sasl-gssapi (of the same version as cyrus-sasl-lib on base image [dnf list installed | grep sasl]), required for using kerberos auth in ldap
docker run --rm -it --user 0 --entrypoint bash registry.access.redhat.com/ubi8/dotnet-50-runtime:5.0-37
dnf download --resolve --arch x86_64 --downloaddir ./pkg cyrus-sasl-gssapi-2.1.27-6.el8_5
docker ps
docker cp b85f5c2db3cb:/opt/app-root/app/pkg/cyrus-sasl-gssapi-2.1.27-6.el8_5.x86_64.rpm ./build/rpm_packages/cyrus-sasl-gssapi-2.1.27-6.el8_5.x86_64.rpm

# openldap-clients, required for ldapsearch util which is used for diagnostic purposes
docker run --rm -it --user 0 --entrypoint bash registry.access.redhat.com/ubi8/dotnet-50-runtime:5.0-37
dnf download --resolve --arch x86_64 --downloaddir ./pkg openldap-clients-2.4.46-18.el8
docker ps
docker cp a033f3c8e8b6:/opt/app-root/app/pkg/openldap-clients-2.4.46-18.el8.x86_64.rpm ./build/rpm_packages/openldap-clients-2.4.46-18.el8.x86_64.rpm

# krb5-workstation, required for kinit, ktutil and other krb utils which is used for diagnostic purposes
docker run --rm -it --user 0 --entrypoint bash registry.access.redhat.com/ubi8/dotnet-50-runtime:5.0-37
dnf download --resolve --arch x86_64 --downloaddir ./pkg krb5-workstation-1.18.2-14.el8
docker ps
docker cp 139f6099a003:/opt/app-root/app/pkg ./build/rpm_packages/krb5-workstation

# libgdiplus, required for System.Drawing.Common
docker run --rm -it --user 0 --entrypoint bash registry.access.redhat.com/ubi8/dotnet-50-runtime:5.0-37
rpmkeys --import "http://keyserver.ubuntu.com/pks/lookup?op=get&search=0x3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF"
su -c 'curl https://download.mono-project.com/repo/centos8-stable.repo | tee /etc/yum.repos.d/mono-centos8-stable.repo'
dnf download --resolve --arch x86_64 --downloaddir ./pkg libgdiplus0
docker ps
docker cp dd414903cc35:/opt/app-root/app/pkg/ ./build/rpm_packages/libgdiplus/

# misc diagnostic utils
docker run --rm -it --user 0 --entrypoint bash registry.access.redhat.com/ubi8/dotnet-50-runtime:5.0-37
yum install -y https://dl.fedoraproject.org/pub/epel/epel-release-latest-8.noarch.rpm
dnf download --resolve --arch x86_64 --downloaddir ./pkg procps-ng
dnf download --resolve --arch x86_64 --downloaddir ./pkg htop
dnf download --resolve --arch x86_64 --downloaddir ./pkg nc
dnf download --resolve --arch x86_64 --downloaddir ./pkg nano
dnf download --resolve --arch x86_64 --downloaddir ./pkg iputils
dnf download --resolve --arch x86_64 --downloaddir ./pkg net-tools
docker ps
docker cp aacedc1fa2e0:/opt/app-root/app/pkg/ ./build/rpm_packages/utils/
