// Port of Apps/WebClient/src/ClientApp/src/models/configData.ts (Health Gateway repo).
export type Dictionary<T> = Record<string, T>;

export interface ExternalConfiguration {
    openIdConnect: OpenIdConnectConfiguration;
    identityProviders: IdentityProviderConfiguration[];
    webClient: WebClientConfiguration;
    serviceEndpoints: Dictionary<string>;
}

/** `webClient.mobile`: what iOS and Android use in place of the web client's settings. */
export interface MobileConfiguration {
    /** major.minor.build. An older app must update to continue. Missing: no check. */
    minimumVersion?: string;
    timeouts?: {
        /** Idle time in milliseconds before the app logs out, or locks with device security on. Default 20 minutes. */
        idle?: number;
    };
    /** Required on iOS and Android: there is no fallback to `openIdConnect` (see `nativeOidcConfig`). */
    authentication?: MobileAuthenticationConfiguration;
}

export interface MobileAuthenticationConfiguration {
    /** The Keycloak realm the native apps sign in to. */
    endpoint?: string;
    /** The native app's client ID is this plus "-ios" or "-android". */
    clientIdPrefix?: string;
    /** The login picker's providers on iOS and Android, in place of the top-level `identityProviders`. */
    identityProviders?: IdentityProviderConfiguration[];
}

export interface IdentityProviderConfiguration {
    id: string;
    name: string;
    /** Font Awesome icon name recommended by the server (e.g. "address-card"). */
    icon: string;
    /** Keycloak kc_idp_hint value. */
    hint: string;
    disabled: boolean;
}

export interface OpenIdConnectConfiguration {
    authority: string;
    clientId: string;
    audience?: string;
    responseType?: string;
    scope: string;
    callbacks: Dictionary<string> | null;
}

export interface WebClientConfiguration {
    logLevel: string;
    timeouts: TimeOutsConfiguration;
    accessMyHealthUrl?: string;
    betaUrl?: string;
    classicUrl?: string;
    externalURLs: Dictionary<string>;
    featureToggleConfiguration: FeatureToggleConfiguration;
    hoursForDeletion: number;
    minPatientAge: number;
    maxDependentAge: number;
    emailVerificationExpirySeconds?: number;
    clientIP?: string;
    offlineMode?: OfflineModeConfiguration | null;
    /** iOS and Android only; web never reads it. */
    mobile?: MobileConfiguration | null;
}

export interface FeatureToggleConfiguration {
    homepage: HomepageSettings;
    notificationCentre: { enabled: boolean };
    timeline: { comment: boolean };
    datasets: NamedToggle[];
    dependents: DependentsSettings;
    services: ServicesSettings;
    profile: ProfileSettings;
}

export interface NamedToggle {
    name: string;
    enabled: boolean;
}

export interface HomepageSettings {
    showRecommendationsLink: boolean;
    showImmunizationRecordLink: boolean;
    otherRecordSources: { enabled: boolean; sources: NamedToggle[] };
}

export interface DependentsSettings {
    enabled: boolean;
    timelineEnabled: boolean;
    datasets: NamedToggle[];
}

export interface ServicesSettings {
    enabled: boolean;
    services: NamedToggle[];
}

export interface ProfileSettings {
    notifications: {
        enabled: boolean;
        type: ProfileNotificationTypeSettings[];
    };
}

export interface ProfileNotificationTypeSettings {
    name: string;
    enabled: boolean;
    preferences: { email: boolean; sms: boolean };
}

export interface TimeOutsConfiguration {
    /** Idle time in milliseconds before the client logs the user out. */
    idle: number;
    /** Milliseconds before the logout-complete page returns home (served as a string). */
    logoutRedirect: string | number;
    /** Minutes a user must wait between SMS verification resends. */
    resendSMS: number;
}

export interface OfflineModeConfiguration {
    startDateTime: string;
    endDateTime?: string;
    message: string;
    whitelist: string[];
}
