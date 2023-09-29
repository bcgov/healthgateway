#!/usr/bin/env bash
set -e
if [ "$#" -eq 0 ]; then
    echo "You must provide the image tag to use"
    exit 1
fi

tag=$1

oc tag $(ImageRepo)/webclient:${RELEASE_ARTIFACTS_WEBCLIENTIMAGE_BUILDNUMBER} webclient:$tag
oc tag $(ImageRepo)/admin:${RELEASE_ARTIFACTS_ADMINBLAZORIMAGE_BUILDNUMBER} admin:$tag
oc tag $(ImageRepo)/adminwebclient:${RELEASE_ARTIFACTS_ADMINWEBCLIENTIMAGE_BUILDNUMBER} webadmin:$tag
oc tag $(ImageRepo)/clinicaldocument:${RELEASE_ARTIFACTS_CLINICALDOCUMENTSIMAGE_BUILDNUMBER} clinicaldoc:$tag
oc tag $(ImageRepo)/encounter:${RELEASE_ARTIFACTS_ENCOUNTERSERVICEIMAGE_BUILDNUMBER} encounter:$tag
oc tag $(ImageRepo)/gatewayapi:${RELEASE_ARTIFACTS_GATEWAYAPIIMAGE_BUILDNUMBER} gatewayapi:$tag
oc tag $(ImageRepo)/hangfire:${RELEASE_ARTIFACTS_HANGFIREIMAGE_BUILDNUMBER} jobscheduler:$tag
oc tag $(ImageRepo)/hgcdogs:${RELEASE_ARTIFACTS_HGCDOGSIMAGE_BUILDNUMBER} hgcdogs:$tag
oc tag $(ImageRepo)/immunization:${RELEASE_ARTIFACTS_IMMSSERVICEIMAGE_BUILDNUMBER} immunization:$tag
oc tag $(ImageRepo)/laboratory:${RELEASE_ARTIFACTS_LABORATORYSERVICEIMAGE_BUILDNUMBER} laboratory:$tag
oc tag $(ImageRepo)/medication:${RELEASE_ARTIFACTS_MEDICATIONSERVICEIMAGE_BUILDNUMBER} medication:$tag
oc tag $(ImageRepo)/offline:${RELEASE_ARTIFACTS_OFFLINEIMAGE_BUILDNUMBER} offline:$tag
oc tag $(ImageRepo)/patient:${RELEASE_ARTIFACTS_PATIENTSERVICEIMAGE_BUILDNUMBER} patient:$tag
