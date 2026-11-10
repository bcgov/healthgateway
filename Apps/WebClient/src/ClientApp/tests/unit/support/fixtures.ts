import { jest } from "@jest/globals";
import { act } from "react";

import type { ExternalConfiguration } from "@/models/configData";
import type { Encounter } from "@/models/encounter";
import type { ImmunizationResult } from "@/models/immunization";
import type {
    Covid19LaboratoryOrder,
    Covid19LaboratoryTest,
    LaboratoryOrder,
} from "@/models/laboratory";
import type { MedicationStatement } from "@/models/medication";
import {
    type BcCancerScreening,
    BcCancerScreeningType,
    type DiagnosticImagingExam,
    HealthDataType,
} from "@/models/patientData";
import type { RequestResult } from "@/models/requestResult";
import type { OidcTokenDetails, OidcUserInfo } from "@/models/user";
import type { UserProfile } from "@/models/userProfile";
import type { IAuthService } from "@/services/auth/authService";
import type { IHttpClient } from "@/services/http/httpClient";
import { useAuthStore } from "@/stores/auth.store";
import {
    useClinicalDocumentFileStore,
    useClinicalDocumentStore,
} from "@/stores/clinicalDocument.store";
import { useCommentStore } from "@/stores/comment.store";
import { useConfigStore } from "@/stores/config.store";
import { useCovid19TestResultStore } from "@/stores/covid19TestResult.store";
import { useDependentStore } from "@/stores/dependent.store";
import { useErrorStore } from "@/stores/error.store";
import { useHealthVisitStore } from "@/stores/healthVisit.store";
import { useHospitalVisitStore } from "@/stores/hospitalVisit.store";
import { useImmunizationStore } from "@/stores/immunization.store";
import { useLabResultStore } from "@/stores/labResult.store";
import { useLoadingStore } from "@/stores/loading.store";
import { useMedicationStore } from "@/stores/medication.store";
import { useNoteStore } from "@/stores/note.store";
import { useNotificationStore } from "@/stores/notification.store";
import { usePatientDataStore } from "@/stores/patientData.store";
import { useSpecialAuthorityRequestStore } from "@/stores/specialAuthorityRequest.store";
import { useUserStore } from "@/stores/user.store";

/** A dev-like `/configuration` payload with every feature on. */
export const configFixture: ExternalConfiguration = {
    openIdConnect: {
        authority: "https://sso.test/auth/realms/hg",
        clientId: "hg",
        scope: "openid",
        callbacks: null,
    },
    identityProviders: [
        {
            id: "bcsc",
            name: "BC Services Card",
            icon: "address-card",
            hint: "bcsc",
            disabled: false,
        },
    ],
    webClient: {
        // "None" keeps the app logger silent in tests (services.initialize applies it); see jest.setup.js
        logLevel: "None",
        timeouts: { idle: 300000, logoutRedirect: "10000", resendSMS: 1 },
        externalURLs: {},
        featureToggleConfiguration: {
            homepage: {
                showRecommendationsLink: true,
                showImmunizationRecordLink: true,
                otherRecordSources: {
                    enabled: true,
                    sources: [{ name: "Fraser", enabled: true }],
                },
            },
            notificationCentre: { enabled: true },
            timeline: { comment: true },
            datasets: [
                { name: "Immunization", enabled: true },
                { name: "Medication", enabled: false },
            ],
            dependents: { enabled: true, timelineEnabled: true, datasets: [] },
            services: {
                enabled: true,
                services: [{ name: "OrganDonorRegistration", enabled: true }],
            },
            profile: { notifications: { enabled: true, type: [] } },
        },
        hoursForDeletion: 720,
        minPatientAge: 12,
        maxDependentAge: 12,
        offlineMode: null,
        mobile: {
            authentication: {
                endpoint: "https://sso.test/auth/realms/hg-mobile",
                clientIdPrefix: "hg-mobile",
                identityProviders: [
                    {
                        id: "bcsc",
                        name: "BC Services Card",
                        icon: "address-card",
                        hint: "bcsc-mobile",
                        disabled: false,
                    },
                ],
            },
        },
    },
    serviceEndpoints: {
        GatewayApi: "https://api.test/gateway/",
        Patient: "https://api.test/patient/",
        Immunization: "https://api.test/immunization/",
        ClinicalDocument: "https://api.test/clinicaldocument/",
        Encounter: "https://api.test/encounter/",
        HospitalVisit: "https://api.test/hospitalvisit/",
        Laboratory: "https://api.test/laboratory/",
        Medication: "https://api.test/medication/",
        PatientData: "https://api.test/patientdata/",
        SpecialAuthority: "https://api.test/specialauthority/",
    },
};

export const bcscUserInfo: OidcUserInfo = {
    sub: "sub-1",
    hdid: "HDID1",
    idp: "BCSC",
    given_name: "Dr",
    family_name: "Gateway",
};

