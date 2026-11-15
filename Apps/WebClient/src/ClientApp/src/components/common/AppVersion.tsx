import Constants from "expo-constants";

import { formatAppVersion } from "@/utils/appVersion";
import { cn } from "@/utils/cn";

import { AppText } from "./AppText";

interface Props {
    className?: string;
}

/**
 * "v<major.minor.fix>.<build>" on every platform (v3.0.0.0057), plus " - Custom" on a pipeline build not from dev: the
 * WebClient pipeline's run name. Baked in from `version.json` by `app.config.ts`: `version` and `extra.build`.
 */
/** The installed app's version as `AppVersion` shows it, or undefined when the build carries none. */
export function appVersionText(): string | undefined {
    const version = Constants.expoConfig?.version;
    if (!version) return undefined;
    const extra = Constants.expoConfig?.extra as
        { versionLabel?: unknown; build?: unknown } | undefined;
    const label = extra?.versionLabel;
    const build = extra?.build;
    return formatAppVersion(
        version,
        typeof build === "number" ? build : undefined,
        typeof label === "string" ? label : undefined
    );
}

export function AppVersion({ className }: Props) {
    const text = appVersionText();
    if (!text) return null;
    return (
        <AppText
            testID="appVersion"
            variant="body-medium"
            className={cn("text-bc-grey", className)}
        >
            {text}
        </AppText>
    );
}
