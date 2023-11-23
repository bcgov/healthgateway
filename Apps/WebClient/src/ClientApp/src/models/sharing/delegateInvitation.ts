import { DataSource } from "@/constants/dataSource";
import { DelegationStatus } from "@/constants/delegateInvitationStatus";

export interface Delegation {
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
    status: DelegationStatus;

    /**
     * The expiry date of access.
     */
    expiryDate: string;

    /**
     * The list of data sources the delegate has access to.
     */
    dataSources?: DataSource[];

    /**
     * The sharing code to send to the invited delegate.
     */
    sharingCode: string;
}
