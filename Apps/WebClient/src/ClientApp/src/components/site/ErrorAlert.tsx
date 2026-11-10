import { usePathname } from "expo-router";
import { useEffect, useState } from "react";
import { Pressable, View } from "react-native";

import {
    AppText,
    HgAlert,
    HgButton,
    Icon,
    MessageDialog,
} from "@/components/ui";
import { Path } from "@/constants/path";
import { useThemeColors } from "@/hooks/useTheme";
import { useErrorStore } from "@/stores/error.store";
import { useUserStore } from "@/stores/user.store";
import { copyToClipboard } from "@/utils/clipboard";
import { formatDate, today } from "@/utils/dateUtil";
import { hrefProps } from "@/utils/hrefProps";
import { openExternal } from "@/utils/openExternal";

import { TooManyRequests } from "./TooManyRequests";

const SUPPORT_EMAIL_LINK = "mailto:HealthGateway@gov.bc.ca";

/**
 * Port of ErrorCardComponent.vue: the page "too many requests" message, then the error banner with
 * "Multiple errors have occurred" for several errors, and a View / Hide Details panel listing
 * `source/date/hdid[:traceId]` per error with a Copy button.
 */
export function ErrorAlert() {
    const colors = useThemeColors();
    const pathname = usePathname();
    const errors = useErrorStore((s) => s.errors);
    const clearErrors = useErrorStore((s) => s.clearErrors);
    const hdid = useUserStore((s) => s.user.hdid);
    const [expanded, setExpanded] = useState(false);
    const [copied, setCopied] = useState(false);

    // errors are per page
    useEffect(() => clearErrors(), [pathname, clearErrors]);

    if (pathname === Path.Root) return null;

    const date = formatDate(today());
    const details = errors.map(
        (e) =>
            `${e.source?.trim() ?? ""}/${date}/${hdid}${e.traceId ? `:${e.traceId.trim()}` : ""}`
    );
    const title =
        errors.length > 1 ? "Multiple errors have occurred" : errors[0]?.title;

    return (
        <View>
            <TooManyRequests />
            {errors.length > 0 ? (
                <HgAlert
                    type="error"
                    live="assertive"
                    closable
                    onClose={clearErrors}
                    title={title}
                    testID="errorBanner"
                >
                    {/* v-expansion-panel inside the alert */}
                    <View className="mt-4 rounded-md border-2 border-error-border overflow-hidden">
                        <Pressable
                            testID="errorDetailsBtn"
                            accessibilityRole="button"
                            accessibilityState={{ expanded }}
                            onPress={() => setExpanded((v) => !v)}
                            className="flex-row items-center justify-between px-4 py-3 bg-error-panel"
                        >
                            <AppText className="font-bold text-error">
                                {expanded ? "Hide Details" : "View Details"}
                            </AppText>
                            <Icon
                                name={expanded ? "chevron-up" : "chevron-down"}
                                size={14}
                                color={colors.error}
                            />
                        </Pressable>
                        {expanded ? (
                            <View className="px-4 pt-4 pb-2 border-t border-error-border bg-error-panel">
                                <View className="flex-row flex-wrap items-center mb-4">
                                    <AppText>
                                        Try refreshing the page. If this issue
                                        persists, contact{" "}
                                        <AppText
                                            accessibilityRole="link"
                                            className="text-link"
                                            // a real mailto anchor on web, so the keyboard can follow it
                                            {...hrefProps(SUPPORT_EMAIL_LINK)}
                                            onPress={(event) => {
                                                event.preventDefault();
                                                void openExternal(
                                                    SUPPORT_EMAIL_LINK
                                                );
                                            }}
                                        >
                                            HealthGateway@gov.bc.ca
                                        </AppText>{" "}
                                        and provide{" "}
                                    </AppText>
                                    <HgButton
                                        testID="copyToClipBoardBtn"
                                        text="Copy"
                                        icon="copy"
                                        variant="secondary"
                                        onPress={() =>
                                            void copyToClipboard(
                                                details.join("\n")
                                            ).then(
                                                (ok) => ok && setCopied(true)
                                            )
                                        }
                                    />
                                </View>
                                {details.map((d, i) => (
                                    <AppText
                                        key={i}
                                        testID={`error-details-span-${i + 1}`}
                                        className="mb-2"
                                    >
                                        {d}
                                    </AppText>
                                ))}
                            </View>
                        ) : null}
                    </View>
                </HgAlert>
            ) : null}
            <MessageDialog
                visible={copied}
                okOnly
                title="Copy to Clipboard"
                message="Copied Successfully"
                onClose={() => setCopied(false)}
            />
        </View>
    );
}
