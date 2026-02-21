<script setup lang="ts">
import { computed } from "vue";

import SectionHeaderComponent from "@/components/common/SectionHeaderComponent.vue";
import UserProfileAddressComponent from "@/components/private/profile/UserProfileAddressComponent.vue";
import UserProfileEmailComponent from "@/components/private/profile/UserProfileEmailComponent.vue";
import UserProfileManageAccountComponent from "@/components/private/profile/UserProfileManageAccountComponent.vue";
import UserProfileNotificationsComponent from "@/components/private/profile/UserProfileNotificationsComponent.vue";
import UserProfileSmsComponent from "@/components/private/profile/UserProfileSmsComponent.vue";
import { DateWrapper } from "@/models/dateWrapper";
import { useAppStore } from "@/stores/app";
import { useUserStore } from "@/stores/user";

const emit = defineEmits<{
    (e: "email-updated", value: string): void;
}>();

const appStore = useAppStore();
const userStore = useUserStore();

const formattedLoginDateTimes = computed(() =>
    userStore.user.lastLoginDateTimes.map((time) =>
        DateWrapper.fromIso(time).format(
            appStore.isPacificTime ? "yyyy-MMM-dd, t" : "yyyy-MMM-dd, t ZZZZ"
        )
    )
);
</script>

<template>
    <SectionHeaderComponent title="Full Name" />
    <p data-testid="fullName">{{ userStore.userName }}</p>
    <SectionHeaderComponent title="Personal Health Number" />
    <p data-testid="PHN">
        {{ userStore.patient.personalHealthNumber }}
    </p>
    <UserProfileEmailComponent @email-updated="emit('email-updated', $event)" />
    <UserProfileSmsComponent />
    <UserProfileAddressComponent />
    <UserProfileNotificationsComponent />
    {{
        /* AB#16941 - Hide login history as that was not in Sales Force Implementation */ ""
    }}
    <SectionHeaderComponent v-if="false" title="Login History" />
    <ul v-if="false" id="lastLoginDate" class="text-body-1">
        <li
            v-for="(item, index) in formattedLoginDateTimes"
            :key="index"
            data-testid="lastLoginDateItem"
        >
            {{ item }}
        </li>
    </ul>
    <UserProfileManageAccountComponent />
</template>
