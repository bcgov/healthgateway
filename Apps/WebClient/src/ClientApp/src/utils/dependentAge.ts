import type { Dependent } from "@/models/dependent";
import { fromIsoDate, today } from "@/utils/dateUtil";

/** Older than the configured age: the guardian's access has ended. */
export const isDependentExpired = (
    dependent: Dependent,
    maxDependentAge: number
) =>
    today().diff(
        fromIsoDate(dependent.dependentInformation.dateOfBirth),
        "years"
    ).years > maxDependentAge;
