ARG DOTNET_VERSION=3.1

FROM mcr.microsoft.com/dotnet/core/sdk:${DOTNET_VERSION} AS build
ARG BUILD_USER=builder
USER root
# Fetch stock nodejs and install
ARG NODE_VERSION=v10.13.0
RUN mkdir -p /usr/local/lib/nodejs && \
    cd /usr/local/lib/nodejs && \
    echo "Downloading https://nodejs.org/dist/${NODE_VERSION}/node-${NODE_VERSION}-linux-x64.tar.gz" && \
    curl -sL https://nodejs.org/dist/${NODE_VERSION}/node-${NODE_VERSION}-linux-x64.tar.gz | tar -xz && \
    rm -f node-${NODE_VERSION}-linux-x64.tar.gz

RUN adduser --disabled-password --gecos "Build user" ${BUILD_USER}
USER ${BUILD_USER}
ENV PATH=/usr/local/lib/nodejs/node-${NODE_VERSION}-linux-x64/bin:$PATH

WORKDIR /home/${BUILD_USER}
#Copy Required Projects
COPY --chown=${BUILD_USER}:${BUILD_USER} . .

RUN npm install && \
    dotnet publish -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:${DOTNET_VERSION} AS runtime
ARG BUILD_USER=builder
ENV ASPNETCORE_URLS=http://+:8080
WORKDIR /app
COPY --from=build /home/${BUILD_USER}/out ./
EXPOSE 8080
ENTRYPOINT ["dotnet","Test.dll"]
