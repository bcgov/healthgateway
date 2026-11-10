// Port of constants/validationRegEx.ts and utility/phoneUtil.ts
export const EmailRegex =
    /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;

export const PhoneNumberMaskedRegex = /^\D?(\d{3})\D?\D?(\d{3})\D?(\d{4})$/;

export function isValidEmail(email: string): boolean {
    return !email || email.length === 0 || EmailRegex.test(email);
}

export function stripPhoneMask(value: string): string {
    return value.replace(/\D/g, "");
}

/** Formats digits as (###) ###-#### progressively while typing. */
export function maskPhone(value: string): string {
    const d = stripPhoneMask(value).slice(0, 10);
    if (d.length === 0) return "";
    if (d.length < 4) return `(${d}`;
    if (d.length < 7) return `(${d.slice(0, 3)}) ${d.slice(3)}`;
    return `(${d.slice(0, 3)}) ${d.slice(3, 6)}-${d.slice(6)}`;
}

/** PhoneUtil.formatPhone: digits only, then (###) ###-#### when there are ten */
export function formatPhone(phone: string | null | undefined): string {
    return stripPhoneMask(phone ?? "").replace(
        /(\d{3})(\d{3})(\d{4})/,
        "($1) $2-$3"
    );
}

const PHNsigDigits = [2, 4, 8, 5, 10, 9, 7, 3];

/** Port of ValidationUtil.validatePhn */
export function validatePhn(phn: string, ignoreWhitespace = true): boolean {
    const value = ignoreWhitespace ? phn.replaceAll(/\s/g, "") : phn;
    if (!/^\d{10}$/.test(value) || !value.startsWith("9")) return false;
    let checksum = 0;
    for (let i = 1; i < 9; i++)
        checksum += (Number.parseInt(value[i]) * PHNsigDigits[i - 1]) % 11;
    checksum = 11 - (checksum % 11);
    return Number.parseInt(value[9]) === checksum;
}

/** maska phnMask "#### ### ###" */
export function maskPhn(value: string): string {
    const digits = value.replace(/\D/g, "").slice(0, 10);
    return [digits.slice(0, 4), digits.slice(4, 7), digits.slice(7)]
        .filter(Boolean)
        .join(" ");
}
