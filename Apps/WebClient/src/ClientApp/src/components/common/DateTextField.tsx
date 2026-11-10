import { useState } from "react";
import { View } from "react-native";

import {
    datePlaceholder,
    fromIsoFormat,
    type HgDatePickerProps,
    isValidDateText,
    maskDate,
    toIsoFormat,
} from "./hgDatePicker.shared";
import { HgTextField } from "./HgTextField";

/** The masked date field shared by both platforms; `calendar` is the platform's picker button. */
export function DateTextField({
    value = "",
    onChange,
    label = "Date",
    state,
    errorMessages = [],
    onBlur,
    onValidityChange,
    testID,
    nativeID,
    hideDetails = false,
    autoComplete,
    calendar,
}: HgDatePickerProps & { calendar: React.ReactNode }) {
    const [text, setText] = useState(fromIsoFormat(value));
    const [touched, setTouched] = useState(false);
    const [prevValue, setPrevValue] = useState(value);
    if (prevValue !== value) {
        // the model changed outside the field (e.g. from the calendar)
        setPrevValue(value);
        if (value !== (isValidDateText(text) && text ? toIsoFormat(text) : ""))
            setText(fromIsoFormat(value));
    }
    const valid = isValidDateText(text);
    const internalErrors = touched && !valid ? ["Invalid date"] : [];
    // `state` (the caller's own validity) only adds its messages; the field shows red when any message is present
    void state;

    function handleChange(raw: string) {
        const masked = maskDate(raw, raw.length < text.length);
        setText(masked);
        const converted =
            !isValidDateText(masked) || !masked ? "" : toIsoFormat(masked);
        if (converted !== value) {
            setPrevValue(converted);
            onChange(converted);
        }
    }

    return (
        <View className="flex-row items-start">
            <View className="flex-1">
                <HgTextField
                    testID={testID}
                    nativeID={nativeID}
                    label={label}
                    value={text}
                    onChangeText={handleChange}
                    onBlur={() => {
                        setTouched(true);
                        onBlur?.();
                        onValidityChange?.(isValidDateText(text));
                    }}
                    placeholder={datePlaceholder}
                    autoComplete={autoComplete}
                    autoCapitalize="characters"
                    autoCorrect={false}
                    clearable
                    maxLength={11}
                    errorMessages={
                        hideDetails ? [] : [...internalErrors, ...errorMessages]
                    }
                />
            </View>
            <View className="ml-2 mt-6">{calendar}</View>
        </View>
    );
}
