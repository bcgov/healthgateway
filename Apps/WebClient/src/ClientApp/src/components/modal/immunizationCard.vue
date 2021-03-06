﻿<script lang="ts">
import Vue from "vue";
import { Component, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import LoadingComponent from "@/components/loading.vue";
import MessageModalComponent from "@/components/modal/genericMessage.vue";
import EventBus, { EventMessageName } from "@/eventbus";
import { DateWrapper } from "@/models/dateWrapper";
import { ImmunizationEvent } from "@/models/immunizationModel";
import PatientData from "@/models/patientData";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";
import PDFUtil from "@/utility/pdfUtil";
import SnowPlow from "@/utility/snowPlow";

interface Dose {
    product: string;
    date: string;
    agent: string;
    lot: string;
    provider: string;
}

@Component({
    components: {
        "message-modal": MessageModalComponent,
        "hg-loading": LoadingComponent,
    },
})
export default class ImmunizationCardComponent extends Vue {
    @Action("getPatientData", { namespace: "user" })
    getPatientData!: () => Promise<void>;

    @Action("retrieve", { namespace: "immunization" })
    retrieveImmunizations!: (params: { hdid: string }) => Promise<void>;

    @Getter("isLoading", { namespace: "user" })
    isPatientLoading!: boolean;

    @Getter("isLoading", { namespace: "immunization" })
    isImmunizationLoading!: boolean;

    @Getter("patientData", { namespace: "user" })
    patientData!: PatientData;

    @Getter("covidImmunizations", { namespace: "immunization" })
    covidImmunizations!: ImmunizationEvent[];

    private eventBus = EventBus;

    private logger!: ILogger;
    private readonly modalId: string = "covid-card-modal";
    private isVisible = false;
    private isPreview = true;

    private doses: Dose[] = [];

    private get isLoading(): boolean {
        return this.isPatientLoading || this.isImmunizationLoading;
    }

    @Ref("messageModal")
    readonly messageModal!: MessageModalComponent;

    @Ref("cardModal")
    readonly cardModal!: Vue;

    @Watch("covidImmunizations", { deep: true })
    private onImmunizationsChange() {
        this.doses = [];

        for (let index = 0; index < this.covidImmunizations.length; index++) {
            const element = this.covidImmunizations[index];
            const agent =
                this.covidImmunizations[index].immunization
                    .immunizationAgents[0];
            this.doses.push({
                product: agent.productName,
                date: DateWrapper.format(element.dateOfImmunization),
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
    }

    private get userName(): string {
        return this.patientData.firstname + " " + this.patientData.lastname;
    }

    private get birthdate(): string {
        return this.formatDate(this.patientData.birthdate);
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.eventBus.$on(EventMessageName.TimelineCovidCard, this.showModal);
        this.onImmunizationsChange();
    }

    public showModal(): void {
        this.getPatientData();
        this.retrieveImmunizations({ hdid: this.patientData.hdid });
        this.isVisible = true;
    }

    public hideModal(): void {
        this.isVisible = false;
    }

    private formatDate(date: string | undefined): string {
        return date === undefined
            ? ""
            : DateWrapper.format(date, "yyyy-MMM-dd");
    }

    private showConfirmationModal() {
        this.messageModal.showModal();
    }

    private downloadPdf() {
        this.isPreview = false;
        SnowPlow.trackEvent({
            action: "download_card",
            text: "COVID Card PDF",
        });
        this.$nextTick(() => {
            PDFUtil.generatePdf(
                "HealthGateway_ImmunizationHistory.pdf",
                this.cardModal.$refs.content as HTMLElement
            ).finally(() => {
                this.isPreview = true;
            });
        });
    }
}
</script>

<template>
    <b-modal
        id="covidImmunizationCard"
        ref="cardModal"
        v-model="isVisible"
        data-testid="covidImmunizationCard"
        header-text-variant="light"
        content-class="immunization-covid-card-modal-content"
        header-class="immunization-covid-card-modal-header"
        :no-close-on-backdrop="true"
        hide-footer
        centered
    >
        <hg-loading :is-loading="isLoading" />
        <template #modal-header="{ close }">
            <b-row class="w-100 h-100">
                <b-col class="image-container">
                    <img
                        v-show="isPreview"
                        class="img-fluid"
                        src="@/assets/images/gov/bcid-logo-rev-en.svg"
                        width="190px"
                        alt="BC Health Gateway" />
                    <img
                        v-show="!isPreview"
                        class="img-fluid"
                        src="@/assets/images/gov/bcid-logo-rev-en.png"
                        width="181"
                        height="44"
                        alt="BC Health Gateway"
                /></b-col>
                <b-col cols="auto" class="align-self-center">
                    <!-- Emulate built in modal header close button action -->
                    <hg-button
                        data-html2canvas-ignore="true"
                        class="close"
                        aria-label="Close"
                        variant="icon"
                        @click="close()"
                        >×</hg-button
                    >
                </b-col>
            </b-row>
        </template>
        <b-row>
            <b-col>
                <b-row class="pb-3 title d-flex">
                    <b-col class="ml-1 label col-4 d-flex justify-content-end"
                        >Name</b-col
                    >
                    <b-col class="value text-wrap text-break">{{
                        userName
                    }}</b-col>
                </b-row>
                <b-row class="pb-3 title d-flex">
                    <b-col class="ml-1 label col-4 d-flex justify-content-end"
                        >Date of Birth</b-col
                    >
                    <b-col class="value" data-testid="patientBirthdate">{{
                        birthdate
                    }}</b-col>
                </b-row>
                <b-row class="pb-4 title">
                    <b-col class="ml-1 label col-4 d-flex justify-content-end"
                        >Immunization</b-col
                    >
                    <b-col class="value">COVID-19</b-col>
                </b-row>
                <b-row
                    v-for="(dose, index) in doses"
                    :key="index"
                    class="dose-wrapper"
                    :class="{ 'mb-4': index === 0 }"
                >
                    <b-col
                        class="
                            left-pane
                            text-center
                            justify-content-center
                            align-self-center
                        "
                        ><span class="dose-label"
                            >Dose {{ index + 1 }}</span
                        ></b-col
                    >
                    <b-col class="right-pane"
                        ><b-row class="pb-2">
                            <b-col class="field">
                                <b-row class="value" align-h="between">
                                    <b-col cols="6">{{ dose.product }}</b-col>
                                    <b-col cols="4" data-testid="doseDate">
                                        {{ dose.date }}
                                    </b-col>
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
        <b-row data-html2canvas-ignore="true" class="mt-2">
            <b-col class="d-flex justify-content-center">
                <hg-button
                    data-testid="exportCardBtn"
                    variant="secondary"
                    @click="showConfirmationModal"
                >
                    Download PDF
                </hg-button>
            </b-col>
        </b-row>
        <message-modal
            ref="messageModal"
            title="Sensitive Document Download"
            message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
            @submit="downloadPdf"
        />
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
        min-width: 75px;
        max-width: 90px;
        font-size: 0.9em;
    }
    .value {
        margin-left: 10px;
        border-bottom: 1px solid $muted-color;
        font-weight: bold;
    }
}

.dose-wrapper {
    background-color: $primary;
    border-radius: 15px 0px 0px 15px;

    .left-pane {
        max-width: 55px;
        padding: 5px;
        @media (max-width: 360px) {
            padding: 5px;
            max-width: 55px;
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
            padding-top: 5px;
            padding-bottom: 5px;
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
    .close {
        color: white;
        text-shadow: none;
        opacity: 1;
        font-size: 1.5em;
    }
    .image-container {
        min-height: 50px;
    }
}
</style>
