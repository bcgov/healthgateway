import { useRouter } from "expo-router";
import { useState } from "react";
import { Platform, Pressable, View } from "react-native";

import HealthGatewayLogo from "@/assets/images/gov/health-gateway-logo.svg";
import {
    AppText,
    ExternalLink,
    HgButton,
    HgCard,
    HgMenu,
    type HgTab,
    HgTabPanel,
    HgTabs,
    HgTextField,
    HgWindow,
    Icon,
    InfoPopover,
    MessageDialog,
} from "@/components/ui";
import { elevation1 } from "@/components/ui/elevation";
import { ExternalUrl } from "@/constants/externalUrls";
import { Action, Destination, Origin, Text, Type } from "@/constants/tracking";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import { columnWidth, useColumns } from "@/hooks/useGrid";
import { useThemeColors } from "@/hooks/useTheme";
import { type Dependent, formatDependentName } from "@/models/dependent";
import { toHref } from "@/navigation/href";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { selectWebConfig, useConfigStore } from "@/stores/config.store";
import {
    selectIsLive,
    useConnectivityStore,
} from "@/stores/connectivity.store";
import { useDependentStore } from "@/stores/dependent.store";
import { useUserStore } from "@/stores/user.store";
import { cn } from "@/utils/cn";
import { ConfigUtil } from "@/utils/configUtil";
import { formatDate, fromIsoDate } from "@/utils/dateUtil";
import { decorativeProps } from "@/utils/decorative";
import { isDependentExpired } from "@/utils/dependentAge";

import { RecommendationsDialog } from "../reports/RecommendationsDialog";
import { ReportsComponent } from "../reports/ReportsComponent";

export { isDependentExpired };

/** "Your access has expired" with the remove button, shared by both dependent cards. */
export function DependentExpired({
    maxDependentAge,
    testID,
    removeTestID,
    onRemove,
}: {
    maxDependentAge: number;
    testID?: string;
    removeTestID?: string;
    onRemove: () => void;
}) {
    return (
        <View testID={testID} className="items-center gap-4 py-2">
            <AppText variant="title-large" className="text-center">
                Your access has expired
            </AppText>
            <AppText className="text-center">
                You no longer have access to this dependent as they have turned{" "}
                {maxDependentAge}
            </AppText>
            <HgButton
                testID={removeTestID}
                className="self-center"
                variant="secondary"
                text="Remove Dependent"
                requiresConnection
                onPress={onRemove}
            />
        </View>
    );
}

/** DependentDashboardTabComponent.vue */
function DashboardTab({ dependent }: { dependent: Dependent }) {
    const router = useRouter();
    const colors = useThemeColors();
    const columns = useColumns();
    const [recommendationsOpen, setRecommendationsOpen] = useState(false);
    const showRecommendations = ConfigUtil.showRecommendationsLink();
    const width = columnWidth(columns);
    return (
        <View className="flex-row flex-wrap">
            <View className="p-1" style={{ width }}>
                <HgCard
                    testID={`dependent-health-records-button-${dependent.ownerId}`}
                    title="Health Records"
                    icon={
                        <HealthGatewayLogo
                            height={25}
                            width={25}
                            // decorative: the card title names it
                            {...decorativeProps()}
                        />
                    }
                    actionIcon={
                        <Icon
                            name="chevron-right"
                            size={16}
                            color={colors.primary}
                        />
                    }
                    onPress={() => {
                        services.tracking.trackEvent({
                            action: Action.Visit,
                            text: Text.InternalLink,
                            destination: Destination.Timeline,
                            origin: Origin.Dependents,
                        });
                        router.push(
                            toHref(`/dependents/${dependent.ownerId}/timeline`)
                        );
                    }}
                />
            </View>
            {showRecommendations ? (
                <View className="p-1" style={{ width }}>
                    <HgCard
                        testID={`recommendations-card-${dependent.ownerId}`}
                        title="Vaccine Recommendations"
                        icon={
                            <Icon
                                name="calendar-check"
                                size={16}
                                color={colors.primary}
                            />
                        }
                        onPress={() => {
                            services.tracking.trackEvent({
                                action: Action.Visit,
                                text: Text.InternalLink,
                                destination:
                                    Destination.ImmunizationRecommendationDialog,
                                origin: Origin.Dependents,
                            });
                            setRecommendationsOpen(true);
                        }}
                    />
                </View>
            ) : null}
            {showRecommendations ? (
                <RecommendationsDialog
                    visible={recommendationsOpen}
                    onClose={() => setRecommendationsOpen(false)}
                    hdid={dependent.ownerId}
                    isDependent
                />
            ) : null}
        </View>
    );
}

