<script lang="ts">
import Vue from "vue";
import { Component, Watch } from "vue-property-decorator";
import { Getter } from "vuex-class";

import type { WebClientConfiguration } from "@/models/configData";
import type { Ticket } from "@/models/ticket";

@Component
export default class QueueView extends Vue {
    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Getter("ticket", { namespace: "waitlist" })
    ticket?: Ticket;

    @Getter("ticketIsProcessed", { namespace: "waitlist" })
    ticketIsProcessed!: boolean;

    @Watch("ticketIsProcessed")
    private onTicketIsProcessedChanged(value: boolean): void {
        if (value) {
            this.redirect();
        }
    }

    public get queuePosition(): number | undefined {
        return this.ticket?.queuePosition;
    }

    private created(): void {
        if (
            !this.config.featureToggleConfiguration.waitingQueue.enabled ||
            !this.ticket ||
            this.ticketIsProcessed
        ) {
            this.redirect();
        }
    }

    private redirect(): void {
        const path = this.$route.query.redirect
            ? this.$route.query.redirect.toString()
            : "/";
        this.$router.push({ path });
    }
}
</script>

<template>
    <div
        class="queue flex-grow-1 d-flex flex-column align-items-center justify-content-center p-3 p-md-4 text-center"
    >
        <img
            src="@/assets/images/queue/waiting.png"
            class="img-fluid p-3"
            data-testid="queue-image"
            alt=""
        />
        <p>
            Health Gateway is busier than normal at this moment. We have placed
            you in a queue.
        </p>
        <h2 v-if="queuePosition !== undefined" class="mt-4">
            <div class="mb-3">Your position in the queue:</div>
            <div data-testid="queue-position">{{ queuePosition }}</div>
        </h2>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.queue {
    color: $hg-brand-primary;
}
</style>
