<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";

import PhoneImage from "@/assets/images/timeline/811.png";
import CovidImage from "@/assets/images/timeline/CovidSidecardImage.png";
import HealthlinkImage from "@/assets/images/timeline/healthlink.png";

interface Healthcard {
    title: string;
    description: string;
    imageSrc: string;
    urlLink: string;
}

@Component
export default class HealthlinkSidebarComponent extends Vue {
    private currentCard: Healthcard = {
        title: "",
        description: "",
        imageSrc: "",
        urlLink: "",
    };
    private cardPool: Healthcard[] = [
        {
            title: "COVID-19",
            description: "View more information about COVID-19.",
            imageSrc: CovidImage,
            urlLink:
                "http://www.bccdc.ca/health-info/diseases-conditions/covid-19",
        },
        {
            title: "8-1-1",
            description:
                "Speak to a pharmacist from 5pm to 9am Pacific Time everyday of the year.",
            imageSrc: PhoneImage,
            urlLink:
                "https://www.healthlinkbc.ca/services-and-resources/about-8-1-1",
        },
        {
            title: "Healthlink",
            description:
                "HealthLink BC provides reliable non-emergency health information and advice in British Columbia.",
            imageSrc: HealthlinkImage,
            urlLink: "https://www.healthlinkbc.ca",
        },
    ];
    private cardIndex = 1;

    private mounted(): void {
        this.rotate();
    }

    private rotate(): void {
        if (this.cardIndex !== this.cardPool.length - 1) {
            this.cardIndex = this.cardIndex + 1;
        } else {
            this.cardIndex = 0;
        }
        this.currentCard = this.cardPool[this.cardIndex];
        setTimeout(this.rotate, this.cardDelay);
    }

    private get cardDelay(): number {
        return this.cardIndex === 0 ? 25000 : 7000;
    }
}
</script>

<template>
    <a :href="currentCard.urlLink" rel="noopener" target="_blank">
        <b-card
            class="box d-none d-lg-block mx-auto bg-white border-0 rounded shadow"
            no-body
        >
            <b-card-body>
                <img
                    class="img-fluid mx-auto d-block card-image"
                    :src="currentCard.imageSrc"
                    width="auto"
                    height="auto"
                    :alt="currentCard.title"
                />
                <b-card-text class="mt-3">
                    {{ currentCard.description }}
                </b-card-text>
            </b-card-body>
        </b-card>
    </a>
</template>

<style lang="scss" scoped>
.box {
    border: none;
    cursor: pointer;
    font-size: 0.9rem;
    max-width: 280px;
}

.card-image {
    max-height: 140px;
}
</style>
