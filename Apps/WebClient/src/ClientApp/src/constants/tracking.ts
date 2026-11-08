// Port of Apps/WebClient/src/ClientApp/src/plugins/extensions.ts (Snowplow event vocabulary).
// Values must stay byte-identical to the web client so analytics line up across apps.

export const ACTION_SCHEMA = "iglu:ca.bc.gov.gateway/action/jsonschema/3-0-0";

export enum Action {
    Load = "Load",
    View = "View",
    Visit = "Visit",
    ButtonClick = "button_click",
    CardClick = "card_click",
    Download = "download",
    Email = "email",
    InternalLink = "internal_link",
    ExternalLink = "external_link",
    Submit = "submit",
}

export enum Actor {
    Guardian = "Guardian",
    User = "User",
}

export enum Dataset {
    BcCancerScreening = "BC Cancer Screening",
    ClinicalDocuments = "Clinical Documents",
    Covid19Tests = "COVID-19 Tests",
    HealthVisits = "Health Visits",
    HospitalVisits = "Hospital Visits",
    ImagingReports = "Imaging Reports",
    Immunizations = "Immunizations",
    LabResults = "Lab Results",
    Notes = "Notes",
    Medications = "Medications",
    SpecialAuthorityRequests = "Special Authority Requests",
}

export enum Destination {
    BookVaccine = "Book a Vaccine",
    HealthGatewayBeta = "Health Gateway Beta",
    HealthRecords = "Health Records",
    ImmunizationRecommendationDialog = "Immunization Recommendations Dialog",
    ImmunizationSchedule = "Immunization Schedule",
    ImmunizationsHealthLinkBC = "Immunizations - HealthLink BC",
    OrganDonorRegistration = "Organ Donor Registration",
    PrimaryCare = "Primary Care",
    AccessMyHealth = "AccessMyHealth",
    AddressChangeBC = "Address Change BC",
    Dependents = "Dependents",
    Download = "Download",
    Export = "Export",
    FraserHealthRecordRequest = "Fraser Health Record Request",
    HealthElife = "HealthElife",
    HealthGateway = "Health Gateway",
    HealthLinkBC = "HealthLink BC",
    HealthLinkBC811 = "HealthLink BC 8-1-1",
    HealthConnectRegistry = "HealthLink BC Health Connect Registry",
    Home = "Home",
    ImmunizationRecordBC = "Immunization Record BC",
    Login = "Login",
    MyHealth = "MyHealth",
    MyHealthKey = "myhealthkey",
    MyHealthPortal = "MyHealthPortal",
    Profile = "Profile",
    RecordSources = "Record Sources",
    Registration = "Registration",
    ScreeningContact = "Screening Contact",
    Services = "Services",
    SupportEmail = "Support Email",
    SupportGuide = "Support Guide",
    TermsOfService = "Terms of Service",
    Timeline = "Timeline",
    TransplantBC = "Transplant BC",
}

export enum Format {
    Csv = "CSV",
    Image = "Image",
    Pdf = "PDF",
    XLSX = "XLSX",
}

export enum Origin {
    AccessMyHealthDialog = "Access My Health Dialog",
    Breadcrumb = "Breadcrumb",
    Dependents = "Dependents",
    Download = "Download",
    Footer = "Footer",
    Home = "Home",
    ImmunizationRecommendationDialog = "Immunization Recommendations Dialog",
    Landing = "Landing",
    OtherRecordSources = "Other Record Sources",
    Profile = "Profile",
    ServicesPage = "Services Page",
    Timeline = "Timeline",
    VPPLoginPage = "VPP Login Page",
}

export enum Rating {
    Skip = "Skip",
    One = "1",
    Two = "2",
    Three = "3",
    Four = "4",
    Five = "5",
}

