FROM mcr.microsoft.com/dotnet/aspnet:9.0
ENV ASPNETCORE_ENVIRONMENT=Production
RUN apt-get update -y && \
    apt-get install -y curl netcat-openbsd ca-certificates
WORKDIR /app
COPY tls-chain.crt /usr/local/share/ca-certificates/tls-chain.crt
COPY tls-root.crt /usr/local/share/ca-certificates/tls-root.crt
COPY trusted-root.crt /usr/local/share/ca-certificates/trusted-root.crt
RUN update-ca-certificates
