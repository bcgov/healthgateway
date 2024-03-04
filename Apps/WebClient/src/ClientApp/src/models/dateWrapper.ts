import {
    DateTime,
    DateTimeUnit,
    Duration,
    DurationLike,
    DurationUnit,
} from "luxon";

const pacificTimeZone = "America/Vancouver";
const locale = "en-US"; // times display as 4:07 PM in en-US locale and 4:07 p.m. in en-CA locale

/**
 * Typed representation of a ISO Date, time is not relevant.
 */
export type StringISODate = string;

/**
 * Typed representation of a ISO Date with time.
 */
export type StringISODateTime = string;

/**
 * Defines the options for parsing the date time. Ignored if constructed
 * from a copy or a DateTime object.
 */
export interface ParseOptions {
    /**
     * True if it should be parsed as a UTC date time.
     */
    isUtc?: boolean;
    /**
     * True if the string representation contains also time information with no zone or offset.
     */
    hasTime?: boolean;
}

/**
 * Interface for {@link DateWrapper} that can be used as a type in reactive Vue references.
 */
export interface IDateWrapper {
    /**
     * Formats the date using the given a set of tokens.
     * See https://moment.github.io/luxon/#/formatting for the rules.
     * @param formatString The tokens to format the string as.
     * @returns A string representation of the datetime with the given format, or the empty string if the date is invalid.
     */
    format(formatString?: string): string;

    /**
     * Formats the datetime to the ISO format (YYYY-MM-DDTHH:MM:SS:M-Z).
     * @param toUtc (optional) Whether to set the time zone to UTC.
     * @returns The formatted string representation, or the empty string if the date is invalid.
     */
    toISO(toUtc?: boolean): StringISODate;

    /**
     * Formats the date to the ISO format without time (YYYY-MM-DD).
     * @returns The formatted string representation, or the empty string if the date is invalid.
     */
    toISODate(): StringISODate;

    /**
     * Converts to a JS Date object.
     * @returns The Date object.
     */
    toJSDate(): Date;

    /**
     * Gives the number of milliseconds since the Unix epoch.
     * @returns The number milliseconds since the Unix epoch.
     */
    fromEpoch(): number;

    /**
     * Determines if the date is valid.
     * @returns True if the date is valid.
     */
    isValid(): boolean;

    /**
     * Gets the year.
     * @returns Calendar year.
     */
    year(): number;

    /**
     * Get the month (1-12).
     * @returns Calendar month.
     */
    month(): number;

    /**
     * Get the days.
     * @returns Calendar day.
     */
    day(): number;

    /**
     * Gets the milliseconds (not from epoch).
     * @returns Milliseconds from the second.
     */
    millisecond(): number;

    /**
     * Returns the weekday.
     * @returns Day of the week (0 to 6, Sunday is 0).
     */
    weekday(): number;

    /**
     * Difference between two dates in milliseconds.
     * @param other Other date to compare.
     * @param unit (optional) Unit of comparision (year, hour, minute, second, ...).
     * @returns The difference as a Duration object.
     */
    diff(other: IDateWrapper, unit?: DurationUnit): Duration;

    /**
     * Checks if two dates are the same. Can use a provided unit of time.
     * @param other Date to compare.
     * @param unit (optional) Unit of time to compare.
     * @returns True if they are the same date, false otherwise.
     */
    isSame(other: IDateWrapper, unit?: DurationUnit): boolean;

    /**
     * Checks if this date is before a given date.
     * @param other Date to compare.
     * @returns True if this date is before the given date, false otherwise.
     */
    isBefore(other: IDateWrapper): boolean;

    /**
     * Checks if this date is before or the same as a given date.
     * @param other Date to compare.
     * @returns True if this date is before or the same as the given date, false otherwise.
     */
    isBeforeOrSame(other: IDateWrapper): boolean;

    /**
     * Checks if this date is after a given date.
     * @param other Date to compare.
     * @returns True if this date is after the given date, false otherwise.
     */
    isAfter(other: IDateWrapper): boolean;

    /**
     * Checks if this date is after or the same as a given date.
     * @param other Date to compare.
     * @returns True if this date is after or the same as the given date, false otherwise.
     */
    isAfterOrSame(other: IDateWrapper): boolean;

    /**
     * Returns a modified date where a particular unit has been reset to its minimum value.
     * @param unit The unit of time (day, month, year, ...).
     * @returns A new DateWrapper.
     */
    startOf(unit: DurationUnit): DateWrapper;

