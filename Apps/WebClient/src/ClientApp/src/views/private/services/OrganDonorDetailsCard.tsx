import { useEffect, useState } from "react";
import { View } from "react-native";

import OdrLogo from "@/assets/images/services/odr-logo.svg";
import {
    AppText,
    ExternalLink,
    HgButton,
    InfoTooltip,
    MessageDialog,
} from "@/components/common";
import { elevation1 } from "@/components/common/elevation";
import { DataSource } from "@/constants/dataSource";
import { ErrorSourceType } from "@/constants/errorType";
import { SENSITIVE_DOCUMENT_MESSAGE } from "@/constants/sensitiveDocument";
import {
    Action,
    Destination,
    Format,
    Origin,
    Text,
    TrackedExternalUrl,
    Type,
} from "@/constants/tracking";
import {
    type OrganDonorRegistration,
    PatientDataType,
} from "@/models/patientData";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { useErrorStore } from "@/stores/error.store";
import {
    patientDataRecords,
    selectPatientDataFileLoading,
    selectPatientDataState,
    usePatientDataStore,
} from "@/stores/patientData.store";
import { useUserStore } from "@/stores/user.store";

import { savePatientDataFile } from "../timeline/entries/patientDataFile";

const REGISTER_YOUR_DECISION_URL =
    "http://www.transplant.bc.ca/Pages/Register-your-Decision.aspx";

/** Port of OrganDonorDetailsCard.vue */
export function OrganDonorDetailsCard({ hdid }: { hdid: string }) {
    const state = usePatientDataStore(selectPatientDataState(hdid));
    const retrieveFile = usePatientDataStore((s) => s.retrievePatientDataFile);
    const blockedDataSources = useUserStore((s) => s.user.blockedDataSources);
    const [confirmVisible, setConfirmVisible] = useState(false);
    const registrationData = patientDataRecords(state, [
        PatientDataType.OrganDonorRegistrationStatus,
    ])[0] as OrganDonorRegistration | undefined;
    const isLoadingFile = usePatientDataStore(
        selectPatientDataFileLoading(registrationData?.registrationFileId)
    );
    const showOrganDonorRegistration = !blockedDataSources.includes(
        DataSource.OrganDonorRegistration
    );

    useEffect(() => {
        if (!showOrganDonorRegistration)
            useErrorStore
                .getState()
                .addCustomError(
                    "Organ Donor Registration is not available at this time. Please try again later.",
                    ErrorSourceType.User,
                    undefined
                );
    }, [showOrganDonorRegistration]);

    function getDecisionFile() {
        const fileId = registrationData?.registrationFileId;
        if (!fileId) return;
        services.tracking.trackEvent({
            action: Action.Download,
            text: Text.DownloadOrganDonorDecision,
            type: Type.OrganDonor,
            format: Format.Pdf,
        });
        retrieveFile(hdid, fileId)
            .then((file) =>
                savePatientDataFile(file, "Organ_Donor_Registration.pdf")
            )
            .catch((error: unknown) => logger.error(describeError(error)));
    }

    if (!showOrganDonorRegistration) return null;

    return (
        <View
            testID="organ-donor-registration-card"
            style={elevation1}
            className="rounded bg-surface"
        >
            <View className="flex-row items-center px-4 pt-3">
                <View className="mr-4 py-2">
                    <OdrLogo height={30} width={30} />
                </View>
                <AppText
                    testID="card-button-title"
                    className="flex-1 text-[22px] leading-7 tracking-normal py-2"
                >
                    Organ Donor Registration
                </AppText>
            </View>
            <View className="px-4 pb-4 pt-2 gap-4">
                <View className="flex-row items-center">
                    <AppText className="text-bc-grey">Status: </AppText>
                    <AppText
                        testID="organ-donor-registration-status"
                        className="font-bold"
                    >
                        {registrationData?.status}
                    </AppText>
                    <View className="ml-2">
                        <InfoTooltip
                            accessibilityLabel="More information about the registration status"
                            testID="organ-donor-registration-status-info-button"
                            tooltipTestID="organ-donor-registration-status-info-popover"
                            text={registrationData?.statusMessage}
                        />
                    </View>
                </View>
                <View className="flex-row flex-wrap items-center">
                    <AppText className="text-bc-grey">Decision: </AppText>
                    {registrationData?.status === "Registered" &&
                    registrationData.registrationFileId ? (
                        <HgButton
                            testID="organ-donor-registration-download-button"
                            variant="secondary"
                            icon="download"
                            text="Download"
                            requiresConnection
                            loading={isLoadingFile}
                            onPress={() => setConfirmVisible(true)}
                        />
                    ) : registrationData?.status === "Registered" ? (
                        <AppText
                            testID="organ-donor-registration-decision-registered-no-file"
                            className="font-bold shrink"
                        >
                            Download only available to registrants after
                            November 2022
                        </AppText>
                    ) : (
                        <AppText
                            testID="organ-donor-registration-decision-no-file"
                            className="font-bold"
                        >
                            Not Available
                        </AppText>
                    )}
                </View>
                <View>
                    <AppText className="mb-4">
                        <ExternalLink
                            testID="organ-donor-registration-link"
                            href={REGISTER_YOUR_DECISION_URL}
                            onPress={() =>
                                services.tracking.trackEvent({
                                    action: Action.ExternalLink,
                                    text: Text.RegisterOrganDonorDecision,
                                    origin: Origin.ServicesPage,
                                    destination: Destination.TransplantBC,
                                    type: Type.ServiceTile,
                                    url: TrackedExternalUrl.OrganDonorRegistration,
                                })
                            }
                        >
                            {registrationData?.organDonorRegistrationLinkText}
                        </ExternalLink>
                    </AppText>
                    <AppText>
                        It may take 2 hours for updates to show here
                    </AppText>
                </View>
            </View>
            <MessageDialog
                visible={confirmVisible}
                title="Sensitive Document"
                message={SENSITIVE_DOCUMENT_MESSAGE}
                onClose={() => setConfirmVisible(false)}
                onSubmit={getDecisionFile}
            />
        </View>
    );
}
