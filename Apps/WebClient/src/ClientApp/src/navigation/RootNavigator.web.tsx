import { Slot } from "expo-router";

/** Web: the browser's history is the back, so the root stays a Slot (native: `RootNavigator.tsx`). */
export function RootNavigator() {
    return <Slot />;
}
