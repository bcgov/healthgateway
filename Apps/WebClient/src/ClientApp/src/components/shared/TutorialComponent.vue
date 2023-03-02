<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import User from "@/models/user";
import { UserPreference } from "@/models/userPreference";

@Component
export default class TutorialComponent extends Vue {
    @Prop({ required: true })
    private preferenceType!: string;

    @Prop({ required: true })
    private target!: string;

    @Prop({ required: false, default: "default" })
    private placement!: string;

    @Prop({ required: false, default: true })
    private show!: boolean;

    @Prop({ required: false, default: undefined })
    private customClass!: string | undefined;

    @Action("setUserPreference", { namespace: "user" })
    private setUserPreference!: (params: {
        preference: UserPreference;
    }) => Promise<void>;

    @Getter("user", { namespace: "user" })
    private user!: User;

    @Getter("isMobile")
    private isMobileView!: boolean;

    private isHidden = false;

    private get popoverPlacement(): string {
        if (this.placement === "default") {
            return this.isMobileView ? "bottom" : "left";
        } else {
            return this.placement;
        }
    }

    private get showPopover(): boolean {
        return (
            this.show &&
            !this.isHidden &&
            this.user.preferences[this.preferenceType]?.value === "true"
        );
    }

    private dismissTutorial(): void {
        this.isHidden = true;

        const preference = {
            ...this.user.preferences[this.preferenceType],
            value: "false",
        };
        this.setUserPreference({ preference });
    }
}
</script>

<template>
    <b-popover
        triggers="manual"
        :show="showPopover"
        :target="target"
        :placement="popoverPlacement"
        fallback-placement="clockwise"
        boundary="viewport"
        :custom-class="customClass"
    >
        <div>
            <hg-button
                class="float-right text-dark p-0 ml-2"
                variant="icon"
                @click="dismissTutorial"
                >×</hg-button
            >
        </div>
        <slot />
    </b-popover>
</template>
