#
#docker build --force-rm --tag myapp_basedon_dotnetbase:latest --file ./Dockerfile . --progress=plain
FROM ubuntu:focal-20240416 AS dotnetbase
RUN apt update && DEBIAN_FRONTEND=noninteractive apt install -y curl libicu66
ENV DOTNET_ROOT /usr/local/bin
RUN curl -LO https://download.visualstudio.microsoft.com/download/pr/19144d78-6f95-4810-a9f6-3bf86035a244/23f4654fc5352e049b517937f94be839/dotnet-sdk-6.0.421-linux-x64.tar.gz && \
    curl -LO https://download.visualstudio.microsoft.com/download/pr/a2b96f83-e22a-4fa6-a10e-709b3effac9a/0d6ade6c0ceebc8ef7dbf2b1a6d86f17/aspnetcore-runtime-5.0.17-linux-x64.tar.gz && \
    mkdir -p "$DOTNET_ROOT" && \
    tar zxf "dotnet-sdk-6.0.421-linux-x64.tar.gz" -C "$DOTNET_ROOT" && \
    tar zxf "aspnetcore-runtime-5.0.17-linux-x64.tar.gz" -C "$DOTNET_ROOT" && \
    rm -f dotnet-sdk-6.0.421-linux-x64.tar.gz && \
    rm -f aspnetcore-runtime-5.0.17-linux-x64.tar.gz && \
    dotnet dev-certs https -q && \
    dotnet --info
RUN curl -LO https://github.com/PowerShell/PowerShell/releases/download/v7.4.2/powershell_7.4.2-1.deb_amd64.deb && \
    dpkg -i powershell_7.4.2-1.deb_amd64.deb && \
    apt-get install -f && \
    rm -f powershell_7.4.2-1.deb_amd64.deb

##########################################
docker run --rm -it --name ub -v "${pwd}:/src_orig" ubuntu:noble-20250404
apt update
apt install -y git curl
mkdir /src && cp -r /src_orig/.git /src/ && cd /src && git reset --hard

curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --install-dir /usr/share/dotnet --channel 8.0
export DOTNET_ROOT=/usr/share/dotnet
export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
dotnet --info

apt search icu
apt install -y libicu74
curl -LOf https://github.com/PowerShell/PowerShell/releases/download/v7.4.7/powershell-lts_7.4.7-1.deb_amd64.deb
dpkg -i powershell-lts_7.4.7-1.deb_amd64.deb
apt-get install -f
rm -f powershell-lts_7.4.7-1.deb_amd64.deb

apt install openjdk-17-jre
apt install -y wget apt-transport-https gpg
wget -qO - https://packages.adoptium.net/artifactory/api/gpg/key/public | gpg --dearmor | tee /etc/apt/trusted.gpg.d/adoptium.gpg > /dev/null
echo "deb https://packages.adoptium.net/artifactory/deb $(awk -F= '/^VERSION_CODENAME/{print$2}' /etc/os-release) main" | tee /etc/apt/sources.list.d/adoptium.list
apt update
apt install -y temurin-17-jre

## netcore 3.1 supports openssl up to version 1.1
curl -LOf https://security.ubuntu.com/ubuntu/pool/main/o/openssl/libssl1.1_1.1.1f-1ubuntu2.24_amd64.deb
dpkg -i libssl1.1_1.1.1f-1ubuntu2.24_amd64.deb
rm -f libssl1.1_1.1.1f-1ubuntu2.24_amd64.deb
cp /etc/ssl/openssl.cnf /etc/openssl-1.1.cnf
sed -i 's/openssl_conf = openssl_init/#openssl_conf = openssl_init/g' /etc/openssl-1.1.cnf
export OPENSSL_CONF=/etc/openssl-1.1.cnf
export CLR_OPENSSL_VERSION_OVERRIDE=1.1

## netcore 3.1 supports icu version up to 70.x
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=
export CLR_ICU_VERSION_OVERRIDE=74.2

## trash about locales
export LANG=en_US.UTF-8
export LC_ALL=en_US.UTF-8
locale
echo $(apt search icu-devtools | grep devtools | awk '{split($0,a," ");print a[2]}' | awk '{split($0,a,"-");print a[1]}')
dpkg-reconfigure locales
apt-get install locales
locale-gen en_US.UTF-8
echo "LC_ALL=en_US.UTF-8" >> /etc/environment
echo "en_US.UTF-8 UTF-8" >> /etc/locale.gen
echo "LANG=en_US.UTF-8" > /etc/locale.conf
