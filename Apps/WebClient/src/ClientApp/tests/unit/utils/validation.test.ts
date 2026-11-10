import { describe, expect, it } from "@jest/globals";

import {
    isValidEmail,
    maskPhn,
    maskPhone,
    PhoneNumberMaskedRegex,
    stripPhoneMask,
    validatePhn,
} from "@/utils/validation";

describe("isValidEmail (validationRegEx port)", () => {
    it.each([
        "",
        "dr@example.test",
        "first.last@sub.example.ca",
        '"quoted"@example.test',
    ])("accepts %s", (v) => expect(isValidEmail(v)).toBe(true));
    it.each([
        "dr",
        "dr@",
        "@example.test",
        "dr@example",
        "dr @example.test",
        "dr@exa mple.test",
    ])("rejects %s", (v) => expect(isValidEmail(v)).toBe(false));
});

describe("phone mask (phoneUtil port)", () => {
    it("formats progressively as (###) ###-####", () => {
        expect(maskPhone("")).toBe("");
        expect(maskPhone("2")).toBe("(2");
        expect(maskPhone("250")).toBe("(250");
        expect(maskPhone("2505")).toBe("(250) 5");
        expect(maskPhone("250555")).toBe("(250) 555");
        expect(maskPhone("2505551")).toBe("(250) 555-1");
        expect(maskPhone("2505551234")).toBe("(250) 555-1234");
    });

    it("ignores non-digits and extra digits", () => {
        expect(maskPhone("(250) 555-1234 ext 9")).toBe("(250) 555-1234");
        expect(stripPhoneMask("(250) 555-1234")).toBe("2505551234");
    });

    it("the masked regex accepts ten digits with or without separators", () => {
        expect(PhoneNumberMaskedRegex.test("2505551234")).toBe(true);
        expect(PhoneNumberMaskedRegex.test("(250) 555-1234")).toBe(true);
        expect(PhoneNumberMaskedRegex.test("250555123")).toBe(false);
    });
});

describe("PHN (ValidationUtil.validatePhn, phnMask)", () => {
    it("accepts valid PHNs, with or without spaces", () => {
        expect(validatePhn("9874307168")).toBe(true);
        expect(validatePhn("9874 307 168")).toBe(true);
    });

    it("rejects a wrong check digit, length or first digit", () => {
        expect(validatePhn("9874307169")).toBe(false);
        expect(validatePhn("987430716")).toBe(false);
        expect(validatePhn("1874307168")).toBe(false);
        expect(validatePhn("9874 307 168", false)).toBe(false);
    });

    it("masks progressively as #### ### ###", () => {
        expect(maskPhn("9")).toBe("9");
        expect(maskPhn("98743")).toBe("9874 3");
        expect(maskPhn("98743071")).toBe("9874 307 1");
        expect(maskPhn("9874307168999")).toBe("9874 307 168");
        expect(maskPhn("98a74")).toBe("9874");
    });
});
