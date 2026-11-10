import { View } from "react-native";

import { LabelValue, SectionHeading } from "@/components/common";
import { selectUserName, useUserStore } from "@/stores/user.store";

import { AddressSection } from "./AddressSection";
import { EmailSection } from "./EmailSection";
import { ManageAccountSection } from "./ManageAccountSection";
import { NotificationsSection } from "./NotificationsSection";
import { SmsSection } from "./SmsSection";

/** Port of ActiveUserProfileComponent.vue (login history is hidden on web too). */
export function ActiveProfile({
    onEmailUpdated,
}: {
    onEmailUpdated: (email: string) => void;
}) {
    const userName = useUserStore(selectUserName);
    const phn = useUserStore((s) => s.patient?.personalHealthNumber);
    return (
        <View>
            {/* the page's h1 is the title: its sections are h2 and their labels h3 (WCAG 1.3.1) */}
            <SectionHeading title="Personal Information" level={2} />
            <LabelValue
                label="Full Name"
                value={userName}
                testID="fullName"
                level={3}
            />
            <LabelValue
                label="Personal Health Number"
                value={phn}
                testID="PHN"
                level={3}
            />
            <SectionHeading
                title="Contact Information"
                includeDivider
                level={2}
            />
            <EmailSection onEmailUpdated={onEmailUpdated} />
            <SmsSection />
            <AddressSection />
            <NotificationsSection />
            <ManageAccountSection />
        </View>
    );
}
