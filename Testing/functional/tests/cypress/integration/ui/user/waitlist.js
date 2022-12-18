const { AuthMethod } = require("../../../support/constants");

const serviceUnavailable = 503;

function addSeconds(date, seconds) {
    date.setSeconds(date.getSeconds() + seconds);
    return date;
}

function getTicket(status, queuePosition = 0) {
    var now = new Date();
    var createdTime = new Date(now);
    var checkInAfter = addSeconds(new Date(now), 4);
    var tokenExpires = addSeconds(new Date(now), 60);

    const ticket = {
        id: "ef273c16-f66f-4a84-b690-4a8cb8e46a79",
        room: "healthgateway",
        nonce: "638069293450757560.ZmNhMTZjZTMtOWU0NS00NDI4LThlODEtOWEzOTZiYTY4MDRmZGNhY2EyNjQtYmE1OS00NGExLWIwMmMtNzU0OTYwZGM3ZWZh",
        createdTime: Math.round(createdTime.getTime() / 1000),
        checkInAfter: Math.round(checkInAfter.getTime() / 1000),
        tokenExpires: Math.round(tokenExpires.getTime() / 1000),
        queuePosition: queuePosition,
        status: status,
        token: "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpYXQiOjE2NzEzMzIxODUsImp0aSI6ImVmMjczYzE2LWY2NmYtNGE4NC1iNjkwLTRhOGNiOGU0NmE3OSIsInN1YiI6ImVmMjczYzE2LWY2NmYtNGE4NC1iNjkwLTRhOGNiOGU0NmE3OSIsImF6cCI6IkhlYWx0aEdhdGV3YXkiLCJuYmYiOjE2NzEzNjA5ODUsImV4cCI6MTY3MTQzMjk4NSwiaXNzIjoiaHR0cHM6Ly90aWNrZXRzLmhlYWx0aGdhdGV3YXkuZ292LmJjLmNhIiwiYXVkIjoiSGVhbHRoR2F0ZXdheSJ9.XaWB0gnj_6QSBE2SbMEvhjtj-BzMveuy5eklmDr1Bp-B_AG0BpaRNIC3vE-nShKTG8psKHV6bgdW6E95YAv6TEaAibgOTIb9gK22mTeLUTOfkDD15mEIWi731Qn1lb8TjGoR1O3znNo_WoEqZiLOvmW9Q8DBgJH1dHnUBsNA3DH7covWWbBMGnuU6IgTHNlYk97SmDv_kz90CSZQ7qJzNlb-GqZTLSbS8EpeJ_O-N64LSSauXMc0cs54BabSsfnRTtcbJzv4WoEy2kq82vUM3TfKbsyVoN0PPjySAZHu2UdTi31AfQs1VLGuc8okqguN1Hthc-vEM5Yd1IJrx7srZA",
    };
    return ticket;
}

describe("Waitlist", () => {
    beforeEach(() => {
        cy.enableModules(["Ticket", "Patient"]);
    });

    it("Verify create ticket not called on unprotected page", () => {
        cy.intercept("POST", "**/Ticket?room=healthgateway", {
            statusCode: serviceUnavailable,
        });
        cy.logout();
        cy.visit("/faq");
        cy.log("Verify faq page accessed without ticket.");
        cy.url().should("include", "/faq");
    });

    it("Verify create ticket and check in called on protected page", () => {
        cy.intercept(
            "POST",
            "**/Ticket?room=healthgateway",
            getTicket("Processed")
        );
        cy.intercept("PUT", "**/Ticket/check-in", getTicket("Processed"));
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
        cy.log("Verify protected page has gotten a processed ticket.");
        cy.url().should("include", "/home");

        cy.log("Verify checkin and page has not changed.");
        cy.wait(5000);
        cy.url().should("include", "/home");
    });

    it("Verify getting a queued ticket and then successfully checking in on a protected page", () => {
        cy.intercept(
            "POST",
            "**/Ticket?room=healthgateway",
            getTicket("Queued", 1)
        );
        cy.intercept("PUT", "**/Ticket/check-in", getTicket("Processed"));
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
        cy.log("Verify protected page has gotten a queued ticket.");
        cy.url().should("include", "/queue");

        cy.log("Verify checkin and page has redirected.");
        cy.wait(5000);
        cy.url().should("include", "/home");
    });

    it("Verify getting a busy ticket on a protected page", () => {
        cy.intercept("POST", "**/Ticket?room=healthgateway", {
            statusCode: serviceUnavailable,
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
        cy.log("Verify protected page has gotten a busy ticket.");
        cy.url().should("include", "/busy");
    });
});
