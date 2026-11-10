import { useRouter } from "expo-router";
import { useState } from "react";
import { Platform, View } from "react-native";

import HealthRecordsIcon from "@/assets/images/home/health-records-icon.svg";
import HealthRecordsIconDark from "@/assets/images/home/health-records-icon-dark.svg";
import OtherRecordSourcesImage from "@/assets/images/home/other-record-sources.svg";
import OtherRecordSourcesImageDark from "@/assets/images/home/other-record-sources-dark.svg";
import HealthLinkLogo from "@/assets/images/services/health-link-logo.svg";
import HealthLinkLogoDark from "@/assets/images/services/health-link-logo-dark.svg";
import OdrLogo from "@/assets/images/services/odr-logo.svg";
import { AppTourDialog } from "@/components/site/AppTourDialog";
import {
    AppText,
    HgButton,
    HgCard,
    Icon,
    LogoPlate,
    PageTitle,
} from "@/components/ui";
import {
    EntryType,
    entryTypes,
    getEntryTypeByModule,
    ServiceName,
} from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ExternalUrl } from "@/constants/externalUrls";
import { Path } from "@/constants/path";
import {
    Action,
    Dataset,
    Destination,
    InternalUrl,
    Origin,
    Text,
    TrackedExternalUrl,
    Type,
} from "@/constants/tracking";
import { UserPreferenceType } from "@/constants/userPreferenceType";
import { useColumns } from "@/hooks/useGrid";
import { useColorScheme, useThemeColors } from "@/hooks/useTheme";
import { isSessionEnded, ResultError } from "@/models/errors";
import { TimelineFilterBuilder } from "@/models/timeline/timelineFilter";
import { toHref } from "@/navigation/href";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { useErrorStore } from "@/stores/error.store";
import { useTimelineStore } from "@/stores/timeline.store";
import { useQuickLinks, useUserStore } from "@/stores/user.store";
import { ConfigUtil } from "@/utils/configUtil";
import { openExternal } from "@/utils/openExternal";
import { RecommendationsDialog } from "@/views/reports/RecommendationsDialog";

import { AddQuickLinkDialog } from "./AddQuickLinkDialog";
import { HomeBanners } from "./HomeBanners";

