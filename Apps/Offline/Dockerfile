FROM nginx:1.21
RUN rm /usr/share/nginx/html/*.html -f
COPY nginx.conf /etc/nginx/nginx.conf
COPY ./src /usr/share/nginx/html
EXPOSE 8080