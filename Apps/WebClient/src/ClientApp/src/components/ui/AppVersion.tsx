import Constants from "expo-constants";

import { cn } from "@/utils/cn";

import { AppText } from "./AppText";

interface Props {
    className?: string;
}

/** "Version major.minor.build", baked in from `version.json` by `app.config.ts`. */
export function AppVersion({ className }: Props) {
    const version = Constants.expoConfig?.version;
    if (!version) return null;
    return (
        <AppText
            testID="appVersion"
            variant="body-medium"
            className={cn("text-bc-grey", className)}
        >
            Version {version}
        </AppText>
    );
}
