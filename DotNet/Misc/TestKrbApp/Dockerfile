FROM registry.access.redhat.com/ubi8/dotnet-50-runtime:5.0-39 AS base
USER root
WORKDIR /app

COPY ["build_deps/cyrus-sasl-gssapi-2.1.27-6.el8_5.x86_64.rpm", "."]
RUN rpm --install cyrus-sasl-gssapi-2.1.27-6.el8_5.x86_64.rpm && rm -f cyrus-sasl-gssapi-2.1.27-6.el8_5.x86_64.rpm

COPY ["build_deps/openldap-clients-2.4.46-18.el8.x86_64.rpm", "."]
RUN rpm --install openldap-clients-2.4.46-18.el8.x86_64.rpm && rm -f openldap-clients-2.4.46-18.el8.x86_64.rpm

COPY ["build_deps/krb5-workstation", "/rpm_packages"]
RUN rpm --upgrade /rpm_packages/*.rpm && rm -rf /rpm_packages

COPY ["build_deps/utils", "/rpm_packages"]
RUN rpm -i /rpm_packages/*.rpm && rm -rf /rpm_packages

ENV ASPNETCORE_URLS=http://*:80
ENV KRB5_KTNAME=/app/service.keytab
ENV KRB5_CLIENT_KTNAME=/app/client.keytab
ENV KRB5_TRACE=/dev/stdout

ENV Logging__LogLevel__Microsoft.Hosting.Lifetime=Information
ENV Logging__LogLevel__Microsoft=Information
ENV Logging__LogLevel__Default=Trace

FROM base AS base_with_deps
COPY ["build_deps/powershell-lts-7.2.1-1.rh.x86_64.rpm", "."]
RUN rpm --install powershell-lts-7.2.1-1.rh.x86_64.rpm && rm -f powershell-lts-7.2.1-1.rh.x86_64.rpm

FROM registry.access.redhat.com/ubi8/dotnet-60:6.0-3 AS build
USER root
WORKDIR /src
COPY ["./build_deps/root_ca.crt", "/usr/share/pki/ca-trust-source/anchors/"]
RUN update-ca-trust
COPY ["build_deps/NuGet.Config", "./build_deps/"]
COPY ["build_deps/packages", "./build_deps/packages"]
COPY ["TestApp.csproj", "."]
RUN dotnet restore "./TestApp.csproj" --configfile "./build_deps/NuGet.Config"
COPY . .
RUN dotnet publish "TestApp.csproj" -c Release --no-self-contained -o /app/publish

FROM base_with_deps AS final
COPY --from=build /app/publish .
RUN chgrp 0 /app && chmod g=u /app
ENV ASPNETCORE_URLS=http://*:5000
ENTRYPOINT ["dotnet", "TestApp.dll"]