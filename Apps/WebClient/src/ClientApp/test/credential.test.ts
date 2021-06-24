import "@/plugins/inversify.config";

import StoreOptionsStub from "@test/stubs/store/storeOptionsStub";
import { createLocalVue, shallowMount, Wrapper } from "@vue/test-utils";
import { BButton, BCard } from "bootstrap-vue";
import VueContentPlaceholders from "vue-content-placeholders";
import VueRouter from "vue-router";
import Vuex, { Store } from "vuex";

import CredentialCollectionCardView from "@/components/credential/credentialCollectionCard.vue";
import CredentialInstructionsView from "@/components/credential/credentialInstructions.vue";
import CredentialListView from "@/components/credential/credentialList.vue";
import CredentialManagementView from "@/components/credential/credentialManagement.vue";
import { ImmunizationEvent } from "@/models/immunizationModel";
import {
    ConnectionStatus,
    CredentialStatus,
    WalletConnection,
    WalletCredential,
} from "@/models/wallet";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";
import { GatewayStoreOptions, RootState } from "@/store/types";
import CredentialsView from "@/views/credentials.vue";

let store: Store<RootState>;

const isAVueInstance = "is a Vue instance";
const walletConnectionId = "ed5026a8-a5f3-43ff-acb3-10ff46c18211";
const issuerConnectionId = "198fbae1-c2b4-4075-b46b-61e844d63696";
const immunizationIds = [
    "7b6df3eb-72b2-4ed6-1f90-08d92ba03fd1",
    "8b6df3eb-72b2-4ed6-1f90-08d92ba03fd2",
];

function createCredentialsViewWrapper(
    options?: GatewayStoreOptions
): Wrapper<CredentialsView> {
    const localVue = createLocalVue();
    localVue.use(Vuex);
    localVue.use(VueRouter);
    localVue.use(VueContentPlaceholders);
    if (options === undefined) {
        options = new StoreOptionsStub();
    }

    options = options as GatewayStoreOptions;

    store = new Vuex.Store(options);

    return shallowMount(CredentialsView, {
        localVue,
        store: store,
        stubs: {},
    });
}

function createCredentialInstructionsViewWrapper(
    // eslint-disable-next-line @typescript-eslint/ban-types
    propsData: object,
    options?: GatewayStoreOptions
): Wrapper<CredentialInstructionsView> {
    const localVue = createLocalVue();
    localVue.use(Vuex);
    localVue.use(VueRouter);
    localVue.use(VueContentPlaceholders);
    if (options === undefined) {
        options = new StoreOptionsStub();
    }

    options = options as GatewayStoreOptions;

    store = new Vuex.Store(options);

    const ChildComponentStub = {
        name: "LoadingComponent",
        template: "<div v-show='isLoading' id='loadingStub'/>",
        props: ["is-loading"],
    };

    return shallowMount(CredentialInstructionsView, {
        localVue,
        propsData: propsData,
        store: store,
        stubs: {
            LoadingComponent: ChildComponentStub,
            "hg-icon": true,
            "hg-button": true,
            "page-title": true,
        },
    });
}

function createCredentialManagementViewWrapper(
    options?: GatewayStoreOptions
): Wrapper<CredentialManagementView> {
    const localVue = createLocalVue();
    localVue.use(Vuex);
    localVue.use(VueRouter);
    localVue.use(VueContentPlaceholders);
    if (options === undefined) {
        options = new StoreOptionsStub();
    }

    options = options as GatewayStoreOptions;

    store = new Vuex.Store(options);

    return shallowMount(CredentialManagementView, {
        localVue,
        store: store,
        stubs: {
            "hg-button": BButton,
            "page-title": true,
        },
    });
}

function createCredentialCollectionCardViewWrapper(
    options?: GatewayStoreOptions
): Wrapper<CredentialCollectionCardView> {
    const localVue = createLocalVue();
    localVue.use(Vuex);
    localVue.use(VueRouter);
    localVue.use(VueContentPlaceholders);
    if (options === undefined) {
        options = new StoreOptionsStub();
    }

    options = options as GatewayStoreOptions;

    store = new Vuex.Store(options);

    return shallowMount(CredentialCollectionCardView, {
        localVue,
        store: store,
        stubs: {
            "b-card": BCard,
            "hg-button": BButton,
            "hg-icon": true,
            "status-label": true,
        },
    });
}

