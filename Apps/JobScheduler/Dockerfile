FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
WORKDIR /app
COPY . .
EXPOSE 8080
ENTRYPOINT ["dotnet","JobScheduler.dll"]
