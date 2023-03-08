import { DependentInformation } from "@/models/dependent";

export default abstract class DependentUtil {
    public static formatName(
        dependentInfo: DependentInformation | undefined
    ): string {
        const firstName = dependentInfo?.firstname;
        const lastInitial = dependentInfo?.lastname?.slice(0, 1);
        return [firstName, lastInitial].filter((s) => Boolean(s)).join(" ");
    }
}
