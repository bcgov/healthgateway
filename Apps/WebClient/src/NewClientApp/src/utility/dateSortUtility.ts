import { DateWrapper } from "@/models/dateWrapper";

export default class DateSortUtility {
    /**
     * Sort comparison test to determine dates in descending order.
     * @param aDate instance of DateWrapper
     * @param bDate instance of DateWrapper
     */
    static descending(aDate: DateWrapper, bDate: DateWrapper): number {
        if (aDate.isBefore(bDate)) {
            return 1;
        }

        return aDate.isAfter(bDate) ? -1 : 0;
    }

    /**
     * Sort comparison test to determine dates in ascending order.
     * @param aDate instance of DateWrapper
     * @param bDate instance of DateWrapper
     */
    static ascending(aDate: DateWrapper, bDate: DateWrapper): number {
        return -1 * DateSortUtility.descending(aDate, bDate);
    }

    /**
     * Sort comparison test to determine dates in descending order. Using the DateWrapper class to handle the date comparison.
     * @param aDate valid date string
     * @param bDate valid date string
     */
    static descendingByString(aDate: string, bDate: string): number {
        return DateSortUtility.descending(
            new DateWrapper(aDate),
            new DateWrapper(bDate)
        );
    }

    /**
     * Sort comparison test to determine dates in ascending order. Using the DateWrapper class to handle the date comparison.
     * @param aDate valid date string
     * @param bDate valid date string
     */
    static ascendingByString(aDate: string, bDate: string): number {
        return -1 * DateSortUtility.descendingByString(aDate, bDate);
    }

    /**
     * Sort comparison test to determine dates in descending order. Using the DateWrapper class to handle the date comparison.
     * undefined dates are sorted to the end of the list.
     * @param aDate
     * @param bDate
     */
    static descendingByOptionalString(aDate?: string, bDate?: string): number {
        if (aDate && bDate) {
            return DateSortUtility.descendingByString(aDate, bDate);
        }

        const firstDateEmpty = aDate === undefined;
        const secondDateEmpty = bDate === undefined;

        if (firstDateEmpty && secondDateEmpty) {
            return 0;
        }

        return firstDateEmpty ? 1 : -1;
    }
}
