import { useEffect } from "react";
import { View } from "react-native";

import { Breadcrumb } from "@/components/site/Breadcrumb";
import { OfflineNotice } from "@/components/site/OfflineNotice";
import { AppText, Loading, PageTitle } from "@/components/ui";
import { Path } from "@/constants/path";
import { usePagePull } from "@/hooks/usePagePull";
import { describeError, logger } from "@/services/logger";
import {
    selectFeatureToggles,
    selectWebConfig,
    useConfigStore,
} from "@/stores/config.store";
import {
    selectIsLive,
    useConnectivityStore,
} from "@/stores/connectivity.store";
import {
    selectDependentsAreLoading,
    useDependentStore,
} from "@/stores/dependent.store";
import { refreshDependentsAndRecords } from "@/stores/refreshDatasets";
import { useUserStore } from "@/stores/user.store";

import { AddDependentDialog } from "./AddDependentDialog";
import { DependentCard } from "./DependentCard";
import { LegacyDependentCard } from "./LegacyDependentCard";

const breadcrumbItems = [
    {
        text: "Dependents",
        to: Path.Dependents,
        active: true,
        testId: "breadcrumb-dependents",
    },
];

function NoSavedDependents() {
    return (
        <OfflineNotice testID="dependentsOfflineNotice">
            {
                "Your dependents aren't saved on this device. They need a connection and a working login."
            }
        </OfflineNotice>
    );
}

/** Port of DependentManagementView.vue: always refreshes the list. */
function DependentManagementView() {
    const hdid = useUserStore((s) => s.user.hdid);
    const dependents = useDependentStore((s) => s.dependents);
    const isLoading = useDependentStore(selectDependentsAreLoading);
    const retrieveDependents = useDependentStore((s) => s.retrieveDependents);
    const isLive = useConnectivityStore(selectIsLive);
    const syncing = useConnectivityStore((s) => s.syncing);
    const pulling = usePagePull(() => refreshDependentsAndRecords(hdid));

    const refreshDependents = () => {
        retrieveDependents(hdid, true).catch((error: unknown) =>
            logger.error(describeError(error))
        );
    };

    useEffect(() => {
        if (!hdid || !isLive) return;
        retrieveDependents(hdid, true).catch((error: unknown) =>
            logger.error(describeError(error))
        );
    }, [hdid, isLive, retrieveDependents]);

    return (
        <View>
            <Breadcrumb items={breadcrumbItems} />
            <Loading isLoading={isLoading && !pulling && !syncing} />
            <PageTitle
                title="Dependents"
                append={<AddDependentDialog onSubmit={refreshDependents} />}
            />
            {!isLive && dependents.length === 0 ? <NoSavedDependents /> : null}
            {dependents.map((dependent) => (
                <DependentCard key={dependent.ownerId} dependent={dependent} />
            ))}
        </View>
    );
}

/** Port of LegacyDependentsView.vue: uses the stored list when there is one. */
function LegacyDependentsView() {
    const hdid = useUserStore((s) => s.user.hdid);
    const maxDependentAge = useConfigStore(
        (s) => selectWebConfig(s)?.maxDependentAge ?? 0
    );
    const dependents = useDependentStore((s) => s.dependents);
    const isLoading = useDependentStore(selectDependentsAreLoading);
    const retrieveDependents = useDependentStore((s) => s.retrieveDependents);
    const isLive = useConnectivityStore(selectIsLive);
    const syncing = useConnectivityStore((s) => s.syncing);
    const pulling = usePagePull(() => refreshDependentsAndRecords(hdid));

    const refreshDependents = () => {
        retrieveDependents(hdid, true).catch((error: unknown) =>
            logger.error(describeError(error))
        );
    };

    useEffect(() => {
        if (!hdid || !isLive) return;
        retrieveDependents(hdid, false).catch((error: unknown) =>
            logger.error(describeError(error))
        );
    }, [hdid, isLive, retrieveDependents]);

    return (
        <View>
            <Breadcrumb items={breadcrumbItems} />
            <Loading isLoading={isLoading && !pulling && !syncing} />
            <PageTitle
                title="Dependents"
                append={<AddDependentDialog onSubmit={refreshDependents} />}
            />
            <AppText className="font-bold">
                You can add your dependents under the age of {maxDependentAge}{" "}
                to view their health records.
            </AppText>
            {!isLive && dependents.length === 0 ? <NoSavedDependents /> : null}
            {dependents.map((dependent) => (
                <LegacyDependentCard
                    key={dependent.ownerId}
                    dependent={dependent}
                    onNeedsUpdate={refreshDependents}
                />
            ))}
        </View>
    );
}

/** Port of DependentViewSelectorComponent.vue */
export function DependentsView() {
    const timelineEnabled = useConfigStore(
        (s) => selectFeatureToggles(s)?.dependents.timelineEnabled ?? false
    );
    return timelineEnabled ? (
        <DependentManagementView />
    ) : (
        <LegacyDependentsView />
    );
}
