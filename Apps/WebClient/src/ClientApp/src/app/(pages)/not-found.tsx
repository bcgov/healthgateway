import { AppText } from "@/components/ui";
import { PageError } from "@/views/errors/PageError";

/** Port of NotFoundView.vue */
export default function NotFound() {
    return (
        <PageError title="404" subtitle="Page Not Found" testID="not-found">
            <AppText className="text-center">
                The page you were looking for does not exist.
            </AppText>
        </PageError>
    );
}
