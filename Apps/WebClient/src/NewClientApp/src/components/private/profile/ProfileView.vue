<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faExclamationTriangle } from "@fortawesome/free-solid-svg-icons";
import { computed, ref } from "vue";

import LoadingComponent from "@/components/common/LoadingComponent.vue";
import PageTitleComponent from "@/components/common/PageTitleComponent.vue";
import ActiveUserProfileComponent from "@/components/private/profile/ActiveUserProfileComponent.vue";
import InactiveUserProfileComponent from "@/components/private/profile/InactiveUserProfileComponent.vue";
import BreadcrumbComponent from "@/components/site/BreadcrumbComponent.vue";
import { Loader } from "@/constants/loader";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { useLoadingStore } from "@/stores/loading";
import { useUserStore } from "@/stores/user";

library.add(faExclamationTriangle);

const breadcrumbItems: BreadcrumbItem[] = [
    {
        text: "Profile",
        to: "/profile",
        active: true,
        dataTestId: "breadcrumb-profile",
    },
];

const loadingStore = useLoadingStore();
const userStore = useUserStore();

const showCheckEmailAlert = ref(false);

const isLoading = computed(() => loadingStore.isLoading(Loader.UserProfile));
</script>

<template>
    <BreadcrumbComponent :items="breadcrumbItems" />
    <LoadingComponent :is-loading="isLoading" />
    <v-alert
        v-if="userStore.userIsActive && showCheckEmailAlert"
        data-testid="verifyEmailTxt"
        class="d-print-none mb-4"
        closable
        type="info"
        variant="outlined"
        border
        title="Please check your email"
        text="Please check your email for an email verification link. If you
            didn't receive one, please check your junk mail."
        @click:close="showCheckEmailAlert = false"
    />
    <PageTitleComponent title="Profile" />
    <v-skeleton-loader
        v-if="isLoading"
        type="heading, text, heading, paragraph"
    />
    <div v-show="!isLoading">
        <ActiveUserProfileComponent
            v-if="userStore.userIsActive"
            @email-updated="showCheckEmailAlert = Boolean($event)"
        />
        <InactiveUserProfileComponent v-else />
    </div>
</template>
