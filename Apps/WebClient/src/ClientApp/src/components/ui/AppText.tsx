import { Text, type TextProps } from "react-native";

import { cn } from "@/utils/cn";

type Variant =
    "body" | "body-medium" | "label" | "h1" | "h2" | "h3" | "title-large";

// Vuetify type scale used by the web client (size / line-height / letter-spacing)
const variantClass: Record<Variant, string> = {
    body: "font-sans text-body-large tracking-[0.5px] text-bc-black",
    "body-medium": "font-sans text-body-medium tracking-[0.25px] text-bc-black",
    label: "font-bold text-body-medium tracking-[0.25px] text-bc-black",
    h1: "font-bold text-headline-large text-primary",
    h2: "font-bold text-headline-medium text-primary",
    h3: "font-bold text-headline-small text-primary",
    "title-large": "font-sans text-[22px] leading-7 text-bc-black",
};

export interface AppTextProps extends TextProps {
    variant?: Variant;
    className?: string;
}

/** Text with the BC Sans font and the web client's type scale applied by default; `className` overrides win. */
export function AppText({
    variant = "body",
    className = "",
    ...rest
}: AppTextProps) {
    return <Text {...rest} className={cn(variantClass[variant], className)} />;
}
