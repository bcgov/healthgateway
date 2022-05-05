#!/usr/bin/env bash
outfile=$1
if [ -z "$outfile" ]
  then
    echo "Output file needs to be specified"
    exit 98
fi
if [ -f "$outfile" ]; then
  echo "Output file $outfile already exists, exiting"
  exit 99
fi

echo "Output file $outfile will be created and contain the OpenShift tag delete commands"

file=$(mktemp istemp.json.XXXXXXXXXX)
trap "rm -f $file" 0 2 3 15
echo "OpenShift Imagestream data will be temporarily saved to $file"
echo

#The set of imagestreams to clean old builds from
imageStreams=("adminwebclient" "admin" "encounter" "hangfire" "hgcdogs" "immunization" "laboratory" "medication" "odrproxy" "patient" "webclient" "gatewayapi")
for isName in "${imageStreams[@]}"; do
  echo "Extracting ImageStream information for $isName"
  oc get imagestream $isName -o json > $file

  echo "Determining Production release -2 build image for $isName"
  imageHash=$(cat $file | jq '.status.tags[] | select(.tag=="production") | .items[2].image // empty')
  if [ ! -z "$imageHash" ]; then
    echo "N-2 Production build image is $imageHash"
    echo "Determining build number for image $imageHash"
    buildNumber=$(cat $file | jq '.status.tags[] | select(.tag!="dev" and .tag!="test" and .tag!="mock" and .tag!="production" and .tag!="latest" and .items[].image=='$imageHash') | .tag')
    echo "$isName image $imageHash maps to build $buildNumber"

    echo "Looking for builds prior to $buildNumber to cleanup"
    oldBuilds=($(cat $file | jq '.status.tags[] | select(.tag!="dev" and .tag!="test" and .tag!="mock" and .tag!="production" and .tag!="latest" and .tag<'$buildNumber').tag'))
    declare -i count=0
    for build in "${oldBuilds[@]}"; do 
        count=$count+1
        build="${build:1:-1}" #strip leading and trailing quotes
        echo "oc tag -d $isName:$build" >> $outfile
    done
    echo Found $count builds to cleanup and wrote to $outfile
    echo
  else
    echo "$isName does not have 3 Production versions, nothing to do."
    echo
  fi
done  