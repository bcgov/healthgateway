import { AppText } from "@/components/ui";

/** The landing page's heading and description, also shown by the lock screen. */
export function LandingIntro() {
    return (
        <>
            <AppText
                variant="h1"
                accessibilityRole="header"
                aria-level={1}
                className="mb-6"
            >
                Access your health information online
            </AppText>
            <AppText className="mb-6">
                Health Gateway provides secure and convenient access to your
                health records in British Columbia
            </AppText>
        </>
    );
}
