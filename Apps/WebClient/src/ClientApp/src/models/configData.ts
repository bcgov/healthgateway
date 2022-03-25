import { RegistrationStatus } from "@/constants/registrationStatus";
import { Dictionary } from "@/models/baseTypes";
import { StringISODate } from "@/models/dateWrapper";

// A collection of configuration items for use by Health Gateway and approved applications.
export class ExternalConfiguration {
    // Gets or sets the OpenIdConnect configuration.
    public openIdConnect!: OpenIdConnectConfiguration;

    // Gets or sets the List of Identity providers.
    public identityProviders!: IdentityProviderConfiguration[];

    // Gets or sets the Health Gateway Webclient specific configuration.
    public webClient!: WebClientConfiguration;

    // Gets or sets the Service Endpoints.
    public serviceEndpoints!: Dictionary<string>;
}

// An object representing a configured Health Gateway IdentityProvider.
export interface IdentityProviderConfiguration {
    // Gets or sets the Id of the Identity Provider.
    id: string;
    // Gets or sets the name of the Identity Provider.
    name: string;
    // Gets or sets the Font Awesome Icon that we recommend to use.
    icon: string;
    // Gets or sets the Identity Provider hint.
    hint: string;
    // Gets or sets a value indicating whether this identity provider should be used.
    disabled: boolean;
}

// Configuration to be used by external clients for authentication.
export interface OpenIdConnectConfiguration {
    // Gets or sets the OpenIDConnect Authority.
    authority: string;
    // Gets or sets the OpenIDConnect Authority Url.
    uri: string;
    // Gets or sets the OpenIdConnect Client ID.
    clientId: string;
    // Gets or sets the OpenIDConnect realms.
    realms: string;
}

// Configuration data to be used by the Health Gateway Webclient.
export interface WebClientConfiguration {
    // Gets or sets the logging level used by the Webclient.
    logLevel: string;
    // Gets or sets the Webclient timeout values.
    timeouts: TimeOutsConfiguration;
    // Gets or sets the Webclient timeout values.
    registrationStatus: RegistrationStatus;
    // Gets or sets the ExternalURLs used by the Webclient.
    externalURLs: Dictionary<string>;
    // Gets or sets the Module state.
    modules: Dictionary<boolean>;
    // Gets or sets the hours for deletion.
    hoursForDeletion: number;
    // Gets or sets the minimum required patient age allowed for registration.
    minPatientAge: number;
    // Gets or sets the maximum age of a dependent
    maxDependentAge: number;
    // Gets or sets the client IP as reported by the server.
    clientIP?: string;
    // Gets or sets the sets the offline configuration.
    offlineMode?: OfflineModeConfiguration;
}

// Various timeout values used by the VUE WebClient application.
export interface TimeOutsConfiguration {
    // Gets or sets the idle time in seconds that the Webclient will use before it automatically logs the user out.
    idle: number;
    // Gets or sets the amount of time in seconds after which the user will be redirected from the logout page back to the home.
    logoutRedirect: string;
    // Gets or sets the aount of time in seconds that a user will have to wait between resending an SMS verification code.
    resendSMS: number;
}

// Configuration for offline mode.
export interface OfflineModeConfiguration {
    // The start datetime for offline mode.
    startDateTime: StringISODate;
    // The end datetime for offline mode.
    endDateTime?: StringISODate;
    //The message to display if in offline mode
    message: string;
    //The list of IPs that can connect during offline mode.
    whitelist: string[];
}
