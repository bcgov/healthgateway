<template>
    <b-row>
        <b-col class="leftPane d-none d-md-block"></b-col>
        <b-col>
            <b-collapse
                :id="'entryDetails-' + index + '-' + datekey"
                v-model="detailsVisible"
            >
                <CommentSection :parent-entry="entry"></CommentSection>
            </b-collapse>
        </b-col>
    </b-row>
</template>
