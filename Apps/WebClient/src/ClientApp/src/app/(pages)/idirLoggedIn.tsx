import { AppText } from "@/components/ui";
import { PageError } from "@/views/errors/PageError";

/** Port of IdirLoggedInView.vue */
export default function IdirLoggedIn() {
    return (
        <PageError title="403" subtitle="IDIR Login" testID="idirLoggedIn">
            <AppText className="text-center">
                You’ve logged in with your IDIR. Please log out and use your BC
                Services Card.
            </AppText>
        </PageError>
    );
}
