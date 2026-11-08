import { describe, expect, it } from "@jest/globals";

import { HttpError, ResultError } from "@/models/errors";
import { UserNoteService } from "@/services/userNote.service";
import { useConfigStore } from "@/stores/config.store";
import { configFixture, FakeHttp, requestResult } from "@/test/fixtures";

const note = {
    id: "n1",
    hdId: "H1",
    title: "T",
    text: "x",
    journalDate: "2026-10-01",
    version: 1,
};

describe("UserNoteService (restUserNoteService port)", () => {
    it("CRUD on GatewayApi Note/{hdid}; delete sends the note as a JSON string", async () => {
        useConfigStore.setState({
            config: {
                ...configFixture,
                webClient: {
                    ...configFixture.webClient,
                    featureToggleConfiguration: {
                        ...configFixture.webClient.featureToggleConfiguration,
                        datasets: [{ name: "Note", enabled: true }],
                    },
                },
            },
            status: "loaded",
        });
        const http = new FakeHttp()
            .reply(requestResult([note]))
            .reply(requestResult(note))
            .reply(requestResult(note))
            .reply(requestResult(undefined));
        const svc = new UserNoteService(http, configFixture);
        await svc.getNotes("H1");
        await expect(svc.createNote("H1", note)).resolves.toEqual(note);
        await expect(svc.updateNote("H1", note)).resolves.toEqual(note);
        await svc.deleteNote("H1", note);
        expect(http.calls.map((c) => `${c.method} ${c.url}`)).toEqual([
            "GET https://api.test/gateway/Note/H1",
            "POST https://api.test/gateway/Note/H1",
            "PUT https://api.test/gateway/Note/H1",
            "DELETE https://api.test/gateway/Note/H1",
        ]);
        expect(http.calls[3].body).toBe(JSON.stringify(note));
        expect(http.calls[3].headers).toEqual({
            "Content-Type": "application/json; charset=utf-8",
        });
    });

    it("an error result is thrown as a ResultError; transport failures are ClientApp-CI-HGU", async () => {
        const http = new FakeHttp()
            .reply(
                requestResult(note, 0, {
                    errorCode: "E",
                    traceId: "t",
                    resultMessage: "nope",
                })
            )
            .fail(new HttpError("down", 503));
        const svc = new UserNoteService(http, configFixture);
        await expect(svc.createNote("H1", note)).rejects.toBeInstanceOf(
            ResultError
        );
        await expect(svc.updateNote("H1", note)).rejects.toMatchObject({
            errorCode: "ClientApp-CI-HGU",
            statusCode: 503,
        });
    });
});
