import { Redirect } from "expo-router";
import { Platform } from "react-native";

import { Path } from "@/constants/path";
import { LoginProvidersSheet } from "@/views/auth/LoginProvidersSheet";

/** Native: the login sheet, outside the page groups so the root stack presents it over the page that opened it. */
export default function LoginProvidersRoute() {
    // web has the login page's own list
    if (Platform.OS === "web") return <Redirect href={Path.Login} />;
    return <LoginProvidersSheet />;
}
