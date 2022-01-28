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
import { PCRDataSource } from "@/constants/pcrTestDataSource";
import BannerError from "@/models/bannerError";
import { IdentityProviderConfiguration } from "@/models/configData";
import { DateWrapper } from "@/models/dateWrapper";
import PCRTestData from "@/models/pcrTestData";
import RegisterTestKitPublicRequest from "@/models/registerTestKitPublicRequest";
import RegisterTestKitRequest from "@/models/registerTestKitRequest";
import User, { OidcUserProfile } from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { IAuthenticationService, ILogger } from "@/services/interfaces";
import { IPCRTestService } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";
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
export default class PCRTest extends Vue {
    // ### Props ###
    @Prop() serialNumber!: string;

    // ### Store ###
    @Action("addError", { namespace: "errorBanner" })
    addError!: (error: BannerError) => void;

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
    private pcrTestService!: IPCRTestService;

    // ### Data ###
    private redirectPath = "/pcrtest/" + this.serialNumber;

    private noSerialNumber = false;

    private noPhn = false;

    private oidcUser!: OidcUserProfile;

    private registrationComplete = false;

    private logger!: ILogger;

    private loading = false;

    private bcsclogo: string = Image06;

    private testTakenMinutesAgoValues = [
        { value: 1, text: "1 minute ago" },
        { value: 5, text: "5 minutes ago" },
        { value: 10, text: "10 minutes ago" },
        { value: 15, text: "15 minutes ago" },
        { value: 30, text: "30 minutes ago" },
        { value: 60, text: "1 hour ago" },
        { value: 120, text: "2 hours ago" },
        { value: 180, text: "3 hours ago" },
        { value: 240, text: "4 hours ago" },
        { value: 300, text: "5 hours ago" },
        { value: 360, text: "6 hours ago" },
        { value: 420, text: "7 hours ago" },
        { value: 480, text: "8 hours ago" },
        { value: 540, text: "9 hours ago" },
        { value: 600, text: "10 hours ago" },
        { value: 660, text: "11 hours ago" },
        { value: 720, text: "12 hours ago" },
        { value: 780, text: "13 hours ago" },
        { value: 840, text: "14 hours ago" },
        { value: 900, text: "15 hours ago" },
        { value: 960, text: "16 hours ago" },
        { value: 1020, text: "17 hours ago" },
        { value: 1080, text: "18 hours ago" },
        { value: 1140, text: "19 hours ago" },
        { value: 1200, text: "20 hours ago" },
        { value: 1260, text: "21 hours ago" },
        { value: 1320, text: "22 hours ago" },
        { value: 1380, text: "23 hours ago" },
        { value: 1440, text: "1 day ago" },
        { value: 2880, text: "2 days ago" },
        { value: 4320, text: "3 days ago" },
        { value: 5760, text: "4 days ago" },
        { value: 7200, text: "5 days ago" },
        { value: 8640, text: "6 days ago" },
        { value: 10080, text: "7 days ago" },
    ];

    private pcrTest: PCRTestData = {
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
        testKitId: this.serialNumber,
    };

    // Variables for PCRDataSource enum referenced in render
    private DSNONE = PCRDataSource.None;
    private DSKEYCLOAK = PCRDataSource.Keycloak;
    private DSMANUAL = PCRDataSource.Manual;

    // Set this to none initially to show options
    private dataSource: PCRDataSource = this.DSNONE;

