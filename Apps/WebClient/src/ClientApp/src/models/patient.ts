export interface Name {
    givenName?: string;
    surname?: string;
}

export interface Address {
    city: string;
    state: string;
    postalCode: string;
    country: string;
    streetLines: string[];
}

export interface Patient {
    hdid: string;
    personalHealthNumber: string;
    preferredName?: Name;
    commonName?: Name;
    legalName?: Name;
    birthdate?: string;
    physicalAddress?: Address | null;
    postalAddress?: Address | null;
}
