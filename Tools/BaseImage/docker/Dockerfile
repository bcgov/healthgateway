FROM mcr.microsoft.com/dotnet/aspnet:8.0
ENV ASPNETCORE_ENVIRONMENT=Production
RUN apt-get update -y && \ 
    apt-get install -y curl netcat-openbsd
WORKDIR /app
RUN mkdir -p Resources && \
    chmod uog+rwx Resources
    