    // ### Lifecycle ###
    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.pcrTestService = container.get<IPCRTestService>(
            SERVICE_IDENTIFIER.PCRTestService
        );
        this.setSidebarButtonState(false);
        this.setHeaderButtonState(false);
    }

    private destroyed() {
        this.setSidebarButtonState(true);
        this.setHeaderButtonState(true);
    }

    @Watch("oidcIsAuthenticated")
    private mounted() {
        if (!this.serialNumber || this.serialNumber === "") {
            this.noSerialNumber = true;
        }
        if (this.oidcIsAuthenticated) {
            this.dataSource = this.DSKEYCLOAK;
        }
        if (this.dataSource === PCRDataSource.Keycloak) {
            this.loading = true;
            // If the user chooses to log in with keycloak, log them in and get their info
            // Load the user name and current email
            let authenticationService = container.get<IAuthenticationService>(
                SERVICE_IDENTIFIER.AuthenticationService
            );
            var oidcUserPromise = authenticationService.getOidcUserProfile();
            Promise.all([oidcUserPromise])
                .then((results) => {
                    // Load oidc user details
                    if (results[0]) {
                        this.oidcUser = results[0];
                        this.pcrTest.hdid = this.oidcUser.hdid;
                    }
                    this.loading = false;
                })
                .catch((err) => {
                    this.logger.error(`Error loading profile: ${err}`);
                    this.addError(
                        ErrorTranslator.toBannerError("Profile loading", err)
                    );
                    this.loading = false;
                });
        }
    }

    // ### Getters ###
    private get pcrDataSource(): PCRDataSource {
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

    private get fullName(): string {
        if (this.oidcIsAuthenticated) {
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
    }

    // Sets the data source to either NONE (landing), KEYCLOAK (login), or MANUAL (registration)
    private setDataSource(dataSource: PCRDataSource) {
        this.loading = true;
        if (
            dataSource === PCRDataSource.Keycloak &&
            !this.oidcIsAuthenticated
        ) {
            this.oidcLogin(this.identityProviders[0].hint);
        } else {
            this.loading = false;
        }
        this.resetForm();
        this.dataSource = dataSource;
    }

    // ### Validation ###
    private isManualInputValidator = requiredIf(
        () => this.dataSource === this.DSMANUAL
    );

    private noPhnInputValidator = requiredIf(
        () => this.dataSource === this.DSMANUAL && this.noPhn
    );

    private phnValidator = (value: string) => {
        if (this.dataSource === this.DSMANUAL && !this.noPhn) {
            var phn = value.replace(/\D/g, "");
            return PHNValidator.IsValid(phn);
        } else {
            return true;
        }
    };

    private validations() {
        return {
            pcrTest: {
                firstName: {
                    required: this.isManualInputValidator,
                },
                lastName: {
                    required: this.isManualInputValidator,
                },
                phn: {
                    // issue
                    required: this.isManualInputValidator,
                    phnValidator: this.phnValidator,
                },
                dob: {
                    // issue
                    required: this.isManualInputValidator,
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
                    required: this.noPhnInputValidator,
                },
                city: {
                    required: this.noPhnInputValidator,
                },
                postalOrZip: {
                    required: this.noPhnInputValidator,
                    minLength: minLength(7),
                },
                testTakenMinutesAgo: {
                    required: required,
                    minValue: minValue(0),
                },
                testKitId: {
                    required: requiredIf(() => this.noSerialNumber),
                },
            },
        };
    }

    private isValid(param: Validation): boolean | undefined {
        return param.$dirty ? !param.$invalid : undefined;
    }

    // ### Form Actions ###
    private handleSubmit() {
        switch (this.dataSource) {
            // ### Submitted through OIDC
            case this.DSKEYCLOAK:
                var testKitRequest: RegisterTestKitRequest = {
                    hdid: this.oidcUser.hdid,
                    testKitId: this.pcrTest.testKitId,
                    testTakenMinutesAgo: this.pcrTest.testTakenMinutesAgo,
                };
                console.log(testKitRequest);
                this.loading = true;
                // Get user's hdid
                const hdid = this.user.hdid;
                this.pcrTestService
                    .registerTestKit(hdid, testKitRequest)
                    .then((response) => {
                        console.log("registerTestKit Response: ", response);
                        this.loading = false;
                        this.registrationComplete = true;
                    })
                    .catch((err) => {
                        this.logger.error(`registerTestKit Error: ${err}`);
                        this.addError(
                            ErrorTranslator.toBannerError(
                                "Test kit registration",
                                err
                            )
                        );
                        this.loading = false;
                    });
                break;
            // ### Submitted through manual input
            case this.DSMANUAL:
                var testKitPublicRequest: RegisterTestKitPublicRequest = {
                    firstName: this.pcrTest.firstName,
                    lastName: this.pcrTest.lastName,
                    phn: this.pcrTest.phn.replace(/\D/g, ""),
                    dob: this.pcrTest.dob,
                    contactPhoneNumber: this.pcrTest.contactPhoneNumber.replace(
                        /\D/g,
                        ""
                    ),
                    streetAddress: this.pcrTest.streetAddress,
                    city: this.pcrTest.city,
                    postalOrZip: this.pcrTest.postalOrZip.replace(/\D/g, ""),
                    testTakenMinutesAgo: this.pcrTest.testTakenMinutesAgo,
                    testKitId: this.pcrTest.testKitId,
                };
                console.log(testKitPublicRequest);
                this.loading = true;
                this.pcrTestService
                    .registerTestKitPublic(testKitPublicRequest)
                    .then((response) => {
                        console.log(
                            "registerTestKitPublic Response: ",
                            response
                        );
                        this.loading = false;
                        this.registrationComplete = true;
                    })
                    .catch((err) => {
                        this.logger.error(
                            `registerTestKitPublic Error: ${err}`
                        );
                        this.addError(
                            ErrorTranslator.toBannerError(
                                "Test kit registration",
                                err
                            )
                        );
                        this.loading = false;
                    });
                break;
            default:
                break;
        }
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
            testKitId: this.noSerialNumber ? "" : this.serialNumber,
        };
    }

    private handleCancel() {
        this.resetForm();
        this.dataSource = PCRDataSource.None;
    }

    // Auth
    private async oidcLogin(hint: string) {
        // if the login action returns it means that the user already had credentials.
        await this.authenticateOidc({
            idpHint: hint,
            redirectPath: this.oidcRedirectPath,
        })
            .then(() => {
                if (this.oidcIsAuthenticated) {
                    this.loading = false;
                }
            })
            .catch((error) => {
                this.loading = false;
                this.addError(
                    ErrorTranslator.toBannerError(
                        "User Information loading",
                        error
                    )
                );
            });
    }
}
</script>

