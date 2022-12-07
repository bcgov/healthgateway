<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";

import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import HealthlinkSidebarComponent from "@/components/timeline/HealthlinkSidebarComponent.vue";
import BreadcrumbItem from "@/models/breadcrumbItem";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        BreadcrumbComponent,
        HealthlinkComponent: HealthlinkSidebarComponent,
    },
};

@Component(options)
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
    <div>
        <b-row>
            <b-col>
                <BreadcrumbComponent :items="breadcrumbItems" />
                <page-title title="Contact Us" />
                <div class="pb-3">
                    <h4>BC Vaccine Card</h4>
                    <p>
                        If you have questions about the BC Vaccine Card, please
                        contact
                        <a href="tel:18338382323">1‑833‑838‑2323</a>. Service is
                        available every day: 7 am to 7 pm or 9 am to 5 pm on
                        holidays.
                    </p>
                </div>
                <div class="pb-3">
                    <h4>Health Gateway</h4>
                    <p>
                        If you have any questions about the Health Gateway or
                        information provided within, please contact us at the
                        email below.
                    </p>
                    <p>
                        <strong>Email: </strong>
                        <a href="mailto:healthgateway@gov.bc.ca"
                            >healthgateway@gov.bc.ca</a
                        >
                    </p>
                </div>
                <h4 class="mb-3">Other Available Resources</h4>
                <h5>
                    <a
                        rel="noopener"
                        href="https://www.healthlinkbc.ca/"
                        target="_blank"
                        >HealthLink BC</a
                    >
                </h5>
                <p>
                    By dialing 8‑1‑1 toll‑free in British Columbia, you can
                    speak with a person who can help you connect with a health
                    professional.
                </p>
                <h5>
                    <a
                        rel="noopener"
                        href="http://www.bccdc.ca/health-info/diseases-conditions/covid-19/testing/testing-information"
                        target="_blank"
                    >
                        COVID‑19 Testing Information
                    </a>
                </h5>
                <p>
                    Non-medical information about COVID‑19 is available
                    7:30am‑8pm, 7 days a week at 1‑888‑COVID19 (1‑888‑268‑4319).
                </p>
            </b-col>
            <b-col class="my-3 col-3 col-lg-3 no-print">
                <HealthlinkComponent />
            </b-col>
        </b-row>
    </div>
</template>
