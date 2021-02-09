<script lang="ts">
import Vue from "vue";
import { Component, Watch } from "vue-property-decorator";
import { Getter } from "vuex-class";

import { DateWrapper } from "@/models/dateWrapper";
import { ImmunizationEvent } from "@/models/immunizationModel";
import { OidcUserProfile } from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { IAuthenticationService } from "@/services/interfaces";

interface Dose {
    product: string;
    date: string;
    agent: string;
    lot: string;
    provider: string;
}

@Component
export default class ImmunizationCardComponent extends Vue {
    @Getter("oidcIsAuthenticated", {
        namespace: "auth",
    })
    oidcIsAuthenticated!: boolean;

    @Getter("getStoredImmunizations", { namespace: "immunization" })
    immunizations!: ImmunizationEvent[];

    private readonly modalId: string = "covid-card-modal";
    private isVisible = false;

    private authenticationService!: IAuthenticationService;
    private oidcUser: OidcUserProfile | null = null;

    private doses: Dose[] = [];

    private get userName(): string {
        return this.oidcUser
            ? this.oidcUser.given_name + " " + this.oidcUser.family_name
            : "";
    }

    @Watch("immunizations", { deep: true })
    private onImmunizationsChange() {
        const covidImmunizations = this.immunizations
            .filter((x) => x.immunization.name.toLowerCase().includes("a"))
            .sort((a, b) => {
                const firstDate = new DateWrapper(a.dateOfImmunization);
                const secondDate = new DateWrapper(b.dateOfImmunization);

                const vale = firstDate.isAfter(secondDate)
                    ? 1
                    : firstDate.isBefore(secondDate)
                    ? -1
                    : 0;

                return vale;
            });
        console.log(covidImmunizations);

        for (let index = 0; index < covidImmunizations.length; index++) {
            const element = covidImmunizations[index];
            const agent =
                covidImmunizations[index].immunization.immunizationAgents[0];
            this.doses.push({
                product: agent.productName,
                date: DateWrapper.format(
                    element.dateOfImmunization,
                    "MMM dd, yyyy"
                ),
                agent: agent.name,
                lot: agent.lotNumber,
                provider: element.providerOrClinic,
            });
        }

        const maxDoses = 2;

        if (this.doses.length > maxDoses) {
            this.doses.splice(maxDoses, this.doses.length);
        } else if (this.doses.length < maxDoses) {
            while (this.doses.length < maxDoses)
                this.doses.push({
                    product: "",
                    date: "",
                    agent: "",
                    lot: "",
                    provider: "",
                });
        }

        console.log(this.doses);
    }

    @Watch("oidcIsAuthenticated")
    private onPropertyChanged() {
        if (this.oidcIsAuthenticated) {
            this.loadOidcUser();
        }
    }

    private mounted() {
        this.authenticationService = container.get<IAuthenticationService>(
            SERVICE_IDENTIFIER.AuthenticationService
        );
        if (this.oidcIsAuthenticated) {
            this.loadOidcUser();
        }
    }

    public showModal(): void {
        this.isVisible = true;
    }

    public hideModal(): void {
        this.isVisible = false;
    }

    private loadOidcUser(): void {
        this.authenticationService.getOidcUserProfile().then((oidcUser) => {
            if (oidcUser) {
                this.oidcUser = oidcUser;
            }
        });
    }
}
</script>

