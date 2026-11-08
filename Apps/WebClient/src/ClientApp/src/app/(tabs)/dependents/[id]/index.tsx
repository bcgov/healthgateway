import { Redirect, useLocalSearchParams } from "expo-router";

import { toHref } from "@/navigation/href";

/** router/index.ts: `/dependents/:id` redirects to the dependent's timeline. */
export default function DependentRedirect() {
    const { id } = useLocalSearchParams<{ id: string }>();
    return <Redirect href={toHref(`/dependents/${id}/timeline`)} />;
}
