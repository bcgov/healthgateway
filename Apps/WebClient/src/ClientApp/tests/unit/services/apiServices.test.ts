import { describe, expect, it } from "@jest/globals";

import { CommunicationType } from "@/models/communication";
import { HttpError, ResultError } from "@/models/errors";
import { CommunicationService } from "@/services/communication.service";
import { ConfigService } from "@/services/config.service";
import { PatientService } from "@/services/patient.service";
import { UserFeedbackService } from "@/services/userFeedback.service";
import { UserProfileService } from "@/services/userProfile.service";
import { configFixture, FakeHttp, makeProfile } from "@/test/fixtures";

const BASE = "https://api.test/gateway/UserProfile";
const V = "api-version=2.0";
const JSON_HEADERS = { "Content-Type": "application/json; charset=utf-8" };

describe("UserProfileService (restUserProfileService port)", () => {
    const setup = () => {
        const http = new FakeHttp();
        return { http, svc: new UserProfileService(http, configFixture) };
    };

    it.each([
        [
            "getProfile",
            (s: UserProfileService) => s.getProfile("H"),
            "GET",
            `${BASE}/H?${V}`,
        ],
        [
            "getProfileForLogin",
            (s: UserProfileService) => s.getProfileForLogin("H"),
            "GET",
            `${BASE}/H/login?${V}`,
        ],
        [
            "closeAccount",
            (s: UserProfileService) => s.closeAccount("H"),
            "DELETE",
            `${BASE}/H?${V}`,
        ],
        [
            "recoverAccount",
            (s: UserProfileService) => s.recoverAccount("H"),
            "GET",
            `${BASE}/H/recover?${V}`,
        ],
        [
            "getTermsOfService",
            (s: UserProfileService) => s.getTermsOfService(),
            "GET",
            `${BASE}/termsofservice?${V}`,
        ],
        [
            "validateSms",
            (s: UserProfileService) => s.validateSms("H", "123456"),
            "GET",
            `${BASE}/H/sms/validate/123456?${V}`,
        ],
        [
            "validateAge",
            (s: UserProfileService) => s.validateAge("H"),
            "GET",
            `${BASE}/H/Validate?${V}`,
        ],
        [
            "validateEmail",
            (s: UserProfileService) => s.validateEmail("H", "key"),
            "GET",
            `${BASE}/H/email/validate/key?${V}`,
        ],
        [
            "isPhoneNumberValid",
            (s: UserProfileService) => s.isPhoneNumberValid("250 555 1234"),
            "GET",
            `${BASE}/IsValidPhoneNumber/250%20555%201234?${V}`,
        ],
    ])(
        "%s uses the web client's method and URL",
        async (_name, call, method, url) => {
            const { http, svc } = setup();
            http.reply(true);
            await call(svc);
            expect(http.calls[0].method).toBe(method);
            expect(http.calls[0].url).toBe(url);
        }
    );

    it("sends email, SMS number and accepted terms as JSON strings with the JSON content type", async () => {
        const { http, svc } = setup();
        http.reply(undefined).reply(undefined).reply(undefined);
        await svc.updateEmail("H", "dr@example.test");
        await svc.updateSmsNumber("H", "2505551234");
        await svc.updateAcceptedTerms("H", "tos-2");
        expect(
            http.calls.map((c) => [c.method, c.url, c.body, c.headers])
        ).toEqual([
            ["PUT", `${BASE}/H/email?${V}`, '"dr@example.test"', JSON_HEADERS],
            ["PUT", `${BASE}/H/sms?${V}`, '"2505551234"', JSON_HEADERS],
            ["PUT", `${BASE}/H/acceptedterms?${V}`, '"tos-2"', JSON_HEADERS],
        ]);
    });

    it("creates a profile at the profile's hdid and posts/puts preferences", async () => {
        const { http, svc } = setup();
        const profile = makeProfile();
        const pref = {
            preference: "quickLinks",
            value: "[]",
            version: 0,
            createdDateTime: "now",
        };
        http.reply(profile).reply(pref).reply(pref).reply(undefined);
        await svc.createProfile({ profile });
        await svc.createUserPreference("H", pref);
        await svc.updateUserPreference("H", pref);
        await svc.updateNotificationSettings("H", {
            type: "BcCancerScreening",
            emailEnabled: true,
        });
        expect(http.calls.map((c) => [c.method, c.url])).toEqual([
            ["POST", `${BASE}/HDID1?${V}`],
            ["POST", `${BASE}/H/preference?${V}`],
            ["PUT", `${BASE}/H/preference?${V}`],
            ["PUT", `${BASE}/H/notificationsettings?${V}`],
        ]);
        expect(http.calls[0].body).toEqual({ profile });
        expect(http.calls[3].headers).toEqual(JSON_HEADERS);
    });

    it("translates failures into ResultError ClientApp-CI-HGU keeping status and traceId", async () => {
        const { http, svc } = setup();
        http.fail(new HttpError("Conflict", 409, "t-9"));
        const error = (await svc
            .updateEmail("H", "x")
            .catch((e: unknown) => e)) as ResultError;
        expect(error).toBeInstanceOf(ResultError);
        expect(error.errorCode).toBe("ClientApp-CI-HGU");
        expect(error.statusCode).toBe(409);
        expect(error.traceId).toBe("t-9");
    });
});

