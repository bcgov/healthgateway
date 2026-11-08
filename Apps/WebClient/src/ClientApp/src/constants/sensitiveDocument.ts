import { Platform } from "react-native";

/** The download warning. Native says "shared device" where the web client says "public computer" (stated deviation). */
export const SENSITIVE_DOCUMENT_MESSAGE = `The file that you are downloading contains personal information. If you are on a ${
    Platform.OS === "web" ? "public computer" : "shared device"
}, please ensure that the file is deleted before you log off.`;
