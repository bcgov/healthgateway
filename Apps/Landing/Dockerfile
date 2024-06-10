FROM nginx:1.27

RUN rm /usr/share/nginx/html/*.html -f
COPY nginx.conf /etc/nginx/nginx.conf
COPY ./src/dist /usr/share/nginx/html
EXPOSE 8080