/** Port of HomeView.vue: banners, "Add Quick Link", the quick-link cards and "Other record sources". */
export function HomeView() {
    const colors = useThemeColors();
    const dark = useColorScheme() === "dark";
    // Dark variants of the BC-owned artwork; third-party logos sit on a LogoPlate instead.
    const RecordsIcon = dark ? HealthRecordsIconDark : HealthRecordsIcon;
    const SourcesImage = dark
        ? OtherRecordSourcesImageDark
        : OtherRecordSourcesImage;
    const HealthLink = dark ? HealthLinkLogoDark : HealthLinkLogo;
    const router = useRouter();
    const columns = useColumns();
    const user = useUserStore((s) => s.user);
    const quickLinks = useQuickLinks();
    const setUserPreference = useUserStore((s) => s.setUserPreference);
    const updateQuickLinks = useUserStore((s) => s.updateQuickLinks);
    const errorStore = useErrorStore();
    const [addOpen, setAddOpen] = useState(false);
    const [recommendationsOpen, setRecommendationsOpen] = useState(false);

    const hidden = (p: string) => user.preferences[p]?.value === "true";
    const showImmunizationRecord =
        ConfigUtil.isImmunizationRecordLinkEnabled() &&
        ConfigUtil.isDatasetEnabled(EntryType.Immunization) &&
        !hidden(UserPreferenceType.HideImmunizationRecordQuickLink);
    const showRecommendations =
        ConfigUtil.showRecommendationsLink() &&
        ConfigUtil.isDatasetEnabled(EntryType.Immunization) &&
        !hidden(UserPreferenceType.HideRecommendationsQuickLink);
    const showHealthConnect =
        ConfigUtil.isServiceEnabled(ServiceName.HealthConnectRegistry) &&
        !hidden(UserPreferenceType.HideHealthConnectRegistryQuickLink);
    const showOrganDonor =
        ConfigUtil.isServiceEnabled(ServiceName.OrganDonorRegistration) &&
        !hidden(UserPreferenceType.HideOrganDonorQuickLink);
    const showOtherRecordSources =
        ConfigUtil.isOtherRecordSourcesFeatureEnabled();

    const enabledQuickLinks = (quickLinks ?? []).filter((q) =>
        q.filter.modules.every((m) => {
            const t = getEntryTypeByModule(m)?.type;
            return t !== undefined && ConfigUtil.isDatasetEnabled(t);
        })
    );
    const isAddDisabled =
        entryTypes.filter(
            (d) =>
                ConfigUtil.isDatasetEnabled(d.type) &&
                !enabledQuickLinks.some(
                    (q) =>
                        q.filter.modules.length === 1 &&
                        q.filter.modules[0] === d.moduleName
                )
        ).length === 0 &&
        !hidden(UserPreferenceType.HideImmunizationRecordQuickLink) &&
        !hidden(UserPreferenceType.HideOrganDonorQuickLink) &&
        !hidden(UserPreferenceType.HideHealthConnectRegistryQuickLink) &&
        !hidden(UserPreferenceType.HideRecommendationsQuickLink);

    function handlePreferenceError(error: unknown) {
        if (isSessionEnded(error)) return;
        logger.error(describeError(error));
        if (error instanceof ResultError && error.statusCode === 429)
            errorStore.setTooManyRequestsError();
        else errorStore.addError(ErrorType.Update, ErrorSourceType.Profile);
    }
    const setPreferenceValue = (type: string, value: string) =>
        setUserPreference({ ...user.preferences[type], value }).catch(
            handlePreferenceError
        );
    const removeQuickLink = (target: (typeof enabledQuickLinks)[number]) =>
        updateQuickLinks((quickLinks ?? []).filter((q) => q !== target)).catch(
            handlePreferenceError
        );

    const track = (data: Parameters<typeof services.tracking.trackEvent>[0]) =>
        services.tracking.trackEvent(data);

    const cardWidth =
        columns === 1
            ? "w-full"
            : columns === 2
              ? "w-1/2"
              : columns === 3
                ? "w-1/3"
                : "w-1/4";
    const cell = (key: string, node: React.ReactNode) => (
        <View key={key} className={`${cardWidth} p-3 flex`}>
            {node}
        </View>
    );
    // .quick-link-icon on the 22px card title: font-size 1.25em (a 27.5px glyph) in a 1.25em box (34.375px),
    // for v-icons and images alike; the Health Link logo (.quick-link-icon-large) is 2.1em tall
    const iconSize = 27.5;
    const svgIconSize = 34.375;
    const quickLinkIcon = (name: string) => (
        <View
            className="items-center justify-center"
            style={{ width: svgIconSize, height: svgIconSize }}
        >
            <Icon
                name={name}
                size={iconSize}
                color={colors.primary}
                fixedWidth
            />
        </View>
    );

    const cards: React.ReactNode[] = [
        cell(
            "health-records",
            <HgCard
                title="Health Records"
                testID="health-records-card"
                icon={<RecordsIcon width={svgIconSize} height={svgIconSize} />}
                onPress={() => {
                    track({
                        action: Action.InternalLink,
                        text: Text.HealthRecords,
                        origin: Origin.Home,
                        destination: Destination.HealthRecords,
                        type: Type.HomeTile,
                        url: InternalUrl.Timeline,
                    });
                    router.push(Path.Timeline);
                }}
            >
                <AppText>
                    View your available health records, including dispensed
                    medications, health visits, lab results, immunizations, and
                    more.
                </AppText>
            </HgCard>
        ),
    ];
    if (showImmunizationRecord)
        cards.push(
            cell(
                "immunization-record",
                <HgCard
                    title="Immunization Record"
                    testID="immunization-record-card-button"
                    icon={quickLinkIcon("cloud-arrow-down")}
                    onPress={() => {
                        track({
                            action: Action.InternalLink,
                            text: Text.ImmunizationRecord,
                            origin: Origin.Home,
                            destination: Destination.Download,
                            dataset: Dataset.Immunizations,
                            type: Type.HomeTile,
                            url: InternalUrl.Reports,
                        });
                        router.push(
                            toHref(
                                `${Path.Reports}?defaultEntryType=${EntryType.Immunization}`
                            )
                        );
                    }}
                    onRemove={() =>
                        void setPreferenceValue(
                            UserPreferenceType.HideImmunizationRecordQuickLink,
                            "true"
                        )
                    }
                >
                    <AppText>
                        Download a record of your immunizations, including
                        recommended vaccines.
                    </AppText>
                </HgCard>
            )
        );
    if (showRecommendations)
        cards.push(
            cell(
                "recommendations",
                <HgCard
                    title="Immunizations"
                    testID="recommendations-card-button"
                    icon={quickLinkIcon("syringe")}
                    onPress={() => {
                        track({
                            action: Action.ButtonClick,
                            text: Text.ViewRecommendedImmunizations,
                            origin: Origin.Home,
                            dataset: Dataset.Immunizations,
                            type: Type.HomeTile,
                        });
                        setRecommendationsOpen(true);
                    }}
                    onRemove={() =>
                        void setPreferenceValue(
                            UserPreferenceType.HideRecommendationsQuickLink,
                            "true"
                        )
                    }
                >
                    <AppText>
                        View immunizations you received from community
                        pharmacies or public health, including vaccine
                        recommendations.
                    </AppText>
                </HgCard>
            )
        );
    if (showHealthConnect)
        cards.push(
            cell(
                "health-connect",
                <HgCard
                    testID="health-connect-registry-card"
                    accessibilityLabel="Health Connect Registry"
                    icon={<HealthLink height={46.2} width={201.3} />}
                    onPress={() => {
                        track({
                            action: Action.ExternalLink,
                            text: Text.HealthLinkBC,
                            destination: Destination.HealthConnectRegistry,
                            origin: Origin.Home,
                            type: Type.HomeTile,
                            url: TrackedExternalUrl.HealthConnectRegistry,
                        });
                        void openExternal(ExternalUrl.HealthConnectRegistry);
                    }}
                    onRemove={() =>
                        void setPreferenceValue(
                            UserPreferenceType.HideHealthConnectRegistryQuickLink,
                            "true"
                        )
                    }
                >
                    <AppText>
                        Register for the Health Connect Registry and get matched
                        with a family doctor or nurse practitioner in your
                        community.
                    </AppText>
                </HgCard>
            )
        );
    if (showOrganDonor)
        cards.push(
            cell(
                "organ-donor",
                <HgCard
                    title="Organ Donor Registration"
                    testID="organ-donor-registration-card"
                    icon={
                        <LogoPlate>
                            <OdrLogo width={svgIconSize} height={svgIconSize} />
                        </LogoPlate>
                    }
                    onPress={() => {
                        track({
                            action: Action.InternalLink,
                            text: Text.OrganDonor,
                            origin: Origin.Home,
                            destination: Destination.Services,
                            type: Type.HomeTile,
                            url: InternalUrl.Services,
                        });
                        router.push(Path.Services);
                    }}
                    onRemove={() =>
                        void setPreferenceValue(
                            UserPreferenceType.HideOrganDonorQuickLink,
                            "true"
                        )
                    }
                >
                    <AppText>
                        Check whether you have registered your decision on organ
                        donation with BC Transplant. If you have registered your
                        decision, you can review the details here.
                    </AppText>
                </HgCard>
            )
        );
    enabledQuickLinks.forEach((q, index) => {
        const details =
            q.filter.modules.length === 1
                ? getEntryTypeByModule(q.filter.modules[0])
                : undefined;
        cards.push(
            cell(
                `quick-${q.name}-${index}`,
                <HgCard
                    title={q.name}
                    testID="quick-link-card"
                    icon={quickLinkIcon(details?.icon ?? "magnifying-glass")}
                    onPress={() => {
                        if (details) {
                            track({
                                action: Action.InternalLink,
                                text: Text.QuickLink,
                                origin: Origin.Home,
                                dataset: details.dataset,
                                destination: `${details.dataset} Timeline`,
                                url: InternalUrl.Timeline,
                            });
                        }
                        // timelineStore.setFilter(withEntryTypes) then /timeline, as the web client does
                        const types = q.filter.modules
                            .map((m) => getEntryTypeByModule(m)?.type)
                            .filter((t): t is EntryType => t !== undefined);
                        useTimelineStore
                            .getState()
                            .setFilter(
                                TimelineFilterBuilder.create().withEntryTypes(
                                    types
                                )
                            );
                        router.push(Path.Timeline);
                    }}
                    onRemove={() => void removeQuickLink(q)}
                >
                    <AppText>
                        {details?.description ??
                            "View your filtered health records."}
                    </AppText>
                </HgCard>
            )
        );
    });

    return (
        <View>
            {Platform.OS === "web" ? <AppTourDialog /> : null}
            <HomeBanners />
            <PageTitle
                title="Home"
                append={
                    <HgButton
                        testID="add-quick-link-button"
                        requiresConnection
                        text="Add Quick Link"
                        icon="plus"
                        variant="secondary"
                        disabled={isAddDisabled}
                        onPress={() => setAddOpen(true)}
                    />
                }
            />
            {/* v-row / v-col: 24px gutters (12px each side) */}
            <View className="flex-row flex-wrap -m-3 items-stretch">
                {cards}
            </View>
            {showOtherRecordSources ? (
                <View className="mt-4">
                    <HgCard
                        testID="other-record-sources-card"
                        accessibilityLabel="Other record sources"
                        onPress={() => {
                            track({
                                action: Action.InternalLink,
                                text: Text.OtherRecordSources,
                                origin: Origin.Home,
                                destination: Destination.RecordSources,
                                type: Type.HomeTile,
                                url: InternalUrl.OtherRecordSources,
                            });
                            router.push(Path.OtherRecordSources);
                        }}
                    >
                        <View className="flex-row items-start">
                            <View className="mr-4 md:mr-6">
                                <SourcesImage height={56} width={92} />
                            </View>
                            <View className="flex-1">
                                <AppText
                                    variant="title-large"
                                    className="font-bold mb-1 md:mb-2"
                                >
                                    Other record sources
                                </AppText>
                                <AppText>
                                    Some health records may not appear in Health
                                    Gateway. Learn about other trusted regional
                                    patient websites to help you find your
                                    health records.
                                </AppText>
                            </View>
                        </View>
                    </HgCard>
                </View>
            ) : null}
            <AddQuickLinkDialog
                visible={addOpen}
                onClose={() => setAddOpen(false)}
            />
            <RecommendationsDialog
                visible={recommendationsOpen}
                onClose={() => setRecommendationsOpen(false)}
                hdid={user.hdid}
            />
        </View>
    );
}