function createCredentialListViewWrapper(
    options?: GatewayStoreOptions
): Wrapper<CredentialListView> {
    const localVue = createLocalVue();
    localVue.use(Vuex);
    localVue.use(VueRouter);
    localVue.use(VueContentPlaceholders);
    if (options === undefined) {
        options = new StoreOptionsStub();
    }

    options = options as GatewayStoreOptions;

    store = new Vuex.Store(options);

    return shallowMount(CredentialListView, {
        localVue,
        store: store,
        stubs: {
            "hg-button": BButton,
            "hg-icon": true,
        },
    });
}

function getConnection(
    status: ConnectionStatus,
    credentials: WalletCredential[]
): WalletConnection {
    return {
        walletConnectionId: walletConnectionId,
        hdid: "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A",
        issuerConnectionId: issuerConnectionId,
        connectedDate: "2021-06-10T16:52:26.278145",
        disconnectedDate: "",
        invitationEndpoint:
            "https://484e8cc4a469.ngrok.io/?c_i=eyJAdHlwZSI6ICJkaWQ6c292OkJ6Q2JzTlloTXJqSGlxWkRUVUFTSGc7c3BlYy9jb25uZWN0aW9ucy8xLjAvaW52aXRhdGlvbiIsICJAaWQiOiAiYmI3YzMyMWQtMDE1Ni00NTA4LTlhMWQtYjA5NjBjZDg0ODgyIiwgInJlY2lwaWVudEtleXMiOiBbIkcyb0NGa2E0ZU1DRTVON3ltcnBWb3RRTUN1dm1RQkdxOGtkajZhdjc3aVpzIl0sICJsYWJlbCI6ICJIRy1BZ2VudCIsICJzZXJ2aWNlRW5kcG9pbnQiOiAiaHR0cHM6Ly80ODRlOGNjNGE0Njkubmdyb2suaW8ifQ==",
        status: status,
        version: "27014",
        credentials: credentials,
    };
}

function getCreatedCredential(): WalletCredential {
    return {
        credentialId: "fec08b2a-d371-4c38-9a85-d0a989e025c0",
        walletConnectionId: walletConnectionId,
        issuerConnectionId: issuerConnectionId,
        sourceType: "Immunization",
        sourceId: immunizationIds[0],
        addedDate: "2021-06-10T16:53:30.907194",
        revokedDate: undefined,
        status: CredentialStatus.Created,
        version: "27019",
    };
}

function getAddedCredential(): WalletCredential {
    return {
        credentialId: "eec08b2a-d371-4c38-9a85-d0a989e025c9",
        walletConnectionId: walletConnectionId,
        issuerConnectionId: issuerConnectionId,
        sourceType: "Immunization",
        sourceId: immunizationIds[1],
        addedDate: "2021-06-10T16:53:30.907194",
        revokedDate: undefined,
        status: CredentialStatus.Added,
        version: "27019",
    };
}

function getImmunizations(): ImmunizationEvent[] {
    return [
        {
            id: immunizationIds[0],
            isSelfReported: false,
            location: "TEST LOCATION 1",
            dateOfImmunization: "2013-09-20T00:00:00",
            status: "Completed",
            targetedDisease: "Covid",
            providerOrClinic: "TEST LOCATION 1",
            immunization: {
                name: "Mocked Name",
                immunizationAgents: [
                    {
                        code: "414003004",
                        name: "DTaP-IPV",
                        lotNumber: "123456",
                        productName: "COVID",
                    },
                    {
                        code: "777003004",
                        name: "Another Agent",
                        lotNumber: "1234567789",
                        productName: "COVID 2",
                    },
                ],
            },
            forecast: undefined,
        },
        {
            id: immunizationIds[1],
            isSelfReported: false,
            location: "TEST LOCATION 2",
            dateOfImmunization: "2010-03-28T00:00:00",
            status: "Completed",
            providerOrClinic: "Provider A",
            targetedDisease: "Covid",
            immunization: {
                name: "Mocked Name",
                immunizationAgents: [
                    {
                        code: "414004005",
                        name: "DTaP-IPV-Hib",
                        lotNumber: "LOT A",
                        productName: "Product A",
                    },
                ],
            },
            forecast: undefined,
        },
    ];
}