<template>
    <div>
        <!-- LANDING -->
        <b-container v-if="isLoading">
            <LoadingComponent :is-loading="isLoading" :is-custom="true" />
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
                                data-testid="btnLogin"
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
                                id="btnManual"
                                aria-label="BC Services Card Login"
                                data-testid="btnManual"
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
            v-if="
                !registrationComplete && pcrDataSource !== DSNONE && !isLoading
            "
        >
            <b-row id="title" class="mt-4">
                <b-col>
                    <h1 class="h4 mb-2 font-weight-normal">
                        <strong>Register a test kit</strong>
                    </h1>
                </b-col>
            </b-row>
            <b-row>
                <b-col>
                    <form>
                        <!-- NAME -->
                        <b-row v-if="pcrDataSource === DSKEYCLOAK" class="pt-2">
                            <b-col>
                                <label for="pcrTestFullName">Name:</label>
                                <strong id="prcTestFullName">
                                    {{ fullName }}
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row v-if="noSerialNumber" class="mt-2">
                            <b-col>
                                <label for="testKitId">
                                    Test Kit Serial Number
                                </label>
                                <b-form-input
                                    id="testKitId"
                                    v-model="pcrTest.testKitId"
                                    type="text"
                                    placeholder="Serial Number"
                                    :state="isValid($v.pcrTest.testKitId)"
                                    @blur.native="$v.pcrTest.testKitId.$touch()"
                                />
                                <b-form-invalid-feedback
                                    v-if="
                                        $v.pcrTest.testKitId.$dirty &&
                                        !$v.pcrTest.testKitId.required
                                    "
                                    aria-label="Invalid Test Kit ID"
                                    force-show
                                >
                                    Invalid PCR Test Kit ID.
                                </b-form-invalid-feedback>
                            </b-col>
                        </b-row>
                        <b-row
                            v-if="pcrDataSource === DSMANUAL"
                            data-testid="pcrName"
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
                            data-testid="pcrPHN"
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
                                                placeholder="PHN"
                                                data-testid="phnInput"
                                                aria-label="Personal Health Number"
                                                :state="isValid($v.pcrTest.phn)"
                                                :disabled="noPhn"
                                                @blur.native="
                                                    $v.pcrTest.phn.$touch()
                                                "
                                            />
                                            <b-form-invalid-feedback
                                                :state="isValid($v.pcrTest.phn)"
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
                            data-testid="pcrHomeAddress"
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
                                        >
                                            City is required.
                                        </b-form-invalid-feedback>
                                    </b-col>
                                </b-row>
                                <b-row class="mt-2">
                                    <b-col>
                                        <label for="pcrZip"
                                            >Postal Code / ZIP</label
                                        >
                                        <b-form-input
                                            id="pcrZip"
                                            v-model="pcrTest.postalOrZip"
                                            v-mask="'A#A #A#'"
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
                        <b-row data-testid="pcrDOB" class="mt-2">
                            <b-col v-if="pcrDataSource === DSMANUAL">
                                <b-form-group
                                    label="Date of Birth"
                                    label-for="dob"
                                    :state="isValid($v.pcrTest.dob)"
                                >
                                    <hg-date-dropdown
                                        id="dob"
                                        v-model="pcrTest.dob"
                                        :state="isValid($v.pcrTest.dob)"
                                        :allow-future="false"
                                        data-testid="dobInput"
                                        aria-label="Date of Birth"
                                        @blur="$v.pcrTest.dob.$touch()"
                                    />
                                    <b-form-invalid-feedback
                                        v-if="
                                            $v.pcrTest.dob.$dirty &&
                                            !$v.pcrTest.dob.required
                                        "
                                        aria-label="Invalid Date of Birth"
                                        data-testid="feedbackDobIsRequired"
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
                                    >
                                        Date of birth must be in the past.
                                    </b-form-invalid-feedback>
                                </b-form-group>
                            </b-col>
                        </b-row>
                        <!-- MOBILE NUMBER -->
                        <b-row data-testid="pcrMobileNumber">
                            <b-col>
                                <b-row class="mt-2">
                                    <b-col>
                                        <label for="pcrMobileNumber"
                                            >Mobile Number (optional)
                                        </label>
                                        <b-form-input
                                            id="pcrMobileNumber"
                                            v-model="pcrTest.contactPhoneNumber"
                                            v-mask="smsMask"
                                            type="tel"
                                            placeholder="(250) 908 4345"
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
                                    data-testid="testTakenMinutesAgo"
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
                                    aria-label="Postal code is required"
                                    force-show
                                >
                                    Time since test taken is required.
                                </b-form-invalid-feedback>
                            </b-col>
                        </b-row>
                        <!-- PRIVACY STATEMENT -->
                        <b-row data-testid="pcrPrivacyStatement" class="pt-2">
                            <b-col
                                ><hg-button
                                    id="privacy-statement"
                                    aria-label="Privacy Statement"
                                    href="#"
                                    tabindex="0"
                                    variant="link"
                                    data-testid="btnPrivacyStatement"
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
                                    data-testid="btnCancel"
                                    @click="handleCancel"
                                >
                                    Cancel
                                </hg-button>
                            </b-col>
                            <b-col cols="8">
                                <hg-button
                                    id="btn-submit"
                                    block
                                    variant="primary"
                                    data-testid="btnSubmit"
                                    :disabled="$v.pcrTest.$invalid"
                                    @click="handleSubmit"
                                >
                                    Submit
                                </hg-button>
                            </b-col>
                        </b-row>
                    </form>
                </b-col>
            </b-row>

            <b-row data-testid="pcrFormActions"></b-row>
        </b-container>

        <!-- SUCCESS -->

        <b-container v-if="registrationComplete && !isLoading">
            <b-row class="text-center pt-3" align-v="center">
                <b-col>
                    <b-alert
                        data-testid="registrationSuccessBanner"
                        variant="success"
                        class="no-print"
                        :show="true"
                    >
                        <h4 data-testid="registrationSuccessBannerTitle">
                            Success!
                        </h4>
                        <span>
                            Your kit has been registered to your profile.
                        </span>
                        <div class="pt-2">
                            <router-link to="/">
                                <hg-button
                                    id="btnContinue"
                                    aria-label="Continue"
                                    data-testid="btnContinue"
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
