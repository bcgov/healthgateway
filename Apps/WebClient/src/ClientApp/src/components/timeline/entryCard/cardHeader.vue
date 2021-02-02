<template>
    <b-row class="entryHeading px-2 py-3">
        <b-col class="leftPane">
            <div class="icon">
                <font-awesome-icon
                    :icon="entryIcon"
                    size="lg"
                ></font-awesome-icon>
            </div>
        </b-col>
        <b-col class="entryTitleWrapper">
            <b-row class="justify-content-between">
                <b-col data-testid="laboratoryTitle">
                    <strong>{{ entry.summaryTitle }}</strong>
                </b-col>
            </b-row>
            <b-row class="py-2">
                <b-col class="text-muted">
                    <strong v-show="entry.isStatusFinal">
                        Result:
                        <span :class="entry.labResultOutcome"
                            >{{ entry.labResultOutcome }}
                        </span></strong
                    >
                </b-col>
                <b-col class="rightPane">
                    <div>
                        <b-btn
                            variant="link"
                            class="detailsButton"
                            @click="toggleDetails()"
                        >
                            <span v-if="detailsVisible">Hide Details</span>
                            <span v-else>View Details</span>
                        </b-btn>
                    </div>
                </b-col>
            </b-row>
        </b-col>
    </b-row>
</template>
