<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faInfoCircle } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Prop, Watch } from "vue-property-decorator";
import {
    maxLength,
    minLength,
    minValue,
    required,
    requiredIf,
} from "vuelidate/lib/validators";
import { Validation } from "vuelidate/vuelidate";
import { Action, Getter } from "vuex-class";

import Image06 from "@/assets/images/landing/006-BCServicesCardLogo.png";
import ErrorCardComponent from "@/components/errorCard.vue";
import LoadingComponent from "@/components/loading.vue";
import HgDateDropdownComponent from "@/components/shared/hgDateDropdown.vue";
import HgTimeDropdownComponent from "@/components/shared/hgTimeDropdown.vue";
import { ActionType } from "@/constants/actionType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { PcrDataSource } from "@/constants/pcrTestDataSource";
import { IdentityProviderConfiguration } from "@/models/configData";
import { DateWrapper } from "@/models/dateWrapper";
import PcrTestData from "@/models/pcrTestData";
import RegisterTestKitPublicRequest from "@/models/registerTestKitPublicRequest";
import RegisterTestKitRequest from "@/models/registerTestKitRequest";
import { ResultError } from "@/models/requestResult";
import User, { OidcUserProfile } from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { IAuthenticationService, ILogger } from "@/services/interfaces";
import { IPcrTestService } from "@/services/interfaces";
import { Mask, phnMask, smsMask } from "@/utility/masks";
import PHNValidator from "@/utility/phnValidator";

library.add(faInfoCircle);

interface ISelectOption {
    text: string;
    value: unknown;
}

@Component({
    components: {
        LoadingComponent,
        ErrorCard: ErrorCardComponent,
        "hg-date-dropdown": HgDateDropdownComponent,
        "hg-time-dropdown": HgTimeDropdownComponent,
    },
})
export default class PcrTestView extends Vue {
    // ### Props ###
    @Prop() serialNumber!: string;

    // ### Store ###
    @Action("addError", { namespace: "errorBanner" })
    addError!: (params: {
        errorType: ErrorType;
        source: ErrorSourceType;
        traceId: string | undefined;
    }) => void;

    @Action("addCustomError", { namespace: "errorBanner" })
    addCustomError!: (params: {
        title: string;
        source: ErrorSourceType;
        traceId: string | undefined;
    }) => void;

    @Action("authenticateOidc", { namespace: "auth" })
    authenticateOidc!: (params: {
        idpHint: string;
        redirectPath: string;
    }) => Promise<void>;

    @Action("setHeaderButtonState", { namespace: "navbar" })
    setHeaderButtonState!: (visible: boolean) => void;

    @Action("setSidebarButtonState", { namespace: "navbar" })
    setSidebarButtonState!: (visible: boolean) => void;

    @Action("toggleSidebar", { namespace: "navbar" })
    toggleSidebar!: () => void;

    @Getter("user", { namespace: "user" }) user!: User;

    @Getter("isSidebarShown", { namespace: "navbar" })
    isSidebarShown!: boolean;

    @Getter("oidcIsAuthenticated", { namespace: "auth" })
    oidcIsAuthenticated!: boolean;

    @Getter("identityProviders", { namespace: "config" })
    identityProviders!: IdentityProviderConfiguration[];

    // ### Service ###
    private pcrTestService!: IPcrTestService;

    // ### Data ###
    private redirectPath = "/pcrtest/" + this.serialNumber;

    private noSerialNumber = false;

    private noTestKitCode = false;

    private noPhn = false;

    private oidcUser?: OidcUserProfile = undefined;

    private registrationComplete = false;

    private logger!: ILogger;

    private loading = false;

    private bcsclogo: string = Image06;

    private errorMessage = "";

    private testTakenMinutesAgoValues = [
        { value: 5, text: "Just now" },
        { value: 30, text: "Within 30 minutes" },
        { value: 120, text: "Within 2 hours" },
        { value: 240, text: "Within 4 hours" },
        { value: 360, text: "Within 6 hours" },
        { value: 480, text: "Within 8 hours" },
        { value: 720, text: "Within 12 hours" },
        { value: 1080, text: "Within 18 hours" },
        { value: 1440, text: "Within 24 hours" },
    ];

    private pcrTest: PcrTestData = {
        firstName: "",
        lastName: "",
        phn: "",
        dob: "",
        contactPhoneNumber: "",
        streetAddress: "",
        city: "",
        postalOrZip: "",
        testTakenMinutesAgo: -1,
        hdid: "",
        testKitCid: this.serialNumber,
        testKitCode: "",
    };

