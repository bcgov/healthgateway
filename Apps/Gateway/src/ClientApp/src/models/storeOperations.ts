import { DateWrapper } from "@/models/dateWrapper";

export enum LoadStatus {
    NONE,
    REQUESTED,
    ASYNC_REQUESTED,
    PARTIALLY_LOADED,
    LOADED,
    DEFERRED,
    PROTECTED,
    ERROR,
}

export enum OperationType {
    ADD,
    UPDATE,
    DELETE,
}

export class Operation {
    public readonly id: string;
    public readonly operationType: OperationType;
    public readonly date: DateWrapper;

    constructor(id: string, operationType: OperationType) {
        this.id = id;
        this.operationType = operationType;
        this.date = new DateWrapper();
    }
}
