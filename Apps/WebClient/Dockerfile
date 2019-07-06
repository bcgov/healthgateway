FROM docker-registry.default.svc:5000/q6qfzk-tools/dotnet22-base:latest

COPY src .
RUN dotnet build
EXPOSE 5000
USER 1001
CMD ["dotnet","run"]