describe("Credentials View", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");

    test(isAVueInstance, () => {
        const wrapper = createCredentialsViewWrapper();
        expect(wrapper).toBeTruthy();
    });
});

describe("Credential Instructions Component", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");

    test(isAVueInstance, () => {
        const wrapper = createCredentialInstructionsViewWrapper({
            isLoading: true,
            hasCovidImmunizations: false,
        });
        expect(wrapper).toBeTruthy();
    });

    test("Loading state", () => {
        let wrapper = createCredentialInstructionsViewWrapper({
            isLoading: true,
            hasCovidImmunizations: true,
        });

        // Check values
        expect(wrapper.find("#loadingStub").isVisible()).toBe(true);

        wrapper = createCredentialInstructionsViewWrapper({
            isLoading: false,
            hasCovidImmunizations: true,
        });

        // Check values
        expect(wrapper.find("#loadingStub").isVisible()).toBe(false);
    });

    test("No immunizations", () => {
        const wrapper = createCredentialInstructionsViewWrapper({
            isLoading: false,
            hasCovidImmunizations: false,
        });

        // Check values
        expect(
            wrapper.find("[data-testid=noCovidImmunizations]").exists()
        ).toBe(true);
        expect(
            wrapper.find("[data-testid=hasCovidImmunizations]").exists()
        ).toBe(false);
    });

    test("Some immunizations", () => {
        const wrapper = createCredentialInstructionsViewWrapper({
            isLoading: false,
            hasCovidImmunizations: true,
        });

        // Check values
        expect(
            wrapper.find("[data-testid=noCovidImmunizations]").exists()
        ).toBe(false);
        expect(
            wrapper.find("[data-testid=hasCovidImmunizations]").exists()
        ).toBe(true);
    });
});

describe("Credential Management Component", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");

    const credentialListSelector = "[data-testid=credentialList]";
    const credentialVerificationSelector =
        "[data-testid=credentialVerification]";

    test(isAVueInstance, () => {
        const wrapper = createCredentialManagementViewWrapper();
        expect(wrapper).toBeTruthy();
    });

    test("Disconnected", () => {
        const wrapper = createCredentialManagementViewWrapper();
        expect(wrapper.find(credentialListSelector).exists()).toBe(false);
        expect(wrapper.find(credentialVerificationSelector).exists()).toBe(
            false
        );
    });

    test("Connected", () => {
        // Setup vuex store
        const options = new StoreOptionsStub();
        const credentials = [getAddedCredential()];
        options.modules.credential.getters.connection = () =>
            getConnection(ConnectionStatus.Connected, credentials);
        options.modules.credential.getters.credentials = () => credentials;
        const wrapper = createCredentialManagementViewWrapper(options);

        // Check values
        expect(wrapper.find(credentialListSelector).exists()).toBe(true);
        expect(wrapper.find(credentialVerificationSelector).exists()).toBe(
            true
        );
    });
});

