FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
WORKDIR /app
COPY . .
RUN mkdir -p Resources && \
    chmod uog+rwx Resources
EXPOSE 8080
ENTRYPOINT ["dotnet","JobScheduler.dll"]
