export default abstract class DateUtil {
    public static changeDay(date: string, num: number) {
        let dt = new Date(date);
        return new Date(dt.setDate(dt.getDate() + num));
    }

    public static changeMonth(date: Date, num: number) {
        let dt = new Date(date);
        return new Date(dt.setMonth(dt.getMonth() + num));
    }

    public static getMonthFirstDate(date: Date) {
        // return first day of this month
        return new Date(date.getFullYear(), date.getMonth(), 1);
    }

    public static getMonthLastDate(date: Date) {
        // get last day of this month
        let dt = new Date(date.getFullYear(), date.getMonth() + 1, 1); // 1st day of next month
        return new Date(dt.setDate(dt.getDate() - 1)); // last day of this month
    }
}
