/** See reanimatedMock.js: on the JavaScript thread a scheduled call just runs. */
module.exports = {
    scheduleOnRN: (fn, ...args) => fn(...args),
};