export const tokenDetails: OidcTokenDetails = {
    idToken: "id-token",
    accessToken: "access-token",
    refreshToken: "refresh-token",
    expired: false,
    refreshTokenTime: Math.floor(Date.now() / 1000) + 300,
    hdid: "HDID1",
};

export const makeProfile = (
    overrides: Partial<UserProfile> = {}
): UserProfile => ({
    hdid: "HDID1",
    acceptedTermsOfService: true,
    email: "dr@example.test",
    termsOfServiceId: "tos-1",
    isEmailVerified: true,
    smsNumber: "2505551234",
    isSMSNumberVerified: false,
    hasTermsOfServiceUpdated: false,
    lastLoginDateTimes: ["2026-10-29T10:00:00Z", "2026-10-28T10:00:00Z"],
    preferences: {},
    notificationSettings: [],
    ...overrides,
});

/** Records every request and answers from a queue (or throws from it). */
export class FakeHttp implements IHttpClient {
    calls: {
        method: string;
        url: string;
        body?: unknown;
        headers?: Record<string, string>;
    }[] = [];
    private queue: unknown[] = [];
    authorization?: string;

    reply(value: unknown) {
        this.queue.push(value);
        return this;
    }
    fail(error: Error) {
        this.queue.push(error);
        return this;
    }
    setAuthorizationHeader(token: string) {
        this.authorization = `Bearer ${token}`;
    }
    unsetAuthorizationHeader() {
        this.authorization = undefined;
    }
    private next<T>(
        method: string,
        url: string,
        body?: unknown,
        headers?: Record<string, string>
    ) {
        this.calls.push({ method, url, body, headers });
        const value = this.queue.shift();
        if (value instanceof Error) return Promise.reject(value);
        return Promise.resolve(value as T);
    }
    endSession() {}
    getPublic<T>(url: string) {
        return this.get<T>(url);
    }
    get<T>(url: string, headers?: Record<string, string>) {
        return this.next<T>("GET", url, undefined, headers);
    }
    post<T>(url: string, body?: unknown, headers?: Record<string, string>) {
        return this.next<T>("POST", url, body, headers);
    }
    put<T>(url: string, body?: unknown, headers?: Record<string, string>) {
        return this.next<T>("PUT", url, body, headers);
    }
    delete<T>(url: string, body?: unknown, headers?: Record<string, string>) {
        return this.next<T>("DELETE", url, body, headers);
    }
}

/** An auth service whose answers the test controls. */
export function fakeAuth(
    details?: OidcTokenDetails,
    info: OidcUserInfo = bcscUserInfo
): IAuthService {
    return {
        init: jest.fn(async () => undefined),
        signIn: jest.fn(async () => details ?? tokenDetails),
        signOut: jest.fn(async () => undefined),
        refreshToken: jest.fn(async () => false),
        getOidcTokenDetails: jest.fn(() => details),
        getOidcUserInfo: jest.fn(async () => info),
        clearState: jest.fn(() => undefined),
    } as unknown as IAuthService;
}

/**
 * Store changes that mounted components subscribe to must happen inside React's act(), or React warns that an
 * update "was not wrapped in act". Outside a component-test environment (plain unit tests) it just runs.
 */
export function updateStores(change: () => void) {
    const env = globalThis as { IS_REACT_ACT_ENVIRONMENT?: boolean };
    if (env.IS_REACT_ACT_ENVIRONMENT) act(change);
    else change();
}

/** Back to the state of a fresh app start. */
export function resetStores() {
    updateStores(() => {
        useAuthStore.setState({
            tokenDetails: undefined,
            isAuthenticated: false,
        });
        useUserStore.getState().clearUserData();
        useConfigStore.setState({
            config: undefined,
            status: "idle",
            errorMessage: undefined,
        });
        useErrorStore.getState().clearErrors();
        useErrorStore.setState({
            appError: undefined,
            tooManyRequestsError: undefined,
            tooManyRequestsWarning: undefined,
        });
        useLoadingStore.setState({ requests: {}, loaders: {} });
        useImmunizationStore.getState().reset();
        useMedicationStore.getState().reset();
        useLabResultStore.getState().reset();
        useCovid19TestResultStore.getState().reset();
        useHealthVisitStore.getState().reset();
        useNoteStore.getState().reset();
        useSpecialAuthorityRequestStore.getState().reset();
        useClinicalDocumentStore.getState().reset();
        useClinicalDocumentFileStore.getState().reset();
        useHospitalVisitStore.getState().reset();
        usePatientDataStore.getState().reset();
        useCommentStore.getState().reset();
        useDependentStore.getState().reset();
        useNotificationStore.getState().reset();
    });
}

