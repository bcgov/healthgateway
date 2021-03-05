<script lang="ts">
import Vue from "vue";
import { Component, Ref, Watch } from "vue-property-decorator";
import { Getter } from "vuex-class";

import MessageModalComponent from "@/components/modal/genericMessage.vue";
import EventBus, { EventMessageName } from "@/eventbus";
import { DateWrapper } from "@/models/dateWrapper";
import { ImmunizationEvent } from "@/models/immunizationModel";
import PatientData from "@/models/patientData";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";
import PDFUtil from "@/utility/pdfUtil";

interface Dose {
    product: string;
    date: string;
    agent: string;
    lot: string;
    provider: string;
}

@Component({
    components: {
        MessageModalComponent,
    },
})
export default class ImmunizationCardComponent extends Vue {
    @Getter("user", { namespace: "user" }) user!: User;

    @Getter("patientData", { namespace: "user" })
    patientData!: PatientData;

    @Getter("immunizations", { namespace: "immunization" })
    immunizations!: ImmunizationEvent[];

    private eventBus = EventBus;

    private logger!: ILogger;
    private readonly modalId: string = "covid-card-modal";
    private isVisible = false;

    private doses: Dose[] = [];

    @Ref("messageModal")
    readonly messageModal!: MessageModalComponent;

    @Ref("cardModal")
    readonly cardModal!: Vue;

    @Watch("immunizations", { deep: true })
    private onImmunizationsChange() {
        this.doses = [];
        const covidImmunizations = this.immunizations
            .filter((x) => x.targetedDisease?.toLowerCase().includes("covid"))
            .sort((a, b) => {
                const firstDate = new DateWrapper(a.dateOfImmunization);
                const secondDate = new DateWrapper(b.dateOfImmunization);

                const value = firstDate.isAfter(secondDate)
                    ? 1
                    : firstDate.isBefore(secondDate)
                    ? -1
                    : 0;

                return value;
            });

        for (let index = 0; index < covidImmunizations.length; index++) {
            const element = covidImmunizations[index];
            const agent =
                covidImmunizations[index].immunization.immunizationAgents[0];
            this.doses.push({
                product: agent.productName,
                date: this.formatDate(element.dateOfImmunization),
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
    }

    public showModal(): void {
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
        PDFUtil.generatePdf(
            "HealthGateway_ImmunizationHistory.pdf",
            this.cardModal.$refs.content as HTMLElement
        );
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
        <template #modal-header="{ close }">
            <b-row class="w-100 h-100">
                <b-col>
                    <img
                        class="img-fluid"
                        src="@/assets/images/gov/bcid-logo-rev-en.png"
                        width="181"
                        height="44"
                        alt="Go to healthgateway home page"
                /></b-col>
                <b-col cols="auto" class="align-self-center">
                    <!-- Emulate built in modal header close button action -->
                    <b-button
                        data-html2canvas-ignore="true"
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
                    <b-col class="value">{{ birthdate }}</b-col>
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
                <b-button
                    variant="outline-primary"
                    data-testid="exportCardBtn"
                    class="mb-1"
                    @click="showConfirmationModal"
                >
                    Download PDF
                </b-button>
            </b-col>
        </b-row>
        <MessageModalComponent
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
        max-width: 80px;
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
}
</style>
