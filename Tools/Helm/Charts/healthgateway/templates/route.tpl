# route template
{{- define "route.tpl" }}
{{- $top := index . 0 -}}
{{- $context := index . 1 -}}
{{- $name := printf "%s-%s" $top.Release.Name $context.name -}}
{{- $namespace := $top.Values.namespace | default $top.Release.Namespace -}}
{{- $labels := include "standard.labels" $top -}}
{{- $port := ($context.port | default 8080) -}}
{{- $protocol := ($context.protocol | default "tcp") -}}
{{- $timeout := $context.routeTimeout | default  "60s" -}}
{{- range $host := $context.hosts }}
kind: Route
apiVersion: route.openshift.io/v1
metadata:
  name: {{ $name }}-{{ $host.host }}-rt
  namespace: {{ $namespace }}
  labels: {{ $labels | nindent 4 }}
  annotations:
    haproxy.router.openshift.io/hsts_header: max-age=31536000;includeSubDomains;preload
    haproxy.router.openshift.io/balance: leastconn
    haproxy.router.openshift.io/timeout: {{ $timeout }}
spec:
  host: {{ $host.host }}
  path: {{ $host.path | default "" }}
  port:
    targetPort: {{ printf "%s-%s" ($port | toString) $protocol }}
  tls:
    insecureEdgeTerminationPolicy: Redirect
    termination: edge
    {{- if $host.key }}
    key: | {{ $top.Files.Get $host.key | trim | nindent 6 }}
    certificate: | {{ $top.Files.Get $host.certificate | trim | nindent 6 }}
    caCertificate: | {{ $top.Files.Get $host.caCertificate | trim | nindent 6 }}
    {{- end }}
  to:
    kind: Service
    name: {{ $name }}-svc
    weight: 100
---
{{- end -}}
{{- end -}}
