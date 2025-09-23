export interface SnowplowWindow extends Window {
    snowplow(eventName: string): void;

    snowplow(eventName: string, data: SnowPlowEvent): void;
}

export interface SnowPlowEvent {
    schema: string;
    data: EventData;
}

export interface EventData {
    action: Action;
    text: Text;
    actor?: Actor;
    dataset?: Dataset;
    destination?: Destination | string;
    format?: Format;
    origin?: Origin;
    rating?: Rating;
    type?: Type;
    url?: string;
}

export const enum Action {
    Download = "Download",
    Load = "Load",
    Submit = "Submit",
    View = "View",
    Visit = "Visit",
}

export const enum Actor {
    Guardian = "Guardian",
    User = "User",
}

export const enum Dataset {
    BcCancer = "BC Cancer",
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

export const enum Destination {
    BcGovImmunizations = "BC Gov Immunizations",
    BcVaccineCard = "BC Vaccine Card",
    BookVaccine = "Book a Vaccine",
    HealthGatewayBeta = "Health Gateway Beta",
    ImmunizationRecommendationDialog = "Immunization Recommendations Dialog",
    OrganDonorRegistration = "Organ Donor Registration",
    OtherRecordSources = "Other Record Sources",
    PrimaryCare = "Primary Care",
    PublicHealthImmunizationSchedule = "Public Health Immunization Schedule",
    Timeline = "Timeline",
}

export const enum Format {
    Csv = "CSV",
    Image = "Image",
    Pdf = "PDF",
    XLSX = "XLSX",
}

export const enum Origin {
    Dependents = "Dependents",
    Exports = "Exports",
    Home = "Home",
    ImmunizationRecommendationDialog = "Immunization Recommendations Dialog",
}

export const enum Rating {
    Skip = "Skip",
    One = "1",
    Two = "2",
    Three = "3",
    Four = "4",
    Five = "5",
}

export const enum Text {
    AppRating = "App Rating",
    Data = "Data",
    Document = "Document",
    Export = "Export",
    ExternalLink = "External Link",
    InternalLink = "Internal Link",
    Page = "Page",
    Request = "Request",
}

export const enum Type {
    Covid19ProofOfVaccination = "COVID-19 Proof of Vaccination",
    OrganDonorRegistration = "Organ Donor Registration",
    PublicCovid19ProofOfVaccination = "Public COVID-19 Proof of Vaccination",
    Recall = "Recall",
    Result = "Result",
}
