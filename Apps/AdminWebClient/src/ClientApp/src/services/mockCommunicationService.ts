import { injectable } from "inversify";
import { IHttpDelegate, ICommunicationService } from "@/services/interfaces";
import Communication from "@/models/communication";

@injectable()
export class MockCommunicationService implements ICommunicationService {
    private notImplemented = "Method not implemented.";
    private mockComms = [
        {
            id: "b1001",
            subject: "banner 1",
            text:
                "B1 - Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum s",
            effectiveDateTime: new Date("2020-08-05"),
            expiryDateTime: new Date("2020-08-07")
        },
        {
            id: "b1002",
            subject: "banner 2",
            text:
                "B2 - Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum s",
            effectiveDateTime: new Date("2020-06-30"),
            expiryDateTime: new Date("2020-07-01")
        },
        {
            id: "b1003",
            subject: "banner 3",
            text:
                "B3 - Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum s",
            effectiveDateTime: new Date("2020-05-03"),
            expiryDateTime: new Date("2020-05-28")
        },
        {
            id: "b1004",
            subject: "banner 4",
            text:
                "B4 - Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum s",
            effectiveDateTime: new Date("2020-07-30"),
            expiryDateTime: new Date("2020-08-01")
        },
        {
            id: "b1005",
            subject: "banner 5",
            text:
                "B5 - Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum s",
            effectiveDateTime: new Date("2020-06-23"),
            expiryDateTime: new Date("2020-06-25")
        },
        {
            id: "b10012",
            subject: "banner 1",
            text:
                "B1 - Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum s",
            effectiveDateTime: new Date("2020-06-18"),
            expiryDateTime: new Date("2020-07-20")
        },
        {
            id: "b10052",
            subject: "banner 52",
            text:
                "B5 - Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum s",
            effectiveDateTime: new Date("2020-06-15"),
            expiryDateTime: new Date("2020-06-17")
        }
    ];

    getAll(): Promise<Communication[]> {
        return new Promise(resolve => {
            resolve(this.mockComms);
        });
    }

    initialize(http: IHttpDelegate): void {
        // Nothing to do
    }

    add(communication: Communication): Promise<void> {
        throw new Error(this.notImplemented);
    }
}
