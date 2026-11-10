import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { CommentEntryType } from "@/constants/commentEntryType";
import { ResultError } from "@/models/errors";
import type { UserComment } from "@/models/userComment";
import { services } from "@/services/container";
import { UserCommentService } from "@/services/userComment.service";
import { entryCommentsGetter, useCommentStore } from "@/stores/comment.store";
import { useErrorStore } from "@/stores/error.store";
import {
    configFixture,
    fakeAuth,
    FakeHttp,
    requestResult,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";

const comment = (id: string, created: string, text = "c"): UserComment => ({
    id,
    userProfileId: "HDID1",
    parentEntryId: "SA-1",
    text,
    createdDateTime: created,
    version: 1,
    entryTypeCode: CommentEntryType.SpecialAuthorityRequest,
});
const commentsOn = {
    ...configFixture,
    webClient: {
        ...configFixture.webClient,
        featureToggleConfiguration: {
            ...configFixture.webClient.featureToggleConfiguration,
            timeline: { comment: true },
        },
    },
};

describe("UserCommentService (restUserCommentService port)", () => {
    it("profile comments, per-entry comments, create / update / delete on UserProfile/{hdid}/Comment", async () => {
        const c = comment("1", "2024-01-01T00:00:00");
        const http = new FakeHttp()
            .reply(requestResult({}))
            .reply(requestResult([]))
            .reply(requestResult(c))
            .reply(requestResult(c))
            .reply(requestResult(undefined));
        const svc = new UserCommentService(http, commentsOn);
        await svc.getCommentsForProfile("H1");
        await svc.getCommentsForEntry("H1", "SA-1");
        await svc.createComment("H1", c);
        await svc.updateComment("H1", c);
        await svc.deleteComment("H1", c);
        expect(http.calls.map((x) => `${x.method} ${x.url}`)).toEqual([
            "GET https://api.test/gateway/UserProfile/H1/Comment",
            "GET https://api.test/gateway/UserProfile/H1/Comment/Entry?parentEntryId=SA-1",
            "POST https://api.test/gateway/UserProfile/H1/Comment",
            "PUT https://api.test/gateway/UserProfile/H1/Comment",
            "DELETE https://api.test/gateway/UserProfile/H1/Comment",
        ]);
    });

    it("comments off: nothing is requested and create answers undefined", async () => {
        const http = new FakeHttp();
        const commentsOff = {
            ...configFixture,
            webClient: {
                ...configFixture.webClient,
                featureToggleConfiguration: {
                    ...configFixture.webClient.featureToggleConfiguration,
                    timeline: { comment: false },
                },
            },
        };
        const svc = new UserCommentService(http, commentsOff);
        expect((await svc.getCommentsForProfile("H1")).resourcePayload).toEqual(
            {}
        );
        await expect(
            svc.createComment("H1", comment("1", "x"))
        ).resolves.toBeUndefined();
        expect(http.calls).toHaveLength(0);
    });
});

describe("comment store (stores/comment.ts port)", () => {
    beforeEach(() => {
        services.initialize(commentsOn, fakeAuth());
        signInRegistered();
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("loads all comments by entry; creates in date order, updates and deletes in place", async () => {
        jest.spyOn(
            services.userComment,
            "getCommentsForProfile"
        ).mockResolvedValue(
            requestResult({ "SA-1": [comment("1", "2024-01-02T00:00:00")] })
        );
        jest.spyOn(services.userComment, "createComment").mockResolvedValue(
            comment("0", "2024-01-01T00:00:00")
        );
        jest.spyOn(services.userComment, "updateComment").mockResolvedValue(
            comment("1", "2024-01-02T00:00:00", "edited")
        );
        jest.spyOn(services.userComment, "deleteComment").mockResolvedValue(
            undefined
        );
        await useCommentStore.getState().retrieveComments("H1");
        await useCommentStore
            .getState()
            .createComment("H1", comment("x", "now"));
        expect(
            useCommentStore.getState().comments["SA-1"].map((c) => c.id)
        ).toEqual(["0", "1"]);
        await useCommentStore.getState().updateComment("H1", comment("1", "x"));
        expect(
            entryCommentsGetter(useCommentStore.getState().comments)(
                "SA-1"
            )?.[1].text
        ).toBe("edited");
        await useCommentStore.getState().deleteComment("H1", comment("0", "x"));
        expect(
            useCommentStore.getState().comments["SA-1"].map((c) => c.id)
        ).toEqual(["1"]);
        expect(
            entryCommentsGetter(useCommentStore.getState().comments)("other")
        ).toBeNull();
    });

    it("a 429 on save errors the page; other failures banner 'Unable to add comment'", async () => {
        jest.spyOn(services.userComment, "createComment")
            .mockRejectedValueOnce(new ResultError("E", "busy", 429))
            .mockRejectedValueOnce(new ResultError("E", "x", 500, "t"));
        await expect(
            useCommentStore.getState().createComment("H1", comment("x", "now"))
        ).rejects.toThrow();
        expect(useErrorStore.getState().tooManyRequestsError).toBe("page");
        await expect(
            useCommentStore.getState().createComment("H1", comment("x", "now"))
        ).rejects.toThrow();
        expect(useErrorStore.getState().errors[0].title).toBe(
            "Unable to add comment"
        );
    });
});
