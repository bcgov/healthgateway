import { createContext, useContext } from "react";
import { View } from "react-native";

import { cn } from "@/utils/cn";

import { AppText } from "./AppText";

const TextClassContext = createContext<string | undefined>(undefined);

export interface BulletListProps {
    children: React.ReactNode;
    className?: string;
    /** applied to every item's bullet and text */
    textClassName?: string;
    testID?: string;
}

export interface BulletListItemProps {
    children: React.ReactNode;
    testID?: string;
}

/** Renders a bulleted list of `BulletListItem` components, as a ul on web. */
export function BulletList({
    children,
    className,
    textClassName,
    testID,
}: BulletListProps) {
    return (
        <TextClassContext.Provider value={textClassName}>
            <View role="list" className={className} testID={testID}>
                {children}
            </View>
        </TextClassContext.Provider>
    );
}

/** Renders a `BulletList` item, as an li on web, with wrapped lines aligned to the text rather than the bullet. */
export function BulletListItem({ children, testID }: BulletListItemProps) {
    const textClassName = useContext(TextClassContext);
    return (
        <View role="listitem" className="flex-row" testID={testID}>
            <AppText className={cn("w-6", textClassName)}>•</AppText>
            <AppText className={cn("flex-1", textClassName)}>
                {children}
            </AppText>
        </View>
    );
}
