import { useRouter } from "expo-router";
import { DateTime } from "luxon";
import { useState, useSyncExternalStore } from "react";
import { Alert, Platform, Pressable, View } from "react-native";

import {
    AppText,
    HgButton,
    HgRadio,
    HgSwitch,
    Icon,
    PageTitle,
} from "@/components/common";
import { Breadcrumb } from "@/components/site/Breadcrumb";
import {
    type Environment,
    selectableEnvironments,
} from "@/config/environments";
import { Path } from "@/constants/path";
import { useLocalAuthMethod } from "@/hooks/useAppLock";
import { useThemeColors } from "@/hooks/useTheme";
import { useAppLockStore } from "@/stores/appLock.store";
import { useAuthStore } from "@/stores/auth.store";
import { useDependentStore } from "@/stores/dependent.store";
import {
    selectEnvironment,
    useEnvironmentStore,
} from "@/stores/environment.store";
import {
    type CachedDataset,
    cachedDatasets,
    clearAllDatasets,
    clearDataset,
    resyncAllDatasets,
    resyncDataset,
} from "@/stores/firstRecordSync";
import {
    datasetFailedAt,
    datasetRefreshedAt,
    getSavedTimesVersion,
    REFRESH_AFTER_MS,
    subscribeSavedTimes,
} from "@/stores/savedCopyState";
import { switchEnvironment } from "@/stores/switchEnvironment";
import { useUserStore } from "@/stores/user.store";
import type { LocalAuthMethod } from "@/utils/localAuth";
import { resetApp, runReset } from "@/utils/resetApp";

const methodName: Record<LocalAuthMethod, string> = {
    faceId: "Face ID",
    touchId: "Touch ID",
    biometrics: "biometrics",
    passcode: "your passcode",
    none: "your device security",
};

const datasetNames: Record<string, string> = {
    immunization: "Immunizations",
    medication: "Medications",
    labResult: "Lab Results",
    covid19TestResult: "COVID‑19 Tests",
    healthVisit: "Health Visits",
    notes: "My Notes",
    specialAuthorityRequest: "Special Authority",
    clinicalDocument: "Clinical Documents",
    hospitalVisit: "Hospital Visits",
    patientData: "Imaging, Cancer Screening and Organ Donor",
    comments: "Comments",
    dependents: "Dependents List",
};

const when = (at: number) =>
    DateTime.fromMillis(at).toFormat("yyyy-MMM-dd, h:mm a");

function statusText(key: string): string {
    const refreshedAt = datasetRefreshedAt(key);
    const failedAt = datasetFailedAt(key);
    const saved =
        refreshedAt === undefined
            ? "Not saved"
            : `${Date.now() - refreshedAt >= REFRESH_AFTER_MS ? "Expired, saved" : "Saved"} ${when(refreshedAt)}`;
    return failedAt === undefined
        ? saved
        : `${saved}. Last sync failed ${when(failedAt)}`;
}

