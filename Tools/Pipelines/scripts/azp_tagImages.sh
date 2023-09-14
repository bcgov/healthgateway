#!/usr/bin/env bash
set -e
if [ "$#" -eq 0 ]; then
    echo "You must provide the image tag to use"
    exit 1
fi

tag=$1

oc tag $(ImageRepo)/hgcdogs:$(Release.Artifacts.HGCDogsImage.BuildNumber) hgcdogs:$tag
oc tag $(ImageRepo)/adminwebclient:$(Release.Artifacts.AdminWebClientImage.BuildNumber) webadmin:$tag
oc tag $(ImageRepo)/encounter:$(Release.Artifacts.EncounterServiceImage.BuildNumber) encounter:$tag
oc tag $(ImageRepo)/laboratory:$(Release.Artifacts.LaboratoryImage.BuildNumber) laboratory:$tag
oc tag $(ImageRepo)/hangfire:$(Release.Artifacts.HangfireImage.BuildNumber) jobscheduler:$tag
oc tag $(ImageRepo)/medication:$(Release.Artifacts.MedicationServiceImage.BuildNumber) medication:$tag
oc tag $(ImageRepo)/immunization:$(Release.Artifacts.ImmsServiceImage.BuildNumber) immunization:$tag
oc tag $(ImageRepo)/patient:$(Release.Artifacts.PatientServiceImage.BuildNumber) patient:$tag
oc tag $(ImageRepo)/webclient:$(Release.Artifacts.WebClientImage.BuildNumber) webclient:$tag
oc tag $(ImageRepo)/gatewayapi:$(Release.Artifacts.GatewayApiImage.BuildNumber) gatewayapi:$tag
oc tag $(ImageRepo)/admin:$(Release.Artifacts.AdminBlazorImage.BuildNumber) admin:$tag
oc tag $(ImageRepo)/offline:$(Release.Artifacts.OfflineImage.BuildNumber) offline:$tag
oc tag $(ImageRepo)/clinicaldocument:$(Release.Artifacts.ClinicalDocumentImage.BuildNumber) clinicaldoc:$tag

