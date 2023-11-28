import { DataSource } from "@/constants/dataSource";
import { DelegationStatus } from "@/constants/delegationStatus";

export interface Delegation {
    /**
     * The delegation id.
     */
    id: string;

    /**
     * The friendly name of the delegate.
     */
    nickname: string;

    /**
     * The delegation status.
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