function CacheSection({ hdid }: { hdid: string }) {
    // the times live outside React: repaint when they change
    useSyncExternalStore(subscribeSavedTimes, getSavedTimesVersion);
    const dependents = useDependentStore((s) => s.dependents);
    const [busy, setBusy] = useState<string[]>([]);
    const datasets = cachedDatasets(hdid);
    const allBusy = busy.includes("all");

    const track = async (id: string, work: () => Promise<void>) => {
        setBusy((b) => [...b, id]);
        try {
            await work();
        } finally {
            setBusy((b) => b.filter((x) => x !== id));
        }
    };

    const confirmClearOne = (dataset: CachedDataset) =>
        Alert.alert(
            `Clear ${datasetNames[dataset.name] ?? dataset.name}?`,
            "Its saved records will be removed from this device until you resync it or next open the app.",
            [
                { text: "Cancel", style: "cancel" },
                {
                    text: "Clear",
                    style: "destructive",
                    onPress: () => clearDataset(dataset),
                },
            ]
        );

    const confirmClearAll = () =>
        Alert.alert(
            "Clear all saved records?",
            "All of your saved records, and your dependents', will be removed from this device until you resync them or next open the app.",
            [
                { text: "Cancel", style: "cancel" },
                {
                    text: "Clear All",
                    style: "destructive",
                    onPress: () => void clearAllDatasets(hdid),
                },
            ]
        );

    const people = [...new Set(datasets.map((d) => d.person))];
    const personName = (person: string) => {
        if (person === hdid) return "Your records";
        const info = dependents.find(
            (d) => d.ownerId === person
        )?.dependentInformation;
        return info ? `${info.firstname} ${info.lastname}` : "Dependent";
    };

    return (
        <View testID="cacheSection">
            <AppText variant="h3" className="mt-8 mb-2">
                Cache
            </AppText>
            <AppText className="mb-4">
                Records saved on this device, encrypted, so they open quickly
                and offline. Each record type is kept for 24 hours.
            </AppText>
            <View className="flex-row flex-wrap gap-3 mb-4">
                <HgButton
                    testID="cacheResyncAllBtn"
                    text="Resync All"
                    variant="secondary"
                    loading={allBusy}
                    disabled={allBusy}
                    onPress={() =>
                        void track("all", () => resyncAllDatasets(hdid))
                    }
                />
                <HgButton
                    testID="cacheClearAllBtn"
                    text="Clear All"
                    variant="error-secondary"
                    disabled={allBusy}
                    onPress={confirmClearAll}
                />
            </View>
            {people.map((person) => (
                <View key={person} className="mb-4">
                    <AppText className="font-bold mb-1">
                        {personName(person)}
                    </AppText>
                    {datasets
                        .filter((d) => d.person === person)
                        .map((dataset) => (
                            <View
                                key={dataset.key}
                                testID={`cacheRow-${dataset.name}`}
                                className="py-3 border-t border-border-divider"
                            >
                                <AppText>
                                    {datasetNames[dataset.name] ?? dataset.name}
                                </AppText>
                                <AppText
                                    testID={`cacheStatus-${dataset.name}`}
                                    variant="body-medium"
                                    className="mt-1"
                                >
                                    {statusText(dataset.key)}
                                </AppText>
                                <View className="flex-row gap-3 mt-2">
                                    <HgButton
                                        testID={`cacheResync-${dataset.name}`}
                                        text="Resync"
                                        variant="link"
                                        loading={busy.includes(dataset.key)}
                                        disabled={
                                            allBusy ||
                                            busy.includes(dataset.key)
                                        }
                                        onPress={() =>
                                            void track(dataset.key, () =>
                                                resyncDataset(hdid, dataset)
                                            )
                                        }
                                    />
                                    <HgButton
                                        testID={`cacheClear-${dataset.name}`}
                                        text="Clear"
                                        variant="link"
                                        disabled={
                                            allBusy ||
                                            busy.includes(dataset.key)
                                        }
                                        onPress={() => confirmClearOne(dataset)}
                                    />
                                </View>
                            </View>
                        ))}
                </View>
            ))}
        </View>
    );
}

