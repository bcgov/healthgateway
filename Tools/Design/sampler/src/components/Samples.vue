<template>
  <div>
    <h2>Typography</h2>
    <div class="section">
      <h1>h1 Heading</h1>
      <h2>h2 Heading</h2>
      <h3>h3 Heading</h3>
      <h4>h4 Heading</h4>

      <div>Regular text</div>
    </div>
    <h2>Colors</h2>
    <div class="section">
      <div class="p-3 mb-2 bg-primary text-white">.bg-primary</div>
      <div class="p-3 mb-2 bg-secondary text-white">.bg-secondary</div>
      <div class="p-3 mb-2 bg-success text-white">.bg-success</div>
      <div class="p-3 mb-2 bg-danger text-white">.bg-danger</div>
      <div class="p-3 mb-2 bg-warning text-dark">.bg-warning</div>
      <div class="p-3 mb-2 bg-info text-white">.bg-info</div>
      <div class="p-3 mb-2 bg-light text-dark">.bg-light</div>
      <div class="p-3 mb-2 bg-dark text-white">.bg-dark</div>
      <div class="p-3 mb-2 bg-white text-dark">.bg-white</div>
    </div>

    <h2>Buttons</h2>
    <div class="section">
      <h3>Clear Background</h3>
      <div class="section">
        <h4>Active</h4>
        <b-button>Button</b-button>
        <b-button variant="primary">Primary</b-button>
        <b-button variant="secondary">Secondary</b-button>
        <b-button variant="success">Success</b-button>
        <b-button variant="danger">Danger</b-button>
        <b-button variant="warning">Warning</b-button>
        <b-button variant="info">Info</b-button>
        <b-button variant="light">Light</b-button>
        <b-button variant="dark">Dark</b-button>
        <br />
        <h4>Disabled</h4>
        <b-button disabled>Button</b-button>
        <b-button disabled variant="primary">Primary</b-button>
        <b-button disabled variant="secondary">Secondary</b-button>
        <b-button disabled variant="success">Success</b-button>
        <b-button disabled variant="danger">Danger</b-button>
        <b-button disabled variant="warning">Warning</b-button>
        <b-button disabled variant="info">Info</b-button>
        <b-button disabled variant="light">Light</b-button>
        <b-button disabled variant="dark">Dark</b-button>
      </div>

      <h3>Dark Background</h3>
      <div class="text-white section bg-dark">
        <h4>Active</h4>
        <b-button>Button</b-button>
        <b-button variant="primary">Primary</b-button>
        <b-button variant="secondary">Secondary</b-button>
        <b-button variant="success">Success</b-button>
        <b-button variant="danger">Danger</b-button>
        <b-button variant="warning">Warning</b-button>
        <b-button variant="info">Info</b-button>
        <b-button variant="light">Light</b-button>
        <b-button variant="dark">Dark</b-button>
        <br />
        <h4>Disabled</h4>
        <b-button disabled>Button</b-button>
        <b-button disabled variant="primary">Primary</b-button>
        <b-button disabled variant="secondary">Secondary</b-button>
        <b-button disabled variant="success">Success</b-button>
        <b-button disabled variant="danger">Danger</b-button>
        <b-button disabled variant="warning">Warning</b-button>
        <b-button disabled variant="info">Info</b-button>
        <b-button disabled variant="light">Light</b-button>
        <b-button disabled variant="dark">Dark</b-button>
      </div>
    </div>
    <h2>Icons</h2>
    <div class="section">
      <b-row>
        <b-col cols="auto">
          <font-awesome-icon icon="pills" aria-hidden="true" size="xs" />
          Extra Small
        </b-col>
        <b-col cols="auto">
          <font-awesome-icon icon="pills" aria-hidden="true" size="sm" />
          Small
        </b-col>
        <b-col cols="auto">
          <font-awesome-icon icon="pills" aria-hidden="true" />
          Medium</b-col
        ><b-col cols="auto">
          <font-awesome-icon
            icon="pills"
            aria-hidden="true"
            size="lg"
            swap-opacity
          />
          Large
        </b-col>
      </b-row>
    </div>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";

@Component
export default class HelloWorld extends Vue {}
</script>

<style scoped lang="scss">
.section {
  padding: 1em;
}

.dark-bg {
  background-color: black;
}

.btn {
  margin-right: 1em;
}
br {
  display: block;
  margin-bottom: 2em;
}
</style>
