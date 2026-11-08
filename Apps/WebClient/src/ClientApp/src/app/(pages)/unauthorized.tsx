import { AppText } from "@/components/ui";
import { PageError } from "@/views/errors/PageError";

/** Port of UnauthorizedView.vue */
export default function Unauthorized() {
    return (
        <PageError title="401" subtitle="Unauthorized" testID="unauthorized">
            <AppText className="text-center">
                You do not have permission to view this page.
            </AppText>
        </PageError>
    );
}
