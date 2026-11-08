import { type Ref, useEffect, useImperativeHandle, useMemo } from "react";
import { View } from "react-native";

import { AppText, ExternalLink, HgDataTable } from "@/components/ui";
import {
    Action,
    Destination,
    Origin,
    Text,
    TrackedExternalUrl,
    Type,
} from "@/constants/tracking";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import type { ImmunizationAgent } from "@/models/immunization";
import type { ReportField } from "@/models/report";
import { TemplateType } from "@/models/report";
import type { ReportFilter } from "@/models/reportFilter";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import {
    selectImmunizations,
    selectImmunizationsAreLoading,
    selectRecommendations,
    useImmunizationStore,
} from "@/stores/immunization.store";
import { descending, formatDate, fromIsoDate } from "@/utils/dateUtil";

import type { ReportHandle } from "./DatasetReports";

interface ImmunizationRow extends Record<string, unknown> {
    date: string;
    immunization: string;
    agents: ImmunizationAgent[];
    provider_clinic: string;
}

interface RecommendationRow extends Record<string, unknown> {
    immunization: string;
    due_date: string;
}

const immunizationFields: ReportField[] = [
    {
        key: "date",
        title: "Date",
        width: "15%",
        thTestID: "immunizationDateTitle",
        tdTestID: "immunizationDateItem",
    },
    {
        key: "immunization",
        title: "Immunization",
        width: "25%",
        thTestID: "immunizationNameTitle",
        tdTestID: "immunizationNameItem",
    },
    {
        key: "agents",
        title: "Agents",
        width: "45%",
        thTestID: "immunizationAgentTitle",
        tdTestID: "immunizationAgentItem",
    },
    {
        key: "provider_clinic",
        title: "Provider / Clinic",
        width: "15%",
        thTestID: "immunizationProviderClinicTitle",
        tdTestID: "immunizationProviderClinicItem",
    },
];

const recommendationFields: ReportField[] = [
    {
        key: "immunization",
        title: "Immunization",
        tdAlign: "start",
        thAlign: "start",
        thTestID: "recommendationTitle",
        tdTestID: "recommendationItem",
    },
    {
        key: "due_date",
        title: "Due Date",
        thTestID: "recommendationDateTitle",
        tdTestID: "recommendationDateItem",
        tdNoWrap: true,
    },
];

/** defineExpose({ generateReport }) */
export type ImmunizationReportHandle = ReportHandle;

interface Props {
    hdid: string;
    filter: ReportFilter;
    isDependent?: boolean;
    template?: TemplateType;
    hideImmunizations?: boolean;
    hideRecommendations?: boolean;
    hideRecommendationHeader?: boolean;
    forceShow?: boolean;
    /** #recommendations-description slot */
    recommendationsDescription?: React.ReactNode;
    onIsLoadingChanged?: (isLoading: boolean) => void;
    onIsEmptyChanged?: (isEmpty: boolean) => void;
    ref?: Ref<ImmunizationReportHandle>;
}

function trackLink(
    url: TrackedExternalUrl,
    text: Text,
    destination: Destination
) {
    services.tracking.trackEvent({
        action: Action.ExternalLink,
        text,
        destination,
        origin: Origin.Download,
        type: Type.Immunizations,
        url,
    });
}