    /**
     * Returns the date with its zone changed to the local (browser) time.
     * @returns A new DateWrapper.
     */
    toLocal(): DateWrapper;

    /**
     * Adds a duration to this date.
     * @param duration The duration as a unit or object.
     * @returns A new DateWrapper.
     */
    add(duration: DurationLike): DateWrapper;

    /**
     * Substracts a duration from this date.
     * @param duration The duration as a unit or object.
     * @returns A new DateWrapper.
     */
    subtract(duration: DurationLike): DateWrapper;

    /**
     * Returns the time zone's offset from UTC in minutes.
     * @returns Time zone's offset from UTC in minutes.
     */
    offset(): number;
}

/**
 * Class that wraps a library definition of a date time. Immutable to prevent bad date manipulation.
 * Centralizes date operations and modifications while abstracting library specific implementations.
 */
export class DateWrapper implements IDateWrapper {
    static readonly defaultFormat = "yyyy-MMM-dd";

    /**
     * Internal object that holds date and time logic.
     */
    private readonly _internal_date: DateTime;

    /**
     * Gets the internal date time object. Use only for debugging. DO NOT USE OTHERWISE.
     */
    public get internalDate(): DateTime {
        return this._internal_date;
    }

    /**
     * Creates an immutable DateWrapper from a Luxon DateTime.
     * @param dateTime Luxon DateTime.
     */
    private constructor(dateTime: DateTime) {
        this._internal_date = dateTime;
    }

    /**
     * Creates a new DateWrapper set to the current day and time.
     * @returns A new DateWrapper object.
     */
    public static now(): DateWrapper {
        return new DateWrapper(DateTime.local({ zone: pacificTimeZone }));
    }

    /**
     * Creates a new DateWrapper set to the start of the current day.
     * @returns A new DateWrapper object.
     */
    public static today(): DateWrapper {
        return new DateWrapper(
            DateTime.local({ zone: pacificTimeZone })
        ).startOf("day");
    }

    /**
     * Creates a new DateWrapper from an ISO date string.
     * @param dateString ISO date string.
     * @param defaultZone The time zone to use for date strings that do not include a timezone offset. Defaults to the Pacific Time Zone.
     * @returns A new DateWrapper object. If the date string is empty or invalid, the method isValid() will return false.
     */
    public static fromIso(
        dateString?: string | null,
        defaultZone: string = pacificTimeZone
    ): DateWrapper {
        return new DateWrapper(
            DateTime.fromISO(dateString || "", { zone: defaultZone }).setZone(
                pacificTimeZone
            )
        );
    }

    /**
     * Creates a new DateWrapper from a string containing a date. Any time information is discarded.
     * @param dateString Date string in "yyyy-MM-dd" format.
     * @returns A new DateWrapper object for the given date at the start of the day.
     * If the date string is empty or invalid, the method isValid() will return false.
     */
    public static fromIsoDate(dateString?: string | null): DateWrapper {
        return new DateWrapper(
            DateTime.fromFormat(
                (dateString || "").substring(0, 10),
                "yyyy-MM-dd",
                {
                    zone: pacificTimeZone,
                }
            )
        );
    }

    /**
     * Creates a new DateWrapper from numerical parameters for year/month/day.
     * @param year The year to be set.
     * @param month The month to be set.
     * @param day The day to be set.
     * @returns A new DateWrapper object.
     */
    public static fromNumerical(
        year: number,
        month: number,
        day: number
    ): DateWrapper {
        return new DateWrapper(
            DateTime.fromObject({ year, month, day }, { zone: pacificTimeZone })
        );
    }

    /**
     * Creates a new DateWrapper from a custom date format.
     * @dateString dateString The date string.
     * @param format The format string.
     * @returns A new DateWrapper object.
     */
    public static fromStringFormat(
        dateString: string,
        format?: string
    ): DateWrapper {
        return new DateWrapper(
            DateTime.fromFormat(dateString, format ?? this.defaultFormat)
        );
    }

    /**
     * Returns the number of days in a month for a given year.
     * @param year The year to check.
     * @param month The month to check.
     * @returns The number of days in the month.
     */
    public static daysInMonth(year: number, month: number): number | undefined {
        return DateTime.local(year, month, { zone: pacificTimeZone })
            .daysInMonth;
    }

    /** {@inheritDoc IDateWrapper.format} */
    public format(formatString?: string): string {
        return this.internalDate.isValid
            ? this.internalDate.toFormat(
                  formatString ?? DateWrapper.defaultFormat,
                  {
                      locale,
                  }
              )
            : "";
    }

