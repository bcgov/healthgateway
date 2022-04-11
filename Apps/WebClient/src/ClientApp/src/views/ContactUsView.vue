<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";

import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import HealthlinkSidebarComponent from "@/components/timeline/healthlink.vue";
import BreadcrumbItem from "@/models/breadcrumbItem";

@Component({
    components: {
        BreadcrumbComponent,
        HealthlinkComponent: HealthlinkSidebarComponent,
    },
})
export default class ContactUsView extends Vue {
    private breadcrumbItems: BreadcrumbItem[] = [
        {
            text: "Contact Us",
            to: "/contact-us",
            active: true,
            dataTestId: "breadcrumb-contact-us",
        },
    ];
}
</script>

<template>
    <div class="m-3 m-md-4 flex-grow-1 d-flex flex-column">
        <b-row>
            <b-col>
                <BreadcrumbComponent :items="breadcrumbItems" />
                <page-title title="Contact Us" />
                <b-row>
                    <b-col>
                        <b-row>
                            <b-col>
                                <h4>BC Vaccine Card</h4>
                            </b-col>
                        </b-row>
                        <b-row class="pb-2">
                            <b-col>
                                If you have questions about the BC Vaccine Card,
                                please contact
                                <a href="tel:18338382323">1-833-838-2323</a>.
                                Service is available every day: 7 am to 7 pm or
                                9 am to 5 pm on holidays.
                            </b-col>
                        </b-row>
                        <b-row class="pb-4"> </b-row>
                        <b-row>
                            <b-col>
                                <h4>Health Gateway</h4>
                            </b-col>
                        </b-row>
                        <b-row class="pb-2">
                            <b-col>
                                If you have any questions about the Health
                                Gateway or information provided within, please
                                contact us at the email below.
                            </b-col>
                        </b-row>
                        <b-row class="pb-4">
                            <b-col>
                                <strong>Email: </strong
                                ><a href="mailto:healthgateway@gov.bc.ca"
                                    >healthgateway@gov.bc.ca</a
                                >
                            </b-col>
                        </b-row>
                        <b-row class="pb-2">
                            <b-col>
                                <h4>Other Available Resources</h4>
                            </b-col>
                        </b-row>
                        <b-row class="pb-2">
                            <b-col>
                                <strong>
                                    <a
                                        rel="noopener"
                                        href="https://www.healthlinkbc.ca/"
                                        target="_blank"
                                        >HealthLink BC</a
                                    >
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row class="pb-2">
                            <b-col>
                                By dialing 8-1-1 toll-free in British Columbia,
                                you can speak with a person who can help you
                                connect with a health professional.
                            </b-col>
                        </b-row>
                        <b-row class="pb-2">
                            <b-col>
                                <strong>
                                    <a
                                        rel="noopener"
                                        href="http://www.bccdc.ca/health-info/diseases-conditions/covid-19/testing/testing-information"
                                        target="_blank"
                                    >
                                        COVID-19 Testing Information
                                    </a>
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row class="pb-2">
                            <b-col>
                                Non-medical information about COVID-19 is
                                available 7:30am-8pm, 7 days a week at
                                1-888-COVID19 (1-888-268-4319).
                            </b-col>
                        </b-row>
                    </b-col>
                </b-row>
            </b-col>
            <b-col class="my-3 col-3 col-lg-3 column-wrapper no-print">
                <HealthlinkComponent />
            </b-col>
        </b-row>
    </div>
</template>
