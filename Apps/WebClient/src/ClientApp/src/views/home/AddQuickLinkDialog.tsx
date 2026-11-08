import { useState } from "react";
import { Pressable, View } from "react-native";

import { TooManyRequests } from "@/components/site/TooManyRequests";
import { AppText, Dialog, HgAlert, HgButton } from "@/components/ui";
import { webAttributeRef, webClassRef } from "@/components/ui/webClass";
import {
    EntryType,
    entryTypes,
    getEntryTypeByModule,
    ServiceName,
} from "@/constants/entryType";
import { TooManyRequestsLocation } from "@/constants/errorType";
import { UserPreferenceType } from "@/constants/userPreferenceType";
import { isSessionEnded, isTooManyRequestsError } from "@/models/errors";
import type { QuickLink } from "@/models/quickLink";
import { describeError, logger } from "@/services/logger";
import { useErrorStore } from "@/stores/error.store";
import { useQuickLinks, useUserStore } from "@/stores/user.store";
import { ConfigUtil } from "@/utils/configUtil";
import { spaceToggleProps } from "@/utils/spaceToggle";

interface Props {
    visible: boolean;
    onClose: () => void;
}

const FEATURE_CHIPS: {
    value: string;
    text: string;
    preference: string;
    enabled: () => boolean;
}[] = [
    {
        value: "organ-donor-registration",
        text: "Organ Donor Registration",
        preference: UserPreferenceType.HideOrganDonorQuickLink,
        enabled: () =>
            ConfigUtil.isServiceEnabled(ServiceName.OrganDonorRegistration),
    },
    {
        value: "health-connect-registry",
        text: "Health Connect Registry",
        preference: UserPreferenceType.HideHealthConnectRegistryQuickLink,
        enabled: () =>
            ConfigUtil.isServiceEnabled(ServiceName.HealthConnectRegistry),
    },
    {
        value: "immunization-record",
        text: "Immunization Record",
        preference: UserPreferenceType.HideImmunizationRecordQuickLink,
        enabled: () =>
            ConfigUtil.isDatasetEnabled(EntryType.Immunization) &&
            ConfigUtil.isImmunizationRecordLinkEnabled(),
    },
    {
        value: "recommendations-dialog",
        text: "Vaccine Recommendations",
        preference: UserPreferenceType.HideRecommendationsQuickLink,
        enabled: () =>
            ConfigUtil.isDatasetEnabled(EntryType.Immunization) &&
            ConfigUtil.showRecommendationsLink(),
    },
];

/** Port of AddQuickLinkComponent.vue: chip picker of datasets (and hidden feature cards) → saves quick links / preferences. */
export function AddQuickLinkDialog({ visible, onClose }: Props) {
    const user = useUserStore((s) => s.user);
    const quickLinks = useQuickLinks();
    const updateQuickLinks = useUserStore((s) => s.updateQuickLinks);
    const setUserPreference = useUserStore((s) => s.setUserPreference);
    const errorStore = useErrorStore();
    const [selected, setSelected] = useState<string[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(false);

    const datasetChips = entryTypes
        .filter((d) => ConfigUtil.isDatasetEnabled(d.type))
        .filter(
            (d) =>
                !quickLinks?.some(
                    (q) =>
                        q.filter.modules.length === 1 &&
                        q.filter.modules[0] === d.moduleName
                )
        )
        .map((d) => ({ value: d.moduleName, text: d.name }));
    const featureChips = FEATURE_CHIPS.filter(
        (c) => c.enabled() && user.preferences[c.preference]?.value === "true"
    );
    const chips = [
        ...datasetChips,
        ...featureChips.map((c) => ({ value: c.value, text: c.text })),
    ];

    const toggle = (v: string) =>
        setSelected((s) =>
            s.includes(v) ? s.filter((x) => x !== v) : [...s, v]
        );

    // clearErrors(): the dialog's banner error and its too-many-requests error
    function clearErrors() {
        setError(false);
        if (
            useErrorStore.getState().tooManyRequestsError ===
            TooManyRequestsLocation.AddQuickLinkModal
        )
            errorStore.clearTooManyRequestsError();
    }

    function cancel() {
        setSelected([]);
        clearErrors();
        onClose();
    }

    async function submit() {
        clearErrors();
        setLoading(true);
        const links: QuickLink[] = [...(quickLinks ?? [])];
        for (const module of selected) {
            const details = getEntryTypeByModule(module);
            if (details)
                links.push({
                    name: details.name,
                    filter: { modules: [module] },
                });
        }
        try {
            const tasks: Promise<void>[] = [updateQuickLinks(links)];
            for (const chip of FEATURE_CHIPS) {
                if (selected.includes(chip.value)) {
                    tasks.push(
                        setUserPreference({
                            ...user.preferences[chip.preference],
                            value: "false",
                        })
                    );
                }
            }
            await Promise.all(tasks);
            onClose();
        } catch (e) {
            if (isSessionEnded(e)) return;
            logger.error(describeError(e));
            if (isTooManyRequestsError(e))
                errorStore.setTooManyRequestsError(
                    TooManyRequestsLocation.AddQuickLinkModal
                );
            else setError(true);
        } finally {
            setSelected([]);
            setLoading(false);
        }
    }

    return (
        <Dialog
            visible={visible}
            title="Add a quick link"
            onRequestClose={cancel}
            testID="add-quick-link-modal"
            actions={
                <>
                    <HgButton
                        testID="cancel-add-quick-link-btn"
                        text="Cancel"
                        variant="secondary"
                        onPress={cancel}
                    />
                    <HgButton
                        testID="add-quick-link-btn"
                        text="Save"
                        disabled={selected.length === 0 || loading}
                        loading={loading}
                        onPress={() => void submit()}
                    />
                </>
            }
        >
            <TooManyRequests
                location={TooManyRequestsLocation.AddQuickLinkModal}
            />
            {error ? (
                <HgAlert
                    type="error"
                    live="assertive"
                    closable
                    onClose={() => setError(false)}
                    testID="quick-link-modal-error"
                    title="Unable to update Quick Links"
                >
                    <AppText>
                        If you continue to have issues, please contact
                        HealthGateway@gov.bc.ca.
                    </AppText>
                </HgAlert>
            ) : null}
            {/* v-chip-group column multiple */}
            <View
                testID="quick-link-modal-text"
                className="flex-row flex-wrap gap-2 py-2"
            >
                {chips.map((chip) => {
                    const on = selected.includes(chip.value);
                    return (
                        <Pressable
                            ref={(node) => {
                                webClassRef({ "v-chip": true })(node);
                                webAttributeRef(
                                    "name",
                                    `${chip.value}-filter`
                                )(node);
                            }}
                            key={chip.value}
                            testID={`${chip.value}-filter`}
                            accessibilityRole="checkbox"
                            aria-checked={on}
                            onPress={() => toggle(chip.value)}
                            {...spaceToggleProps(() => toggle(chip.value))}
                            className={`rounded-full border border-primary px-3 min-h-8 justify-center ${on ? "bg-primary" : "bg-surface"}`}
                        >
                            <AppText
                                variant="body-medium"
                                className={
                                    on ? "text-on-primary" : "text-primary"
                                }
                            >
                                {chip.text}
                            </AppText>
                        </Pressable>
                    );
                })}
                {chips.length === 0 ? (
                    <AppText>
                        All available quick links have been added.
                    </AppText>
                ) : null}
            </View>
        </Dialog>
    );
}
