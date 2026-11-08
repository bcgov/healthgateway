import Constants from "expo-constants";

import { formatAppVersion } from "@/utils/appVersion";
import { cn } from "@/utils/cn";

import { AppText } from "./AppText";

interface Props {
    className?: string;
}

/**
 * "Version major.minor (build)" on every platform, plus " - Custom" on a pipeline build not from dev: baked in from
 * `version.json` by `app.config.ts`. Its pipeline run (and image tag) is major.minor.build, plus -custom.
 */
export function AppVersion({ className }: Props) {
    const version = Constants.expoConfig?.version;
    if (!version) return null;
    const label = (Constants.expoConfig?.extra as { versionLabel?: unknown })
        ?.versionLabel;
    return (
        <AppText
            testID="appVersion"
            variant="body-medium"
            className={cn("text-bc-grey", className)}
        >
            Version{" "}
            {formatAppVersion(
                version,
                typeof label === "string" ? label : undefined
            )}
        </AppText>
    );
}
