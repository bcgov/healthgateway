/** Port of models/notification.ts */
export enum NotificationActionType {
    None = "None",
    InternalLink = "InternalLink",
    ExternalLink = "ExternalLink",
}

export interface Notification {
    id: string;
    categoryName: string;
    displayText: string;
    actionUrl: string;
    actionType: NotificationActionType;
    scheduledDateTimeUtc: string;
}
