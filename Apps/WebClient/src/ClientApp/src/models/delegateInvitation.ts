import { DataSource } from "@/constants/dataSource";

/**
 * Represents the delegate invitation model
 */
export interface DelegateInvitation {
    /**
     * The delegate invitation id.
     */
    id: string;

    /**
     * The friendly name of the delegate.
     */
    nickname: string;

    /**
     * The delegate invitation status.
     */
    status: string;

    /**
     * The email address to send the invitation.
     */
    email: string;

    /**
     * The expiry date of access.
     */
    expiryDate: string;

    /**
     * The list of data sources the delegate has access to.
     */
    dataSources: DataSource[];
}
