FROM docker.io/alpine:3.21.3 AS base_deps
RUN apk add --upgrade --no-cache \
    ca-certificates \
    krb5 \
    libgcc \
    libintl \
    libssl3 \
    libstdc++ \
    tzdata \
    userspace-rcu \
    zlib \
    openssl \
    icu-libs \
    icu-data-full \
    libgdiplus \
    curl \
    less \
    nano \
    ncurses-terminfo-base \
    htop \
    && \
    ln -sf /usr/lib/libgdiplus.so.0.0.0 /usr/lib/libgdiplus.so
ENV ASPNETCORE_URLS=
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV LC_ALL=en_US.UTF-8
ENV LANG=en_US.UTF-8
STOPSIGNAL SIGINT
WORKDIR /
CMD ["/bin/sh"]

FROM base_deps AS tools_deps
RUN apk add --no-cache \
    lttng-ust \
    openssh-client \
    openldap-clients \
    && \
    curl -L https://github.com/PowerShell/PowerShell/releases/download/v7.4.7/powershell-7.4.7-linux-musl-x64.tar.gz -o /tmp/powershell.tar.gz && \
    mkdir -p /opt/microsoft/powershell/7 && \
    tar zxf /tmp/powershell.tar.gz -C /opt/microsoft/powershell/7 && \
    rm -f /tmp/powershell.tar.gz && \
    chmod +x /opt/microsoft/powershell/7/pwsh && \
    ln -s /opt/microsoft/powershell/7/pwsh /usr/bin/pwsh

FROM tools_deps AS dotnet_sdk
#RUN apk add --no-cache dotnet8-sdk
ENV DOTNET_ROOT=/usr/local/bin/dotnet
ENV PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true
ENV DOTNET_RUNNING_IN_CONTAINER=true
RUN curl -L https://builds.dotnet.microsoft.com/dotnet/Sdk/9.0.300/dotnet-sdk-9.0.300-linux-musl-x64.tar.gz -o /tmp/dotnet-sdk.tar.gz && \
    mkdir -p "$DOTNET_ROOT" && \
    tar zxf /tmp/dotnet-sdk.tar.gz -C "$DOTNET_ROOT" && \
    rm -f /tmp/dotnet-sdk.tar.gz && \
    dotnet dev-certs https -q && \
    dotnet --info

FROM dotnet_sdk AS dotnet_sdk_and_runtime
RUN curl -L https://builds.dotnet.microsoft.com/dotnet/aspnetcore/Runtime/8.0.17/aspnetcore-runtime-8.0.17-linux-musl-x64.tar.gz -o /tmp/aspnetcore-runtime.tar.gz && \
    tar zxf /tmp/aspnetcore-runtime.tar.gz -C "$DOTNET_ROOT" && \
    rm -f /tmp/aspnetcore-runtime.tar.gz && \
    dotnet --info

FROM base_deps AS myapp
COPY --from=build "/publish-linux-musl-x64" "/app"
WORKDIR /app
RUN chgrp 0 /app && chmod g=u /app && \
    chmod -R g=u /root
ENV HOME=/root
EXPOSE 5000
ENTRYPOINT ["./myapp"]
CMD ["http://127.0.0.1:5000"]
