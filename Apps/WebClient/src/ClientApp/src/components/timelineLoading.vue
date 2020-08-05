<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.backdrop {
    position: fixed;
    z-index: 9998;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background-color: rgba(254, 254, 254, 0.5);
    display: table;
    transition: opacity 0.3s ease;
}
.spinner {
    width: 100px;
    height: 100px;
    position: absolute;
    margin: 100px auto;

    top: 50%;
    left: 50%;
    margin-top: -50px;
    margin-left: -50px;
}
.text {
    color: $primary;
    width: 280px;
    height: 100px;
    position: absolute;
    margin: 100px auto;
    top: 50%;
    left: 50%;
    margin-top: 55px;
    margin-left: -130px;
    text-shadow: 4px 4px 4px $primary_text;
}

.double-bounce {
    opacity: 0;
    background-color: $primary;
    color: $primary_text;
    display: flex;
    justify-content: center;
    align-items: center;
    width: 100%;
    height: 100%;
    border-radius: 50%;
    position: absolute;
    top: 0;
    left: 0;
    animation-name: sk-bounce;
    animation-duration: 2s;
    animation-timing-function: ease-in-out;
    animation-direction: alternate;
    animation-iteration-count: 1;
    animation-play-state: running;
    animation-fill-mode: none;
}
#first {
    animation-delay: 0s;
}
#second {
    animation-delay: 2s;
}
#third {
    animation-delay: 4s;
}
#fourth {
    animation-delay: 6s;
}
@keyframes sk-bounce {
    0% {
        opacity: 1;
        transform: scale(0);
    }
    50% {
        opacity: 1;
        transform: scale(1);
    }
    100% {
        opacity: 1;
        transform: scale(0);
    }
}
</style>
<template>
    <div class="backdrop">
        <div class="spinner">
            <div id="first" class="double-bounce">
                <font-awesome-icon
                    :icon="medIcon"
                    class="icon1"
                    size="2x"
                ></font-awesome-icon>
            </div>
            <div id="second" class="double-bounce">
                <font-awesome-icon
                    :icon="labIcon"
                    class="icon2"
                    size="2x"
                ></font-awesome-icon>
            </div>
            <div id="third" class="double-bounce">
                <font-awesome-icon
                    :icon="immIcon"
                    class="icon2"
                    size="2x"
                ></font-awesome-icon>
            </div>
            <div id="fourth" class="double-bounce">
                <font-awesome-icon
                    :icon="noteIcon"
                    class="icon2"
                    size="2x"
                ></font-awesome-icon>
            </div>
        </div>
        <div class="text">
            <h5>Gathering your health records {{ ellipsis }}</h5>
        </div>
    </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import {
    IconDefinition,
    faPills,
    faEdit,
    faFlask,
    faSyringe,
} from "@fortawesome/free-solid-svg-icons";

@Component
export default class TimelineLoadingComponent extends Vue {
    private step: number = 0;
    private intervalId: number = 0;
    private get ellipsis(): string {
        return ".".padEnd(this.step + 1, ".");
    }
    private get medIcon(): IconDefinition {
        return faPills;
    }

    private get labIcon(): IconDefinition {
        return faFlask;
    }

    private get immIcon(): IconDefinition {
        return faSyringe;
    }

    private get noteIcon(): IconDefinition {
        return faEdit;
    }

    private mounted() {
        this.resetTimeout();
    }

    private destroyed() {
        window.clearInterval(this.intervalId);
    }

    private resetTimeout() {
        this.intervalId = window.setInterval(() => {
            this.step++;
            if (this.step >= 4) {
                this.step = 0;
                this.resetAnimation("first");
                this.resetAnimation("second");
                this.resetAnimation("third");
                this.resetAnimation("fourth");
            }
        }, 2000);
    }

    private resetAnimation(elementId: string): boolean {
        var el: HTMLElement | null = document.getElementById(elementId);
        if (el == null) {
            return false;
        }
        el.style.animation = "none";
        el.offsetHeight; /* trigger reflow */
        el.style.animation = "";
        return true;
    }
}
</script>
