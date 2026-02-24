import { ProfileNotificationType } from "@/constants/profileNotifications";

export interface TooltipContent {
    text: string;
    linkText?: string;
    href?: string;
    suffix?: string;
}
export interface NotificationPreferenceType {
    id: string;
    type: ProfileNotificationType;
    label: string;
    tooltip: TooltipContent;
}
