describe("GatewayApi Communication Service", () => {
    const communicationTypes = [
        ["Banner", 0],
        ["In-App", 2],
    ];

    communicationTypes.forEach(([name, type]) => {
        it(`Verify Get ${name} Communication`, () => {
            cy.readConfig().then((config) => {
                cy.request({
                    url: `${config.serviceEndpoints.GatewayApi}Communication/${type}`,
                    followRedirect: false,
                    failOnStatusCode: false,
                }).should((response) => {
                    expect(response.status).to.eq(200);
                    expect(response.body).to.not.be.null;
                });
            });
        });
    });
});
