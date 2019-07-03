#!/usr/bin/env bash

#generate new admin password to override silly default
admin_password=$(oc -o json get secret sonarqube-admin-password | sed -n 's/.*"password": "\(.*\)",/\1/p' | base64 --decode)

echo "Generated password is '$admin_password'. Enter to continue..."
read BLAH

# figure out the sonarqube route so we know where to access the API
sonarqube_url=$(oc get routes | awk '/sonarqube/{print $2}')
curl -G -X POST -v -u admin:admin --data-urlencode "login=admin" --data-urlencode "password=$admin_password" --data-urlencode "previousPassword=admin" "https://$sonarqube_url/api/users/change_password"

# Create users
curl -G -X POST -v -u admin:$admin_password --data-urlencode "login=azure" --data-urlencode "password=password" --data-urlencode "name=AzureAgent" "https://$sonarqube_url/api/users/create/"

curl -G -X POST -v -u admin:$admin_password --data-urlencode "login=FuriousLlama" --data-urlencode "password=password" --data-urlencode "name=Manuel Rodriguez" "https://$sonarqube_url/api/users/create"

curl -G -X POST -v -u admin:$admin_password --data-urlencode "login=sslaws" --data-urlencode "password=password" --data-urlencode "name=Stephen Laws" "https://$sonarqube_url/api/users/create"

curl -G -X POST -v -u admin:$admin_password --data-urlencode "login=TiagoGraf89" --data-urlencode "password=password" --data-urlencode "name=Tiago Graf" "https://$sonarqube_url/api/users/create"

curl -G -X POST -v -u admin:$admin_password --data-urlencode "login=bradhead" --data-urlencode "password=password" --data-urlencode "name=Brad Head" "https://$sonarqube_url/api/users/create"
