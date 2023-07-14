import { DateWrapper } from "@/models/dateWrapper";

export default class DateWrapperSortUtility {
    /**
     * Sort comparison test to determine dates in descending order.
     * @param aDate instance of DateWrapper
     *  @param bDate instance of DateWrapper
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
        return -1 * DateWrapperSortUtility.descending(aDate, bDate);
    }

    /**
     * Sort comparison test to determine dates in descending order. Using the DateWrapper class to handle the date comparison.
     * @param aDate valid date string
     * @param bDate valid date string
     */
    static descendingByString(aDate: string, bDate: string): number {
        return DateWrapperSortUtility.descending(
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
        const firstDate = new DateWrapper(aDate);
        const secondDate = new DateWrapper(bDate);

        return -1 * DateWrapperSortUtility.descending(firstDate, secondDate);
    }

    /**
     * Sort comparison test to determine dates in descending order. Using the DateWrapper class to handle the date comparison.
     * undefined dates are sorted to the end of the list.
     * @param aDate
     * @param bDate
     */
    static descendingByOptionalString(aDate?: string, bDate?: string): number {
        if (aDate && bDate) {
            return DateWrapperSortUtility.descendingByString(aDate, bDate);
        }

        const firstDateEmpty = aDate === undefined;
        const secondDateEmpty = bDate === undefined;

        if (firstDateEmpty && secondDateEmpty) {
            return 0;
        }

        return firstDateEmpty ? 1 : -1;
    }
}
