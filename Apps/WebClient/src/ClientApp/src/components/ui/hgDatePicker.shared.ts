import { DateTime } from "luxon";
import type { TextInputProps } from "react-native";

import {
    defaultDateFormat,
    formatDate,
    fromIsoDate,
    pacificTimeZone,
} from "@/utils/dateUtil";

/** placeholder: DateWrapper.defaultFormat upper-cased */
export const datePlaceholder = defaultDateFormat.toUpperCase();

/** "2026-10-30" → "2026-OCT-30" (fromIsoFormat) */
export const fromIsoFormat = (value: string): string =>
    formatDate(fromIsoDate(value)).toUpperCase();

/** "2026-OCT-30" → "2026-10-30" (toIsoFormat, DateWrapper.fromStringFormat) */
export const toIsoFormat = (value: string): string =>
    DateTime.fromFormat(value, defaultDateFormat, {
        locale: "en-US",
        zone: pacificTimeZone,
    }).toISODate() ?? "";

/** the "Invalid date" rule: empty passes */
export const isValidDateText = (value: string): boolean =>
    value
        ? DateTime.fromFormat(value, defaultDateFormat, { locale: "en-US" })
              .isValid
        : true;

/**
 * maska "####-@@@-##", eager, upper-cased: digits for #, letters for @; a dash is written as soon as the section
 * before it is complete. When deleting, a trailing dash is not put back (so backspace can remove it).
 */
export function maskDate(raw: string, deleting = false): string {
    const pattern = "####-@@@-##";
    const chars = [...raw.toUpperCase()].filter((c) => /[0-9A-Z]/.test(c));
    let out = "";
    let ci = 0;
    for (const token of pattern) {
        if (token === "-") {
            out += "-";
            continue;
        }
        let matched = false;
        while (ci < chars.length) {
            const c = chars[ci++];
            if (
                (token === "#" && /\d/.test(c)) ||
                (token === "@" && /[A-Z]/.test(c))
            ) {
                out += c;
                matched = true;
                break;
            }
        }
        if (!matched) break;
    }
    return deleting && out.endsWith("-") ? out.slice(0, -1) : out;
}

export interface HgDatePickerProps {
    /** ISO date (yyyy-MM-dd) or "" */
    value?: string;
    onChange: (iso: string) => void;
    label?: string;
    /** false = invalid from the caller's own rules */
    state?: boolean;
    errorMessages?: string[];
    minDate?: DateTime;
    maxDate?: DateTime;
    onBlur?: () => void;
    onValidityChange?: (valid: boolean) => void;
    testID?: string;
    /** the input's id, as the web client's `id` falls through to v-text-field's input */
    nativeID?: string;
    /** v-text-field hide-details: no messages under the field */
    hideDetails?: boolean;
    /** what the date is (WCAG 1.3.5), e.g. a birth date; default off */
    autoComplete?: TextInputProps["autoComplete"];
}

export const defaultMinDate = () =>
    DateTime.fromObject(
        { year: 1900, month: 1, day: 1 },
        { zone: pacificTimeZone }
    );
export const defaultMaxDate = () =>
    DateTime.fromObject(
        { year: 2099, month: 12, day: 31 },
        { zone: pacificTimeZone }
    );
