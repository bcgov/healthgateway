import { ProfileNotificationType } from "@/constants/profileNotifications";
import { InformationModalContent } from "@/models/informationModal";

export interface NotificationModalConfig {
    buttonText: string;
    content: InformationModalContent;
}
export interface NotificationPreferenceType {
    id: string;
    type: ProfileNotificationType;
    label: string;
    modal: NotificationModalConfig;
}