/** Port of ImmunizationReportComponent.vue */
export function ImmunizationReport({
    hdid,
    filter,
    isDependent = false,
    template,
    hideImmunizations = false,
    hideRecommendations = false,
    hideRecommendationHeader = false,
    forceShow = false,
    recommendationsDescription,
    onIsLoadingChanged,
    onIsEmptyChanged,
    ref,
}: Props) {
    const { mdAndUp } = useBreakpoint();
    const immunizations = useImmunizationStore(selectImmunizations(hdid));
    const recommendations = useImmunizationStore(selectRecommendations(hdid));
    const isLoading = useImmunizationStore(selectImmunizationsAreLoading(hdid));
    const retrieveImmunizations = useImmunizationStore(
        (s) => s.retrieveImmunizations
    );

    const visibleImmunizations = useMemo(
        () =>
            immunizations
                .filter((record) =>
                    filter.allowsDate(fromIsoDate(record.dateOfImmunization))
                )
                .sort((a, b) =>
                    descending(
                        fromIsoDate(a.dateOfImmunization),
                        fromIsoDate(b.dateOfImmunization)
                    )
                ),
        [immunizations, filter]
    );
    const immunizationItems = useMemo(
        () =>
            visibleImmunizations.map<ImmunizationRow>((x) => ({
                date: formatDate(fromIsoDate(x.dateOfImmunization)),
                immunization: x.immunization.name,
                agents: x.immunization.immunizationAgents,
                provider_clinic: x.providerOrClinic,
            })),
        [visibleImmunizations]
    );
    const visibleRecommendations = useMemo(
        () => recommendations.filter((x) => x.recommendedVaccinations),
        [recommendations]
    );
    const recommendationItems = useMemo(
        () =>
            visibleRecommendations.map<RecommendationRow>((x) => ({
                immunization: x.recommendedVaccinations,
                due_date: formatDate(fromIsoDate(x.agentDueDate)),
            })),
        [visibleRecommendations]
    );
    const isEmpty = visibleImmunizations.length === 0;
    const isRecommendationEmpty = visibleRecommendations.length === 0;

    useImperativeHandle(
        ref,
        () => ({
            generateReport: (reportFormatType, headerData) =>
                services.report.generateReport({
                    data: {
                        header: headerData,
                        records: immunizationItems,
                        recommendations: recommendationItems,
                    },
                    template:
                        template ??
                        (isDependent
                            ? TemplateType.DependentImmunization
                            : TemplateType.Immunization),
                    type: reportFormatType,
                }),
        }),
        [immunizationItems, recommendationItems, template, isDependent]
    );

    useEffect(() => {
        onIsLoadingChanged?.(isLoading);
    }, [isLoading, onIsLoadingChanged]);

    useEffect(() => {
        onIsEmptyChanged?.(isEmpty && isRecommendationEmpty);
    }, [isEmpty, isRecommendationEmpty, onIsEmptyChanged]);

    useEffect(() => {
        // signed out while mounted (the store clears the hdid before navigation): nothing to load
        if (!hdid) return;
        logger.debug("Retrieving immunizations");
        retrieveImmunizations(hdid).catch((error: unknown) =>
            logger.error(
                `Error loading immunization data: ${describeError(error)}`
            )
        );
    }, [hdid, retrieveImmunizations]);

    if (isRecommendationEmpty && isEmpty && !isLoading)
        return <AppText>No records found.</AppText>;
    // `d-none d-md-block` unless forceShow; dependents only with forceShow
    if ((isDependent && !forceShow) || (!forceShow && !mdAndUp)) return null;

    return (
        <View>
            {!hideImmunizations ? (
                <View>
                    <AppText variant="title-large" className="font-bold mb-2">
                        Immunization Record
                    </AppText>
                    <AppText className="mb-4">
                        Health Gateway shows immunizations from public health
                        clinics and pharmacies in B.C. If you got vaccinated at
                        a pharmacy, try searching your medications, too.
                    </AppText>
                    <AppText className="mb-4">
                        You can add or update immunizations by visiting{" "}
                        <ExternalLink
                            href={TrackedExternalUrl.ImmunizationRecordBC}
                            onPress={() =>
                                trackLink(
                                    TrackedExternalUrl.ImmunizationRecordBC,
                                    Text.ImmunizationRecordDownload,
                                    Destination.ImmunizationRecordBC
                                )
                            }
                        >
                            immunizationrecord.gov.bc.ca
                        </ExternalLink>
                        .
                    </AppText>
                    {isEmpty && !isLoading ? (
                        <AppText className="mb-4">No records found.</AppText>
                    ) : null}
                    {(!isEmpty || isLoading) && mdAndUp ? (
                        <HgDataTable
                            caption="Immunization history"
                            testID="immunization-history-report-table"
                            loading={isLoading}
                            items={immunizationItems}
                            fields={immunizationFields}
                            density="compact"
                            renderHeader={{
                                agents: () => (
                                    <View className="flex-row items-center">
                                        <AppText
                                            variant="body-medium"
                                            className="flex-1 pr-4"
                                        >
                                            Agent
                                        </AppText>
                                        <AppText
                                            variant="body-medium"
                                            className="flex-1 px-4"
                                        >
                                            Product
                                        </AppText>
                                        <AppText
                                            variant="body-medium"
                                            className="flex-1 pl-4"
                                        >
                                            Lot Number
                                        </AppText>
                                    </View>
                                ),
                            }}
                            renderItem={{
                                agents: (item) => (
                                    <View>
                                        {item.agents.map((agent, i) => (
                                            <View key={i} className="flex-row">
                                                <AppText className="flex-1 pr-4 text-body-small">
                                                    {agent.name}
                                                </AppText>
                                                <AppText className="flex-1 px-4 text-body-small">
                                                    {agent.productName}
                                                </AppText>
                                                <AppText className="flex-1 pl-4 text-body-small">
                                                    {agent.lotNumber}
                                                </AppText>
                                            </View>
                                        ))}
                                    </View>
                                ),
                            }}
                        />
                    ) : null}
                </View>
            ) : null}
            {!hideRecommendations ? (
                <View className={!hideImmunizations ? "mt-4" : ""}>
                    {!hideRecommendationHeader ? (
                        <AppText
                            variant="title-large"
                            className="font-bold mb-2"
                        >
                            Recommended Immunizations
                        </AppText>
                    ) : null}
                    {recommendationsDescription ?? (
                        <AppText className="mb-4">
                            Recommended immunizations are based on the{" "}
                            <ExternalLink
                                href={TrackedExternalUrl.ImmunizationSchedule}
                                onPress={() =>
                                    trackLink(
                                        TrackedExternalUrl.ImmunizationSchedule,
                                        Text.BcVaccineSchedule,
                                        Destination.ImmunizationSchedule
                                    )
                                }
                            >
                                BC Vaccine Schedule
                            </ExternalLink>
                            . For more information, please visit{" "}
                            <ExternalLink
                                href={TrackedExternalUrl.Immunizations}
                                onPress={() =>
                                    trackLink(
                                        TrackedExternalUrl.Immunizations,
                                        Text.ImmunizationsHealthLinkBC,
                                        Destination.ImmunizationsHealthLinkBC
                                    )
                                }
                            >
                                Immunizations | HealthLink BC
                            </ExternalLink>
                            .
                        </AppText>
                    )}
                    {isRecommendationEmpty && !isLoading ? (
                        // last paragraph in its block: no bottom margin
                        <AppText>No recommendations found.</AppText>
                    ) : null}
                    {!isRecommendationEmpty || isLoading ? (
                        <HgDataTable
                            caption="Recommended immunizations"
                            testID="recommendation-history-report-table"
                            loading={isLoading}
                            items={recommendationItems}
                            fields={recommendationFields}
                        />
                    ) : null}
                </View>
            ) : null}
        </View>
    );
}