    /** {@inheritDoc IDateWrapper.toISO} */
    public toISO(toUtc = false): StringISODate {
        let dateTime = this.internalDate;
        if (toUtc) {
            dateTime = dateTime.toUTC();
        }
        return dateTime.toISO() ?? "";
    }

    /** {@inheritDoc IDateWrapper.toISODate} */
    public toISODate(): StringISODate {
        return this.internalDate.toISODate() ?? "";
    }

    /** {@inheritDoc IDateWrapper.toJSDate} */
    public toJSDate(): Date {
        return this.internalDate.toJSDate();
    }

    /** {@inheritDoc IDateWrapper.fromEpoch} */
    public fromEpoch(): number {
        return this.internalDate.valueOf();
    }

    /** {@inheritDoc IDateWrapper.isValid} */
    public isValid(): boolean {
        return this.internalDate.isValid;
    }

    /** {@inheritDoc IDateWrapper.year} */
    public year(): number {
        return this.internalDate.year;
    }

    /** {@inheritDoc IDateWrapper.month} */
    public month(): number {
        return this.internalDate.month;
    }

    /** {@inheritDoc IDateWrapper.day} */
    public day(): number {
        return this.internalDate.day;
    }

    /** {@inheritDoc IDateWrapper.millisecond} */
    public millisecond(): number {
        return this.internalDate.millisecond;
    }

    /** {@inheritDoc IDateWrapper.weekday} */
    public weekday(): number {
        const weekday = this.internalDate.weekday;
        // luxor weekdays are 1-7 where 7 is sunday
        if (weekday === 7) {
            return 0;
        } else {
            return weekday;
        }
    }

    /** {@inheritDoc IDateWrapper.diff} */
    public diff(other: IDateWrapper, unit?: DurationUnit): Duration {
        return this.internalDate.diff(DateWrapper.getDateTime(other), unit);
    }

    /** {@inheritDoc IDateWrapper.isSame} */
    public isSame(other: IDateWrapper, unit?: DurationUnit): boolean {
        if (unit) {
            const otherDate = DateWrapper.getDateTime(other);
            return this.internalDate.hasSame(otherDate, unit as DateTimeUnit);
        } else {
            return this.fromEpoch() === other.fromEpoch();
        }
    }

    /** {@inheritDoc IDateWrapper.isBefore} */
    public isBefore(other: IDateWrapper): boolean {
        return this.fromEpoch() < other.fromEpoch();
    }

    /** {@inheritDoc IDateWrapper.isBeforeOrSame} */
    public isBeforeOrSame(other: IDateWrapper): boolean {
        return this.fromEpoch() <= other.fromEpoch();
    }

    /** {@inheritDoc IDateWrapper.isAfter} */
    public isAfter(other: IDateWrapper): boolean {
        return this.fromEpoch() > other.fromEpoch();
    }

    /** {@inheritDoc IDateWrapper.isAfterOrSame} */
    public isAfterOrSame(other: IDateWrapper): boolean {
        return this.fromEpoch() >= other.fromEpoch();
    }

    /** {@inheritDoc IDateWrapper.startOf} */
    public startOf(unit: DurationUnit): DateWrapper {
        const temp_date = this.internalDate.startOf(unit as DateTimeUnit);
        return new DateWrapper(temp_date);
    }

    /** {@inheritDoc IDateWrapper.toLocal} */
    public toLocal(): DateWrapper {
        return new DateWrapper(this.internalDate.toLocal());
    }

    /** {@inheritDoc IDateWrapper.add} */
    public add(duration: DurationLike): DateWrapper {
        const temp_date = this.internalDate.plus(duration);
        return new DateWrapper(temp_date);
    }

    /** {@inheritDoc IDateWrapper.subtract} */
    public subtract(duration: DurationLike): DateWrapper {
        const temp_date = this.internalDate.minus(duration);
        return new DateWrapper(temp_date);
    }

    /** {@inheritDoc IDateWrapper.offset} */
    public offset(): number {
        return this._internal_date.offset;
    }

    /**
     * Retrieves an immutable Luxon DateTime object set to the same time as the given IDateWrapper.
     * @param wrapper An IDateWrapper.
     * @returns An immutable Luxon DateTime object set to the same time as the given IDateWrapper.
     */
    private static getDateTime(wrapper: IDateWrapper): DateTime {
        if (wrapper instanceof DateWrapper) {
            return wrapper.internalDate;
        } else {
            return DateTime.fromMillis(wrapper.fromEpoch());
        }
    }
}
