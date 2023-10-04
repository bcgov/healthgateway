#!/usr/bin/env bash

apps=("offline" "webclient" "admin" "adminwebclient" "encounter" "hangfire" "immunization" "laboratory" "medication" "mock" "patient" "gatewayapi" "clinicaldocument" "hgcdogs")
declare -A artifacts # associative array
artifacts["offline"]="OfflineImage"
artifacts["webclient"]="WebClientImage"
artifacts["admin"]="AdminBlazorImage"
artifacts["adminwebclient"]="AdminWebClientImage"
artifacts["encounter"]="EncounterServiceImage"
artifacts["hangfire"]="HangfireImage"
artifacts["immunization"]="ImmsServiceImage"
artifacts["laboratory"]="LaboratoryImage"
artifacts["medication"]="MedicationServiceImage"
artifacts["mock"]="MockServiceImage"
artifacts["patient"]="PatientServiceImage"
artifacts["gatewayapi"]="GatewayApiImage"
artifacts["clinicaldocument"]="ClinicalDocumentImage"
artifacts["hgcdogs"]="HGCDogsImage"

for app in "${apps[@]}"; do
    artifactName=${artifacts[$app]}
    variableName="RELEASE_ARTIFACTS_${artifactName^^}_BUILDNUMBER"
    build=$(eval echo \$$variableName)
    echo "Importing image for ${app} with build number: ${build}"
    oc import-image "${app}:${build}" --from="${IMAGEREPO}/${app}:${build}" --confirm --reference-policy=local --output=name
done