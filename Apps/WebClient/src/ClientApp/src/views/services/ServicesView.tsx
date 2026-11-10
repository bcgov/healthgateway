import { useEffect } from "react";
import { View } from "react-native";

import { Breadcrumb } from "@/components/site/Breadcrumb";
import { Loading } from "@/components/site/Loading";
import { OfflineNotice } from "@/components/site/OfflineNotice";
import { AppText, PageTitle } from "@/components/ui";
import { DataSource } from "@/constants/dataSource";
import { ServiceName } from "@/constants/entryType";
import { Path } from "@/constants/path";
import { columnWidth, useColumns } from "@/hooks/useGrid";
import { usePagePull } from "@/hooks/usePagePull";
import { PatientDataType } from "@/models/patientData";
import { describeError, logger } from "@/services/logger";
import {
    selectIsLive,
    useConnectivityStore,
} from "@/stores/connectivity.store";
import {
    selectPatientDataAreLoading,
    usePatientDataStore,
} from "@/stores/patientData.store";
import { markPatientDataStale } from "@/stores/refreshDatasets";
import { useUserStore } from "@/stores/user.store";
import { ConfigUtil } from "@/utils/configUtil";

import { OrganDonorDetailsCard } from "./OrganDonorDetailsCard";

/** Port of ServicesView.vue */
export function ServicesView() {
    const hdid = useUserStore((s) => s.user.hdid);
    const blockedDataSources = useUserStore((s) => s.user.blockedDataSources);
    const columns = useColumns();
    const isLive = useConnectivityStore(selectIsLive);
    const hasSavedStatus = usePatientDataStore(
        (s) =>
            s.states[hdid]?.data[
                PatientDataType.OrganDonorRegistrationStatus
            ] !== undefined
    );
    const dataAreLoading = usePatientDataStore(
        selectPatientDataAreLoading(hdid)
    );
    const retrievePatientData = usePatientDataStore(
        (s) => s.retrievePatientData
    );
    const isOrganDonorServiceEnabled = ConfigUtil.isServiceEnabled(
        ServiceName.OrganDonorRegistration
    );
    const isOrganDonorServiceBlocked = blockedDataSources.includes(
        DataSource.OrganDonorRegistration
    );
    const syncing = useConnectivityStore((s) => s.syncing);
    const available = isOrganDonorServiceEnabled && !isOrganDonorServiceBlocked;
    const pulling = usePagePull(async () => {
        if (!available) return;
        markPatientDataStale(hdid);
        await retrievePatientData(hdid, [
            PatientDataType.OrganDonorRegistrationStatus,
        ]);
    });
    // a pull keeps the status on screen until the new one arrives
    const patientDataAreLoading =
        isOrganDonorServiceEnabled && dataAreLoading && !pulling;

    useEffect(() => {
        if (
            !hdid ||
            !isLive ||
            !isOrganDonorServiceEnabled ||
            isOrganDonorServiceBlocked
        )
            return;
        retrievePatientData(hdid, [
            PatientDataType.OrganDonorRegistrationStatus,
        ]).catch((error: unknown) => logger.error(describeError(error)));
    }, [
        hdid,
        isLive,
        isOrganDonorServiceEnabled,
        isOrganDonorServiceBlocked,
        retrievePatientData,
    ]);

    return (
        <View>
            <Breadcrumb
                items={[
                    {
                        text: "Services",
                        to: Path.Services,
                        active: true,
                        testId: "breadcrumb-services",
                    },
                ]}
            />
            <Loading isLoading={patientDataAreLoading && !syncing} />
            <PageTitle title="Services" />
            <AppText className="mb-4 tracking-normal">
                You can check and update your Organ Donor Registry information
                here. More health services will be added in future.
            </AppText>
            {isLive ? null : (
                <OfflineNotice testID="servicesOfflineNotice">
                    {hasSavedStatus
                        ? "This is your saved status. Downloading your decision needs a connection and a working login."
                        : "Your organ donor status isn't saved on this device. It needs a connection and a working login."}
                </OfflineNotice>
            )}
            {!patientDataAreLoading &&
            isOrganDonorServiceEnabled &&
            (isLive || hasSavedStatus) ? (
                <View className="flex-row -mx-3">
                    <View
                        className="p-3"
                        style={{ width: columnWidth(columns) }}
                    >
                        <OrganDonorDetailsCard hdid={hdid} />
                    </View>
                </View>
            ) : null}
        </View>
    );
}
