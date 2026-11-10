import type { UserPreference } from "@/models/userPreference";

/** Port of utility/preferenceUtil.ts: a preference the user has never saved still has a name to save it under. */
export function setDefaultPreference(
    preferences: Record<string, UserPreference>,
    type: string,
    value: string
): void {
    preferences[type] ??= {
        preference: type,
        value,
        version: 0,
        createdDateTime: new Date().toISOString(),
    };
}
