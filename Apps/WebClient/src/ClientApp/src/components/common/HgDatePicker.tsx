import { DateTime } from "luxon";
import { useState } from "react";
import { Platform, Pressable, View } from "react-native";

import { useColorScheme, useThemeColors } from "@/hooks/useTheme";
import { describeError, logger } from "@/services/logger";
import { formatDate, fromIsoDate, pacificTimeZone } from "@/utils/dateUtil";

import { AppModal } from "./AppModal";
import { Backdrop } from "./Backdrop";
import { DateTextField } from "./DateTextField";
import {
    defaultMaxDate,
    defaultMinDate,
    type HgDatePickerProps,
} from "./hgDatePicker.shared";
import { Icon } from "./Icon";

export type { HgDatePickerProps } from "./hgDatePicker.shared";

type NativePicker = typeof import("@react-native-community/datetimepicker");

/**
 * Port of HgDatePickerComponent.vue on native: the masked "YYYY-MMM-DD" field with a calendar button that opens
 * the platform's own date picker (Android's dialog, iOS's inline calendar in a sheet).
 */
export function HgDatePicker(props: HgDatePickerProps) {
    const colors = useThemeColors();
    const scheme = useColorScheme();
    const {
        value = "",
        onChange,
        minDate = defaultMinDate(),
        maxDate = defaultMaxDate(),
        onBlur,
        onValidityChange,
    } = props;
    const [iosPicker, setIosPicker] = useState<NativePicker | undefined>();
    const current = value
        ? fromIsoDate(value)
        : DateTime.local({ zone: pacificTimeZone });

    function apply(date: Date | undefined) {
        if (!date) return;
        const iso = formatDate(
            DateTime.fromJSDate(date).setZone(pacificTimeZone),
            "yyyy-MM-dd"
        );
        onChange(iso);
        onBlur?.();
        onValidityChange?.(true);
    }

    async function openCalendar() {
        try {
            // lazy: keeps the package out of the startup bundle. It does not detect a dev client built before the
            // package was added: the JS still loads and iOS then shows an empty sheet, so rebuild after adding it.
            const picker =
                await import("@react-native-community/datetimepicker");
            if (Platform.OS === "android")
                picker.DateTimePickerAndroid.open({
                    value: current.toJSDate(),
                    mode: "date",
                    minimumDate: minDate.toJSDate(),
                    maximumDate: maxDate.toJSDate(),
                    onValueChange: (_event, date) => apply(date),
                });
            else setIosPicker(picker);
        } catch (error) {
            logger.warn(`Date picker unavailable: ${describeError(error)}`);
        }
    }

    const IosPicker = iosPicker?.default;
    return (
        <>
            <DateTextField
                {...props}
                calendar={
                    <Pressable
                        accessibilityRole="button"
                        accessibilityLabel="Open calendar"
                        onPress={() => void openCalendar()}
                        className="w-11 h-11 items-center justify-center"
                    >
                        <Icon
                            name="calendar"
                            size={18}
                            color={colors["bc-black"]}
                        />
                    </Pressable>
                }
            />
            {IosPicker ? (
                <AppModal
                    transparent
                    animationType="slide"
                    supportedOrientations={["portrait", "landscape"]}
                    onRequestClose={() => setIosPicker(undefined)}
                >
                    <Backdrop
                        className="flex-1 bg-black/40 justify-end"
                        onPress={() => setIosPicker(undefined)}
                    >
                        {/* the sheet claims the touch, so a tap on it is not one on the scrim */}
                        <View
                            className="bg-surface rounded-t-2xl pb-8 pt-2 px-2"
                            onStartShouldSetResponder={() => true}
                        >
                            <IosPicker
                                value={current.toJSDate()}
                                mode="date"
                                display="inline"
                                minimumDate={minDate.toJSDate()}
                                maximumDate={maxDate.toJSDate()}
                                accentColor={colors.focus}
                                // the sheet follows the app's theme, not the system's
                                themeVariant={scheme}
                                onValueChange={(_event, date) => {
                                    // auto-apply
                                    apply(date);
                                    setIosPicker(undefined);
                                }}
                            />
                        </View>
                    </Backdrop>
                </AppModal>
            ) : null}
        </>
    );
}
