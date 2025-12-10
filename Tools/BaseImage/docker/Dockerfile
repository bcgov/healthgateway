FROM mcr.microsoft.com/dotnet/aspnet:10.0
ENV ASPNETCORE_ENVIRONMENT=Production
RUN apt-get update -y && \
    apt-get install -y curl netcat-openbsd ca-certificates
WORKDIR /app
RUN mkdir -p Resources && \
    chmod uog+rwx Resources
COPY tls-chain.crt /usr/local/share/ca-certificates/tls-chain.crt
COPY tls-root.crt /usr/local/share/ca-certificates/tls-root.crt
COPY trusted-root.crt /usr/local/share/ca-certificates/trusted-root.crt
COPY entrust-ov-tls-issuing-rsa-ca-2.crt /usr/local/share/ca-certificates/entrust-ov-tls-issuing-rsa-ca-2.crt
COPY sectigo-public-server-auth-root-r46.crt /usr/local/share/ca-certificates/sectigo-public-server-auth-root-r46.crt
COPY usertrust-rsa-certification-authority.crt /usr/local/share/ca-certificates/usertrust-rsa-certification-authority.crt
RUN update-ca-certificates
