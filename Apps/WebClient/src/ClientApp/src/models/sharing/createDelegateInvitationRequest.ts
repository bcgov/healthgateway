import { DataSource } from "@/constants/dataSource";
import { DelegateInvitationStatus } from "@/constants/delegateInvitationStatus";

/**
 * Represents the request delegate invitation model.
 */
export interface CreateDelegateInvitationRequest {
    /**
     * The friendly name of the delegate.
     */
    nickname?: string;

    /**
     * The delegate invitation status.
     */
    status?: DelegateInvitationStatus;

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
