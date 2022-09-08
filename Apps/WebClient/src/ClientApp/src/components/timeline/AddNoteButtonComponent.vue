<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import UserPreferenceType from "@/constants/userPreferenceType";
import EventBus, { EventMessageName } from "@/eventbus";
import User from "@/models/user";
import { UserPreference } from "@/models/userPreference";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

@Component
export default class AddNoteButtonComponent extends Vue {
    @Getter("user", { namespace: "user" }) user!: User;

    @Action("updateUserPreference", { namespace: "user" })
    updateUserPreference!: (params: { userPreference: UserPreference }) => void;

    @Action("createUserPreference", { namespace: "user" })
    createUserPreference!: (params: { userPreference: UserPreference }) => void;

    private eventBus = EventBus;

    private logger!: ILogger;

    private isNoteTutorialHidden = false;

    private get showNoteTutorial(): boolean {
        return (
            this.isPreferenceActive(
                this.user.preferences[UserPreferenceType.TutorialMenuNote]
            ) && !this.isNoteTutorialHidden
        );
    }

    private isPreferenceActive(preference: UserPreference): boolean {
        return preference?.value === "true";
    }

    private dismissNoteTutorial(): void {
        this.logger.debug("Dismissing note tutorial");

        this.isNoteTutorialHidden = true;

        const preferenceType = UserPreferenceType.TutorialMenuNote;
        const preference = this.user.preferences[preferenceType];
        preference.value = "false";
        this.savePreference(preference);
    }

    private savePreference(userPreference: UserPreference) {
        if (userPreference.hdId != undefined) {
            this.updateUserPreference({ userPreference });
        } else {
            userPreference.hdId = this.user.hdid;
            this.createUserPreference({ userPreference });
        }
    }

    private createNote(): void {
        this.eventBus.$emit(EventMessageName.CreateNote);
    }

    private created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }
}
</script>

<template>
    <div>
        <hg-button
            id="addNoteBtn"
            data-testid="addNoteBtn"
            variant="secondary"
            @click="createNote"
        >
            <hg-icon icon="edit" size="medium" class="mr-lg-2" square />
            <span class="d-none d-lg-inline">Add a Note</span>
        </hg-button>
        <b-popover
            triggers="manual"
            :show="showNoteTutorial"
            target="addNoteBtn"
            placement="bottom"
            boundary="viewport"
        >
            <div>
                <hg-button
                    class="float-right text-dark p-0 ml-2"
                    variant="icon"
                    @click="dismissNoteTutorial()"
                    >×</hg-button
                >
            </div>
            <div data-testid="notesPopover">
                Add your own notes to track important details, such as health
                visit reason, medication side effect, etc.
            </div>
        </b-popover>
    </div>
</template>
