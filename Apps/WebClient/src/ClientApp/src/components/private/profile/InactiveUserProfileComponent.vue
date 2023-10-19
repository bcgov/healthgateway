<script setup lang="ts">
import { Duration, DurationUnit } from "luxon";
import { computed, ref } from "vue";

import DisplayFieldComponent from "@/components/common/DisplayFieldComponent.vue";
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import { ErrorSourceType } from "@/constants/errorType";
import { Loader } from "@/constants/loader";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import { ILogger } from "@/services/interfaces";
import { useConfigStore } from "@/stores/config";
import { useErrorStore } from "@/stores/error";
import { useLoadingStore } from "@/stores/loading";
import { useUserStore } from "@/stores/user";

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const configStore = useConfigStore();
const errorStore = useErrorStore();
const loadingStore = useLoadingStore();
const userStore = useUserStore();

const timeForDeletion = ref(-1);
const intervalHandler = ref(0);

const timeForDeletionString = computed(() => {
    if (userStore.userIsActive) {
        return "";
    }

    const duration = Duration.fromMillis(timeForDeletion.value);
    const units: DurationUnit[] = ["day", "hour", "minute", "second"];
    for (const unit of units) {
        const amount = Math.floor(duration.as(unit));
        if (amount > 1) {
            return `${amount} ${unit}s`;
        }
    }

    return "Your account will be closed imminently";
});

function recoverAccount(): void {
    loadingStore.applyLoader(
        Loader.UserProfile,
        "recoverAccount",
        userStore.recoverUserAccount().catch((err: ResultError) => {
            logger.error(err.resultMessage);
            if (err.statusCode === 429) {
                errorStore.setTooManyRequestsError("page");
            } else {
                errorStore.addCustomError(
                    `Unable to recover ${ErrorSourceType.Profile}`,
                    ErrorSourceType.Profile,
                    undefined
                );
            }
        })
    );
}

function calculateTimeForDeletion(): void {
    if (userStore.userIsActive || !userStore.user.closedDateTime) {
        return undefined;
    }

    const endDate = DateWrapper.fromIso(userStore.user.closedDateTime).add({
        hour: configStore.webConfig.hoursForDeletion,
    });

    timeForDeletion.value = endDate.diff(DateWrapper.now()).milliseconds;
}

calculateTimeForDeletion();
intervalHandler.value = window.setInterval(
    () => calculateTimeForDeletion(),
    1000
);
</script>

<template>
    <v-alert
        data-testid="emailOptOutMessage"
        class="pt-0"
        type="error"
        variant="text"
        icon="exclamation-triangle"
        title="Account marked for removal"
        text="Your account has been deactivated. If you wish to recover your account,
                click on the “Recover Account” button before the time expires."
    />
    <DisplayFieldComponent
        name="Time remaining for deletion"
        :value="timeForDeletionString"
        horizontal
    />
    <HgButtonComponent
        id="recoverAccountCancelBtn"
        data-testid="recoverAccountCancelBtn"
        variant="primary"
        text="Recover Account"
        @click="recoverAccount"
    />
</template>
