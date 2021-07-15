FROM mcr.microsoft.com/dotnet/aspnet:5.0
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
RUN apt-get update -y && \ 
    apt-get install -y curl telnet
WORKDIR /app
COPY . .
EXPOSE 8080
ENTRYPOINT ["dotnet","Immunization.dll"]
