<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faInfoCircle } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Prop, Watch } from "vue-property-decorator";
import { required } from "vuelidate/lib/validators";
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
import RegisterTestKitRequest from "@/models/registerTestKitRequest";
import User, { OidcUserProfile } from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { IAuthenticationService, ILogger } from "@/services/interfaces";
import { IPCRTestService } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";
import { Mask, phnMask, smsMask } from "@/utility/masks";

library.add(faInfoCircle);

@Component({
    components: {
        LoadingComponent,
        ErrorCard: ErrorCardComponent,
        "hg-date-dropdown": HgDateDropdownComponent,
        "hg-time-dropdown": HgTimeDropdownComponent,
    },
})
export default class PCRTest extends Vue {
    // Props
    @Prop() serialNumber!: string;

    // Store
    @Getter("user", { namespace: "user" }) user!: User;

    @Action("addError", { namespace: "errorBanner" })
    addError!: (error: BannerError) => void;

    @Action("authenticateOidc", { namespace: "auth" })
    authenticateOidc!: (params: {
        idpHint: string;
        redirectPath: string;
    }) => Promise<void>;

    @Getter("oidcIsAuthenticated", { namespace: "auth" })
    oidcIsAuthenticated!: boolean;

    @Getter("identityProviders", { namespace: "config" })
    identityProviders!: IdentityProviderConfiguration[];

    // Service

    private pcrTestService!: IPCRTestService;

    // Data
    private redirectPath = "/pcrtest/" + this.serialNumber;

    private oidcUser!: OidcUserProfile;

    private registrationComplete = false;

    private logger!: ILogger;

    private loading = false;

    private bcsclogo: string = Image06;

    private pcrTest: PCRTestData = {
        firstName: "",
        lastName: "",
        phn: "",
        dob: "",
        contactPhoneNumber: "",
        streetAddress: "",
        city: "",
        postalCode: "",
        dateOfTest: "",
        timeOfTest: "",
        hdid: "",
        testKitCid: this.serialNumber,
    };

    // Variables for PCRDataSource enum referenced in render
    private DSNONE = PCRDataSource.None;
    private DSKEYCLOAK = PCRDataSource.Keycloak;
    private DSMANUAL = PCRDataSource.Manual;

    // Set this to none initially
    private dataSource: PCRDataSource = this.DSNONE;