    // Variables for PcrDataSource enum referenced in render
    private DSNONE = PcrDataSource.None;
    private DSKEYCLOAK = PcrDataSource.Keycloak;
    private DSMANUAL = PcrDataSource.Manual;

    // Set this to none initially to show options
    private dataSource: PcrDataSource = this.DSNONE;

    // ### Lifecycle ###
    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.pcrTestService = container.get<IPcrTestService>(
            SERVICE_IDENTIFIER.PcrTestService
        );
        this.setSidebarButtonState(false);
        this.setHeaderButtonState(false);
    }

    private destroyed() {
        this.setSidebarButtonState(true);
        this.setHeaderButtonState(true);
    }

    private mounted() {
        if (!this.serialNumber || this.serialNumber === "") {
            this.noSerialNumber = true;
            this.noTestKitCode = true;
        }

        this.oidcIsAuthenticatedChanged();
    }

    @Watch("oidcIsAuthenticated")
    private oidcIsAuthenticatedChanged() {
        if (this.oidcIsAuthenticated) {
            this.dataSource = this.DSKEYCLOAK;

            // If the user chooses to log in with keycloak, log them in and get their info
            // Load the user name and current email
            const authenticationService = container.get<IAuthenticationService>(
                SERVICE_IDENTIFIER.AuthenticationService
            );
            this.loading = true;
            authenticationService
                .getOidcUserProfile()
                .then((result) => {
                    // Load oidc user details
                    this.oidcUser = result;
                    this.pcrTest.hdid = this.oidcUser.hdid;
                })
                .catch((err) => {
                    this.logger.error(`Error loading profile: ${err}`);
                    this.addError({
                        errorType: ErrorType.Retrieve,
                        source: ErrorSourceType.Profile,
                        traceId: undefined,
                    });
                })
                .finally(() => (this.loading = false));
        }
    }

    // ### Getters ###
    private get pcrDataSource(): PcrDataSource {
        return this.dataSource;
    }

    private get isLoading(): boolean {
        return this.loading;
    }

    private get phnMask(): Mask {
        return phnMask;
    }

    private get smsMask(): Mask {
        return smsMask;
    }

    private getFullName(): string {
        if (this.oidcUser !== undefined) {
            return this.oidcUser.given_name + " " + this.oidcUser.family_name;
        } else {
            return "";
        }
    }

    // Redirect back to serial number path if user had it before logging in
    private get oidcRedirectPath() {
        return this.noSerialNumber ? "/pcrtest" : this.redirectPath;
    }

    // Build dropdown for testTakenMinutesAgo dropdown
    private get testTakenMinutesAgoOptions() {
        // Initial value is -1, which is the default value for the dropdown
        let testTakenMinutesAgoOptions: ISelectOption[] = [
            { value: -1, text: "Time" },
        ];
        for (var i = 0; i < this.testTakenMinutesAgoValues.length; i++) {
            testTakenMinutesAgoOptions.push({
                value: this.testTakenMinutesAgoValues[i].value,
                text: this.testTakenMinutesAgoValues[i].text,
            });
        }
        return testTakenMinutesAgoOptions;
    }

    // ### Setters ###
    private setHasNoPhn(value: boolean) {
        this.noPhn = value;
        const phnField = this.$v.pcrTest.phn;
        if (phnField) {
            this.pcrTest.phn = "";
            phnField.$touch();
        }
    }

    // Sets the data source to either NONE (landing), KEYCLOAK (login), or MANUAL (registration)
    private setDataSource(dataSource: PcrDataSource) {
        if (
            dataSource === PcrDataSource.Keycloak &&
            !this.oidcIsAuthenticated
        ) {
            this.loading = true;
            // don't need to reset loading property if login succeeds
            // since redirect will kick in and component will be recreated after
            this.oidcLogin(this.identityProviders[0].hint).catch(
                () => (this.loading = false)
            );
        }
        this.resetForm();
        this.dataSource = dataSource;
    }

    private isManualInput(): boolean {
        return this.dataSource === this.DSMANUAL;
    }

    private isManualInputAndNoPhnProvided(): boolean {
        return this.isManualInput() && this.noPhn;
    }

    private isManualInputAndPhnProvided(): boolean {
        return this.isManualInput() && !this.noPhn;
    }

    private isNoSerialNumber(): boolean {
        return this.noSerialNumber;
    }

    // ### Validation ###
    private phnValidator(value: string): boolean {
        this.logger.debug(
            `Data Source: ${this.dataSource}, PHN: ${this.noPhn}`
        );
        if (this.dataSource === this.DSMANUAL && !this.noPhn) {
            var phn = value.replace(/\D/g, "");
            return PHNValidator.IsValid(phn);
        } else {
            return true;
        }
    }

    private testKitCodeValidator(value: string): boolean {
        const pattern = /^([a-zA-Z\d]{7})-([a-zA-Z\d]{5})$|^$/;
        return pattern.test(value);
    }

    private validations() {
        return {
            pcrTest: {
                firstName: {
                    required: requiredIf(this.isManualInput),
                },
                lastName: {
                    required: requiredIf(this.isManualInput),
                },
                phn: {
                    required: requiredIf(this.isManualInputAndPhnProvided),
                    formatted: this.phnValidator,
                },
                dob: {
                    required: requiredIf(this.isManualInput),
                    maxValue: (value: string) => {
                        // only check this if manual mode selected
                        if (this.dataSource === this.DSMANUAL) {
                            return new DateWrapper(value).isBefore(
                                new DateWrapper()
                            );
                            // otherwise, return true
                        } else {
                            return true;
                        }
                    },
                },
                contactPhoneNumber: {
                    required: false,
                    minLength: minLength(14),
                    maxLength: maxLength(14),
                },
                streetAddress: {
                    required: requiredIf(this.isManualInputAndNoPhnProvided),
                },
                city: {
                    required: requiredIf(this.isManualInputAndNoPhnProvided),
                },
                postalOrZip: {
                    required: requiredIf(this.isManualInputAndNoPhnProvided),
                    minLength: minLength(7),
                },
                testTakenMinutesAgo: {
                    required: required,
                    minValue: minValue(0),
                },
                testKitCode: {
                    required: requiredIf(this.isNoSerialNumber),
                    formatted: this.testKitCodeValidator,
                },
            },
        };
    }

    private isValid(param: Validation): boolean | undefined {
        return param.$dirty ? !param.$invalid : undefined;
    }

    // ### Form Actions ###
    private handleSubmit() {
        this.$v.$touch();
        this.errorMessage = "";
        if (this.$v.$invalid) {
            return;
        }

        const shortCodeFirst =
            this.pcrTest.testKitCode.length > 0
                ? this.pcrTest.testKitCode.split("-")[0]
                : "";

        const shortCodeSecond =
            this.pcrTest.testKitCode.length > 0
                ? this.pcrTest.testKitCode.split("-")[1]
                : "";

        switch (this.dataSource) {
            // ### Submitted through OIDC
            case this.DSKEYCLOAK:
                var testKitRequest: RegisterTestKitRequest = {
                    hdid: this.oidcUser?.hdid,
                    testTakenMinutesAgo: this.pcrTest.testTakenMinutesAgo,
                    testKitCid: this.pcrTest.testKitCid,
                    shortCodeFirst,
                    shortCodeSecond,
                };
                this.logger.debug(JSON.stringify(testKitRequest));
                this.loading = true;
                // Get user's hdid
                const hdid = this.user.hdid;
                this.pcrTestService
                    .registerTestKit(hdid, testKitRequest)
                    .then((response) => {
                        this.logger.debug(
                            `registerTestKit Response: ${JSON.stringify(
                                response
                            )}`
                        );
                        this.displaySuccess();
                    })
                    .catch((err: ResultError) => {
                        this.handleError(err, "registerTestKit");
                    });
                break;
            // ### Submitted through manual input
            case this.DSMANUAL:
                const phnDigits = this.pcrTest.phn;
                const phoneDigits = this.pcrTest.contactPhoneNumber;
                const zipDigits = this.pcrTest.postalOrZip;
                var testKitPublicRequest: RegisterTestKitPublicRequest = {
                    firstName: this.pcrTest.firstName,
                    lastName: this.pcrTest.lastName,
                    phn: phnDigits ? phnDigits.replace(/\D/g, "") : "",
                    dob: this.pcrTest.dob,
                    contactPhoneNumber: phoneDigits
                        ? phoneDigits.replace(/\D/g, "")
                        : "",
                    streetAddress: this.pcrTest.streetAddress,
                    city: this.pcrTest.city,
                    postalOrZip: zipDigits ?? "",
                    testTakenMinutesAgo: this.pcrTest.testTakenMinutesAgo,
                    testKitCid: this.pcrTest.testKitCid,
                    shortCodeFirst,
                    shortCodeSecond,
                };
                this.logger.debug(JSON.stringify(testKitPublicRequest));
                this.loading = true;
                this.pcrTestService
                    .registerTestKitPublic(testKitPublicRequest)
                    .then((response) => {
                        this.logger.debug(
                            `registerTestKitPublic Response: ${JSON.stringify(
                                response
                            )}`
                        );
                        this.displaySuccess();
                    })
                    .catch((err: ResultError) => {
                        this.handleError(err, "registerTestKitPublic");
                    });
                break;
            default:
                break;
        }
    }

    private displaySuccess(): void {
        this.resetForm();
        this.loading = false;
        this.registrationComplete = true;
        window.scrollTo(0, 0);
    }

    private handleError(err: ResultError, domain: string) {
        this.logger.error(`${domain} Error: ${err}`);
        if (err.actionCode == ActionType.Processed) {
            this.errorMessage = err.resultMessage;
        } else {
            this.addCustomError({
                title: "Unable to register test kit",
                source: ErrorSourceType.TestKit,
                traceId: err.traceId,
            });
        }
        this.loading = false;
        window.scrollTo(0, 0);
    }

    private resetForm() {
        this.pcrTest = {
            firstName: "",
            lastName: "",
            phn: "",
            dob: "",
            contactPhoneNumber: "",
            streetAddress: "",
            city: "",
            postalOrZip: "",
            testTakenMinutesAgo: -1,
            hdid: "",
            testKitCid: this.noSerialNumber ? "" : this.serialNumber,
            testKitCode: "",
        };
        this.noPhn = false;
        this.$v.$reset();
    }

    private handleCancel() {
        this.resetForm();
        this.dataSource = PcrDataSource.None;
    }

    // Auth
    private async oidcLogin(hint: string): Promise<void> {
        return this.authenticateOidc({
            idpHint: hint,
            redirectPath: this.oidcRedirectPath,
        }).catch((err) => {
            this.logger.error(`oidcLogin Error: ${err}`);
            this.addError({
                errorType: ErrorType.Retrieve,
                source: ErrorSourceType.User,
                traceId: undefined,
            });
        });
    }
}
</script>

