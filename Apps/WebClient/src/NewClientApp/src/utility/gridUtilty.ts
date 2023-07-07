﻿import { computed } from "vue";
import { useDisplay } from "vuetify";

export const getGridCols = computed(() => {
    const { md, lg, xlAndUp } = useDisplay();
    return xlAndUp.value ? 3 : lg.value ? 4 : md.value ? 6 : 12;
});