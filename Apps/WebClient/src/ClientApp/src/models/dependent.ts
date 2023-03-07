import { StringISODate } from "@/models/dateWrapper";

/**
 * Represents the dependent demographic information.
 */
export interface DependentInformation {
    /**
     * The dependent hdid.
     */
    hdid: string;

    /**
     * The dependent phn.
     */
    PHN: string;

    /**
     * The dependent first name.
     */
    firstname: string;

    /**
     * The dependent last name.
     */
    lastname: string;

    /**
     * The dependent birth date.
     */
    dateOfBirth: StringISODate;
}

/**
 * Model that reprensetnts a dependent relationship
 */
export interface Dependent {
    /**
     * The dependent information.
     */
    dependentInformation: DependentInformation;

    /**
     * The owner of the hdid.
     */
    ownerId: string;

    /**
     * The hdid which has delegated access to the owner Id.
     */
    delegateId: string;

    /**
     * The total number of users that have delegated access to this dependent.
     */
    totalDelegateCount: number;

    /**
     * Code that defines the reason for delegation.
     */
    reasonCode: number;

    /**
     * The record version.
     */
    version: number;
}