describe("Credential Collection Card Component", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");

    const connectionMenuSelector = "[data-testid=connectionMenuBtn]";
    const credentialsInWalletSelector = "[data-testid=credentialsInWallet]";
    const connectionPendingDetailsSelector =
        "[data-testid=connectionPendingDetails]";
    const createConnectionButtonSelector =
        "[data-testid=createConnectionButton]";

    test(isAVueInstance, () => {
        const wrapper = createCredentialCollectionCardViewWrapper();
        expect(wrapper).toBeTruthy();
    });

    test("Not connected", () => {
        // Setup vuex store
        const options = new StoreOptionsStub();
        options.modules.credential.getters.connection = () => undefined;
        options.modules.credential.getters.credentials = () => [];
        const wrapper = createCredentialCollectionCardViewWrapper(options);

        // Check values
        expect(wrapper.find(connectionMenuSelector).exists()).toBe(false);
        expect(wrapper.find(credentialsInWalletSelector).exists()).toBe(false);
        expect(wrapper.find(connectionPendingDetailsSelector).exists()).toBe(
            false
        );
        expect(wrapper.find(createConnectionButtonSelector).exists()).toBe(
            true
        );
    });

    test("Pending", () => {
        // Setup vuex store
        const options = new StoreOptionsStub();
        const credentials: WalletCredential[] = [];
        options.modules.credential.getters.connection = () =>
            getConnection(ConnectionStatus.Pending, credentials);
        options.modules.credential.getters.credentials = () => credentials;
        const wrapper = createCredentialCollectionCardViewWrapper(options);

        // Check values
        expect(wrapper.find(connectionMenuSelector).exists()).toBe(false);
        expect(wrapper.find(credentialsInWalletSelector).exists()).toBe(false);
        expect(wrapper.find(connectionPendingDetailsSelector).exists()).toBe(
            true
        );
        expect(wrapper.find(createConnectionButtonSelector).exists()).toBe(
            false
        );
        expect(wrapper.find("[data-testid=qrCodeImage").exists()).toBe(false);
    });

    test("Connected", () => {
        // Setup vuex store
        const options = new StoreOptionsStub();
        const credentials = [getAddedCredential()];
        options.modules.credential.getters.connection = () =>
            getConnection(ConnectionStatus.Connected, credentials);
        options.modules.credential.getters.credentials = () => credentials;
        const wrapper = createCredentialCollectionCardViewWrapper(options);

        // Check values
        expect(wrapper.find(connectionMenuSelector).exists()).toBe(true);
        expect(wrapper.find(credentialsInWalletSelector).exists()).toBe(true);
        expect(wrapper.find(connectionPendingDetailsSelector).exists()).toBe(
            false
        );
        expect(wrapper.find(createConnectionButtonSelector).exists()).toBe(
            false
        );
    });
});

describe("Credential List Component", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");

    const addCredentialButtonSelector = "[data-testid=addCredentialButton]";
    const credentialStatusSelector = "[data-testid=credentialStatus]";
    const credentialMenuButtonSelector = "[data-testid=credentialMenuBtn]";

    test(isAVueInstance, () => {
        const wrapper = createCredentialListViewWrapper();
        expect(wrapper).toBeTruthy();
    });

    test("No credentials", () => {
        // Setup vuex store
        const options = new StoreOptionsStub();
        const credentials: WalletCredential[] = [];
        options.modules.credential.getters.connection = () =>
            getConnection(ConnectionStatus.Connected, credentials);
        options.modules.credential.getters.credentials = () => credentials;
        options.modules.immunization.getters.covidImmunizations = () =>
            getImmunizations();
        const wrapper = createCredentialListViewWrapper(options);

        // Check values
        expect(wrapper.find(addCredentialButtonSelector).exists()).toBe(true);
        expect(wrapper.find(credentialStatusSelector).exists()).toBe(false);
        expect(wrapper.find(credentialMenuButtonSelector).exists()).toBe(false);
    });

    test("Created credential", () => {
        // Setup vuex store
        const options = new StoreOptionsStub();
        const credentials: WalletCredential[] = [getCreatedCredential()];
        options.modules.credential.getters.connection = () =>
            getConnection(ConnectionStatus.Connected, credentials);
        options.modules.credential.getters.credentials = () => credentials;
        options.modules.immunization.getters.covidImmunizations = () => [
            getImmunizations()[0],
        ];
        const wrapper = createCredentialListViewWrapper(options);

        // Check values
        expect(wrapper.find(addCredentialButtonSelector).exists()).toBe(false);
        expect(wrapper.find(credentialStatusSelector).exists()).toBe(true);
        expect(wrapper.find(credentialMenuButtonSelector).exists()).toBe(true);
    });

    test("Added credential", () => {
        // Setup vuex store
        const options = new StoreOptionsStub();
        const credentials: WalletCredential[] = [getAddedCredential()];
        options.modules.credential.getters.connection = () =>
            getConnection(ConnectionStatus.Connected, credentials);
        options.modules.credential.getters.credentials = () => credentials;
        options.modules.immunization.getters.covidImmunizations = () => [
            getImmunizations()[1],
        ];
        const wrapper = createCredentialListViewWrapper(options);

        // Check values
        expect(wrapper.find(addCredentialButtonSelector).exists()).toBe(false);
        expect(wrapper.find(credentialStatusSelector).exists()).toBe(true);
        expect(wrapper.find(credentialMenuButtonSelector).exists()).toBe(true);
    });
});
