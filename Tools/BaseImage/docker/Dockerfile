FROM mcr.microsoft.com/dotnet/aspnet:9.0

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV TLS_CERT_PATH=/etc/ssl/certs/custom

# Install necessary dependencies
RUN apt-get update -y && \
    apt-get install -y --no-install-recommends curl netcat-openbsd ca-certificates && \
    mkdir -p $TLS_CERT_PATH

# Copy certificates into the container
COPY tls/tls-root.crt tls/tls-trusted-root.crt tls/tls-chain.crt $TLS_CERT_PATH/

# Add certificates to the trusted CA store
RUN for cert in tls-root.crt tls-trusted-root.crt tls-chain.crt; do \
        cp $TLS_CERT_PATH/$cert /usr/local/share/ca-certificates/$cert; \
    done && \
    update-ca-certificates && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app
RUN mkdir -p Resources && \
    chmod uog+rwx Resources
    