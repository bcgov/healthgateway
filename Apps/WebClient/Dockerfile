ARG DOTNET_VERSION=2.2
FROM mcr.microsoft.com/dotnet/core/aspnet:${DOTNET_VERSION} AS runtime
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
WORKDIR /app
COPY . .
EXPOSE 8080
ENTRYPOINT ["dotnet","Webclient.dll"]
