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

    private isNoteTutorialEnabled = true;

    private UserPreferenceType = UserPreferenceType;

    private logger!: ILogger;

    private get showNoteTutorial(): boolean {
        return (
            this.isPreferenceActive(
                this.user.preferences[UserPreferenceType.TutorialMenuNote]
            ) && this.isNoteTutorialEnabled
        );
    }

    private set showNoteTutorial(value: boolean) {
        this.isNoteTutorialEnabled = value;
    }

    private isPreferenceActive(tutorialPopover: UserPreference): boolean {
        return tutorialPopover?.value === "true";
    }

    private dismissTutorial(userPreference: UserPreference): void {
        this.logger.debug(
            `Dismissing tutorial ${userPreference.preference}...`
        );
        userPreference.value = "false";
        if (userPreference.hdId != undefined) {
            this.updateUserPreference({
                userPreference,
            });
        } else {
            userPreference.hdId = this.user.hdid;
            this.createUserPreference({
                userPreference,
            });
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
            :show.sync="showNoteTutorial"
            target="addNoteBtn"
            custom-class="popover-style"
            placement="bottom"
            variant="dark"
            boundary="viewport"
        >
            <div>
                <hg-button
                    class="float-right text-dark p-0 ml-2"
                    variant="icon"
                    @click="
                        dismissTutorial(
                            user.preferences[
                                UserPreferenceType.TutorialMenuNote
                            ]
                        )
                    "
                    >×</hg-button
                >
            </div>
            <div data-testid="notesPopover">
                Add Notes to track your important health events e.g. Broke ankle
                in Cuba
            </div>
        </b-popover>
    </div>
</template>
