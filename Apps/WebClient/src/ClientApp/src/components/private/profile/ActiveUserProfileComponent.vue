<script setup lang="ts">
import { computed } from "vue";

import LabelComponent from "@/components/common/LabelComponent.vue";
import SectionHeadingComponent from "@/components/common/SectionHeadingComponent.vue";
import UserProfileAddressComponent from "@/components/private/profile/UserProfileAddressComponent.vue";
import UserProfileEmailComponent from "@/components/private/profile/UserProfileEmailComponent.vue";
import UserProfileManageAccountComponent from "@/components/private/profile/UserProfileManageAccountComponent.vue";
import UserProfileNotificationsComponent from "@/components/private/profile/UserProfileNotificationsComponent.vue";
import UserProfileSmsComponent from "@/components/private/profile/UserProfileSmsComponent.vue";
import { DateWrapper } from "@/models/dateWrapper";
import { useAppStore } from "@/stores/app";
import { useUserStore } from "@/stores/user";

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
    <SectionHeadingComponent title="Personal Information" />
    <LabelComponent title="Full Name" />
    <p data-testid="fullName">{{ userStore.userName }}</p>
    <LabelComponent title="Personal Health Number" />
    <p data-testid="PHN">
        {{ userStore.patient.personalHealthNumber }}
    </p>
    <SectionHeadingComponent title="Contact Information" include-divider />
    <UserProfileEmailComponent />
    <UserProfileSmsComponent />
    <UserProfileAddressComponent />
    <UserProfileNotificationsComponent />
    {{
        /* AB#16941 - Hide login history as that was not in Sales Force Implementation */ ""
    }}
    <SectionHeadingComponent
        v-if="false"
        title="Login History"
        include-divider
    />
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
