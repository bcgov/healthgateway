<template>
  <div class="container">
    <div class="row pt-5">
      <div class="col-lg-12 col-md-12 text-center">
        <h1>Health Gateway</h1>
        <p>
          The Health Gateway is a secure entry point for viewing your health
          information in BC
        </p>
      </div>
    </div>
    <div class="row pb-5 pt-5">
      <div class="col-lg-6 col-md-6 pt-2">
        <div class="shadow-lg p-3 mb-5 bg-white rounded border text-center">
          <h3>
            <strong>New here?</strong>
          </h3>
          <p>Click the button below to register.</p>
          <b-button
            to="registration"
            class="btn btn-secondary"
            id="btnStart"
            role="button"
            >Start now</b-button
          >
        </div>
      </div>
      <div class="col-lg-6 col-md-6 pt-2">
        <div class="shadow-lg p-3 mb-5 bg-white rounded border text-center">
          <h3>
            <strong>Already registered?</strong>
          </h3>
          <p>Click the button below to login.</p>
          <b-button to="login" class="btn btn-primary" id="btnLogin"
            >Login</b-button
          >
        </div>
      </div>
    </div>
    <div class="row pb-5 pt-5">
      <div class="col-lg-4 col-md-4">
        <div class="row">
          <div class="col d-flex justify-content-center">
            <div
              class="border border-light rounded-circle shadow-lg p-3 mb-5 bg-white text-center"
            >
              <h3 class="m-5">
                <span class="fa fa-4x fa-syringe"></span>
              </h3>
            </div>
          </div>
        </div>
        <div class="row pt-1">
          <div class="col text-center">
            <h4>Immunizations</h4>
          </div>
        </div>
      </div>
      <div class="col-lg-4 col-md-4">
        <div class="row">
          <div class="col d-flex justify-content-center">
            <div
              class="border border-light rounded-circle shadow-lg p-3 mb-5 bg-white text-center"
            >
              <h3 class="m-5">
                <span class="fa fa-4x fa-prescription-bottle-alt"></span>
              </h3>
            </div>
          </div>
        </div>
        <div class="row pt-1">
          <div class="col text-center">
            <h4>Medications</h4>
          </div>
        </div>
      </div>
      <div class="col-lg-4 col-md-4 text-center">
        <div class="row">
          <div class="col d-flex justify-content-center">
            <div
              class="border border-light rounded-circle shadow-lg p-3 mb-5 bg-white text-center"
            >
              <h3 class="m-5 pl-1 pr-2">
                <span class="fa fa-4x fa-flask"></span>
              </h3>
            </div>
          </div>
        </div>
        <div class="row pt-1">
          <div class="col text-center">
            <h4>Lab Tests</h4>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";

@Component
export default class LandingComponent extends Vue {}
</script>