/** v-col xl="3" md="4" sm="6" */
const readOnlyFieldWidth = (windowWidth: number): `${number}%` =>
    windowWidth >= 1920
        ? "25%"
        : windowWidth >= 960
          ? "33.3333%"
          : windowWidth >= 600
            ? "50%"
            : "100%";

/** A read-only v-text-field with its label (xl 3, md 4, sm 6) */
export function ReadOnlyField({
    label,
    nativeID,
    testID,
    value,
    children,
}: {
    label: string;
    nativeID: string;
    testID: string;
    value: string;
    children?: React.ReactNode;
}) {
    const { width: windowWidth } = useBreakpoint();
    const width = readOnlyFieldWidth(windowWidth);
    return (
        <View
            className={width === "100%" ? "px-2 mb-3" : "px-2 mb-1"}
            style={{ width }}
        >
            <AppText className="mb-2">{label}</AppText>
            <HgTextField
                accessibilityLabel={label}
                nativeID={nativeID}
                testID={testID}
                value={value}
                editable={false}
                compact
                className="mb-0"
            />
            {children}
        </View>
    );
}

/** DependentProfileTabComponent.vue */
function ProfileTab({ dependent }: { dependent: Dependent }) {
    const id = dependent.ownerId;
    return (
        <View className="flex-row flex-wrap -mx-2">
            <ReadOnlyField
                label="PHN"
                nativeID={`dependent-phn-${id}`}
                testID="dependent-phn"
                value={dependent.dependentInformation.PHN}
            />
            <ReadOnlyField
                label="Date of Birth"
                nativeID={`dependent-dob-${id}`}
                testID="dependent-date-of-birth"
                value={formatDate(
                    fromIsoDate(dependent.dependentInformation.dateOfBirth)
                )}
            />
            <ReadOnlyField
                label="How Many Others Have Access"
                nativeID={`dependent-other-delegate-count-${id}`}
                testID="dependent-other-delegate-count"
                value={String(dependent.totalDelegateCount - 1)}
            >
                <InfoPopover
                    buttonText="What does this mean?"
                    buttonTestID={`other-delegate-info-popover-button-${id}`}
                    popoverTestID={`other-delegate-info-popover-${id}`}
                >
                    <AppText variant="body-medium">
                        This shows you how many people other than you have added
                        your dependent to their Health Gateway account. For
                        privacy, we can’t tell you their names. If this number
                        isn’t what you expect, contact us at{" "}
                        <ExternalLink href={ExternalUrl.HealthGatewayEmail}>
                            HealthGateway@gov.bc.ca
                        </ExternalLink>
                        .
                    </AppText>
                </InfoPopover>
            </ReadOnlyField>
        </View>
    );
}

/** The card's title row: name, ⋮ Delete. */
export function DependentCardTitle({
    name,
    onDelete,
}: {
    name: string;
    onDelete: () => void;
}) {
    const colors = useThemeColors();
    const isLive = useConnectivityStore(selectIsLive);
    return (
        <View className="flex-row items-start">
            <AppText
                testID="dependentName"
                variant="title-large"
                // the page's h1 is Dependents: each dependent is an h2 (WCAG 1.3.1)
                accessibilityRole="header"
                aria-level={2}
                className="flex-1 mb-4"
            >
                {name}
            </AppText>
            <HgMenu
                width={140}
                align="end"
                items={[
                    {
                        title: "Delete",
                        testID: "deleteDependentMenuBtn",
                        onPress: onDelete,
                    },
                ]}
            >
                {(open, isOpen) => (
                    <Pressable
                        testID="dependentMenuBtn"
                        accessibilityRole="button"
                        accessibilityLabel={`More options for ${name}`}
                        aria-expanded={isOpen}
                        {...(Platform.OS === "web"
                            ? ({ "aria-haspopup": "menu" } as object)
                            : {})}
                        accessibilityState={{ disabled: !isLive }}
                        // its only item removes the dependent
                        disabled={!isLive}
                        hitSlop={8}
                        onPress={open}
                        className={cn(
                            "p-2 -mr-2 -mt-1",
                            !isLive && "opacity-40"
                        )}
                    >
                        <Icon
                            name="ellipsis-vertical"
                            size={18}
                            color={colors["bc-black"]}
                        />
                    </Pressable>
                )}
            </HgMenu>
        </View>
    );
}

