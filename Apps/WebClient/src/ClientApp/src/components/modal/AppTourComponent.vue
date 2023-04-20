<script lang="ts">
import { BCarousel } from "bootstrap-vue";
import Vue from "vue";
import { Component, Ref } from "vue-property-decorator";
import { Getter } from "vuex-class";

import addQuickLinkImage from "@/assets/images/tour/add-quicklink.png";
import type { WebClientConfiguration } from "@/models/configData";

interface TourSlide {
    // Title to be displayed for the slide.
    title: string;
    // Description to be displayed for the slide.
    description?: string;
    // Image or Gif to be displayed for the slide.
    imageUri: string;

    // Optional alt text for the image.
    imageAlt?: string;
}

@Component
export default class AppTourComponent extends Vue {
    @Getter("webClient", { namespace: "config" })
    webClient!: WebClientConfiguration;

    @Ref("tourCarousel")
    readonly tourCarousel!: BCarousel;
    slides: TourSlide[] = [
        // TODO: REMOVE THIS
        {
            title: "Add a Quick Link",
            description:
                "Add a quick link to easily access a health record type from your home screen.",
            imageUri: addQuickLinkImage,
            imageAlt: "First slide",
        },
        {
            title: "2 - Add a Quick Link",
            description:
                "2 - Add a quick link to easily access a health record type from your home screen.",
            imageUri: "@/assets/images/tour/filter-timeline.gif",
            imageAlt: "Second slide",
        },
    ];
    slideIndex = 0;

    isVisible = false;

    public showModal(): void {
        this.isVisible = true;
    }

    public hideModal(): void {
        this.isVisible = false;
    }

    next(bvModalEvt: Event): void {
        // Prevent modal from closing
        bvModalEvt.preventDefault();
        if (this.slideIndex < this.slides.length - 1) {
            this.tourCarousel.next();
        }
    }

    previous(bvModalEvt: Event): void {
        // Prevent modal from closing
        bvModalEvt.preventDefault();
        if (this.slideIndex > 0) {
            this.tourCarousel.prev();
        }
    }

    get currentSlide(): TourSlide | undefined {
        return this.slides.length > 0
            ? this.slides[this.slideIndex]
            : undefined;
    }
}
</script>

<template>
    <b-modal
        id="app-tour"
        :visible="isVisible"
        hide-footer
        hide-header
        centered
        no-close-on-backdrop
        body-class="p-0"
        content-class="overflow-hidden"
        size="lg"
    >
        <b-carousel
            id="carousel-1"
            ref="tourCarousel"
            v-model="slideIndex"
            :interval="0"
            indicators
            background="#ababab"
            img-width="1024"
            img-height="480"
            style="text-shadow: 1px 1px 2px #333"
        >
            <b-carousel-slide
                v-for="slide in slides"
                :key="slide.title"
                :img-src="slide.imageUri"
                :img-alt="slide.imageAlt"
            />
        </b-carousel>
        <div class="p-3">
            <b-row v-if="currentSlide">
                <b-col>
                    <h3>{{ currentSlide.title }}</h3>
                    <p>
                        {{ currentSlide.description }}
                    </p>
                </b-col>
            </b-row>
            <b-row>
                <b-col>
                    <hg-button variant="link" @click="hideModal"
                        >skip</hg-button
                    >
                </b-col>
                <b-col class="d-flex justify-content-end">
                    <hg-button variant="secondary" @click="previous($event)"
                        >back</hg-button
                    >
                    <hg-button
                        variant="primary"
                        class="ml-3"
                        @click="next($event)"
                        >Next</hg-button
                    >
                </b-col>
            </b-row>
        </div>
    </b-modal>
</template>

<style scoped></style>
