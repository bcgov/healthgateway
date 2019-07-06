FROM docker-registry.default.svc:5000/q6qfzk-tools/dotnet22-base:latest
ENV APPDLL=WebClient.dll
COPY . .
EXPOSE 5000
USER 1001
CMD ["dotnet","${APPDLL}"]
