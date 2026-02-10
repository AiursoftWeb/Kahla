ARG CSPROJ_PATH="./src/Aiursoft.Kahla.Server"
ARG PROJ_NAME="Aiursoft.Kahla.Server"
ARG FRONT_END_PATH="./src/Aiursoft.Kahla.Frontend"

# ============================
# Prepare node dist
# ============================
FROM hub.aiursoft.com/node:24-alpine AS npm-env
ARG FRONT_END_PATH
WORKDIR /src

# Restore
COPY ${FRONT_END_PATH}/kahla.app/package.json ./kahla.app/
COPY ${FRONT_END_PATH}/kahla.sdk/package.json ./kahla.sdk/
COPY ${FRONT_END_PATH}/package.json .
COPY ${FRONT_END_PATH}/yarn.lock .
COPY ${FRONT_END_PATH}/.yarnrc.yml .
RUN corepack enable && corepack yarn install --immutable

# Build
COPY ${FRONT_END_PATH}/ .
RUN corepack yarn run build

# ============================
# Prepare .NET binaries
# ============================
FROM hub.aiursoft.com/aiursoft/internalimages/dotnet AS build-env
ARG CSPROJ_PATH
ARG PROJ_NAME
WORKDIR /src

# Build
COPY . .
RUN dotnet publish ${CSPROJ_PATH}/${PROJ_NAME}.csproj --configuration Release --no-self-contained --runtime linux-x64 --output /app

# ============================
# Prepare runtime image
# ============================
FROM hub.aiursoft.com/aiursoft/internalimages/dotnetonlyruntime
ARG PROJ_NAME
WORKDIR /app
COPY --from=build-env /app .
COPY --from=npm-env /src/kahla.app/dist/ng/browser ./wwwroot/

# Edit appsettings.json
RUN sed -i 's/DataSource=app.db/DataSource=\/data\/app.db/g' appsettings.json && \
    sed -i 's/\/tmp\/data/\/data/g' appsettings.json && \
    mkdir -p /data

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