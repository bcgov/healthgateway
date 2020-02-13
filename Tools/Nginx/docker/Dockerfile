FROM nginx:mainline-alpine

COPY nginx.conf /etc/nginx/nginx.conf

# support running as arbitrary user which belogs to the root group
RUN chmod g+rwx /var/cache/nginx /var/run /var/log/nginx

RUN addgroup nginx root
USER nginx
