<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Getter } from "vuex-class";

@Component
export default class QueueFullView extends Vue {
    @Getter("tooBusy", { namespace: "waitlist" })
    tooBusy!: boolean;

    private created(): void {
        if (!this.tooBusy) {
            this.$router.push({ path: "/" });
        }
    }
}
</script>

<template>
    <div
        class="flex-grow-1 d-flex flex-column align-items-center justify-content-center p-3 p-md-4"
    >
        <b-alert show variant="danger" data-testid="too-many-tickets">
            Can’t access Health Gateway. Please try again later.
        </b-alert>
        <img
            src="@/assets/images/queue/error.png"
            class="img-fluid"
            data-testid="queue-error-image"
            alt=""
        />
    </div>
</template>