/** Signed-in, fully registered BCSC user with the fixture configuration loaded. */
export function signInRegistered(profile: Partial<UserProfile> = {}) {
    updateStores(() => {
        useConfigStore.setState({ config: configFixture, status: "loaded" });
        useAuthStore.setState({ tokenDetails, isAuthenticated: true });
        const user = useUserStore.getState();
        user.setOidcUserInfo(bcscUserInfo);
        user.setProfile(makeProfile(profile));
        useUserStore.setState({ essentialDataLoaded: true });
    });
}

/** A JSON `fetch` Response. */
export const jsonResponse = (body: unknown, status = 200) =>
    new Response(body === undefined ? null : JSON.stringify(body), {
        status,
        headers: { "Content-Type": "application/json" },
    });

/** Immunization API result: two immunizations and three recommendations (one without vaccinations). */
export const immunizationResultFixture: ImmunizationResult = {
    immunizations: [
        {
            id: "i1",
            status: "Completed",
            dateOfImmunization: "2021-03-01T00:00:00",
            providerOrClinic: "Pharmacy A",
            immunization: {
                name: "COVID-19 mRNA",
                immunizationAgents: [
                    {
                        code: "a",
                        name: "COVID-19 mRNA",
                        lotNumber: "L1",
                        productName: "Comirnaty",
                    },
                ],
            },
        },
        {
            id: "i2",
            status: "Completed",
            dateOfImmunization: "2023-10-15T00:00:00",
            providerOrClinic: "Clinic B",
            immunization: {
                name: "Influenza",
                immunizationAgents: [],
            },
        },
    ],
    recommendations: [
        {
            recommendationSetId: "r1",
            recommendedVaccinations: "Tetanus",
            agentDueDate: "2025-01-01",
            status: "Eligible",
            targetDiseases: [],
            immunization: { name: "Td", immunizationAgents: [] },
        },
        {
            recommendationSetId: "r2",
            recommendedVaccinations: "",
            status: "Eligible",
            targetDiseases: [],
            immunization: { name: "x", immunizationAgents: [] },
        },
        {
            recommendationSetId: "r3",
            recommendedVaccinations: "Shingles",
            agentDueDate: "2027-06-30",
            status: "Eligible",
            targetDiseases: [],
            immunization: { name: "Zoster", immunizationAgents: [] },
        },
    ],
};

/** A dispensed prescription and a pharmacist assessment (MedicationStatement API shape). */
export const medicationStatementsFixture = (): MedicationStatement[] => [
    {
        prescriptionIdentifier: "rx1",
        dispensedDate: "2024-02-10T00:00:00",
        practitionerSurname: "SMITH",
        directions: "Take one daily",
        medicationSummary: {
            din: "2240684",
            brandName: "AMOXIL",
            genericName: "amoxicillin",
            quantity: 30,
            manufacturer: "GSK",
            form: "Capsule",
            isPin: false,
            pharmacyAssessmentTitle: "",
            title: "Amoxicillin",
            subtitle: "500 mg capsule",
            isPharmacistAssessment: false,
            activeIngredients: [
                {
                    activeIngredientCode: 1,
                    ingredient: "AMOXICILLIN",
                    strength: "500",
                    strengthUnit: "MG",
                },
            ],
        },
        dispensingPharmacy: {
            pharmacyId: "BC001",
            name: "Main St Pharmacy",
            addressLine1: "1 Main St",
            addressLine2: "Unit 2",
            city: "Victoria",
            province: "BC",
            phoneNumber: "2505551234",
            faxNumber: "2505559876",
        },
    },
    {
        prescriptionIdentifier: "pa1",
        dispensedDate: "2024-03-01T00:00:00",
        practitionerSurname: "",
        medicationSummary: {
            din: "66128150",
            brandName: "",
            genericName: "",
            isPin: true,
            pharmacyAssessmentTitle: "Pharmacist Assessment",
            prescriptionProvided: true,
            redirectedToHealthCareProvider: true,
            title: "Pharmacist Assessment",
            subtitle: "Minor ailment",
            isPharmacistAssessment: true,
            activeIngredients: [],
        },
        dispensingPharmacy: {
            name: "Corner Pharmacy",
            addressLine1: "9 Oak Ave",
            addressLine2: "",
            city: "Nanaimo",
            province: "BC",
        },
    },
];

/** A RequestResult envelope (ResultType.Success unless given). */
export const requestResult = <T>(
    resourcePayload: T,
    resultStatus = 1,
    resultError?: RequestResult<T>["resultError"]
): RequestResult<T> => ({
    resourcePayload,
    totalResultCount: Array.isArray(resourcePayload)
        ? resourcePayload.length
        : 1,
    pageIndex: 0,
    pageSize: 0,
    resultStatus,
    resultError,
});

