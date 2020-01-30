<template>
  <v-footer app dark paddless>
    <v-row no-gutters>
      <v-col>
        <div class="py-2 white--text">
          {{ new Date().getFullYear() }}
          <strong>&copy;&nbsp;Province of British Columbia.</strong>
        </div>
      </v-col>
    </v-row>
  </v-footer>
</template>

<script lang="ts">
import { Action, Getter } from 'vuex-class';
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class Footer extends Vue {}
</script>