export enum Text {
    Data = "Data",
    Document = "Document",
    ExternalLink = "External Link",
    InternalLink = "Internal Link",
    Page = "Page",
    Request = "Request",
    AboutUs = "About Us",
    AccessMyHealth = "AccessMyHealth",
    AccessMyHealthDialogCancel = "AccessMyHealth Dialog Cancel",
    AccessMyHealthDialogUrl = "AccessMyHealth Dialog URL",
    AccessMyHealthDialogSignin = "AccessMyHealth Dialog Sign in",
    AccessMyHealthTile = "AccessMyHealth Tile",
    AccessMyHealthURL = "AccessMyHealth URL",
    AddDependent = "Add a Dependent",
    AddNote = "Add a Note",
    AppRating = "App Rating",
    BcVaccineSchedule = "BC Vaccine Schedule",
    Cancel = "Cancel",
    DeleteAccount = "Delete Account",
    Dependents = "Dependents",
    DependentRecords = "Dependent records",
    Download = "Download",
    DownloadClinicalDocument = "Download Clinical Document",
    DownloadDependentHistoricImmunizations = "Download Dependent Historic Immunizations",
    DownloadDependentRecommendedImmunizations = "Download Dependent Recommended Immunizations",
    DownloadImagingReport = "Download Imaging Report",
    DownloadLabResult = "Download Lab Result",
    EmailHealthGateway = "Email HealthGateway",
    Export = "Export",
    FilterHealthRecords = "Filter Health Records",
    FindDoctor = "Find family doctor",
    FindYourHealthRecords = "Find your health records",
    FraserHealthRequestTile = "Fraser Health Request Tile",
    FraserHealthRequestURL = "Fraser Health Request URL",
    HealthElifeTile = "HealthElife Tile",
    HealthElifeURL = "HealthElife URL",
    ContinueToHealthGateway = "Continue to Health Gateway",
    HealthGatewayLogo = "Health Gateway Logo",
    HealthRecords = "Health Records",
    HealthLinkBC = "HealthLink BC",
    HealthLinkBC811 = "HealthLink BC 8-1-1",
    HealthServices = "Health services",
    HomeBreadcrumb = "Home Breadcrumb",
    LearnMore = "Learn More",
    ViewRecommendedImmunizations = "View Recommended Immunizations",
    ImmunizationBannerDownload = "Immunization Banner Download",
    ImmunizationRecord = "Immunization Record",
    ImmunizationRecordDownload = "Immunization Record Download",
    ImmunizationUpdateForm = "Immunization Update Form",
    ImmunizationsHealthLinkBC = "Immunizations - HealthLink BC",
    Login = "Log in",
    LoginBCSC = "Log in with BCSC",
    Logout = "Logout",
    MyHealthTile = "MyHealth Tile",
    MyHealthURL = "MyHealth URL",
    MyHealthKeyTile = "myhealthkey Tile",
    MyHealthKeyURL = "myhealthkey URL",
    MyHealthPortalTile = "MyHealthPortal Tile",
    MyHealthPortalURL = "MyHealthPortal URL",
    OrganDonor = "Organ Donor",
    OtherRecordSources = "Other Record Sources",
    Register = "Register",
    RegisterDependent = "Register a Dependent",
    RegisterOrganDonorDecision = "Register Organ Donor Decision",
    DownloadOrganDonorDecision = "Download Organ Donor Decision",
    Profile = "Profile",
    RemoveDependent = "Remove a Dependent",
    QuickLink = "Quicklink",
    RecoverAccount = "Recover Account",
    RecordsManagement = "Records management",
    ReleaseNotes = "Release Notes",
    SendFeedback = "Send Feedback",
    Services = "Services",
    ScreeningNotificationsContactLink = "Screening Notification Contact link",
    ScreeningNotificationsLearnMore = "Screening Notifications Learn More",
    ScreeningEmailToggledOn = "Screening Email Toggled On",
    ScreeningEmailToggledOff = "Screening Email Toggled Off",
    ScreeningSmsToggledOn = "Screening SMS Toggled On",
    ScreeningSmsToggledOff = "Screening SMS Toggled Off",
    SignIn = "Sign in",
    SupportGuide = "Support Guide",
    TermsOfService = "Terms of Service",
    TimelineMissingImmunizations = "Timeline - Missing Immunizations Fill In Online Form",
    VerifyContactInformation = "Verify Contact Information",
    VerifyEmailAddress = "Verify Email Address",
    VerifyMobileNumber = "Verify Mobile Number",
    UpdateMailingAddress = "Update Mailing Address",
}

export enum Type {
    OrganDonorRegistration = "Organ Donor Registration",
    Recall = "Recall",
    Result = "Result",
    BcCancerScreening = "BC Cancer Screening",
    ClinicalDocuments = "Clinical Documents",
    Dependents = "Dependents",
    Filter = "Filter",
    Footer = "Footer",
    Header = "Header",
    HomeTile = "Home Tile",
    InfoBanner = "Info Banner",
    ImagingReports = "Imaging Reports",
    Immunizations = "Immunizations",
    LabResults = "Lab Results",
    Landing = "Landing",
    Login = "Login",
    Logout = "Logout",
    Notes = "Notes",
    Notifications = "Notifications",
    OrganDonor = "Organ Donor",
    Profile = "Profile",
    RecordSourceTile = "Record Source Tile",
    ServiceTile = "Service Tile",
    Sidebar = "Sidebar",
    VPPLogin = "VPP Login",
}

export enum EmailUrl {
    HealthGatewayEmail = "healthgateway@gov.bc.ca",
}