/** The host an environment's configuration comes from, under its name. */
const hostOf = (environment: Environment) =>
    environment.configUrl
        .replace(/^https?:\/\//, "")
        .replace(/\/configuration$/, "");

/** Choosing another environment is a Reset App (`switchEnvironment`), so it asks first. Local only in development builds. */
function EnvironmentSection() {
    const router = useRouter();
    const current = useEnvironmentStore(selectEnvironment);

    const confirmSwitch = (environment: Environment) => {
        if (environment.id === current.id) return;
        Alert.alert(
            `Switch to ${environment.name}?`,
            "The app will be reset to a fresh install on the new environment. You will be logged out and everything saved on this device will be removed. Do you want to continue?",
            [
                { text: "Cancel", style: "cancel" },
                {
                    text: "Switch",
                    style: "destructive",
                    onPress: () =>
                        runReset(() =>
                            switchEnvironment(environment.id, () =>
                                router.replace(Path.Root)
                            )
                        ),
                },
            ]
        );
    };

    return (
        <View testID="environmentSection">
            <AppText variant="h3" className="mt-8 mb-2">
                Environment
            </AppText>
            <AppText className="mb-2">
                The Health Gateway servers this app uses. Switching resets the
                app.
            </AppText>
            <View role="radiogroup" aria-label="Environment">
                {selectableEnvironments().map((environment) => (
                    <HgRadio
                        key={environment.id}
                        testID={`environment-${environment.id}`}
                        label={environment.name}
                        description={hostOf(environment)}
                        selected={environment.id === current.id}
                        onSelect={() => confirmSwitch(environment)}
                    />
                ))}
            </View>
        </View>
    );
}

/** Native page (stated deviation: the web client has none): Device Security, the saved records, Environment and Reset App. */
export function SecurityView() {
    const router = useRouter();
    const colors = useThemeColors();
    const method = useLocalAuthMethod();
    const enabled = useAppLockStore((s) => s.enabled);
    const authenticating = useAppLockStore((s) => s.authenticating);
    const enable = useAppLockStore((s) => s.enable);
    const disable = useAppLockStore((s) => s.disable);
    const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
    const hdid = useUserStore((s) => s.user.hdid);
    const essentialDataLoaded = useUserStore((s) => s.essentialDataLoaded);

    if (Platform.OS === "web") return null;

    const confirmTurnOff = () => {
        Alert.alert(
            "Turn off device security?",
            "Any locally saved data will be removed. Are you sure you want to continue?",
            [
                { text: "Cancel", style: "cancel" },
                {
                    text: "Turn Off",
                    style: "destructive",
                    onPress: () => void disable(),
                },
            ]
        );
    };

    const confirmReset = () => {
        Alert.alert(
            "Reset App?",
            "The app will be reset to a fresh install. You will be logged out and everything saved on this device will be removed. Do you want to continue?",
            [
                { text: "Cancel", style: "cancel" },
                {
                    text: "Reset",
                    style: "destructive",
                    onPress: () =>
                        runReset(() =>
                            resetApp(() => router.replace(Path.Root))
                        ),
                },
            ]
        );
    };

    const name = method ? methodName[method] : undefined;
    const showCache = enabled && isAuthenticated && hdid && essentialDataLoaded;
    return (
        <View>
            <Breadcrumb
                items={[
                    {
                        text: "Security",
                        to: Path.Security,
                        active: true,
                        testId: "breadcrumb-security",
                    },
                ]}
            />
            <PageTitle title="Security" />
            <View className="flex-row items-start">
                <View className="flex-1 mr-3">
                    <AppText variant="h3" className="mb-2">
                        Device Security
                    </AppText>
                    {name ? (
                        <AppText>
                            {method === "none"
                                ? "Turn on a passcode or biometrics in your device settings to use this."
                                : `Ask for ${name} when you open the app.`}
                        </AppText>
                    ) : null}
                </View>
                <HgSwitch
                    testID="deviceSecuritySwitch"
                    accessibilityLabel="Device Security"
                    value={enabled}
                    disabled={authenticating || (method === "none" && !enabled)}
                    onValueChange={(value) => {
                        if (value) void enable();
                        else confirmTurnOff();
                    }}
                />
            </View>
            {showCache ? (
                <CacheSection hdid={hdid} />
            ) : (
                <View testID="cacheUnavailable">
                    <AppText variant="h3" className="mt-8 mb-2">
                        Cache
                    </AppText>
                    <AppText>
                        {enabled
                            ? "Log in to see the records saved on this device."
                            : "Turn on Device Security to save records on this device."}
                    </AppText>
                </View>
            )}
            <EnvironmentSection />
            <AppText variant="h3" className="mt-8 mb-2">
                Reset App
            </AppText>
            <Pressable
                testID="resetAppBtn"
                accessibilityRole="button"
                onPress={confirmReset}
                className="flex-row items-center min-h-[44px] py-2.5 active:bg-surface-hover"
            >
                <Icon
                    name="arrow-rotate-left"
                    size={14}
                    color={colors["bc-grey"]}
                    style={{ width: 24 }}
                />
                <AppText>Reset App</AppText>
            </Pressable>
        </View>
    );
}
