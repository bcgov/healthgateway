import { useIsFocused } from "expo-router";
import { useEffect, useState } from "react";
import { Platform, Pressable, View } from "react-native";

import { AppText, Icon } from "@/components/ui";
import { elevation1 } from "@/components/ui/elevation";
import { EntryType, getEntryTypeDetails } from "@/constants/entryType";
import { Action, Origin } from "@/constants/tracking";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import { useThemeColors } from "@/hooks/useTheme";
import type { TimelineEntry } from "@/models/timeline/timelineEntry";
import { services } from "@/services/container";
import { useTimelineStore } from "@/stores/timeline.store";
import { cn } from "@/utils/cn";
import { formatDate } from "@/utils/dateUtil";

import { CommentSection } from "./comment/CommentSection";

interface Props {
    entry: TimelineEntry;
    title: string;
    entryIcon?: string;
    /** background of the round icon (`icon-class`, e.g. "bg-primary") */
    iconClass?: string;
    canShowDetails?: boolean;
    /** rendered inside the full-screen details dialog on mobile: details open, header not interactive */
    isMobileDetails?: boolean;
    subtitle?: string;
    /** #header-description slot (replaces the subtitle) */
    headerDescription?: React.ReactNode;
    /** #header-menu slot */
    headerMenu?: React.ReactNode;
    hasAttachment?: boolean;
    onAttachmentPress?: () => void;
    /** show the comment section under the details (comments enabled and the dataset takes comments) */
    allowComment?: boolean;
    testID?: string;
    children?: React.ReactNode;
}

/**
 * Port of TimelineEntryComponent.vue: a v-expansion-panel whose header shows the dataset icon, date, type and
 * title. Desktop: pressing the header toggles the details. Mobile: it opens the full-screen details dialog.
 */