export enum TrackedExternalUrl {
    AboutUs = "https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway",
    AccessMyHealth = "https://www.accessmyhealth.ca",
    AddressChangeBC = "https://www.addresschange.gov.bc.ca",
    HealthGateway = "https://www.healthgateway.gov.bc.ca",
    OrganDonorRegistration = "https://www.transplant.bc.ca/organ-donation/register-as-an-organ-donor/register-your-decision",
    HealthConnectRegistry = "https://www.healthlinkbc.ca/find-care/health-connect-registry",
    ImmunizationRecordBC = "https://www.immunizationrecord.gov.bc.ca",
    ImmunizationSchedule = "https://www.healthlinkbc.ca/health-library/immunizations/schedules",
    Immunizations = "https://www.healthlinkbc.ca/health-library/immunizations",
    ReleaseNotes = "https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/release-notes",
    SupportGuide = "https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/guide",
    TermsOfService = "https://www.healthgateway.gov.bc.ca/termsOfService",
    YourHealthInformation = "https://www.healthlinkbc.ca/health-library/health-features/your-health-information",
}

export enum InternalUrl {
    Timeline = "./timeline",
    Dependents = "./dependents",
    Home = "./home",
    Login = "./login",
    OtherRecordSources = "./otherRecordSources",
    Profile = "./profile",
    Registration = "./Registration",
    Reports = "./reports",
    Services = "./services",
}

export interface EventData {
    action: Action;
    text: Text | string;
    actor?: Actor;
    dataset?: Dataset;
    destination?: Destination | string;
    format?: Format;
    origin?: Origin;
    rating?: Rating;
    type?: Type | Dataset;
    url?: string;
}

/** Other record sources tiles (ResourceLinkTileText / ResourceLinkUrlText / ResourceLinkDestination on web). */
export const ResourceLinkTileText: Record<string, Text> = {
    AccessMyHealth: Text.AccessMyHealthTile,
    MyHealth: Text.MyHealthTile,
    MyHealthPortal: Text.MyHealthPortalTile,
    HealthElife: Text.HealthElifeTile,
    MyHealthKey: Text.MyHealthKeyTile,
    FraserHealth: Text.FraserHealthRequestTile,
};
export const ResourceLinkUrlText: Record<string, Text> = {
    AccessMyHealth: Text.AccessMyHealthURL,
    MyHealth: Text.MyHealthURL,
    MyHealthPortal: Text.MyHealthPortalURL,
    HealthElife: Text.HealthElifeURL,
    MyHealthKey: Text.MyHealthKeyURL,
    FraserHealth: Text.FraserHealthRequestURL,
};
export const ResourceLinkDestination: Record<string, Destination> = {
    AccessMyHealth: Destination.AccessMyHealth,
    MyHealth: Destination.MyHealth,
    MyHealthPortal: Destination.MyHealthPortal,
    HealthElife: Destination.HealthElife,
    MyHealthKey: Destination.MyHealthKey,
    FraserHealth: Destination.FraserHealthRecordRequest,
};

/** Landing-page tiles (LandingAccessLinkText / LandingAccessLinkDestination on web). */
export const LandingAccessLinkText: Record<string, string> = {
    Call811: Text.HealthLinkBC811,
    DependentRecords: Text.DependentRecords,
    FindDoctor: Text.FindDoctor,
    HealthRecords: "Health records",
    RecordsManagement: Text.RecordsManagement,
    HealthLinkBC: Text.HealthLinkBC,
    HealthServices: Text.HealthServices,
};

export const LandingAccessLinkDestination: Record<string, string> = {
    Call811: Destination.HealthLinkBC811,
    DependentRecords: Destination.SupportGuide,
    FindDoctor: Destination.HealthConnectRegistry,
    HealthRecords: Destination.SupportGuide,
    RecordsManagement: Destination.SupportGuide,
    HealthLinkBC: Destination.HealthLinkBC,
    HealthServices: Destination.SupportGuide,
};

/** plugins/extensions.ts BcCancerPrograms / BcCancerDestination */
export const BcCancerPrograms = ["Breast", "Cervix", "Colon", "Lung"] as const;
export type BcCancerProgram = (typeof BcCancerPrograms)[number];
export const BcCancerDestination: Record<BcCancerProgram, string> = {
    Breast: "Breast Cancer Screening",
    Cervix: "Cervix Cancer Screening",
    Colon: "Colon Cancer Screening",
    Lung: "Lung Cancer Screening",
} as const;
export function isBcCancerProgram(program: string): program is BcCancerProgram {
    return (BcCancerPrograms as readonly string[]).includes(program);
}
