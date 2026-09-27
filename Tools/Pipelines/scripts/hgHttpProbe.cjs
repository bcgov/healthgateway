// Public, unauthenticated requests only. No external executables or packages.
const http = require("node:http");
const https = require("node:https");
const { performance } = require("node:perf_hooks");

// These asset paths match the failing deployment. A later build may return 404.
const targets = [
    "/configuration",
    "/assets/BreadcrumbComponent-BvG082ru.css",
    "/assets/ProfileView-B1LrqIbU.css",
];
const origin = "https://dev.healthgateway.gov.bc.ca";

function probe(url, { connectTimeoutMs = 10000, timeoutMs = 30000, signal } = {}) {
    return new Promise((resolve) => {
        const started = performance.now();
        const elapsed = () => Number(((performance.now() - started) / 1000).toFixed(6));
        const result = {
            startedAt: new Date().toISOString(), path: new URL(url).pathname,
            http_code: null, remote_ip: null, time_namelookup: null,
            time_connect: null, time_appconnect: null, time_starttransfer: null,
            size_download: 0, content_type: null, connectionError: null,
        };
        let finished = false;
        let request;
        let connectTimer;
        let deadline;
        function finish(code = null) {
            if (finished) return;
            finished = true;
            clearTimeout(connectTimer);
            clearTimeout(deadline);
            signal?.removeEventListener("abort", abort);
            result.connectionError = code;
            result.time_total = elapsed();
            result.finishedAt = new Date().toISOString();
            resolve(result);
        }
        function abort() {
            finish("PROBE_STOPPED");
            request?.destroy();
        }
        if (signal?.aborted) {
            abort();
            return;
        }
        // Direct connection outside Cypress; no redirects, credentials, or cookies.
        // Request gzip like the browser, but discard the compressed body as received.
        const transport = new URL(url).protocol === "https:" ? https : http;
        request = transport.get(url, {
            agent: false,
            headers: { "Accept-Encoding": "gzip", "User-Agent": "HG-HTTP-Diagnostics" },
        }, (response) => {
            result.http_code = response.statusCode;
            result.content_type = response.headers["content-type"] || null;
            result.time_starttransfer = elapsed();
            response.on("data", (chunk) => { result.size_download += chunk.length; });
            response.on("end", () => finish());
            response.on("error", (error) => finish(error.code || "RESPONSE_ERROR"));
            response.on("aborted", () => finish("RESPONSE_ABORTED"));
        });
        signal?.addEventListener("abort", abort, { once: true });
        request.on("socket", (socket) => {
            socket.once("lookup", () => { result.time_namelookup = elapsed(); });
            socket.once("connect", () => {
                result.remote_ip = socket.remoteAddress;
                result.time_connect = elapsed();
                if (transport === http) clearTimeout(connectTimer);
            });
            socket.once("secureConnect", () => {
                result.time_appconnect = elapsed();
                clearTimeout(connectTimer);
            });
        });
        request.on("error", (error) => finish(error.code || "REQUEST_ERROR"));
        connectTimer = setTimeout(() => {
            finish("CONNECT_TIMEOUT");
            request.destroy();
        }, connectTimeoutMs);
        // Wall-clock deadline includes DNS, connection, TLS and the entire body.
        deadline = setTimeout(() => {
            finish("REQUEST_TIMEOUT");
            request.destroy();
        }, timeoutMs);
    });
}

function main() {
    const controller = new AbortController();
    const timers = new Set();
    const context = {
        build: process.env.BUILD_BUILDID, job: process.env.SYSTEM_JOBID,
        jobAttempt: process.env.SYSTEM_JOBATTEMPT, agent: process.env.AGENT_NAME,
    };
    const write = (record) => process.stdout.write(`${JSON.stringify({ ...context, ...record })}\n`);
    async function sample(path) {
        try {
            write(await probe(`${origin}${path}`, { signal: controller.signal }));
        } catch (error) {
            // No raw error messages: URLs or environment data must not leak.
            write({ event: "probe-error", path, code: error.code || "PROBE_ERROR" });
        }
        if (!controller.signal.aborted) {
            const timer = setTimeout(() => { timers.delete(timer); sample(path); }, 15000);
            timers.add(timer);
        }
    }
    function stop() {
        for (const timer of timers) clearTimeout(timer);
        controller.abort();
    }
    process.on("SIGTERM", stop);
    process.on("SIGINT", stop);
    write({ event: "probe-start", startedAt: new Date().toISOString(), origin,
        transport: "node-direct", timingUnit: "seconds", nodeVersion: process.version,
        intervalAfterResponseSeconds: 15, connectTimeoutSeconds: 10, requestTimeoutSeconds: 30 });
    for (const path of targets) sample(path);
}

module.exports = { probe };
if (require.main === module) main();
