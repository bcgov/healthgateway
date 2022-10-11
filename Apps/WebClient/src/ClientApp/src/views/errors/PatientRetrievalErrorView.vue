<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";

import PageErrorComponent from "@/components/PageErrorComponent.vue";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        PageErrorComponent,
    },
};

@Component(options)
export default class PatientRetrievalErrorView extends Vue {}
</script>

<template>
    <PageErrorComponent title="Error retrieving user information">
        <p>
            There may be an issue in our Client Registry. Please contact
            <strong>HealthGateway@gov.bc.ca</strong>
        </p>
    </PageErrorComponent>
</template>
