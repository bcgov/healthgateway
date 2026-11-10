import { afterEach, describe, expect, it } from "@jest/globals";

import {
    AppErrorType,
    ErrorSourceType,
    ErrorType,
    TooManyRequestsLocation,
} from "@/constants/errorType";
import { ResultError } from "@/models/errors";
import { useErrorStore } from "@/stores/error.store";

describe("error store", () => {
    afterEach(() => {
        useErrorStore.setState({
            errors: [],
            appError: undefined,
            tooManyRequestsError: undefined,
            tooManyRequestsWarning: undefined,
        });
    });

    it("builds banner titles and simplified sources like the web client's error translator", () => {
        const s = useErrorStore.getState();
        s.addError(ErrorType.Retrieve, ErrorSourceType.Profile, "t1");
        s.addError(ErrorType.Retrieve, ErrorSourceType.User);
        s.addError(ErrorType.Create, ErrorSourceType.Note);
        s.addError(ErrorType.Update, ErrorSourceType.User);
        s.addError(ErrorType.Delete, ErrorSourceType.QuickLinks);
        s.addError(ErrorType.Download, ErrorSourceType.NoteReport);
        s.addError(ErrorType.Custom, ErrorSourceType.Profile);
        const errors = useErrorStore.getState().errors;
        expect(errors.map((e) => e.title)).toEqual([
            "Unable to retrieve profile",
            "Unable to retrieve users",
            "Unable to add note",
            "Unable to save changes to user",
            "Unable to remove quick links",
            "Unable to download note report",
            "An error has occurred",
        ]);
        // lower case, first space → non-breaking hyphen
        expect(errors[4].source).toBe("quick‑links");
        expect(errors[0]).toEqual({
            title: "Unable to retrieve profile",
            source: "profile",
            traceId: "t1",
        });
    });

    it("addCustomError keeps the given title", () => {
        useErrorStore
            .getState()
            .addCustomError(
                "Unable to close profile",
                ErrorSourceType.Profile,
                "t2"
            );
        expect(useErrorStore.getState().errors).toEqual([
            {
                title: "Unable to close profile",
                source: "profile",
                traceId: "t2",
            },
        ]);
    });

    it("too-many-requests messages are keyed by location (page by default)", () => {
        const s = useErrorStore.getState();
        s.setTooManyRequestsError();
        s.setTooManyRequestsWarning(TooManyRequestsLocation.VerifySmsModal);
        expect(useErrorStore.getState().tooManyRequestsError).toBe("page");
        expect(useErrorStore.getState().tooManyRequestsWarning).toBe(
            "verifySmsModal"
        );
        s.clearTooManyRequestsError();
        s.clearTooManyRequestsWarning();
        expect(useErrorStore.getState().tooManyRequestsError).toBeUndefined();
        expect(useErrorStore.getState().tooManyRequestsWarning).toBeUndefined();
    });

    it("turns a 429 into a page warning for retrieves and a page error otherwise", () => {
        const s = useErrorStore.getState();
        s.handleResultError(
            new ResultError("CE-HG", "x", 429),
            ErrorType.Retrieve,
            ErrorSourceType.User
        );
        expect(useErrorStore.getState().tooManyRequestsWarning).toBe("page");
        expect(useErrorStore.getState().errors).toHaveLength(0);
        s.handleResultError(
            new ResultError("CE-HG", "x", 429),
            ErrorType.Update,
            ErrorSourceType.User
        );
        expect(useErrorStore.getState().tooManyRequestsError).toBe("page");
    });

    it("adds a banner with the traceId for other ResultErrors and plain errors", () => {
        const s = useErrorStore.getState();
        s.handleResultError(
            new ResultError("CE-HG", "x", 500, "trace"),
            ErrorType.Update,
            ErrorSourceType.Profile
        );
        s.handleResultError(
            new Error("boom"),
            ErrorType.Create,
            ErrorSourceType.Comment
        );
        const errors = useErrorStore.getState().errors;
        expect(errors[0]).toEqual({
            title: "Unable to save changes to profile",
            source: "profile",
            traceId: "trace",
        });
        expect(errors[1].title).toBe("Unable to add comment");
        expect(errors[1].traceId).toBeUndefined();
    });

    it("clearErrors clears only the banner; the app error and too-many messages stay", () => {
        const s = useErrorStore.getState();
        s.setAppError(AppErrorType.TooManyRequests);
        s.setTooManyRequestsError();
        s.addError(ErrorType.Retrieve, ErrorSourceType.User);
        s.clearErrors();
        expect(useErrorStore.getState().errors).toHaveLength(0);
        expect(useErrorStore.getState().tooManyRequestsError).toBe("page");
        expect(useErrorStore.getState().appError).toBe(
            AppErrorType.TooManyRequests
        );
    });
});
