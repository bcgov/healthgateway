<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

import LoadingComponent from "@/components/loading.vue";

@Component({
    components: {
        LoadingComponent,
    },
})
export default class CredentialInstructionsView extends Vue {
    @Prop({ required: true }) hasCovidImmunizations!: boolean;
    @Prop({ required: true }) isLoading!: boolean;

    private start(): void {
        this.$emit("started");
    }
}
</script>

<template>
    <div>
        <b-alert
            v-if="!hasCovidImmunizations"
            data-testid="noCovidImmunizations"
            show
            variant="warning"
        >
            <h4>Lorem Ipsum!</h4>
            <p>
                Aww yeah, you successfully read this important alert message.
                This example text is going to run a bit longer so that you can
                see how spacing within an alert works with this kind of content.
            </p>
            <p>
                Whenever you need to, be sure to use margin utilities to keep
                things nice and tidy.
            </p>
        </b-alert>
        <page-title title="Credentials" />
        <div v-if="hasCovidImmunizations" data-testid="hasCovidImmunizations">
            <p>
                In efforts to provide British Columbians with required
                credentials by sd0ejlkfs, you can access your
                <strong>covid immunization and tests</strong> here and store
                them in a digital wallet to use as proof if required.
            </p>
            <p>
                A <strong>smart phone or tablet</strong> is required to securely
                complete this process and store your credentials.
            </p>
            <p>
                If you have any questions please
                <strong>contact us</strong> at email@email.com
            </p>
        </div>
        <b-row>
            <b-col md="4" class="mb-4">
                <strong> 1. Download the Trinsic digital wallet app </strong>
            </b-col>
            <b-col md="4" class="mb-4">
                <strong> 2. Scan the QR code or click the link </strong>
            </b-col>
            <b-col md="4" class="mb-4">
                <strong>
                    3. Accept the vaccine credentials in your wallet app
                </strong>
            </b-col>
        </b-row>
        <b-row>
            <b-col>
                <hg-button
                    variant="primary"
                    data-testid="credentialsStartButton"
                    block
                    :disabled="isLoading"
                    @click="start"
                >
                    Start
                </hg-button>
            </b-col>
        </b-row>
        <LoadingComponent :is-loading="isLoading" />
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
</style>
