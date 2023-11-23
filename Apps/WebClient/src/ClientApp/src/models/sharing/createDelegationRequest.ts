import { DataSource } from "@/constants/dataSource";

/**
 * Represents the request delegate invitation model.
 */
export interface CreateDelegationRequest {
    /**
     * The friendly name of the delegate.
     */
    nickname?: string;

    /**
     * The email address to send the invitation.
     */
    email?: string;

    /**
     * The expiry date of access.
     */
    expiryDate?: string;

    /**
     * The list of data sources the delegate has access to.
     */
    dataSources?: DataSource[];
}
