import { type Href, Link } from "expo-router";

import { cn } from "@/utils/cn";

import { AppText } from "./AppText";

interface Props {
    href: Href;
    children: React.ReactNode;
    onPress?: () => void;
    testID?: string;
    nativeID?: string;
    className?: string;
}

/**
 * The web client's `<router-link>`: an in-app link. Web renders `<a href>` with client-side navigation (Cypress
 * checks the `href`); native pushes the route. Nest inside an AppText to flow with a sentence.
 */
export function InternalLink({
    href,
    children,
    onPress,
    testID,
    nativeID,
    className,
}: Props) {
    return (
        <Link href={href} onPress={onPress} asChild>
            <AppText
                testID={testID}
                nativeID={nativeID}
                accessibilityRole="link"
                className={cn("text-link underline", className)}
            >
                {children}
            </AppText>
        </Link>
    );
}
