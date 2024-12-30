ARG CSPROJ_PATH="./src/Aiursoft.Kahla.Server/"
ARG PROJ_NAME="Aiursoft.Kahla.Server"

# ============================
# Prepare Building Environment
FROM hub.aiursoft.cn/mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
ARG CSPROJ_PATH
ARG PROJ_NAME
WORKDIR /src
COPY . .

# Build
RUN dotnet publish ${CSPROJ_PATH}${PROJ_NAME}.csproj  --configuration Release --no-self-contained --runtime linux-x64 --output /app

# ============================
# Prepare Runtime Environment
FROM hub.aiursoft.cn/mcr.microsoft.com/dotnet/aspnet:9.0
ARG PROJ_NAME
WORKDIR /app
COPY --from=build-env /app .

# Install wget and curl
RUN apt update; DEBIAN_FRONTEND=noninteractive apt install -y wget curl

# Edit appsettings.json to set storage path from /tmp/data to /data
RUN sed -i 's/\/tmp\/data/\/data/g' appsettings.json
RUN mkdir -p /data

VOLUME /data
EXPOSE 5000

ENV SRC_SETTINGS=/app/appsettings.json
ENV VOL_SETTINGS=/data/appsettings.json
ENV DLL_NAME=${PROJ_NAME}.dll

#ENTRYPOINT dotnet $DLL_NAME --urls http://*:5000
ENTRYPOINT ["/bin/bash", "-c", "\
    if [ ! -f \"$VOL_SETTINGS\" ]; then \
        cp $SRC_SETTINGS $VOL_SETTINGS; \
    fi && \
    if [ -f \"$SRC_SETTINGS\" ]; then \
        rm $SRC_SETTINGS; \
    fi && \
    ln -s $VOL_SETTINGS $SRC_SETTINGS && \
    dotnet $DLL_NAME --urls http://*:5000 \
"]

HEALTHCHECK --interval=10s --timeout=3s --start-period=180s --retries=3 CMD \
wget --quiet --tries=1 --spider http://localhost:5000/health || exit 1