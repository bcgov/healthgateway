import { Dictionary } from "@/models/baseTypes";
import { DateWrapper } from "@/models/dateWrapper";
import { UserPreference } from "@/models/userPreference";

export default abstract class PreferenceUtil {
    public static setDefaultValue(
        preferences: Dictionary<UserPreference>,
        type: string,
        value: string
    ): void {
        if (preferences[type] === undefined) {
            preferences[type] = {
                preference: type,
                value,
                version: 0,
                createdDateTime: new DateWrapper().toISO(),
            };
        }
    }
}
