import { DateTime, DurationUnit, Duration, DurationObject } from "luxon";

/**
 * Typed representation of a ISO Date, time is not relevant.
 */
export type StringISODate = string;

/**
 * Class that wraps a library definition of a date time. Immutable to prevent bad date manipulation.
 * Centralizes date operations and modifications while abstracting library specific implementations.
 */
export class DateWrapper {
    /**
     * Value for reference purpouses only. Stores the raw value if initialized as a string, ISO datetime otherwise.
     */
    private readonly _raw_string_value: StringISODate;
    /**
     * Value for reference purpouses only. Stores the source of the date, i.e how it was constructed.
     */
    private readonly _date_source: string;
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
     * Creates an immutable DateWrapper object.
     * @param param type of object to base this object. If none passed creates sets the current date time to NOW.
     * @param isUtc True if the date passed is in UTC, false by default.
     */
    constructor(param?: StringISODate | DateWrapper | DateTime, isUtc = false) {
        if (param) {
            if (param instanceof DateWrapper) {
                this._date_source = "DateWrapper";
                this._raw_string_value = param.toISO();
                this._internal_date = param.internalDate;
            } else if (param instanceof DateTime) {
                this._date_source = "DateTime";
                this._raw_string_value = param.toISO();
                this._internal_date = param;
            } else {
                this._date_source = "string";
                this._raw_string_value = param;
                if (param.includes("z")) {
                    this._internal_date = DateTime.fromISO(param);
                } else if (isUtc) {
                    this._internal_date = DateTime.fromISO(param, {
                        zone: "utc",
                    }).setZone("America/Vancouver");
                } else {
                    this._internal_date = DateTime.fromISO(param, {
                        zone: "America/Vancouver",
                    });
                    // DST times at 00:00 can be ambigious. Coherse the internal representation to be the day.
                    // 3 Hours bypases the ambiguousness of Move forward and fallback
                    if (this._internal_date.isInDST) {
                        this._internal_date = this._internal_date.plus({
                            hour: 3,
                        });
                    }
                }
            }
        } else {
            this._date_source = "new empty Object";
            this._internal_date = DateTime.local();
            this._raw_string_value = this.toISO();
        }
    }

    /**
     * Creates a new DateWrapper from a day, month, year numerical parameters
     * @param year The year to be set
     * @param month The Month to be set
     * @param day The day to be set
     * @returns a new DateWrapper object
     */
    public static fromNumerical(
        year: number,
        month: number,
        day: number
    ): DateWrapper {
        return new DateWrapper(
            year.toString() +
                "-" +
                ("0" + month).slice(-2) +
                "-" +
                ("0" + day).slice(-2)
        );
    }

    /**
     * Formats the date using the given a set of tokens
     * See https://moment.github.io/luxon/docs/manual/formatting.html for the rules
     * @param formatString The tokens to format the string as.
     * @returns a string representation of the datetime with the given format.
     */
    public format(formatString: string): string {
        const formated = this.internalDate.toFormat(formatString);
        return formated;
    }

    /**
     * Formats the date to the ISO format (YYYY-MM-DDTHH:MM:SS:M-Z)
     * @returns the formated string representation
     */
    public toISO(): StringISODate {
        return this.internalDate.toISO();
    }

    /**
     * Formats the date to the ISO format without time (YYYY-MM-DD)
     * @returns the formated string representation
     */
    public toISODate(): StringISODate {
        return this.internalDate.toISODate();
    }

    /**
     * Converts to a JS Date object
     * @returns the Date object
     */
    public toJSDate(): Date {
        return this.internalDate.toJSDate();
    }

    /**
     * Gives the number of milliseconds since the Unix Epoch as a string.
     * @returns string representation of the milliseconds since the epoch
     */
    public fromEpoch(): string {
        return this.internalDate.valueOf().toString();
    }

    /**
     * Gets the year.
     * @returns Calendar year
     */
    public year(): number {
        return this.internalDate.year;
    }

    /**
     * Get the month (1-12).
     * @returns calendar month.
     */
    public month(): number {
        return this.internalDate.month;
    }

    /**
     * Get the days.
     * @returns Calendar day
     */
    public day(): number {
        return this.internalDate.day;
    }

    /**
     * Gets the milliseconds (not epoch)
     * @returns milliseconds from the second
     */
    public millisecond(): number {
        return this.internalDate.millisecond;
    }

    /**
     * Returns the weekday 0-6 (sunday is 0)
     * @returns day of the week (0-6)
     */
    public weekday(): number {
        const weekday = this.internalDate.weekday;
        // luxor weekdays are 1-7 where 7 is sunday
        if (weekday === 7) {
            return 0;
        } else {
            return weekday;
        }
    }

    /**
     * Difference between two Dates in milliseconds
     * @param other Other date to compare to
     * @param unit (optional) unit of comparision (year, hour, minute, second...)
     * @returns the difference in milliseconds
     */
    public diff(other: DateWrapper, unit?: DurationUnit): number {
        return this.internalDate.diff(other.internalDate, unit).milliseconds;
    }

    /**
     * Checks if two dates are the same. Can use a provided unit of time.
     * @param other Date to compare against
     * @param unit (optional) Unit of time to compare
     * @returns True if they are the same date, false otherwise
     */
    public isSame(other: DateWrapper, unit?: DurationUnit): boolean {
        if (unit) {
            return this.internalDate.hasSame(other.internalDate, unit);
        } else {
            return (
                this.internalDate.toMillis() === other.internalDate.toMillis()
            );
        }
    }

    /**
     * Checks if this date is before the parameter date
     * @param other Date to compare against
     * @returns True if this date is before passed date, false otherwise.
     */
    public isBefore(other: DateWrapper): boolean {
        return this.internalDate.toMillis() < other.internalDate.toMillis();
    }

    /**
     * Checks if this date is after the parameter date
     * @param other Date to compare against
     * @returns True if this date is after the passed date, false otherwise
     */
    public isAfter(other: DateWrapper): boolean {
        return this.internalDate.toMillis() > other.internalDate.toMillis();
    }

    /**
     * Gives the start of the date for the given unit.
     * @param unit the unit of time (day, month, year ...)
     * @returns a date with the given start.
     */
    public startOf(unit: DurationUnit) {
        const temp_date = this.internalDate.startOf(unit);
        return new DateWrapper(temp_date);
    }

    /**
     * Substracts a duration from this date.
     * @param duration The duration as a unit or object
     * @returns the calculated date
     */
    public subtract(unit: Duration | number | DurationObject): DateWrapper {
        const temp_date = this.internalDate.minus(unit);
        return new DateWrapper(temp_date);
    }

    /**
     * Adds a duration to this date.
     * @param duration The duration as a unit or object
     * @returns the calculated date
     */
    public add(duration: Duration | number | DurationObject): DateWrapper {
        const temp_date = this.internalDate.plus(duration);
        return new DateWrapper(temp_date);
    }

    /**
     * Checks if the date is in DST.
     * @returns True if the date is in DST, false otherwise.
     */
    public isInDST(): boolean {
        return this._internal_date.isInDST;
    }
}
