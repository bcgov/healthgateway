FROM nginx:1.29@sha256:1881968aff6f7cdcc4b888c00a11f4ce241ad7ec957e0cb4a9e19e93a3ff87ea
RUN rm /usr/share/nginx/html/*.html -f
COPY nginx.conf /etc/nginx/nginx.conf
COPY ./src /usr/share/nginx/html
EXPOSE 8080