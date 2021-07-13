import { DateWrapper } from "@/models/dateWrapper";

export default (date: Date): string => {
    return DateWrapper.format(date.toISOString(), "EEEE, dd MMMM");
};
