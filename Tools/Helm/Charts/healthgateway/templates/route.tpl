# route template
{{- define "route.tpl" }}
{{- $top := index . 0 -}}
{{- $context := index . 1 -}}
{{- $name := printf "%s-%s" $top.Release.Name $context.name -}}
{{- $namespace := $top.Values.namespace | default $top.Release.Namespace -}}
{{- $labels := include "standard.labels" $top -}}
{{- $port := ($context.port | default 8080) -}}
{{- $protocol := ($context.protocol | default "tcp") -}}
{{- $path := ($context.routePath | default "" ) -}}
{{- $host := $context.host -}}
{{- $timeout := $context.routeTimeout | default  "60s" -}}
kind: Route
apiVersion: route.openshift.io/v1
metadata:
  name: {{ $name }}-rt
  namespace: {{ $namespace }}
  labels: {{ $labels | nindent 4 }}
  annotations:
    haproxy.router.openshift.io/hsts_header: max-age=31536000;includeSubDomains;preload
    haproxy.router.openshift.io/balance: leastconn
    haproxy.router.openshift.io/timeout: {{ $timeout }}
spec:
  host: {{ $host | quote }}
  path: {{ $path }}
  port:
    targetPort: {{ printf "%d-%s" $port $protocol }}
  tls:
    insecureEdgeTerminationPolicy: Redirect
    termination: edge
  to:
    kind: Service
    name: {{ $name }}-svc
    weight: 100
{{- end -}}
