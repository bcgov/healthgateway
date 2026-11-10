import { DateTime } from "luxon";

/**
 * The parts of the web client's DateWrapper (models/dateWrapper.ts) the port uses. Dates live in the
 * Pacific time zone and format as `yyyy-MMM-dd` in en-US, like the web client.
 */
export const pacificTimeZone = "America/Vancouver";
export const defaultDateFormat = "yyyy-MMM-dd";
const locale = "en-US";

/** DateWrapper.fromIsoDate: the first 10 characters as yyyy-MM-dd, in Pacific time. */
export function fromIsoDate(dateString?: string | null): DateTime {
    return DateTime.fromFormat(
        (dateString || "").substring(0, 10),
        "yyyy-MM-dd",
        { zone: pacificTimeZone }
    );
}

/**
 * DateWrapper.fromIso: an ISO date-time; strings without an offset are read in Pacific time, and the result is
 * shown in Pacific time.
 */
export function fromIso(
    dateString?: string | null,
    defaultZone: string = pacificTimeZone
): DateTime {
    return DateTime.fromISO(dateString || "", { zone: defaultZone }).setZone(
        pacificTimeZone
    );
}

/** DateWrapper.today() / now() in Pacific time. */
export const today = (): DateTime =>
    DateTime.local({ zone: pacificTimeZone }).startOf("day");
export const now = (): DateTime => DateTime.local({ zone: pacificTimeZone });

/** DateWrapper.format(): "" for an invalid date. */
export function formatDate(
    date: DateTime,
    format: string = defaultDateFormat
): string {
    return date.isValid ? date.toFormat(format, { locale }) : "";
}

/** DateSortUtility.descending: undefined sorts last; later dates first. */
export function descending(a?: DateTime, b?: DateTime): number {
    if (a === undefined && b === undefined) return 0;
    if (a === undefined) return 1;
    if (b === undefined) return -1;
    if (a < b) return 1;
    return a > b ? -1 : 0;
}

/** DateSortUtility.ascending */
export const ascending = (a?: DateTime, b?: DateTime): number =>
    -1 * descending(a, b);

/** appStore.isPacificTime: the device is on Pacific time (dates then omit the zone name) */
export const isPacificTime = (): boolean =>
    now().offset === DateTime.local().offset;
