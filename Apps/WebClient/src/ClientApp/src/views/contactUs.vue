<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

#pageTitle {
    color: $primary;
}

#pageTitle hr {
    border-top: 2px solid $primary;
}
</style>
<template>
    <div class="px-4">
        <b-row>
            <b-col>
                <b-row class="my-3 fluid">
                    <b-col>
                        <div id="pageTitle">
                            <h1 id="subject">Contact Us</h1>
                            <hr />
                        </div>
                    </b-col>
                </b-row>
                <b-row>
                    <b-col>
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
                        <b-row class="pb-4"
                            ><b-col
                                ><strong>Email: </strong
                                >healthgateway@gov.bc.ca</b-col
                            ></b-row
                        >
                        <b-row class="pb-2">
                            <b-col>
                                <h4>Other Available Resources</h4>
                            </b-col>
                        </b-row>
                        <b-row class="pb-2">
                            <b-col>
                                <strong>
                                    <a
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
<script lang="ts">
import Vue from "vue";
import { Component, Ref } from "vue-property-decorator";
import HealthlinkSidebarComponent from "@/components/timeline/healthlink.vue";

@Component({
    components: {
        HealthlinkComponent: HealthlinkSidebarComponent,
    },
})
export default class ContactUsView extends Vue {}
</script>
