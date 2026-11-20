import { Platform } from "react-native";

/**
 * v-elevation 1 (MD3): the same box-shadow on web, iOS, and Android 9+.
 * Earlier versions of Android do not support box-shadow and fall back to a fainter MD2 system elevation.
 */
export const elevation1 =
    Platform.OS === "android" && Platform.Version < 28
        ? { elevation: 2 }
        : {
              boxShadow:
                  "0px 1px 2px 0px rgba(0,0,0,0.3), 0px 1px 3px 1px rgba(0,0,0,0.15)",
          };
