import {
    testAddCommentError,
    testAddQuickLinkError,
    testEditEmailError,
    testEditSmsError,
    testGetConfigurationError,
    testGetProfileErrorOnLoad,
    testRegisterError,
    testRemoveQuickLinkError,
    testValidateEmailError,
    testVerifySmsError,
} from "../../../support/functions/errorAlertActions";

describe("Error Alerts", () => {
    it("Error Retrieving Configuration", () => {
        testGetConfigurationError();
    });

    it("Error Retrieving Profile on Load", () => {
        testGetProfileErrorOnLoad();
    });

    it("Error Registering", () => {
        testRegisterError();
    });

    it("Error Validating Email", () => {
        testValidateEmailError();
    });

    it("Error Adding Quick Link", () => {
        testAddQuickLinkError();
    });

    it("Error Adding Comment", () => {
        testAddCommentError();
    });

    it("Error Removing Quick Link", () => {
        testRemoveQuickLinkError();
    });

    it("Error Editing SMS Number", () => {
        testEditSmsError();
    });

    it("Error On SMS Verification", () => {
        testVerifySmsError();
    });

    it("Error Editing Email", () => {
        testEditEmailError();
    });
});
