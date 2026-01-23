#!/usr/bin/env bash
set -e
if [ "$#" -eq 0 ]; then
    echo "You must provide the image tag to use"
    exit 1
fi

tag=$1

if [ -z "$IMAGE_REPO" ]; then
  echo "Image Repo is not specified tagging locally"
  ImageRepo=""
else
  echo "Image Repo is specified tagging to $IMAGE_REPO"
  ImageRepo="${IMAGE_REPO}/"
fi

oc tag ${ImageRepo}webclient:${RELEASE_ARTIFACTS_WEBCLIENTIMAGE_BUILDNUMBER} webclient:$tag
oc tag ${ImageRepo}admin:${RELEASE_ARTIFACTS_ADMINBLAZORIMAGE_BUILDNUMBER} admin:$tag
oc tag ${ImageRepo}clinicaldocument:${RELEASE_ARTIFACTS_CLINICALDOCUMENTIMAGE_BUILDNUMBER} clinicaldocument:$tag
oc tag ${ImageRepo}encounter:${RELEASE_ARTIFACTS_ENCOUNTERSERVICEIMAGE_BUILDNUMBER} encounter:$tag
oc tag ${ImageRepo}gatewayapi:${RELEASE_ARTIFACTS_GATEWAYAPIIMAGE_BUILDNUMBER} gatewayapi:$tag
oc tag ${ImageRepo}hangfire:${RELEASE_ARTIFACTS_HANGFIREIMAGE_BUILDNUMBER} hangfire:$tag
oc tag ${ImageRepo}hgcdogs:${RELEASE_ARTIFACTS_HGCDOGSIMAGE_BUILDNUMBER} hgcdogs:$tag
oc tag ${ImageRepo}immunization:${RELEASE_ARTIFACTS_IMMSSERVICEIMAGE_BUILDNUMBER} immunization:$tag
oc tag ${ImageRepo}laboratory:${RELEASE_ARTIFACTS_LABORATORYIMAGE_BUILDNUMBER} laboratory:$tag
oc tag ${ImageRepo}medication:${RELEASE_ARTIFACTS_MEDICATIONSERVICEIMAGE_BUILDNUMBER} medication:$tag
oc tag ${ImageRepo}offline:${RELEASE_ARTIFACTS_OFFLINEIMAGE_BUILDNUMBER} offline:$tag
oc tag ${ImageRepo}patient:${RELEASE_ARTIFACTS_PATIENTSERVICEIMAGE_BUILDNUMBER} patient:$tag
