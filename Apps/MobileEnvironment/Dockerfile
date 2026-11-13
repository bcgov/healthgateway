FROM nginx:1.31
COPY nginx.conf /etc/nginx/nginx.conf
COPY released-build.sh /docker-entrypoint.d/40-released-build.sh
RUN chmod 0755 /docker-entrypoint.d/40-released-build.sh
EXPOSE 8080
