import { createServiceTestContext } from "../../../support/functions/serviceTestContext";

describe("Clinical Documents Service", () => {
    const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

    const service = createServiceTestContext();

    it("Verify Swagger", () => {
        service.getConfig().then((config) => {
            cy.log(
                `Verifying Swagger exists for Clinical Documents at Endpoint: ${config.serviceEndpoints.ClinicalDocument}swagger`
            );
            cy.request(
                `${config.serviceEndpoints.ClinicalDocument}swagger/v1/swagger.json`
            ).should((response) => {
                expect(response.status).to.eq(200);
                expect(response.body.info.title).to.eq(
                    "Health Gateway Clinical Document Services documentation"
                );
            });
        });
    });

    it("Verify Clinical Document Unauthorized", () => {
        service.getConfig().then((config) => {
            cy.log(
                `Clinical Document Service Endpoint: ${config.serviceEndpoints.ClinicalDocument}`
            );
            cy.request({
                url: `${config.serviceEndpoints.ClinicalDocument}ClinicalDocument/${HDID}`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Clinical Document Forbidden", () => {
        const BOGUSHDID = "BOGUSHDID";
        service.getTokens().then((tokens) => {
            service.getConfig().then((config) => {
                cy.log(
                    `Clinical Document Service Endpoint: ${config.serviceEndpoints.ClinicalDocument}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.ClinicalDocument}ClinicalDocument/${BOGUSHDID}`,
                    followRedirect: false,
                    failOnStatusCode: false,
                    auth: {
                        bearer: tokens.access_token,
                    },
                    headers: {
                        accept: "application/json",
                    },
                }).should((response) => {
                    expect(response.status).to.eq(403);
                });
            });
        });
    });

    it("Verify Clinical Document Records Authorized", () => {
        service.getTokens().then((tokens) => {
            service.getConfig().then((config) => {
                cy.log(
                    `Clinical Document Service Endpoint: ${config.serviceEndpoints.ClinicalDocument}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.ClinicalDocument}ClinicalDocument/${HDID}`,
                    followRedirect: false,
                    auth: {
                        bearer: tokens.access_token,
                    },
                    headers: {
                        accept: "application/json",
                    },
                }).should((response) => {
                    expect(response.status).to.eq(200);
                    expect(response.body).to.not.be.null;
                    expect(response.body.resourcePayload).to.not.be.null;
                    expect(response.body.resourcePayload[0].id).to.not.be.empty;
                    expect(response.body.resourcePayload[0].name).to.not.be
                        .empty;
                    expect(response.body.resourcePayload[0].fileId).to.not.be
                        .empty;
                });
            });
        });
    });

    it("Verify Clinical Document File Authorized", () => {
        const FILEID = "clinicaldocument_vpp_cer_12345678931";
        service.getTokens().then((tokens) => {
            service.getConfig().then((config) => {
                cy.log(
                    `Clinical Document Service Endpoint: ${config.serviceEndpoints.ClinicalDocument}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.ClinicalDocument}ClinicalDocument/${HDID}/file/${FILEID}`,
                    followRedirect: false,
                    auth: {
                        bearer: tokens.access_token,
                    },
                    headers: {
                        accept: "application/json",
                    },
                }).should((response) => {
                    expect(response.status).to.eq(200);
                    expect(response.body).to.not.be.null;
                    expect(response.body.resourcePayload).to.not.be.null;
                });
            });
        });
    });
});