<template>
    <div>
        <!-- LANDING -->
        <b-container v-if="isLoading">
            <LoadingComponent :is-loading="isLoading" />
        </b-container>
        <b-container
            v-if="
                !registrationComplete && pcrDataSource === DSNONE && !isLoading
            "
        >
            <b-row class="pt-4">
                <b-col>
                    <b-row>
                        <b-col
                            ><strong
                                >Register your COVID-19 test kit using one of
                                the following methods:
                            </strong></b-col
                        >
                    </b-row>
                </b-col>
            </b-row>
            <b-row>
                <b-col>
                    <!-- add whitespace above buttons -->
                    <b-row class="pt-3"></b-row>
                    <b-row class="pt-5"></b-row>
                    <b-row class="pt-5" align="center">
                        <b-col>
                            <hg-button
                                id="btnLogin"
                                aria-label="BC Services Card Login"
                                data-testid="btn-login"
                                variant="primary"
                                class="login-button"
                                @click="setDataSource(DSKEYCLOAK)"
                            >
                                <img
                                    class="mr-2 mb-1"
                                    :src="bcsclogo"
                                    height="16"
                                    alt="BC Services Card App Icon"
                                />
                                <span>Log In with BC Services Card App</span>
                            </hg-button>
                        </b-col>
                    </b-row>
                    <b-row class="my-3 no-gutters align-items-center">
                        <b-col><hr /></b-col>
                        <b-col cols="auto">
                            <h3 class="h5 m-0 px-3 text-muted">OR</h3>
                        </b-col>
                        <b-col><hr /></b-col>
                    </b-row>
                    <b-row align="center">
                        <b-col>
                            <hg-button
                                id="btn-manual"
                                aria-label="BC Services Card Login"
                                data-testid="btn-manual"
                                variant="secondary"
                                class="manual-enter-button"
                                @click="setDataSource(DSMANUAL)"
                            >
                                <span>Manually enter your information</span>
                            </hg-button>
                        </b-col>
                    </b-row>
                </b-col>
            </b-row>
        </b-container>

        <!-- FORM -->

        <b-container
            v-if="!registrationComplete && pcrDataSource !== DSNONE"
            v-show="!isLoading"
        >
            <b-row id="title" class="mt-4">
                <b-col>
                    <h1 class="h4 mb-2 font-weight-normal">
                        <strong>Register a Test Kit</strong>
                    </h1>
                </b-col>
            </b-row>
            <b-row v-if="!!errorMessage" id="processError" class="pt-3">
                <b-col>
                    <b-alert
                        data-testid="alreadyProcessedBanner"
                        variant="warning"
                        dismissible
                        class="no-print"
                        :show="true"
                    >
                        <p data-testid="alreadyProcessedText">
                            {{ errorMessage }}
                        </p>
                    </b-alert>
                </b-col>
            </b-row>
            <b-row>
                <b-col>
                    <form @submit.prevent="handleSubmit">
                        <!-- NAME -->
                        <b-row v-if="pcrDataSource === DSKEYCLOAK" class="pt-2">
                            <b-col>
                                <label for="pcrTestFullName">Name:</label>
                                <strong id="prcTestFullName">
                                    {{ getFullName() }}
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row v-if="noTestKitCode" class="mt-2">
                            <b-col>
                                <label for="testKitCode"> Test Kit Code </label>
                                <b-form-input
                                    id="testKitCode"
                                    v-model="pcrTest.testKitCode"
                                    data-testid="test-kit-code-input"
                                    type="text"
                                    placeholder="Test Kit Code"
                                    :state="isValid($v.pcrTest.testKitCode)"
                                    @blur.native="
                                        $v.pcrTest.testKitCode.$touch()
                                    "
                                />
                                <b-form-invalid-feedback
                                    v-if="
                                        $v.pcrTest.testKitCode.$dirty &&
                                        !$v.pcrTest.testKitCode.required
                                    "
                                    aria-label="PCR Test Kit Code is required"
                                    data-testid="feedback-testkitcode-is-required"
                                >
                                    PCR Test Kit Code is required.
                                </b-form-invalid-feedback>
                                <b-form-invalid-feedback
                                    v-else-if="
                                        $v.pcrTest.testKitCode.$dirty &&
                                        !$v.pcrTest.testKitCode.formatted
                                    "
                                    aria-label="PCR Test Kit Code is invalid"
                                    data-testid="feedback-testkitcode-is-invalid"
                                >
                                    PCR Test Kit Code is invalid.
                                </b-form-invalid-feedback>
                            </b-col>
                        </b-row>
                        <b-row
                            v-if="pcrDataSource === DSMANUAL"
                            data-testid="pcr-name"
                        >
                            <b-col>
                                <b-row class="mt-2">
                                    <b-col>
                                        <label for="pcrFirstName"
                                            >First Name</label
                                        >
                                        <b-form-input
                                            id="pcrFirstName"
                                            v-model="pcrTest.firstName"
                                            data-testid="first-name-input"
                                            type="text"
                                            placeholder="First Name"
                                            :state="
                                                isValid($v.pcrTest.firstName)
                                            "
                                            @blur.native="
                                                $v.pcrTest.firstName.$touch()
                                            "
                                        />
                                        <b-form-invalid-feedback
                                            v-if="
                                                $v.pcrTest.firstName.$dirty &&
                                                !$v.pcrTest.firstName.required
                                            "
                                            aria-label="First name is required"
                                            force-show
                                            data-testid="feedback-firstname-is-required"
                                        >
                                            First name is required.
                                        </b-form-invalid-feedback>
                                    </b-col>
                                </b-row>
                                <b-row class="mt-2">
                                    <b-col>
                                        <label for="pcrLastName"
                                            >Last Name</label
                                        >
                                        <b-form-input
                                            id="pcrLastName"
                                            v-model="pcrTest.lastName"
                                            data-testid="last-name-input"
                                            type="text"
                                            placeholder="Last Name"
                                            :state="
                                                isValid($v.pcrTest.lastName)
                                            "
                                            @blur.native="
                                                $v.pcrTest.lastName.$touch()
                                            "
                                        />
                                        <b-form-invalid-feedback
                                            v-if="
                                                $v.pcrTest.lastName.$dirty &&
                                                !$v.pcrTest.lastName.required
                                            "
                                            aria-label="Last name is required"
                                            force-show
                                            data-testid="feedback-lastname-is-required"
                                        >
                                            Last name is required.
                                        </b-form-invalid-feedback>
                                    </b-col>
                                </b-row>
                            </b-col>
                        </b-row>
                        <!-- PHN -->
                        <b-row
                            v-if="dataSource == DSMANUAL"
                            data-testid="pcr-phn"
                        >
                            <b-col v-if="pcrDataSource === DSMANUAL">
                                <b-row class="mt-2">
                                    <b-col>
                                        <b-form-group
                                            label="Personal Health Number"
                                            label-for="phn"
                                        >
                                            <b-form-input
                                                id="phn"
                                                v-model="pcrTest.phn"
                                                v-mask="phnMask"
                                                data-testid="phn-input"
                                                placeholder="PHN"
                                                aria-label="Personal Health Number"
                                                :state="isValid($v.pcrTest.phn)"
                                                :disabled="noPhn"
                                                @blur.native="
                                                    $v.pcrTest.phn.$touch()
                                                "
                                            />
                                            <b-form-invalid-feedback
                                                aria-label="Valid PHN is required"
                                                :state="isValid($v.pcrTest.phn)"
                                                data-testid="feedback-phn-is-required"
                                            >
                                                Valid PHN is required.
                                            </b-form-invalid-feedback>
                                        </b-form-group>
                                    </b-col>
                                </b-row>
                            </b-col>
                        </b-row>
                        <!-- NO PHN -->
                        <b-row>
                            <b-col>
                                <!-- Checkbox for PHN or no PHN -->
                                <b-form-checkbox
                                    v-if="dataSource == DSMANUAL"
                                    id="phnCheckbox"
                                    v-model="noPhn"
                                    data-testid="phn-checkbox"
                                    @change="setHasNoPhn($event)"
                                >
                                    I don't have a PHN
                                    <hg-button
                                        :id="'pcr-no-phn-info-button'"
                                        aria-label="Result Description"
                                        href="#"
                                        variant="link"
                                        data-testid="pcr-no-phn-info-button"
                                        class="shadow-none p-0 ml-1"
                                    >
                                        <hg-icon
                                            icon="info-circle"
                                            size="small"
                                        />
                                    </hg-button>
                                    <b-popover
                                        :target="'pcr-no-phn-info-button'"
                                        triggers="hover focus"
                                        placement="bottomleft"
                                        data-testid="pcr-no-phn-info-popover"
                                    >
                                        You can find your personal health number
                                        (PHN) on your BC Services Card. If you
                                        do not have a PHN, please enter your
                                        address so we can register your PCR test
                                        kit to you.
                                    </b-popover>
                                </b-form-checkbox>
                            </b-col>
                        </b-row>
                        <b-row
                            v-if="pcrDataSource === DSMANUAL && noPhn"
                            data-testid="pcr-home-address"
                        >
                            <b-col>
                                <b-row class="mt-2">
                                    <b-col>
                                        <label for="pcrStreetAddress"
                                            >Street Address</label
                                        >
                                        <b-form-input
                                            id="pcrStreetAddress"
                                            v-model="pcrTest.streetAddress"
                                            data-testid="pcr-street-address-input"
                                            type="text"
                                            placeholder="Address"
                                            :state="
                                                isValid(
                                                    $v.pcrTest.streetAddress
                                                )
                                            "
                                            @blur.native="
                                                $v.pcrTest.streetAddress.$touch()
                                            "
                                        />
                                        <b-form-invalid-feedback
                                            v-if="
                                                $v.pcrTest.streetAddress
                                                    .$dirty &&
                                                !$v.pcrTest.streetAddress
                                                    .required
                                            "
                                            aria-label="Street address is required"
                                            force-show
                                            data-testid="feedback-streetaddress-is-required"
                                        >
                                            Street address is required.
                                        </b-form-invalid-feedback>
                                    </b-col>
                                </b-row>
                                <b-row class="mt-2">
                                    <b-col>
                                        <label for="pcrCity">City</label>
                                        <b-form-input
                                            id="pcrCity"
                                            v-model="pcrTest.city"
                                            data-testid="pcr-city-input"
                                            type="text"
                                            placeholder="City"
                                            :state="isValid($v.pcrTest.city)"
                                            @blur.native="
                                                $v.pcrTest.city.$touch()
                                            "
                                        />
                                        <b-form-invalid-feedback
                                            v-if="
                                                $v.pcrTest.city.$dirty &&
                                                !$v.pcrTest.city.required
                                            "
                                            aria-label="City is required"
                                            force-show
                                            data-testid="feedback-city-is-required"
                                        >
                                            City is required.
                                        </b-form-invalid-feedback>
                                    </b-col>
                                </b-row>
                                <b-row class="mt-2">
                                    <b-col>
                                        <label for="pcrZip">Postal Code</label>
                                        <b-form-input
                                            id="pcrZip"
                                            v-model="pcrTest.postalOrZip"
                                            v-mask="'A#A #A#'"
                                            data-testid="pcr-zip-input"
                                            type="text"
                                            placeholder="Postal Code"
                                            :state="
                                                isValid($v.pcrTest.postalOrZip)
                                            "
                                            @blur.native="
                                                $v.pcrTest.postalOrZip.$touch()
                                            "
                                        />
                                        <b-form-invalid-feedback
                                            v-if="
                                                $v.pcrTest.postalOrZip.$dirty &&
                                                !$v.pcrTest.postalOrZip.required
                                            "
                                            aria-label="Postal code is required"
                                            force-show
                                            data-testid="feedback-postal-is-required"
                                        >
                                            Postal code is required.
                                        </b-form-invalid-feedback>
                                        <b-form-invalid-feedback
                                            v-else-if="
                                                $v.pcrTest.postalOrZip.$dirty &&
                                                !$v.pcrTest.postalOrZip
                                                    .minLength
                                            "
                                            aria-label="Postal code is required"
                                            force-show
                                        >
                                            Postal code is required.
                                        </b-form-invalid-feedback>
                                    </b-col>
                                </b-row>
                            </b-col>
                        </b-row>
                        <!-- DOB -->
                        <b-row data-testid="pcr-dob" class="mt-2">
                            <b-col v-if="pcrDataSource === DSMANUAL">
                                <b-form-group
                                    label="Date of Birth"
                                    label-for="dob"
                                    :state="isValid($v.pcrTest.dob)"
                                >
                                    <hg-date-dropdown
                                        id="dob"
                                        v-model="pcrTest.dob"
                                        data-testid="dob-input"
                                        :state="isValid($v.pcrTest.dob)"
                                        :allow-future="false"
                                        aria-label="Date of Birth"
                                        @blur="$v.pcrTest.dob.$touch()"
                                    />
                                    <b-form-invalid-feedback
                                        v-if="
                                            $v.pcrTest.dob.$dirty &&
                                            !$v.pcrTest.dob.required
                                        "
                                        aria-label="Invalid Date of Birth"
                                        data-testid="feedback-dob-is-required"
                                        force-show
                                    >
                                        A valid date of birth is required.
                                    </b-form-invalid-feedback>
                                    <b-form-invalid-feedback
                                        v-else-if="
                                            $v.pcrTest.dob.$dirty &&
                                            !$v.pcrTest.dob.maxValue
                                        "
                                        aria-label="Invalid Date of Birth"
                                        force-show
                                        data-testid="feedback-dob-is-invalid"
                                    >
                                        Date of birth must be in the past.
                                    </b-form-invalid-feedback>
                                </b-form-group>
                            </b-col>
                        </b-row>
                        <!-- MOBILE NUMBER -->
                        <b-row
                            v-if="pcrDataSource === DSMANUAL"
                            data-testid="pcr-mobile-number"
                        >
                            <b-col>
                                <b-row class="mt-2">
                                    <b-col>
                                        <label for="pcrMobileNumber"
                                            >Mobile Number (To receive a
                                            notification once your COVID-19 test
                                            result is available)
                                        </label>
                                        <b-form-input
                                            id="pcrMobileNumber"
                                            v-model="pcrTest.contactPhoneNumber"
                                            v-mask="smsMask"
                                            data-testid="contact-phone-number-input"
                                            type="tel"
                                            placeholder="(###) ###-####"
                                            :state="
                                                isValid(
                                                    $v.pcrTest
                                                        .contactPhoneNumber
                                                )
                                            "
                                            @blur.native="
                                                $v.pcrTest.contactPhoneNumber.$touch()
                                            "
                                        />
                                        <b-form-invalid-feedback
                                            v-if="
                                                $v.pcrTest.contactPhoneNumber
                                                    .$dirty &&
                                                (!$v.pcrTest.contactPhoneNumber
                                                    .maxLength ||
                                                    !$v.pcrTest
                                                        .contactPhoneNumber
                                                        .minLength)
                                            "
                                            aria-label="Phone number must be valid."
                                            force-show
                                            data-testid="feedback-phonenumber-valid"
                                        >
                                            Phone number must be valid.
                                        </b-form-invalid-feedback>
                                    </b-col>
                                </b-row>
                            </b-col>
                        </b-row>
                        <!-- TIME SINCE TEST TAKEN -->
                        <b-row class="mt-2">
                            <b-col>
                                <!-- Time since test taken -->
                                <label for="testTakenMinutesAgo"
                                    >Time since test taken</label
                                >
                                <b-form-select
                                    id="testTakenMinutesAgo"
                                    v-model="pcrTest.testTakenMinutesAgo"
                                    data-testid="test-taken-minutes-ago"
                                    :options="testTakenMinutesAgoOptions"
                                    :state="
                                        isValid($v.pcrTest.testTakenMinutesAgo)
                                    "
                                    @blur.native="
                                        $v.pcrTest.testTakenMinutesAgo.$touch()
                                    "
                                >
                                </b-form-select>
                                <b-form-invalid-feedback
                                    v-if="
                                        $v.pcrTest.testTakenMinutesAgo.$dirty &&
                                        (!$v.pcrTest.testTakenMinutesAgo
                                            .required ||
                                            !$v.pcrTest.testTakenMinutesAgo
                                                .minValue)
                                    "
                                    aria-label=" Time since test taken is required"
                                    force-show
                                    data-testid="feedback-testtaken-is-required"
                                >
                                    Time since test taken is required.
                                </b-form-invalid-feedback>
                            </b-col>
                        </b-row>
                        <!-- PRIVACY STATEMENT -->
                        <b-row data-testid="pcr-privacy-statement" class="pt-2">
                            <b-col
                                ><hg-button
                                    id="privacy-statement"
                                    aria-label="Privacy Statement"
                                    href="#"
                                    tabindex="0"
                                    variant="link"
                                    data-testid="btn-privacy-statement"
                                    class="shadow-none p-0"
                                >
                                    <hg-icon
                                        icon="info-circle"
                                        size="small"
                                        class="mr-1"
                                    />
                                    <small>Privacy Statement</small>
                                </hg-button>
                                <b-popover
                                    target="privacy-statement"
                                    triggers="hover focus"
                                    placement="topright"
                                >
                                    Your information is being collected to
                                    provide you with your COVID-19 test result
                                    under s. 26(c) of the
                                    <em
                                        >Freedom of Information and Protection
                                        of Privacy Act</em
                                    >. Contact the Ministry Privacy Officer at
                                    <a
                                        href="mailto:MOH.Privacy.Officer@gov.bc.ca"
                                        >MOH.Privacy.Officer@gov.bc.ca</a
                                    >
                                    if you have any questions about this
                                    collection.
                                </b-popover></b-col
                            >
                        </b-row>
                        <!-- FORM ACTIONS -->
                        <b-row class="my-3">
                            <b-col cols="4">
                                <hg-button
                                    id="btn-cancel"
                                    block
                                    variant="secondary"
                                    data-testid="btn-cancel"
                                    @click="handleCancel"
                                >
                                    Cancel
                                </hg-button>
                            </b-col>
                            <b-col cols="8">
                                <hg-button
                                    id="btn-register-kit"
                                    block
                                    type="submit"
                                    variant="primary"
                                    data-testid="btn-register-kit"
                                >
                                    Register Kit
                                </hg-button>
                            </b-col>
                        </b-row>
                    </form>
                </b-col>
            </b-row>

            <b-row data-testid="pcr-form-actions"></b-row>
        </b-container>

        <!-- SUCCESS -->

        <b-container v-if="registrationComplete && !isLoading">
            <b-row class="text-center pt-3" align-v="center">
                <b-col>
                    <b-alert
                        data-testid="registration-success-banner"
                        variant="success"
                        class="no-print"
                        :show="true"
                    >
                        <h4 data-testid="registration-success-banner-title">
                            Success!
                        </h4>
                        <span>
                            Your kit has been registered to your profile.
                        </span>
                        <div v-if="oidcIsAuthenticated" class="pt-2">
                            <router-link to="/logout">
                                <hg-button
                                    id="logoutBtn"
                                    aria-label="Logout"
                                    data-testid="logoutBtn"
                                    variant="link"
                                    class="continue-button"
                                >
                                    <span>Log out</span>
                                </hg-button>
                            </router-link>
                        </div>
                        <div v-else class="pt-2">
                            <router-link to="/">
                                <hg-button
                                    id="btn-continue"
                                    aria-label="Continue"
                                    data-testid="btn-continue"
                                    variant="link"
                                    class="continue-button"
                                >
                                    <span>Back to home</span>
                                </hg-button>
                            </router-link>
                        </div>
                    </b-alert>
                </b-col>
            </b-row>
        </b-container>
    </div>
</template>

<style scoped lang="scss">
.login-button {
    background-color: #1a5a95 !important;
    border-color: #1a5a95 !important;
}
.phn-info {
    color: #636363 !important;
}
</style>
