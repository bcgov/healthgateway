<template>
    <div>
        <h1>Counter</h1>

        <p>This is a simple example of a Vue.js component.</p>

        <p>Current count: <strong id="currentCount">{{ currentcount }}</strong></p>

        <button id="btnIncrement" @click="incrementCounter">Increment</button>
    </div>
</template>

<script src="./counter.ts"></script>
