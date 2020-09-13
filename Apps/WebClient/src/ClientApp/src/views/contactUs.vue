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
        <b-row class="my-3 fluid justify-content-md-center">
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
                        If you need to contact support regarding any issues you
                        may be having with Health Gateway, you can do so using
                        the contact options below. For all personal health
                        concerns, please contact your doctor or call
                        <strong>9-1-1</strong> in the case of an emergency.
                    </b-col>
                </b-row>
                <b-row><b-col>1515 Blanshard St.</b-col></b-row>
                <b-row><b-col>Victoria, BC</b-col></b-row>
                <b-row class="pb-2"><b-col>V8W 3C8</b-col></b-row>
                <b-row
                    ><b-col
                        ><strong>Phone: </strong>1(800)-500-2394</b-col
                    ></b-row
                >
                <b-row class="pb-4"
                    ><b-col
                        ><strong>Email: </strong>help@gov.bc.ca</b-col
                    ></b-row
                >
                <b-row class="pb-2">
                    <b-col>
                        <h4>Other Available Services</h4>
                    </b-col>
                </b-row>
                <b-row class="pb-2">
                    <b-col>
                        <strong>HealthLink BC</strong>
                    </b-col>
                </b-row>
                <b-row class="pb-2">
                    <b-col>
                        By dialing <strong>8-1-1</strong> toll-free in British
                        Columbia or for
                        <strong>deaf and hearing impaired</strong>, call
                        <strong>7-1-1</strong>. The service is available 24/7.
                        You can speak with a representative who can help you
                        connect with a health professional.
                    </b-col>
                </b-row>
                <b-row class="pb-2">
                    <b-col>
                        <strong>BC Center for Disease Control</strong>
                    </b-col>
                </b-row>
                <b-row class="pb-2">
                    <b-col>
                        The BC Center for Disease Control, a program of the
                        Provinical Health Services Authority, provides
                        provincial and federal leadership in disease
                        surveillance, detection, treatment and consultation.
                    </b-col>
                </b-row>
                <b-row><b-col>655 West 12th Avenue</b-col></b-row>
                <b-row><b-col>Vancouver, BC</b-col></b-row>
                <b-row class="pb-2"><b-col>V5Z 4R4</b-col></b-row>
                <b-row
                    ><b-col><strong>Phone: </strong>604-707-2400</b-col></b-row
                >
                <b-row
                    ><b-col
                        ><strong>Email: </strong>admininfo@bccdc.ca</b-col
                    ></b-row
                >
            </b-col>
        </b-row>
    </div>
</template>
<script lang="ts">
import Vue from "vue";
import { Component, Ref } from "vue-property-decorator";

@Component
export default class ContactUsView extends Vue {}
</script>
