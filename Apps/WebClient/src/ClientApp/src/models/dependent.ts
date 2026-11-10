/** Port of models/dependent.ts and models/addDependentRequest.ts */
export interface DependentInformation {
    hdid: string;
    PHN: string;
    firstname: string;
    lastname: string;
    /** ISO date */
    dateOfBirth: string;
}

export interface Dependent {
    dependentInformation: DependentInformation;
    /** the dependent's hdid */
    ownerId: string;
    /** the hdid that has delegated access to the owner */
    delegateId: string;
    totalDelegateCount: number;
    reasonCode: number;
    version: number;
}

export interface AddDependentRequest {
    firstName: string;
    lastName: string;
    /** ISO date */
    dateOfBirth: string;
    PHN: string;
}

/** Port of utility/dependentUtil.ts: first name and last initial */
export function formatDependentName(
    dependentInfo: DependentInformation | undefined
): string {
    const firstName = dependentInfo?.firstname;
    const lastInitial = dependentInfo?.lastname?.slice(0, 1);
    return [firstName, lastInitial].filter(Boolean).join(" ");
}