    // Lifecycle
    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.pcrTestService = container.get<IPCRTestService>(
            SERVICE_IDENTIFIER.PCRTestService
        );
    }

    @Watch("oidcIsAuthenticated")
    private mounted() {
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
                        console.log(this.oidcUser);
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

    // Getters
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
        this.pcrTest = {
            firstName: "",
            lastName: "",
            phn: "",
            dob: "",
            contactPhoneNumber: "",
            streetAddress: "",
            city: "",
            postalCode: "",
            dateOfTest: "",
            timeOfTest: "",
            hdid: "",
            testKitCid: this.serialNumber,
        };
        this.dataSource = dataSource;
    }

    // Validation
    private validations() {
        return {
            dateOfBirth: {
                required: required,
                maxValue: (value: string) =>
                    new DateWrapper(value).isBefore(new DateWrapper()),
            },
            dateOfTest: {
                required: required,
                maxValue: (value: string) =>
                    new DateWrapper(value).isBefore(new DateWrapper()),
            },
            timeOfTest: {
                required: required,
            },
        };
    }

    private isValid(param: Validation): boolean | undefined {
        return param.$dirty ? !param.$invalid : undefined;
    }

    // Form actions
    private handleSubmit() {
        console.log("Submitting...", this.pcrTest);
        switch (this.dataSource) {
            case this.DSKEYCLOAK:
                console.log("KEYCLOAK");
                break;
            case this.DSMANUAL:
                var request: RegisterTestKitRequest = {};
                console.log(request);
                console.log("MANUAL");
                // Submit the manual test kit registration
                break;
            default:
                console.log("NONE");
                break;
        }
    }

    private handleCancel() {
        this.pcrTest = {
            firstName: "",
            lastName: "",
            phn: "",
            dob: "",
            contactPhoneNumber: "",
            streetAddress: "",
            city: "",
            postalCode: "",
            dateOfTest: "",
            timeOfTest: "",
            hdid: "",
            testKitCid: this.serialNumber,
        };
        this.dataSource = PCRDataSource.None;
    }

    // Auth
    private async oidcLogin(hint: string) {
        // if the login action returns it means that the user already had credentials.
        await this.authenticateOidc({
            idpHint: hint,
            redirectPath: this.redirectPath,
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
                    <b-row class="pt-2">
                        <b-col>Kit's serial #: {{ serialNumber }} </b-col>
                    </b-row>
                </b-col>
            </b-row>
            <b-row>
                <b-col>
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
                                            placeholder="Jay"
                                            required
                                        />
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
                                            placeholder="Doe"
                                            required
                                        />
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
                                                placeholder="9737 364 347"
                                                data-testid="phnInput"
                                                aria-label="Personal Health Number"
                                            />
                                        </b-form-group>
                                    </b-col>
                                </b-row>
                            </b-col>
                        </b-row>
                        <!-- NO PHN -->
                        <b-row
                            v-if="
                                pcrDataSource === DSMANUAL && pcrTest.phn === ''
                            "
                            data-testid="pcrHomeAddress"
                        >
                            <b-col>
                                <b-row>
                                    <b-col
                                        ><span class="phn-info">
                                            If you don't have a Personal Health
                                            Number, please enter your home
                                            address.
                                        </span></b-col
                                    >
                                </b-row>
                                <b-row class="mt-2">
                                    <b-col>
                                        <label for="pcrStreetAddress"
                                            >Street Address</label
                                        >
                                        <b-form-input
                                            id="pcrStreetAddress"
                                            v-model="pcrTest.streetAddress"
                                            type="text"
                                            placeholder="123 Some Street"
                                        />
                                    </b-col>
                                </b-row>
                                <b-row class="mt-2">
                                    <b-col>
                                        <label for="pcrCity">City</label>
                                        <b-form-input
                                            id="pcrCity"
                                            v-model="pcrTest.city"
                                            type="text"
                                            placeholder="This Town"
                                        />
                                    </b-col>
                                </b-row>
                                <b-row class="mt-2">
                                    <b-col>
                                        <label for="pcrZip"
                                            >Postal Code / ZIP</label
                                        >
                                        <b-form-input
                                            id="pcrZip"
                                            v-model="pcrTest.postalCode"
                                            v-mask="'A#A #A#'"
                                            type="text"
                                            placeholder="V0V 0V0"
                                        />
                                    </b-col>
                                </b-row>
                            </b-col>
                        </b-row>
                        <!-- DOB -->
                        <b-row data-testid="pcrDOB" class="mt-2">
                            <b-col v-if="pcrDataSource === DSMANUAL">
                                <b-form-group
                                    label="Date of Birth"
                                    label-for="dateOfBirth"
                                    :state="isValid($v.dateOfBirth)"
                                >
                                    <hg-date-dropdown
                                        id="dateOfBirth"
                                        v-model="pcrTest.dob"
                                        :state="isValid($v.dateOfBirth)"
                                        :allow-future="false"
                                        data-testid="dateOfBirthInput"
                                        aria-label="Date of Birth"
                                        @blur="$v.dateOfBirth.$touch()"
                                    />
                                    <b-form-invalid-feedback
                                        v-if="
                                            $v.dateOfBirth.$dirty &&
                                            !$v.dateOfBirth.required
                                        "
                                        aria-label="Invalid Date of Birth"
                                        data-testid="feedbackDobIsRequired"
                                        force-show
                                    >
                                        A valid date of birth is required.
                                    </b-form-invalid-feedback>
                                    <b-form-invalid-feedback
                                        v-else-if="
                                            $v.dateOfBirth.$dirty &&
                                            !$v.dateOfBirth.maxValue
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
                                        <label
                                            v-if="pcrDataSource === DSMANUAL"
                                            for="pcrMobileNumber"
                                            >Mobile Number</label
                                        >
                                        <label
                                            v-if="pcrDataSource === DSKEYCLOAK"
                                            for="pcrMobileNumber"
                                            >Mobile Number (optional)
                                        </label>
                                        <b-form-input
                                            id="pcrMobileNumber"
                                            v-model="pcrTest.contactPhoneNumber"
                                            v-mask="smsMask"
                                            type="tel"
                                            placeholder="(250) 908 4345"
                                            required
                                        />
                                    </b-col>
                                </b-row>
                            </b-col>
                        </b-row>
                        <!-- DATE OF TEST -->
                        <b-row data-testid="pcrDateOfTest" class="mt-2">
                            <b-col>
                                <b-form-group
                                    label="Date of Test"
                                    label-for="dateOfTest"
                                    :state="isValid($v.dateOfTest)"
                                >
                                    <hg-date-dropdown
                                        id="dateOfTest"
                                        v-model="pcrTest.dateOfTest"
                                        :state="isValid($v.dateOfTest)"
                                        :allow-future="false"
                                        data-testid="dateOfTestInput"
                                        aria-label="Date of Test"
                                        @blur="$v.dateOfTest.$touch()"
                                    />
                                    <b-form-invalid-feedback
                                        v-if="
                                            $v.dateOfTest.$dirty &&
                                            !$v.dateOfTest.required
                                        "
                                        aria-label="Invalid Date of Test"
                                        data-testid="feedbackDobIsRequired"
                                        force-show
                                    >
                                        A valid test date is required.
                                    </b-form-invalid-feedback>
                                    <b-form-invalid-feedback
                                        v-else-if="
                                            $v.dateOfTest.$dirty &&
                                            !$v.dateOfTest.maxValue
                                        "
                                        aria-label="Invalid Date of Test"
                                        force-show
                                    >
                                        Test date must be in the past.
                                    </b-form-invalid-feedback>
                                </b-form-group>
                            </b-col>
                        </b-row>
                        <!-- TIME OF TEST -->
                        <b-row data-testid="pcrTimeOfTest">
                            <b-col>
                                <b-form-group
                                    label="Time of Test"
                                    label-for="timeOfTest"
                                    :state="isValid($v.timeOfTest)"
                                >
                                    <hg-time-dropdown
                                        id="timeOfTest"
                                        v-model="pcrTest.timeOfTest"
                                        :state="isValid($v.timeOfTest)"
                                        :allow-future="false"
                                        data-testid="timeOfTestInput"
                                        aria-label="Time of Test"
                                        @blur="$v.timeOfTest.$touch()"
                                    />
                                    <b-form-invalid-feedback
                                        v-if="
                                            $v.timeOfTest.$dirty &&
                                            !$v.timeOfTest.required
                                        "
                                        aria-label="Invalid Time of Test"
                                        data-testid="feedbackTotIsRequired"
                                        force-show
                                    >
                                        A valid test time is required.
                                    </b-form-invalid-feedback>
                                    <b-form-invalid-feedback
                                        v-else-if="
                                            $v.timeOfTest.$dirty &&
                                            !$v.timeOfTest.maxValue
                                        "
                                        aria-label="Invalid Time of Test"
                                        force-show
                                    >
                                        Test date must be in the past.
                                    </b-form-invalid-feedback>
                                </b-form-group>
                            </b-col>
                        </b-row>
                        <!-- PRIVACY STATEMENT -->
                        <b-row data-testid="pcrPrivacyStatement">
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
                                    :disabled="!$v.$invalid"
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