<template>
    <b-modal
        id="covidImmunizationCard"
        v-model="isVisible"
        data-testid="covidImmunizationCard"
        header-text-variant="light"
        content-class="immunization-covid-card-modal-content"
        header-class="immunization-covid-card-modal-header"
        :no-close-on-backdrop="true"
        hide-footer
        centered
    >
        <template #modal-header="{ close }">
            <b-row class="w-100 h-100">
                <b-col>
                    <img
                        class="img-fluid"
                        src="@/assets/images/gov/bcid-logo-rev-en.svg"
                        width="181"
                        height="44"
                        alt="Go to healthgateway home page"
                /></b-col>
                <b-col cols="auto" class="align-self-center">
                    <!-- Emulate built in modal header close button action -->
                    <b-button
                        type="button"
                        class="close text-light"
                        aria-label="Close"
                        @click="close()"
                        >×</b-button
                    >
                </b-col>
            </b-row>
        </template>
        <b-row>
            <b-col>
                <b-row class="pb-3 title">
                    <b-col cols="2" class="px-1 label">Name</b-col>
                    <b-col class="value">{{ userName }}</b-col>
                </b-row>
                <b-row class="pb-4 title" align-h="between">
                    <b-col cols="4" class="px-1 label">Immunization</b-col>
                    <b-col class="value">COVID-19</b-col>
                </b-row>
                <b-row
                    v-for="(dose, index) in doses"
                    :key="index"
                    class="dose-wrapper"
                    :class="{ 'mb-4': index === 0 }"
                >
                    <b-col
                        class="left-pane text-center justify-content-center align-self-center"
                        ><span class="dose-label"
                            >Dose {{ index + 1 }}</span
                        ></b-col
                    >
                    <b-col class="right-pane"
                        ><b-row class="pb-2">
                            <b-col class="field">
                                <b-row class="value" align-h="between">
                                    <b-col cols="6">{{ dose.product }}</b-col>
                                    <b-col cols="4">{{ dose.date }}</b-col>
                                </b-row>
                                <b-row class="text-muted" align-h="between"
                                    ><b-col class="description" cols="6"
                                        >Product</b-col
                                    >
                                    <b-col class="description" cols="4"
                                        >Date</b-col
                                    >
                                </b-row>
                            </b-col>
                        </b-row>
                        <b-row class="pb-2">
                            <b-col class="field">
                                <b-row class="value" align-h="between">
                                    <b-col cols="6">{{ dose.agent }}</b-col>
                                    <b-col cols="4">{{ dose.lot }}</b-col>
                                </b-row>
                                <b-row class="text-muted" align-h="between"
                                    ><b-col class="description" cols="6"
                                        >Immunizing agent</b-col
                                    >
                                    <b-col class="description" cols="4"
                                        >Lot Number</b-col
                                    >
                                </b-row>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col class="field" cols="10">
                                <b-row class="value" align-h="between">
                                    <b-col>{{ dose.provider }}</b-col>
                                </b-row>
                                <b-row class="text-muted" align-h="between"
                                    ><b-col class="description"
                                        >Provider or Clinic</b-col
                                    >
                                </b-row>
                            </b-col>
                        </b-row></b-col
                    >
                </b-row>
            </b-col>
        </b-row>
    </b-modal>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

$muted-color: #6c757d;

div[class^="col"],
div[class*=" col"] {
    padding: 0px;
    margin: 0px;
}

div[class^="row"],
div[class*=" row"] {
    padding: 0px;
    margin: 0px;
}

.title {
    color: $primary;
    .label {
        font-size: 1.3em;
    }

    .value {
        border-bottom: 1px solid $muted-color;
        font-size: 1.3em;
        font-weight: bold;
        margin-left: 1em;
        @media (max-width: 360px) {
            margin-left: 3em;
        }
    }
}

.dose-wrapper {
    background-color: $primary;
    border-radius: 15px 0px 0px 15px;

    .left-pane {
        max-width: 75px;
        padding: 20px;
        @media (max-width: 360px) {
            padding: 10px;
            max-width: 60px;
        }
        .dose-label {
            color: white;
            font-size: 1.2em;
            font-weight: bold;
        }
    }
    .right-pane {
        background-color: white;
        padding-left: 15px;

        .field {
            padding: 5px;
            .value {
                min-height: 1em;
                color: $primary;
            }
            .description {
                border-top: 1px solid$muted-color;
            }
        }
    }
}
</style>

<style lang="scss">
@import "@/assets/scss/_variables.scss";
.immunization-covid-card-modal-header {
    background-color: $primary;
}
</style>