/** A lab order with a report and three tests (out of range, in range, pending). */
export const laboratoryOrdersFixture = (): LaboratoryOrder[] => [
    {
        labPdfId: "lab1",
        reportingSource: "LifeLabs",
        reportId: "r1",
        collectionDateTime: "2024-05-01T09:30:00",
        timelineDateTime: "2024-05-01T09:30:00",
        commonName: "Lipid panel",
        orderingProvider: "Dr Jones",
        testStatus: "Final",
        orderStatus: "Completed",
        reportAvailable: true,
        laboratoryTests: [
            {
                batteryType: "HDL",
                obxId: "1",
                outOfRange: false,
                loinc: "a",
                testStatus: "Completed",
                result: "-",
            },
            {
                batteryType: "LDL",
                obxId: "2",
                outOfRange: true,
                loinc: "b",
                testStatus: "Completed",
                result: "Out of Range",
            },
            {
                batteryType: "TG",
                obxId: "3",
                outOfRange: false,
                loinc: "c",
                testStatus: "Pending",
                result: "Pending",
            },
        ],
    },
];

const covidTest = (
    overrides: Partial<Covid19LaboratoryTest>
): Covid19LaboratoryTest => ({
    id: "t1",
    testType: "NAT",
    outOfRange: false,
    collectedDateTime: "2021-01-05T10:00:00",
    testStatus: "Final",
    resultReady: true,
    resultDescription: ["Your test is negative.", "For more information see"],
    resultLink: "https://www.bccdc.ca/covid",
    labResultOutcome: "Negative",
    filteredLabResultOutcome: "Negative",
    receivedDateTime: "2021-01-05T12:00:00",
    resultDateTime: "2021-01-06T08:15:00",
    loinc: "x",
    loincName: "SARS-CoV-2 PCR",
    ...overrides,
});

/** COVID-19 orders: one with a single ready result and a report, one with two tests (newest first after sorting). */
export const covid19OrdersFixture = (): Covid19LaboratoryOrder[] => [
    {
        id: "c1",
        phn: "9735353315",
        orderingProviderIds: "",
        orderingProviders: "Dr A",
        reportingLab: "Provincial Lab",
        location: "VCH",
        labType: "PCR",
        messageDateTime: "2021-01-06T08:15:00",
        messageId: "m1",
        additionalData: "",
        reportAvailable: true,
        labResults: [covidTest({})],
    },
    {
        id: "c2",
        phn: "9735353315",
        orderingProviderIds: "",
        orderingProviders: "Dr B",
        reportingLab: "LifeLabs",
        location: "FHA",
        labType: "PCR",
        messageDateTime: "2021-03-02T08:15:00",
        messageId: "m2",
        additionalData: "",
        reportAvailable: false,
        labResults: [
            covidTest({
                id: "a",
                collectedDateTime: "2021-03-01T09:00:00",
                labResultOutcome: "Positive",
                resultDescription: [],
                resultLink: "",
            }),
            covidTest({
                id: "b",
                collectedDateTime: "2021-03-02T09:00:00",
                resultReady: false,
                testStatus: "Pending",
                resultDescription: [],
                resultLink: "",
            }),
        ],
    },
];

/** A recent health visit and one more than six years old. */
export const encountersFixture = (): Encounter[] => [
    {
        id: "e1",
        practitionerName: "Dr Who",
        specialtyDescription: "General Practice",
        encounterDate: "2025-06-01",
        clinic: { clinicId: "c1", name: "Downtown Clinic" },
    },
    {
        id: "e2",
        practitionerName: "",
        specialtyDescription: "Dermatology",
        encounterDate: "2018-01-15",
        clinic: { clinicId: "c2", name: "Skin Clinic" },
    },
];

/** An imaging exam with a report, and BC Cancer result and recall letters. */
export const patientDataFixture = (): (
    DiagnosticImagingExam | BcCancerScreening
)[] => [
    {
        id: "di1",
        type: HealthDataType.DiagnosticImagingExam,
        procedureDescription: "Chest X-ray",
        bodyPart: "Chest",
        modality: "X-Ray",
        organization: "VGH",
        healthAuthority: "VCH",
        examStatus: "Final",
        fileId: "file-di1",
        examDate: "2024-06-10",
        isUpdated: true,
    },
    {
        id: "bc1",
        type: HealthDataType.BcCancerScreening,
        eventType: BcCancerScreeningType.Result,
        programName: "Breast Cancer",
        fileId: "file-bc1",
        eventDateTime: "2024-07-01T09:00:00",
        resultDateTime: "2024-07-05T09:00:00",
    },
    {
        id: "bc2",
        type: HealthDataType.BcCancerScreening,
        eventType: BcCancerScreeningType.Recall,
        programName: "Cervical Cancer",
        fileId: "file-bc2",
        eventDateTime: "2024-08-01T09:00:00",
        resultDateTime: "",
    },
];
