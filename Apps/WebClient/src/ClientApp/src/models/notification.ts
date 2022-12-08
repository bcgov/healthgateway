import { StringISODate } from "@/models/dateWrapper";

export default interface Notification {
    // Gets or sets the id.
    id: string;

    // Gets or sets the category name.
    categoryName: string;

    // Gets or sets the display text.
    displayText: string;

    // Gets or sets the action url.
    actionUrl: string;

    // Gets or sets the action type.
    actionType: NotificationActionType;

    // Gets or sets the scheduled datetime utc.
    scheduledDateTimeUtc: StringISODate;
}

export enum NotificationActionType {
    // No action.
    None = "None",

    // The action links to an internal resource.
    InternalLink = "InternalLink",

    // The action links to an external resource.
    ExternalLink = "ExternalLink",
}
