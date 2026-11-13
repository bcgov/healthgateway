import { Platform } from "react-native";

// The store listings taken over from the old Health Gateway apps (bcgov/myhealthBC-iOS and myhealthBC-android): this
// app is published under their ID, ca.bc.gov.myhealth (app.config.ts).
export const APP_STORE_URL =
    "https://apps.apple.com/ca/app/health-gateway/id1590009068";
export const PLAY_STORE_URL =
    "https://play.google.com/store/apps/details?id=ca.bc.gov.myhealth";

/** This platform's store listing; the https links open the App Store or Google Play app. None on web. */
export const storeListingUrl = (): string | undefined =>
    Platform.select({ ios: APP_STORE_URL, android: PLAY_STORE_URL });
