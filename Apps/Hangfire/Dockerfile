FROM docker-registry.default.svc:5000/q6qfzk-tools/dotnet22-base:latest
LABEL application="Health Gateway Batch System"
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV APPDLL=Hangfire.dll
COPY . .
EXPOSE 8080
USER 1001
CMD ["bash","-c","dotnet $APPDLL"]
