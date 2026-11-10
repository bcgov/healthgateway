// Mirrors Apps/WebClient/src/ClientApp/src/constants/path.ts
export const Path = {
    Root: "/",
    AcceptTermsOfService: "/acceptTermsOfService",
    Dependents: "/dependents",
    Home: "/home",
    IdirLoggedIn: "/idirLoggedIn",
    Login: "/login",
    LoginCallback: "/loginCallback",
    /** Native only: the login picker as a sheet, when there is more than one identity provider. */
    LoginProviders: "/loginProviders",
    Logout: "/logout",
    LogoutComplete: "/logoutComplete",
    NotFound: "/not-found",
    OtherRecordSources: "/otherRecordSources",
    PatientRetrievalError: "/patientRetrievalError",
    Profile: "/profile",
    Security: "/security",
    Registration: "/registration",
    Reports: "/reports",
    Services: "/services",
    TermsOfService: "/termsOfService",
    Timeline: "/timeline",
    Unauthorized: "/unauthorized",
    ValidateEmail: "/validateEmail",
    VppLogin: "/vppLogin",
} as const;

export type PathValue = (typeof Path)[keyof typeof Path];
