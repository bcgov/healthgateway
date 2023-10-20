import { IDateWrapper } from "@/models/dateWrapper";

export default class DateSortUtility {
    /**
     * Sort comparison test to arrange dates in descending order.
     * @param aDate instance of IDateWrapper
     * @param bDate instance of IDateWrapper
     */
    static descending(aDate?: IDateWrapper, bDate?: IDateWrapper): number {
        const firstDateEmpty = aDate === undefined;
        const secondDateEmpty = bDate === undefined;

        if (firstDateEmpty && secondDateEmpty) {
            return 0;
        }

        if (firstDateEmpty) {
            return 1;
        }

        if (secondDateEmpty) {
            return -1;
        }

        if (aDate.isBefore(bDate)) {
            return 1;
        }

        return aDate.isAfter(bDate) ? -1 : 0;
    }

    /**
     * Sort comparison test to arrange dates in ascending order.
     * @param aDate instance of IDateWrapper
     * @param bDate instance of IDateWrapper
     */
    static ascending(aDate?: IDateWrapper, bDate?: IDateWrapper): number {
        return -1 * DateSortUtility.descending(aDate, bDate);
    }
}
