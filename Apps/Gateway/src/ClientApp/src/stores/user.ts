import { defineStore } from "pinia";
import { computed, ref } from "vue";
import User, { OidcUserInfo } from "@/models/user";
import { DateWrapper, StringISODateTime } from "@/models/dateWrapper";
import { LoadStatus } from "@/models/storeOperations";
import Patient from "@/models/patient";
import { QuickLink } from "@/models/quickLink";
import UserPreferenceType from "@/constants/userPreferenceType";
import { QuickLinkUtil } from "@/utility/quickLinkUtil";
import UserProfile, { CreateUserRequest } from "@/models/userProfile";
import {
    ILogger,
    IPatientService,
    IUserProfileService,
} from "@/services/interfaces";
import PreferenceUtil from "@/utility/preferenceUtil";
import { ResultError } from "@/models/errors";
import { useAppStore } from "@/stores/app";
import { useErrorStore } from "@/stores/error";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { UserPreference } from "@/models/userPreference";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { container } from "@/ioc/container";
export const useUserStore = defineStore("user", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    const patientService = container.get<IPatientService>(
        SERVICE_IDENTIFIER.PatientService
    );
    const userProfileService = container.get<IUserProfileService>(
        SERVICE_IDENTIFIER.UserProfileService
    );

    const appStore = useAppStore();
    const errorStore = useErrorStore();

    const user = ref(new User());
    const oidcUserInfo = ref<OidcUserInfo>();
    const smsResendDateTime = ref<DateWrapper>();
    const error = ref<unknown>();
    const statusMessage = ref("");
    const status = ref(LoadStatus.NONE);
    const patient = ref(new Patient());
    const patientRetrievalFailed = ref(false);

    const lastLoginDateTime = computed(() => {
        const loginDateTimes = user.value.lastLoginDateTimes;
        const loginDateTimesLength = user.value.lastLoginDateTimes.length;

        // Check user profile history
        if (loginDateTimesLength > 1) {
            // Return the second entry as that is the actual last login, whereas the first entry is the current login.
            return loginDateTimes[1];
        }
        // First time logging so there is no last login.
        return undefined;
    });

    const isValidIdentityProvider = computed(() => {
        return oidcUserInfo.value === undefined
            ? false
            : oidcUserInfo.value.idp === "BCSC" ||
                  oidcUserInfo.value.idp === undefined;
    });

    const userIsRegistered = computed(() => {
        return user.value === undefined
            ? false
            : user.value.acceptedTermsOfService;
    });

    const userIsActive = computed(() => {
        return user.value === undefined ? false : !user.value.closedDateTime;
    });
    
    const hasTermsOfServiceUpdated = computed(() =>
        user === undefined ? false : user.hasTermsOfServiceUpdated
    );

    const quickLinks = computed(() => {
        const preference =
            user.value.preferences[UserPreferenceType.QuickLinks];
        if (preference === undefined) {
            return undefined;
        }
        return QuickLinkUtil.toQuickLinks(preference.value);
    });

    const isLoading = computed(() => {
        return status.value === LoadStatus.REQUESTED;
    });

    function setOidcUserInfo(userInfo: OidcUserInfo) {
        user.value.hdid = userInfo.hdid;
        oidcUserInfo.value = userInfo;
        setLoadedStatus();
    }

    function setProfileUserData(userProfile: UserProfile) {
        if (userProfile) {
            PreferenceUtil.setDefaultValue(
                userProfile.preferences,
                UserPreferenceType.HideVaccineCardQuickLink,
                "false"
            );
            PreferenceUtil.setDefaultValue(
                userProfile.preferences,
                UserPreferenceType.HideOrganDonorQuickLink,
                "false"
            );
            PreferenceUtil.setDefaultValue(
                userProfile.preferences,
                UserPreferenceType.HideImmunizationRecordQuickLink,
                "false"
            );
        }

        user.value.acceptedTermsOfService = userProfile
            ? userProfile.acceptedTermsOfService
            : false;
        user.value.hasTermsOfServiceUpdated = userProfile
            ? userProfile.hasTermsOfServiceUpdated ?? false
            : false;
        user.value.lastLoginDateTime = userProfile?.lastLoginDateTime;
        user.value.lastLoginDateTimes = userProfile
            ? userProfile.lastLoginDateTimes
            : [];
        user.value.closedDateTime = userProfile
            ? userProfile.closedDateTime
            : undefined;
        user.value.preferences = userProfile ? userProfile.preferences : {};
        user.value.hasEmail = Boolean(userProfile?.email);
        user.value.verifiedEmail = userProfile?.isEmailVerified === true;
        user.value.hasSMS = Boolean(userProfile?.smsNumber);
        user.value.verifiedSMS = userProfile?.isSMSNumberVerified === true;
        user.value.hasTourUpdated = userProfile?.hasTourUpdated === true;

        logger.verbose(`User: ${JSON.stringify(user.value)}`);

        setLoadedStatus(patient.value.hdid !== undefined);
    }
    
    function setEmailVerified() {
        user.value.verifiedEmail = true;
    }

    function updateSMSResendDateTime(date: DateWrapper) {
        smsResendDateTime.value = date;
        setLoadedStatus();
    }

    function setUserError(errorMessage: string) {
        error.value = true;
        statusMessage.value = errorMessage;
        status.value = LoadStatus.ERROR;
    }

    function setLoadedStatus(isCompletelyLoaded: boolean = true) {
        error.value = false;
        statusMessage.value = "Success";
        status.value = isCompletelyLoaded
            ? LoadStatus.LOADED
            : LoadStatus.PARTIALLY_LOADED;
    }

    function setPatient(incomingPatient: Patient) {
        logger.debug(`setPatient: ${JSON.stringify(incomingPatient)}`);
        patient.value = incomingPatient;
        setLoadedStatus(user.value.hdid !== undefined);
    }

    function handleError(
        resultError: ResultError,
        errorType: ErrorType,
        errorSource: ErrorSourceType
    ) {
        logger.error(`Error: ${JSON.stringify(resultError)}`);
        setUserError(resultError.resultMessage);

        if (resultError.statusCode === 429) {
            const errorKey = "page";
            if (errorType === ErrorType.Retrieve) {
                errorStore.setTooManyRequestsWarning(errorKey);
            } else {
                errorStore.setTooManyRequestsError(errorKey);
            }
        } else {
            errorStore.addError(errorType, errorSource, resultError.traceId);
        }
    }

    function createProfile(request: CreateUserRequest): Promise<void> {
        return new Promise((resolve, reject) =>
            userProfileService
                .createProfile(request)
                .then((userProfile) => {
                    logger.verbose(
                        `User Profile: ${JSON.stringify(userProfile)}`
                    );
                    setProfileUserData(userProfile);
                    resolve();
                })
                .catch((resultError: ResultError) => {
                    handleError(
                        resultError,
                        ErrorType.Create,
                        ErrorSourceType.Profile
                    );
                    reject(resultError);
                })
        );
    }

    function retrieveProfile(): Promise<void> {
        return new Promise((resolve, reject) =>
            userProfileService
                .getProfile(user.value.hdid)
                .then((userProfile) => {
                    logger.verbose(
                        `User Profile: ${JSON.stringify(userProfile)}`
                    );
                    setProfileUserData(userProfile);
                    resolve();
                })
                .catch((resultError: ResultError) => {
                    setUserError(resultError.resultMessage);
                    reject(resultError);
                })
        );
    }

    function updateAcceptedTerms(termsOfServiceId: string): Promise<void> {
        return new Promise((resolve, reject) =>
            userProfileService
                .updateAcceptedTerms(user.value.hdid, termsOfServiceId)
                .then((userProfile) => {
                    logger.verbose(
                        `User Profile: ${JSON.stringify(userProfile)}`
                    );
                    setProfileUserData(userProfile);
                    resolve();
                })
                .catch((resultError: ResultError) => {
                    setUserError(resultError.resultMessage);
                    reject(resultError);
                })
        );
    }

    function updateUserEmail(emailAddresss: string): Promise<void> {
        const userProfileService = container.get<IUserProfileService>(
            SERVICE_IDENTIFIER.UserProfileService
        );

        return new Promise((resolve, reject) =>
            userProfileService
                .updateEmail(user.value.hdid, emailAddresss)
                .then(() => resolve())
                .catch((resultError: ResultError) => {
                    setUserError(resultError.resultMessage);
                    reject(resultError);
                })
        );
    }

    function setUserPreference(preference: UserPreference): Promise<void> {
        return new Promise((resolve, reject) => {
            let setPreferencePromise: Promise<UserPreference>;
            if (preference.hdId == undefined) {
                logger.debug(`setUserPreference: creating new preference`);
                setPreferencePromise = userProfileService.createUserPreference(
                    user.value.hdid,
                    { ...preference, hdId: user.value.hdid }
                );
            } else {
                logger.debug(`setUserPreference: updating existing preference`);
                setPreferencePromise = userProfileService.updateUserPreference(
                    user.value.hdid,
                    preference
                );
            }

            return setPreferencePromise
                .then((result) => {
                    logger.debug(
                        `setUserPreference: ${JSON.stringify(result)}`
                    );
                    if (result) {
                        user.value.preferences = Object.assign(
                            {},
                            user.value.preferences,
                            {
                                [result.preference]: result,
                            }
                        );
                        setLoadedStatus();
                    }
                    resolve();
                })
                .catch((resultError: ResultError) => {
                    setUserError(resultError.resultMessage);
                    reject(resultError);
                });
        });
    }

    function clearUserData() {
        user.value = new User();
        oidcUserInfo.value = undefined;
        patient.value = new Patient();
        patientRetrievalFailed.value = false;
        smsResendDateTime.value = undefined;
        error.value = false;
        statusMessage.value = "";
        status.value = LoadStatus.NONE;
    }

    function updateQuickLinks(
        hdid: string,
        quickLinks: QuickLink[]
    ): Promise<void> {
        const jsonString = QuickLinkUtil.toString(quickLinks);
        let preference: UserPreference =
            user.value.preferences[UserPreferenceType.QuickLinks];

        if (preference === undefined) {
            preference = {
                preference: UserPreferenceType.QuickLinks,
                value: jsonString,
                version: 0,
                createdDateTime: new DateWrapper().toISO(),
            };
        } else {
            preference = { ...preference, value: jsonString };
        }

        return setUserPreference(preference);
    }

    function validateEmail(inviteKey: string) {
        return new Promise((resolve, reject) => {
            userProfileService
                .validateEmail(user.value.hdid, inviteKey)
                .then((result) => {
                    if (result.resourcePayload === true) {
                        user.value.verifiedEmail = true;
                    }
                    resolve(result);
                })
                .catch((resultError: ResultError) => {
                    handleError(
                        resultError,
                        ErrorType.Update,
                        ErrorSourceType.User
                    );
                    reject(resultError);
                });
        });
    }

    function closeUserAccount(): Promise<void> {
        return new Promise((resolve, reject) => {
            userProfileService
                .closeAccount(user.value.hdid)
                .then((userProfile) => {
                    setProfileUserData(userProfile);
                    resolve();
                })
                .catch((resultError: ResultError) => {
                    setUserError(resultError.resultMessage);
                    reject(resultError);
                });
        });
    }

    function recoverUserAccount(): Promise<void> {
        return new Promise((resolve, reject) => {
            userProfileService
                .recoverAccount(user.value.hdid)
                .then((userProfile) => {
                    logger.debug(
                        `recoverUserAccount: ${JSON.stringify(userProfile)}`
                    );
                    setProfileUserData(userProfile);
                    resolve();
                })
                .catch((resultError: ResultError) => {
                    setUserError(resultError.resultMessage);
                    reject(resultError);
                });
        });
    }

    function retrieveEssentialData(): Promise<void> {
        return new Promise((resolve) => {
            status.value = LoadStatus.REQUESTED;
            patientService
                .getPatient(user.value.hdid)
                .then((result: Patient) => {
                    if (!result) {
                        logger.debug("Patient retrieval failed");
                        patientRetrievalFailed.value = true;
                        resolve();
                        return;
                    }
                    setPatient(result);
                    userProfileService
                        .getProfile(user.value.hdid)
                        .then((userProfile) => {
                            logger.debug(
                                `User Profile: ${JSON.stringify(userProfile)}`
                            );
                            setProfileUserData(userProfile);
                            resolve();
                        })
                        .catch((resultError: ResultError) => {
                            if (error.statusCode === 429) {
                                logger.debug(
                                    "User profile retrieval failed because of too many requests"
                                );
                                appStore.setAppError(AppErrorType.TooManyRequests);
                            } else {
                                logger.debug("User profile retrieval failed");
                                appStore.setAppError(AppErrorType.General);
                            }
                            resolve();
                        });
                })
                .catch((resultError: ResultError) => {
                    if (error.statusCode === 429) {
                        logger.debug(
                            "Patient retrieval failed because of too many requests"
                        );
                        appStore.setAppError(AppErrorType.TooManyRequests);
                    } else {
                        logger.debug("Patient retrieval failed");
                        patientRetrievalFailed.value = true;
                    }
                    resolve();
                });
        });
    }

    return {
        user,
        oidcUserInfo,
        smsResendDateTime,
        error,
        statusMessage,
        status,
        patient,
        patientRetrievalFailed,
        lastLoginDateTime,
        isValidIdentityProvider,
        userIsRegistered,
        userIsActive,
        hasTermsOfServiceUpdated,
        quickLinks,
        isLoading,
        setEmailVerified,
        updateSMSResendDateTime,
        setOidcUserInfo,
        clearUserData,
        createProfile,
        retrieveProfile,
        updateUserEmail,
        updateAcceptedTerms,
        setUserPreference,
        updateQuickLinks,
        validateEmail,
        closeUserAccount,
        recoverUserAccount,
        retrieveEssentialData,
    };
});