export function TimelineEntryCard({
    entry,
    title,
    entryIcon = "question",
    iconClass = "",
    canShowDetails = true,
    isMobileDetails = false,
    subtitle = "",
    headerDescription,
    headerMenu,
    hasAttachment = false,
    onAttachmentPress,
    allowComment = false,
    testID,
    children,
}: Props) {
    const colors = useThemeColors();
    const { inlineEntryDetails, xs } = useBreakpoint();
    const focused = useIsFocused();
    const openFullscreenEntry = useTimelineStore((s) => s.openFullscreenEntry);
    const addInlineEntry = useTimelineStore((s) => s.addInlineEntry);
    const removeInlineEntry = useTimelineStore((s) => s.removeInlineEntry);
    const isFullscreenEntry = useTimelineStore(
        (s) =>
            s.fullscreenEntry?.id === entry.id &&
            s.fullscreenEntry?.type === entry.type
    );
    const [detailsOpen, setDetailsOpen] = useState(isMobileDetails);
    const [hovered, setHovered] = useState(false);
    // the details close when the layout becomes mobile (watch on layoutStore.isMobile)
    const [wasInline, setWasInline] = useState(inlineEntryDetails);
    if (wasInline !== inlineEntryDetails) {
        setWasInline(inlineEntryDetails);
        if (!inlineEntryDetails && !isMobileDetails) setDetailsOpen(false);
        // native (stated deviation): an entry open full screen carries on, expanded, when the window widens
        else if (
            Platform.OS !== "web" &&
            inlineEntryDetails &&
            !isMobileDetails &&
            isFullscreenEntry
        )
            setDetailsOpen(true);
    }
    const detailsVisible =
        isMobileDetails || (detailsOpen && inlineEntryDetails);

    // what FullscreenTimelineEntry carries to full screen when the window narrows, as the details close above
    const tracksInline =
        Platform.OS !== "web" && focused && detailsVisible && !isMobileDetails;
    useEffect(() => {
        if (!tracksInline) return;
        addInlineEntry(entry);
        return () => removeInlineEntry(entry);
    }, [tracksInline, entry, addInlineEntry, removeInlineEntry]);

    const isInteractive =
        (inlineEntryDetails && canShowDetails) ||
        (!inlineEntryDetails && !isMobileDetails);
    const details = getEntryTypeDetails(entry.type);
    const entryTypeTitle =
        details?.title && details.title.trim() ? details.title : entry.type;
    const displayTitle = title?.trim() ?? "";
    const dateString = formatDate(entry.date);
    const commentCount = entry.comments?.length ?? 0;

    function trackCardExpanded() {
        if (!details) return;
        services.tracking.trackEvent({
            action: Action.CardClick,
            text:
                entry.type === EntryType.BcCancerScreening
                    ? title
                    : details.dataset,
            origin: Origin.Timeline,
            dataset: details.dataset,
            type: details.dataset,
        });
    }

    function handleCardPress() {
        if (!inlineEntryDetails && !isMobileDetails) {
            openFullscreenEntry(entry);
            trackCardExpanded();
        } else if (inlineEntryDetails && canShowDetails) {
            const next = !detailsOpen;
            setDetailsOpen(next);
            if (next) trackCardExpanded();
        }
    }

    /**
     * Web: the header's content lets real presses through to the layer under it, but a press sent straight to the
     * content (Cypress's `click({ force: true })` on a title) bubbles here instead. Buttons look after themselves.
     */
    function handleContentClick(event: {
        target: unknown;
        currentTarget: unknown;
    }) {
        const target = event.target as HTMLElement;
        if (
            (event.currentTarget as HTMLElement).contains(target) &&
            !target.closest("button, [role=button]")
        )
            handleCardPress();
    }

    return (
        <View
            testID={testID}
            className={cn("bg-surface", isMobileDetails ? "flex-1" : "mb-2")}
            style={isMobileDetails ? undefined : elevation1}
        >
            {/* one child, as v-expansion-panel has: Cypress runs `.children().within()` on the card */}
            <View className={isMobileDetails ? "flex-1" : undefined}>
                {/*
              The header is a button layer under the content (not a <button> wrapping it), so the attachment and
              menu buttons are not nested inside a button. The content lets presses through to the layer.
            */}
                <View
                    className={cn(
                        "relative",
                        isInteractive && hovered && "bg-surface-hover"
                    )}
                    {...(Platform.OS === "web"
                        ? { onClick: handleContentClick }
                        : undefined)}
                >
                    <Pressable
                        accessibilityRole={isInteractive ? "button" : undefined}
                        accessibilityLabel={[
                            dateString,
                            entryTypeTitle,
                            displayTitle,
                        ]
                            .filter(Boolean)
                            .join(" ")}
                        accessibilityState={
                            isInteractive
                                ? { expanded: detailsVisible }
                                : undefined
                        }
                        disabled={!isInteractive}
                        onPress={handleCardPress}
                        onHoverIn={() => setHovered(true)}
                        onHoverOut={() => setHovered(false)}
                        className="absolute inset-0"
                    />
                    <View
                        pointerEvents="box-none"
                        className={cn(
                            "flex-row px-6 py-4",
                            detailsVisible ? "min-h-[64px]" : "min-h-[48px]"
                        )}
                    >
                        <View
                            pointerEvents="none"
                            className={cn(
                                // .timeline-entry-header-icon pa-4 d-flex justify-center: the 30px (x-large) v-icon is
                                // centred across and sits at the top of the padding, 1px above the middle
                                "w-16 h-16 rounded-full p-4 items-center",
                                iconClass
                            )}
                        >
                            <Icon
                                name={entryIcon}
                                size={30}
                                color={colors["on-primary"]}
                                fixedWidth
                            />
                        </View>
                        <View className="flex-1 ml-6" pointerEvents="box-none">
                            <View
                                pointerEvents="none"
                                testID={`${String(entry.type).toLowerCase()}Title`}
                                className={
                                    xs
                                        ? ""
                                        : "flex-row flex-wrap items-baseline"
                                }
                            >
                                <AppText
                                    testID="entryCardDate"
                                    className="font-bold mr-2"
                                >
                                    {dateString}
                                </AppText>
                                <AppText
                                    testID="entry-card-type-title"
                                    className="font-bold"
                                >
                                    {entryTypeTitle}
                                </AppText>
                                {displayTitle ? (
                                    <AppText>
                                        {!xs ? (
                                            <AppText
                                                testID="entry-card-details-pipe"
                                                className="font-bold"
                                                aria-hidden
                                            >
                                                {" | "}
                                            </AppText>
                                        ) : null}
                                        <AppText testID="entryCardDetailsTitle">
                                            {displayTitle}
                                        </AppText>
                                    </AppText>
                                ) : null}
                            </View>
                            <View
                                className="flex-row items-center mt-2"
                                pointerEvents="box-none"
                            >
                                <View
                                    pointerEvents="none"
                                    className="flex-1 py-1"
                                    testID="entryCardDetailsSubtitle"
                                >
                                    {headerDescription ?? (
                                        <AppText className="text-[15px]">
                                            {subtitle}
                                        </AppText>
                                    )}
                                </View>
                                <View className="flex-row items-center">
                                    {commentCount > 1 ? (
                                        <AppText
                                            testID="commentCount"
                                            className="text-primary"
                                        >
                                            {commentCount}
                                        </AppText>
                                    ) : null}
                                    {commentCount > 0 ? (
                                        <View
                                            testID="commentIcon"
                                            className="p-1"
                                        >
                                            <Icon
                                                name="comment"
                                                solid={false}
                                                size={12}
                                                color={colors.primary}
                                            />
                                        </View>
                                    ) : null}
                                    {hasAttachment ? (
                                        <Pressable
                                            testID="attachment-button"
                                            accessibilityRole="button"
                                            accessibilityLabel="Download attachment"
                                            onPress={(e) => {
                                                e.stopPropagation();
                                                onAttachmentPress?.();
                                            }}
                                            className="p-2"
                                        >
                                            <Icon
                                                name="paperclip"
                                                size={12}
                                                color={colors.primary}
                                            />
                                        </Pressable>
                                    ) : null}
                                    {headerMenu}
                                </View>
                            </View>
                        </View>
                    </View>
                </View>
                {detailsVisible ? (
                    // v-expansion-panel-text: 8px 24px 16px; my-2 and the indent past the icon on desktop
                    <View className="px-6 pt-2 pb-4">
                        <View
                            className={cn(
                                "my-2",
                                !isMobileDetails && "ml-[88px]"
                            )}
                        >
                            {children}
                            {allowComment ? (
                                <CommentSection
                                    parentEntry={entry}
                                    isMobileDetails={isMobileDetails}
                                />
                            ) : null}
                        </View>
                    </View>
                ) : null}
            </View>
        </View>
    );
}