describe("other API services", () => {
    it("PatientService calls the Patient endpoint with api-version 2.0 and maps errors to ClientApp-CI-PAT", async () => {
        const http = new FakeHttp()
            .reply({ hdid: "H" })
            .fail(new HttpError("x", 429));
        const svc = new PatientService(http, configFixture);
        await svc.getPatient("H");
        expect(http.calls[0]).toMatchObject({
            method: "GET",
            url: "https://api.test/patient/Patient/H?api-version=2.0",
        });
        const error = (await svc
            .getPatient("H")
            .catch((e: unknown) => e)) as ResultError;
        expect(error.errorCode).toBe("ClientApp-CI-PAT");
        expect(error.statusCode).toBe(429);
    });

    it("CommunicationService asks for the active communication of a type", async () => {
        const http = new FakeHttp().reply({
            resourcePayload: null,
            resultStatus: 1,
        });
        await new CommunicationService(http, configFixture).getActive(
            CommunicationType.InApp
        );
        expect(http.calls[0].url).toBe(
            "https://api.test/gateway/Communication/2"
        );
    });

    it("ConfigService fetches the given configuration URL", async () => {
        const http = new FakeHttp().reply(configFixture);
        await expect(
            new ConfigService(http).getConfiguration(
                "https://cfg.test/configuration"
            )
        ).resolves.toBe(configFixture);
        expect(http.calls[0]).toMatchObject({
            method: "GET",
            url: "https://cfg.test/configuration",
        });
    });

    it("UserFeedbackService posts feedback per hdid and ratings to /Rating", async () => {
        const http = new FakeHttp().reply(undefined).reply(undefined);
        const svc = new UserFeedbackService(http, configFixture);
        await svc.submitFeedback("H", {
            comment: "Nice",
            isSatisfied: true,
        });
        await svc.submitRating({ ratingValue: 5, skip: false });
        expect(http.calls.map((c) => [c.method, c.url])).toEqual([
            ["POST", "https://api.test/gateway/UserFeedback/H"],
            ["POST", "https://api.test/gateway/UserFeedback/Rating"],
        ]);
    });
});

describe("service failure translation", () => {
    it.each([
        [
            "CommunicationService",
            () =>
                new CommunicationService(
                    new FakeHttp().fail(new HttpError("x", 500)),
                    configFixture
                ).getActive(CommunicationType.Banner),
        ],
        [
            "UserFeedbackService.submitFeedback",
            () =>
                new UserFeedbackService(
                    new FakeHttp().fail(new HttpError("x", 500)),
                    configFixture
                ).submitFeedback("H", {} as never),
        ],
        [
            "UserFeedbackService.submitRating",
            () =>
                new UserFeedbackService(
                    new FakeHttp().fail(new HttpError("x", 500)),
                    configFixture
                ).submitRating({} as never),
        ],
    ])("%s rejects with ResultError ClientApp-CI-HGU", async (_n, call) => {
        const e = (await call().catch((x: unknown) => x)) as ResultError;
        expect(e).toBeInstanceOf(ResultError);
        expect(e.errorCode).toBe("ClientApp-CI-HGU");
        expect(e.statusCode).toBe(500);
    });
});
