import { useState } from "react";
import { Text as RNText, View } from "react-native";

import {
    Dialog,
    FieldLabel,
    HgButton,
    HgChip,
    HgDatePicker,
    HgTextField,
    Icon,
} from "@/components/ui";
import { type EntryType, getEntryTypeDetails } from "@/constants/entryType";
import { Action, Text, Type } from "@/constants/tracking";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import { useThemeColors } from "@/hooks/useTheme";
import { TimelineFilterBuilder } from "@/models/timeline/timelineFilter";
import { services } from "@/services/container";
import {
    selectSelectedEntryTypes,
    useTimelineStore,
} from "@/stores/timeline.store";

/** "1.2K" past 999, as getFormattedFilterCount */
export function formatFilterCount(num: number | undefined): string {
    if (num === undefined) return "";
    return Math.abs(num) > 999
        ? Number.parseFloat(
              ((Math.round(num / 100) * 100) / 1000).toFixed(1)
          ).toString() + "K"
        : num.toString();
}

/**
 * Port of FilterComponent.vue: the Filter button and its dialog (search, record types with counts, dates).
 * `getCount` gives each dataset's record count.
 */
export function FilterDialog({
    entryTypes,
    getCount,
}: {
    entryTypes: EntryType[];
    getCount: (entryType: EntryType) => number | undefined;
}) {
    const colors = useThemeColors();
    const { isMobile, xs } = useBreakpoint();
    const activeFilter = useTimelineStore((s) => s.filter);
    const activeEntryTypes = useTimelineStore(selectSelectedEntryTypes);
    const setFilter = useTimelineStore((s) => s.setFilter);
    const [isVisible, setIsVisible] = useState(false);
    const [keywordInputText, setKeywordInputText] = useState("");
    const [startDate, setStartDate] = useState("");
    const [endDate, setEndDate] = useState("");
    const [selectedEntryTypes, setSelectedEntryTypes] = useState<EntryType[]>(
        []
    );

    const enabledEntryTypes = entryTypes
        .map((type) => ({
            type,
            display: getEntryTypeDetails(type)?.name ?? "",
        }))
        .filter((e) => e.display !== "");

    function showDialog() {
        setKeywordInputText(activeFilter.keyword);
        setStartDate(activeFilter.startDate);
        setEndDate(activeFilter.endDate);
        setSelectedEntryTypes([...activeEntryTypes]);
        setIsVisible(true);
        services.tracking.trackEvent({
            action: Action.ButtonClick,
            text: Text.FilterHealthRecords,
            type: Type.Filter,
        });
    }

    function apply() {
        setFilter(
            TimelineFilterBuilder.create()
                .withKeyword(keywordInputText)
                .withStartDate(startDate)
                .withEndDate(endDate)
                .withEntryTypes(selectedEntryTypes)
        );
        setIsVisible(false);
    }

    const toggle = (type: EntryType) =>
        setSelectedEntryTypes((s) =>
            s.includes(type) ? s.filter((t) => t !== type) : [...s, type]
        );

    return (
        <>
            <HgButton
                testID="filterDropdown"
                variant="secondary"
                icon="filter"
                // stated deviation: the web client hides the label below sm; it fits on a phone, so it always shows
                text="Filter"
                onPress={showDialog}
            />
            <Dialog
                visible={isVisible}
                title="Filter"
                tone="toolbar"
                persistent
                maxWidth={550}
                fullscreen={isMobile}
                testID="filterContainer"
                onRequestClose={() => setIsVisible(false)}
                actions={
                    <>
                        <HgButton
                            testID="btnFilterCancel"
                            variant="secondary"
                            text="Cancel"
                            onPress={() => setIsVisible(false)}
                        />
                        <HgButton
                            testID="btnFilterApply"
                            text="Apply"
                            onPress={apply}
                        />
                    </>
                }
            >
                {/* the dialog title is an h2: its labels are h3 */}
                <FieldLabel title="Search" level={3} />
                <HgTextField
                    testID="filterTextInput"
                    accessibilityLabel="Search"
                    value={keywordInputText}
                    onChangeText={setKeywordInputText}
                    maxLength={50}
                    placeholder="i.e. Medication Name"
                    prependInner={
                        <Icon
                            name="magnifying-glass"
                            size={16}
                            color={colors["bc-grey"]}
                        />
                    }
                />
                <FieldLabel title="Type" className="mb-0" level={3} />
                {/* v-chip-group column multiple */}
                <View className="flex-row flex-wrap gap-2 py-2 mb-2">
                    {enabledEntryTypes.map((entryType) => (
                        <HgChip
                            key={entryType.type}
                            testID={`${entryType.type}-filter`}
                            selected={selectedEntryTypes.includes(
                                entryType.type
                            )}
                            onPress={() => toggle(entryType.type)}
                        >
                            {entryType.display}{" "}
                            {/* plain Text: takes the chip's colour */}
                            <RNText testID={`${entryType.type}Count`}>
                                ({formatFilterCount(getCount(entryType.type))})
                            </RNText>
                        </HgChip>
                    ))}
                </View>
                <FieldLabel title="Dates" level={3} />
                <View className={xs ? "" : "flex-row -mx-3"}>
                    <View className={xs ? "" : "flex-1 px-3"}>
                        <HgDatePicker
                            testID="filterStartDateInput"
                            nativeID="start-date"
                            hideDetails
                            label="Start Date"
                            value={startDate}
                            onChange={setStartDate}
                        />
                    </View>
                    <View className={xs ? "" : "flex-1 px-3"}>
                        <HgDatePicker
                            testID="filterEndDateInput"
                            nativeID="end-date"
                            hideDetails
                            label="End Date"
                            value={endDate}
                            onChange={setEndDate}
                        />
                    </View>
                </View>
            </Dialog>
        </>
    );
}