/** Port of DependentCardComponent.vue (`dependents.timelineEnabled`) */
export function DependentCard({ dependent }: { dependent: Dependent }) {
    const maxDependentAge = useConfigStore(
        (s) => selectWebConfig(s)?.maxDependentAge ?? 0
    );
    const hdid = useUserStore((s) => s.user.hdid);
    const removeDependentFromStore = useDependentStore(
        (s) => s.removeDependent
    );
    const [selectedTabIndex, setSelectedTabIndex] = useState(0);
    const [confirmVisible, setConfirmVisible] = useState(false);
    const id = dependent.ownerId;
    const isExpired = isDependentExpired(dependent, maxDependentAge);

    const tabs: HgTab[] = [
        {
            value: 0,
            title: "Dashboard",
            nativeID: `dashboard-tab-button-${id}`,
        },
        {
            value: 1,
            title: "Export",
            disabled: isExpired,
            nativeID: `report-tab-button-${id}`,
        },
        {
            value: 2,
            title: "Profile",
            disabled: isExpired,
            nativeID: `profile-tab-button-${id}`,
        },
    ];

    function removeDependent() {
        services.tracking.trackEvent({
            action: Action.ButtonClick,
            text: Text.RemoveDependent,
            type: Type.Dependents,
        });
        removeDependentFromStore(hdid, dependent).catch((error: unknown) =>
            logger.error(describeError(error))
        );
    }

    return (
        <View
            testID={`dependent-card-${id}`}
            style={elevation1}
            className="my-3 rounded bg-surface"
        >
            <View className="bg-card-header rounded-t px-4 pt-4">
                <DependentCardTitle
                    name={formatDependentName(dependent.dependentInformation)}
                    onDelete={() => setConfirmVisible(true)}
                />
                <HgTabs
                    selectedSurface
                    value={selectedTabIndex}
                    onChange={setSelectedTabIndex}
                    tabs={tabs}
                />
            </View>
            <HgWindow
                tabs={tabs}
                value={selectedTabIndex}
                onChange={setSelectedTabIndex}
                testID={`dependent-window-${id}`}
                className="p-4"
            >
                <HgTabPanel
                    active={selectedTabIndex === 0}
                    testID="dashboard-tab"
                >
                    {isExpired ? (
                        <DependentExpired
                            maxDependentAge={maxDependentAge}
                            testID={`dependent-is-expired-div-${id}`}
                            removeTestID={`remove-dependent-btn-${id}`}
                            onRemove={removeDependent}
                        />
                    ) : (
                        <DashboardTab dependent={dependent} />
                    )}
                </HgTabPanel>
                <HgTabPanel active={selectedTabIndex === 1} testID="report-tab">
                    <ReportsComponent
                        hdid={dependent.dependentInformation.hdid}
                        isDependent
                    />
                </HgTabPanel>
                <HgTabPanel
                    active={selectedTabIndex === 2}
                    testID="profile-tab"
                >
                    <ProfileTab dependent={dependent} />
                </HgTabPanel>
            </HgWindow>
            <MessageDialog
                visible={confirmVisible}
                title="Remove Dependent"
                message="Are you sure you want to remove this dependent?"
                onClose={() => setConfirmVisible(false)}
                onSubmit={removeDependent}
            />
        </View>
    );
}
